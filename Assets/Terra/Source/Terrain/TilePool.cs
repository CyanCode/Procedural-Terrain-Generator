using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Terra.Structures;
using Debug = UnityEngine.Debug;

namespace Terra.Terrain {
    /// <summary>
    /// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
    /// using a thread pool.
    /// </summary>
    [Serializable]
    public class TilePool {
        private const int CACHE_SIZE = 50;

        //Keeps track of tiles that were queued for generation.
        [SerializeField]
        private int _queuedTiles = 0;

        private bool _isFirstUpdate = true;

        [SerializeField]
        private float _remapMin = 0;

        [SerializeField]
        private float _remapMax = 1;

        private bool _isGenerating = false;

        private TerraConfig Config {
            get { return TerraConfig.Instance; }
        }

        public TileCache Cache;

        public Thread BackgroundThread;

        /// <summary>
        /// Min value after calling <see cref="CalculateHeightmapRemap"/>. Used 
        /// for remapping the heightmap upon calling <see cref="Tile.Generate"/>
        /// </summary>
        public float RemapMin {
            get { return _remapMin; }
        }

        /// <summary>
        /// Max value after calling <see cref="CalculateHeightmapRemap"/>. Used 
        /// for remapping the heightmap upon calling <see cref="Tile.Generate"/>
        /// </summary>
        public float RemapMax {
            get { return _remapMax; }
        }

        /// <summary>
        /// Returns the amount of tiles that are currently set as 
        /// active.
        /// </summary>
        public int ActiveTileCount {
            get {
                if (Cache == null) {
                    return 0;
                }

                return Cache.ActiveTiles.Count;
            }
        }

        /// <summary>
        /// Returns a list of tiles that are currently active in the 
        /// scene
        /// </summary>
        public List<Tile> ActiveTiles {
            get {
                return Cache.ActiveTiles;
            }
        }

        public TilePool() {
            if (Cache == null)
                Cache = new TileCache(CACHE_SIZE);
        }

        /// <summary>
        /// Calculate a circular list of <see cref="GridPosition"/>s around 
        /// the passed <see cref="center"/> with a passed radius. The returned 
        /// list is sorted by distance from <see cref="center"/>
        /// </summary>
        /// <param name="radius">Radius of circle (in grid units)</param>
        /// <param name="center">Center of circle in world space</param>
        /// <param name="length">Length of grid squares</param>
        /// <returns></returns>
        public static List<GridPosition> GetTilePositionsFromRadius(int radius, Vector2 center, int length) {
            int xPos = Mathf.RoundToInt(center.x / length);
            int zPos = Mathf.RoundToInt(center.y / length);
            SortedList<float, GridPosition> result = new SortedList<float, GridPosition>(25);

            for (var zCircle = -radius; zCircle <= radius; zCircle++) {
                for (var xCircle = -radius; xCircle <= radius; xCircle++) {
                    if (xCircle * xCircle + zCircle * zCircle < radius * radius) {
                        var newPos = new GridPosition(xPos + xCircle, zPos + zCircle);
                        var distance = newPos.Distance(new GridPosition(xPos, zPos));

                        while (result.ContainsKey(distance))
                            distance++;
                        result.Add(distance, newPos);
                    }
                }
            }

            var normResult = new List<GridPosition>(result.Count);
            normResult.AddRange(result.Values);

            return normResult;
        }

        /// <summary>
        /// Manually add (and activate) the passed Tile to the TilePool. This does not modify 
        /// <see cref="t"/>. To automatically create a Tile according to 
        /// <see cref="TerraConfig"/> call <see cref="CreateTileAt"/> instead.
        /// </summary>
        /// <param name="t">Tile to add</param>
        public void AddTile(Tile t) {
            if (t != null) {
                Cache.AddActiveTile(t);
            } else {
                Debug.LogWarning("Trying to add Tile that is null.");
            }
        }

