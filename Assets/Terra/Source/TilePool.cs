﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Terra.Data;

namespace Terra.Terrain {
	/// <summary>
	/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
	/// using a thread pool.
	/// </summary>
	[Serializable]
	public class TilePool {
		private const int CACHE_SIZE = 50;
		
		[SerializeField]
		private bool _isGeneratingTile = false;

		//Keeps track of tiles that were queued for generation.
		[SerializeField]
		private int _queuedTiles = 0;
		private Action<GridPosition[]> _queueCompletedAction;

		private TerraSettings _settings {
			get { return TerraSettings.Instance; }
		}

		public TileCache Cache;

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
		/// <param name="center">Center of circle</param>
		/// <param name="length">Length of grid squares</param>
		/// <returns></returns>
		public static List<GridPosition> GetTilePositionsFromRadius(int radius, GridPosition center, int length) {
			int xPos = center.X / length;
			int zPos = center.Z / length;
			SortedList<float, GridPosition> result = new SortedList<float, GridPosition>(25);

			for (var zCircle = -radius; zCircle <= radius; zCircle++) {
				for (var xCircle = -radius; xCircle <= radius; xCircle++) {
					if (xCircle * xCircle + zCircle * zCircle < radius * radius) {
						var newPos = new GridPosition(xPos + xCircle, zPos + zCircle);
						var distance = newPos.Distance(center);

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
		/// Updates tiles to update when the current queue of tiles 
		/// has finished generating.
		/// </summary>
		public void Update() {
			if (_queueCompletedAction == null) {
				_queueCompletedAction = UpdateNeighbors;
			}

			Cache.PurgeDestroyedTiles();

			if (_queuedTiles < 1) {
				_settings.StartCoroutine(UpdateTiles());
			}

			_settings.StartCoroutine(UpdateColliders(0.5f));
		}

		/// <summary>
		/// Halts the generation of tiles by resetting the queued tile count.
		/// </summary>
		public void ResetQueue() {
			_queuedTiles = 0;
		}

		/// <summary>
		/// Manually add (and activate) the passed Tile to the TilePool. This does not modify 
		/// <see cref="t"/>. To automatically create a Tile according to 
		/// <see cref="TerraSettings"/> call <see cref="AddTileAt"/> instead.
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
		/// Creates a Tile at the passed grid position, activates it, 
		/// and places it in the scene.
		/// This method calls <see cref="Tile.Generate"/>.
		/// </summary>
		/// <param name="p">position in grid to add tile at.</param>
		/// <param name="onComplete">called when the Tile has finished generating.</param>
		public void AddTileAt(GridPosition p, Action<Tile> onComplete) {
			Tile t = Tile.CreateTileGameobject("Tile [" + p.X + ", " + p.Z + "]");
			t.UpdatePosition(p);

			t.Generate(() => {
				AddTile(t);
				onComplete(t);
			}, _settings.Generator.UseMultithreading);
		}

		/// <summary>
		/// Remove all active Tiles from the scene. Skips caching.
		/// </summary>
		public void RemoveAll() {
			_queuedTiles = 0;
			_isGeneratingTile = false;

			for (int i = 0; i < ActiveTileCount; i++) {
				Cache.RemoveTile(ActiveTiles[i]);
				i--;
			}
		}

		/// <summary>
		/// Finds all <b>enabled</b> Tile instances that intersect 
		/// the passed square parameters
		/// </summary>
		/// <param name="trackedPos"><code>TrackedObject</code> position</param>
		/// <param name="extent">Extent of collision square, most likely <code>ColliderGenerationExtent</code></param>
		/// <returns>Found, overlapping, Tile instances</returns>
		public List<Tile> GetTilesInExtent(Vector3 trackedPos, float extent) {
			//TODO Remove params
			List<Tile> tiles = new List<Tile>(Cache.ActiveTiles.Count);

			foreach (Tile t in Cache.ActiveTiles) {
				MeshRenderer renderer = t.GetComponent<MeshRenderer>();

				if (renderer != null) {
					Vector3 tilePos = new Vector3(trackedPos.x, renderer.bounds.center.y, trackedPos.z);
					Bounds trackedBounds = new Bounds(tilePos, new Vector3(extent, renderer.bounds.max.y, extent));

					if (renderer.bounds.Intersects(trackedBounds))
						tiles.Add(t);
				}
			}

			return tiles;
		}

		/// <summary>
		/// Updates tiles that are surrounding the tracked GameObject 
		/// asynchronously. When calling this method using 
		/// <see cref="MonoBehaviour.StartCoroutine(System.Collections.IEnumerator)"/>, 
		/// tiles are generated once per frame
		/// </summary>
		public IEnumerator UpdateTiles() {
			List<GridPosition> nearbyPositions = GetTilePositionsFromRadius();
			List<GridPosition> newPositions = Cache.GetNewTilePositions(nearbyPositions);

			//Remove old positions
			for (int i = 0; i < Cache.ActiveTiles.Count; i++) {
				bool found = false;

				foreach (GridPosition nearby in nearbyPositions) {
					if (Cache.ActiveTiles[i].GridPosition == nearby) { //Position found, ignore
						found = true;
						break;
					}
				}

				if (!found) {
					Cache.CacheTile(Cache.ActiveTiles[i]);
					Cache.ActiveTiles.RemoveAt(i);
					i--;
				}
			}

			//Add new positions
			_queuedTiles = newPositions.Count;
			foreach (GridPosition pos in newPositions) {
				Tile cached = Cache.GetCachedTileAtPosition(pos);

				//Attempt to pull from cache, generate if not available
				if (cached != null) {
					AddTile(cached);
					_queuedTiles--;
				} else {
					//Generate one tile per frame
					if (Application.isPlaying)
						yield return null;

					_isGeneratingTile = true;
					AddTileAt(pos, tile => {
						_queuedTiles--;
						_isGeneratingTile = false;

						if (_queuedTiles == 0)
							_queueCompletedAction(newPositions.ToArray());
					});
				}
			}

			//If tiles were updated synchronously, notify queue completion
			if (newPositions.Count > 0 && _queuedTiles == 0) {
				_queueCompletedAction(newPositions.ToArray());
			}
		}

		/// <summary>
		/// Updates the neighboring Tiles of the passed list of 
		/// Tiles.
		/// </summary>
		public void UpdateNeighbors(GridPosition[] positions) {
			if (positions == null)
				return;

			List<Tile> tiles = new List<Tile>(positions.Length);
			foreach (var pos in positions)
				foreach (var tile in Cache.ActiveTiles)
					if (tile.GridPosition == pos)
						tiles.Add(tile);

			//Get neighboring tiles
			foreach (Tile tile in tiles) {
				GridPosition[] neighborPos = tile.GridPosition.Neighbors;
				Tile[] neighborTiles = new Tile[4];

				for (int i = 0; i < 4; i++) {
					Tile found = Cache.ActiveTiles.Find(t => t.GridPosition == neighborPos[i]);
					neighborTiles[i] = found;
				}

				tile.MeshManager.CalculateNeighboringNormals(new Neighborhood(neighborTiles));
			}
		}

		/// <summary>
		/// Updates colliders that match the Settings specified collider 
		/// generation extent.
		/// </summary>
		/// <param name="delay">Delay in seconds before checking colliders again</param>
		/// <returns>IEnumerator for use in a Coroutine</returns>
		private IEnumerator UpdateColliders(float delay) {
			//If we're generating all colliders the extent of collision generation is 
			//technically infinity (max value works just as well though)
			float extent = _settings.Generator.GenAllColliders ? float.MaxValue : _settings.Generator.ColliderGenerationExtent;
			List<Tile> tiles = GetTilesInExtent(_settings.Generator.TrackedObject.transform.position, extent);

			foreach (Tile t in tiles) {
				t.MeshManager.CalculateCollider();
				yield return null;
			}

			yield return new WaitForSeconds(delay);
		}

		/// <summary>
		/// Takes the passed chunk position and returns all other chunk positions in <see cref="TerraSettings.Generator.GenerationRadius"/>
		/// </summary>
		/// <returns>Tile x & z positions to add to world</returns>
		private List<GridPosition> GetTilePositionsFromRadius() {
			return GetTilePositionsFromRadius(_settings.Generator.GenerationRadius,
				new GridPosition(_settings.Generator.TrackedObject.transform), _settings.Generator.Length);
		}
	}

	/// <summary>
	/// Represents a position in the grid of <see cref="Tile"/>s
	/// </summary>
	[Serializable]
	public struct GridPosition {
		public int X;
		public int Z;

		/// <summary>
		/// The list of neighboring <see cref="GridPosition"/>s ordered 
		/// as Top, Right, Bottom, Left.
		/// </summary>
		public GridPosition[] Neighbors {
			get {
				return new[] {
						new GridPosition(X, Z + 1),
						new GridPosition(X + 1, Z),
						new GridPosition(X, Z - 1),
						new GridPosition(X - 1, Z)
					};
			}
		}

		public GridPosition(int x, int z) {
			X = x;
			Z = z;
		}

		public GridPosition(Transform transf) {
			X = (int)transf.position.x;
			Z = (int)transf.position.z;
		}

		/// <summary>
		/// The distance between <see cref="p"/> and this <see cref="Terra.Terrain.GridPosition"/>
		/// </summary>
		public float Distance(GridPosition p) {
			float x = this.X - p.X;
			float z = this.Z - p.Z;

			return Mathf.Sqrt((x * x) + (z * z));
		}

		public static bool operator ==(GridPosition p1, GridPosition p2) {
			return p1.X == p2.X && p1.Z == p2.Z;
		}

		public static bool operator !=(GridPosition p1, GridPosition p2) {
			return !(p1 == p2);
		}

		public bool Equals(GridPosition other) {
			return X == other.X && Z == other.Z;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is GridPosition && Equals((GridPosition)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (X * 397) ^ Z;
			}
		}

		public override string ToString() {
			return "[" + X + ", " + Z + "]";
		}
	}
}