        /// <summary>
        /// Creates a Tile at the passed grid position and calls 
        /// <see cref="Tile.Generate"/>
        /// </summary>
        /// <param name="p">position in grid to add tile at.</param>
        /// <param name="onComplete">called when the Tile has finished generating.</param>
        public void CreateTileAt(GridPosition p, Action<Tile> onComplete) {
            Tile t = Tile.CreateTileGameobject("Tile [" + p.X + ", " + p.Z + "]");
            t.UpdatePosition(p);

            t.Generate(() => {
                AddTile(t);
                onComplete(t);
            }, RemapMin, RemapMax);
        }

        /// <summary>
        /// Calculates the min and maximum values to use when applying a heightmap 
        /// remap. This sets <see cref="RemapMax"/> and <see cref="RemapMin"/>.
        /// </summary>
        public void CalculateHeightmapRemap() {
#if TERRA_DEBUG
            Stopwatch sw = new Stopwatch();
			sw.Start();
#endif

            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            int res = Config.Generator.RemapResolution;
            var generator = Config.Graph.GetEndGenerator();

            for (int x = 0; x < res; x++) {
                for (int z = 0; z < res; z++) {
                    float value = generator.GetValue(x / (float)res, z / (float)res, 0f);

                    if (value > max) {
                        max = value;
                    }
                    if (value < min) {
                        min = value;
                    }
                }
            }

            //Set remap values for instance
            _remapMin = min;
            _remapMax = max;

#if TERRA_DEBUG
			sw.Stop();
			Debug.Log("CalculateHeightmapRemap took " + sw.ElapsedMilliseconds + "ms to complete. " +
				        "New min=" + min + " New max=" + max);
#endif
        }

        /// <summary>
        /// Remove all active Tiles from the scene. Skips caching.
        /// </summary>
        public void RemoveAll() {
            _queuedTiles = 0;

            for (int i = 0; i < ActiveTileCount; i++) {
                Cache.RemoveActiveTile(ActiveTiles[i]);
                i--;
            }
        }

        /// <summary>
        /// Halts the generation of tiles by resetting the queued tile count.
        /// </summary>
        public void ResetQueue() {
            _queuedTiles = 0;
            _isFirstUpdate = true;
            _isGenerating = false;
        }

        /// <summary>
        /// Updates tiles to generate when the current queue of tiles 
        /// has finished generating.
        /// </summary>
        public void Update() {
            //Register event handlers for object placer
            if (_isFirstUpdate) {
                TerraConfig.Instance.Placer.RegisterTileEventListeners();
            }

            //Calculate remap
            if (Config.Generator.RemapHeightmap && _isFirstUpdate) {
                _isFirstUpdate = false;
                CalculateHeightmapRemap();
            }

            Cache.PurgeDestroyedTiles();

            if (_queuedTiles < 1 && !_isGenerating) {
                Config.StartCoroutine(UpdateTiles());
            }
        }

        /// <summary>
        /// Updates tiles that are surrounding the tracked GameObject 
        /// asynchronously. When calling this method using 
        /// <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>, 
        /// tiles are generated once per frame
        /// </summary>
        public IEnumerator UpdateTiles() {
            List<GridPosition> nearbyPositions = GetTilePositionsFromRadius();
            List<GridPosition> newPositions = Cache.GetNewTilePositions(nearbyPositions);
            List<Tile> toRegenerate = new List<Tile>();

            RemoveOldPositions(ref nearbyPositions, ref toRegenerate);

            //Add new positions
            _queuedTiles = newPositions.Count + toRegenerate.Count;

            foreach (GridPosition pos in newPositions) {
                if (_isGenerating && Application.isPlaying) //Wait and generate one tile per frame
                    yield return null;

                Tile cached = Cache.GetCachedTileAtPosition(pos);

                //Attempt to pull from cache, generate if not available
                if (cached != null) {
                    if (cached.IsHeightmapLodValid()) { //Cached tile is valid, use it
                        AddTile(cached);
                        _queuedTiles--;

                        continue;
                    }

#if TERRA_DEBUG
					Debug.Log("Cached tile " + cached + " has heightmap res=" + cached.MeshManager.HeightmapResolution +
					    ". requested res=" + cached.GetLodLevel().Resolution + ". Regenerating.");
#endif

                    toRegenerate.Add(cached);
                    Cache.AddActiveTile(cached);
                    continue;
                }

                _isGenerating = true;
                CreateTileAt(pos, tile => {
                    _queuedTiles--;

                    if (_queuedTiles == 0)
                        UpdateNeighbors(newPositions.ToArray(), false);

                    _isGenerating = false;
                });
            }

            //Regenerate tiles with outdated heightmaps
            for (int i = 0; i < toRegenerate.Count; i++) {
                Tile t = toRegenerate[i];
#if TERRA_DEBUG
				Debug.Log("Active tile " + t + " has heightmap res=" + t.MeshManager.HeightmapResolution +
							". requested res=" + t.GetLodLevel().Resolution + ". Regenerating.");
#endif

                //Generate one tile per frame
                if (Application.isPlaying)
                    yield return null;

                if (Application.isPlaying) {
                    Config.StartCoroutine(t.UpdateHeightmapAsync(() => {
                        UpdateNeighbors(new[] { t.GridPosition }, false);
                        _queuedTiles--;
                    }, RemapMin, RemapMax));
                } else {
                    t.UpdateHeightmap(RemapMin, RemapMax);
                }
            }

            //If tiles were updated synchronously, notify queue completion
            if (newPositions.Count > 0 && _queuedTiles == 0) {
                UpdateNeighbors(newPositions.ToArray(), false);
            }
        }

        /// <summary>
        /// Updates the neighboring Tiles of the passed list of 
        /// Tiles.
        /// </summary>
        /// <param name="positions">A list of tile positions to updatee</param>
        /// <param name="hideTerrain">Optionally hide the terrain when updating neighbors</param>
        /// <param name="onComplete">Called when a Tile's neighbors have been updated since 
        /// <see cref="TileMesh.SetTerrainHeightmap"/> can use a coroutine</param>
        public void UpdateNeighbors(GridPosition[] positions, bool hideTerrain) {
            if (positions == null)
                return;

            List<Tile> tiles = new List<Tile>(positions.Length);
            foreach (var pos in positions)
                foreach (var tile in Cache.ActiveTiles)
                    if (tile.GridPosition == pos)
                        tiles.Add(tile);

            if (tiles.Count == 0) {
                return;
            }

            //Get neighboring tiles
            foreach (Tile tile in tiles) {
                GridPosition[] neighborPos = tile.GridPosition.Neighbors;
                Tile[] neighborTiles = new Tile[4];

                for (int i = 0; i < 4; i++) {
                    Tile found = Cache.ActiveTiles.Find(t => t.GridPosition == neighborPos[i]);
                    neighborTiles[i] = found;
                }

                tile.MeshManager.SetNeighboringTiles(new Neighborhood(neighborTiles), hideTerrain);
            }
        }

        /// <summary>
        /// Removes old unneeded positions from the passed nearbyPositions list.
        /// </summary>
        /// <param name="nearbyPositions"></param>
        /// <param name="needRegenerating"></param>
        private void RemoveOldPositions(ref List<GridPosition> nearbyPositions, ref List<Tile> needRegenerating) {
            for (int i = 0; i < Cache.ActiveTiles.Count; i++) {
                bool found = false;

                foreach (GridPosition nearby in nearbyPositions) {
                    Tile t = Cache.ActiveTiles[i];

                    if (t.GridPosition == nearby) { //Position found in ActiveTiles
                        if (t.IsHeightmapLodValid()) { //Correct heightmap, ignore
                            found = true;
                            break;
                        }

                        //Invalid heightmap, mark for regeneration
                        found = true;
                        needRegenerating.Add(t);
                    }
                }

                if (!found) {
                    Cache.CacheTile(Cache.ActiveTiles[i]);
                    Cache.ActiveTiles.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Takes the passed chunk position and returns all other chunk positions in <see cref="TerraConfig.Generator.GenerationRadius"/>
        /// </summary>
        /// <returns>Tile x & z positions to add to world</returns>
        private List<GridPosition> GetTilePositionsFromRadius() {
            Vector3 world3D = Config.Generator.TrackedObject.transform.position;
            Vector2 world = new Vector2(world3D.x, world3D.z);
            return GetTilePositionsFromRadius(Config.Generator.GenerationRadius, world, Config.Generator.Length);
        }
    }
}