using System;
using System.Linq;
using System.Threading;
using Terra.Graph.Biome;
using Terra.Structures;
using Terra.Util;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class TilePaint {
		/// <summary>
		/// List of control texture maps. 1 control map holds placement 
		/// information for up to 4 splat maps. This is a cached result 
		/// from calling <see cref="GatherTextures"/>
		/// </summary>
		public Texture2D[] Controls;

		/// <summary>
		/// List of SplatData that are placed (splatted) onto the terrain.
		/// This is a cached result from calling <see cref="GatherTextures"/>
		/// </summary>
		public SplatDetailNode[] Splats;

		/// <summary>
		/// Standard unity terrain representation of an alphamap. Call <see cref="SetAlphamap"/> 
		/// to fill.
		/// See <see cref="https://docs.unity3d.com/ScriptReference/TerrainData.SetAlphamaps.html"/>
		/// </summary>
		public float[,,] Alphamap { get; private set; }

		public float[,,] BiomeMap;
	    public BiomeCombinerNode Combiner;

		private UnityEngine.Terrain _terrain {
			get {
				return _tile.GetComponent<UnityEngine.Terrain>();
			}
		}
        
        [SerializeField]
		private Tile _tile;
		[SerializeField]
		private int _resolution;
		[SerializeField]
		private GridPosition _gridPosition;
		
		public TilePaint(Tile tile) {
			_tile = tile;

			//Get biome combiner from terraconfig
			Combiner = TerraConfig.Instance.Graph.GetBiomeCombiner();
			_resolution = _tile.GetLodLevel().SplatResolution;
			_gridPosition = _tile.GridPosition;
		}

		/// <summary>
		/// Paints this Tile according to <see cref="TerraConfig"/>
		/// </summary>
		/// <param name="async">Create the BiomeMap for this Tile off of the main thread</param>
		/// <param name="onComplete">Optional callback called after painting the terrain</param>
		public void Paint(bool async, Action onComplete = null) {
			if (async) {
				ThreadPool.QueueUserWorkItem(d => { //Worker thread 
					BiomeMap = Combiner.Sampler.GetBiomeMap(TerraConfig.Instance, _gridPosition, _resolution);

					MTDispatch.Instance().Enqueue(() => {
						PostCreateBiomeMap();

						if (onComplete != null) {
							onComplete();
						}
					});
				});
			} else {
				BiomeMap = Combiner.Sampler.GetBiomeMap(TerraConfig.Instance, _gridPosition, _resolution);
				PostCreateBiomeMap();

				if (onComplete != null) {
					onComplete();
				}
			}
		}

		/// <summary>
		/// Calculates the <see cref="Alphamap"/> for this <see cref="Tile"/>. 
		/// </summary>
		public void SetAlphamap() { 
			//Initialize Alphamap structure
			int resolution = _tile.GetLodLevel().SplatResolution;
			Alphamap = new float[resolution, resolution, Splats.Length];

			//Sample weights and fill in textures
			BiomeNode[] connected = Combiner.GetConnectedBiomeNodes();

			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {	
					//biomes splats added in order to the splats array
					for (int z = 0; z < connected.Length; z++) {
						//TODO Expand to n amount of splat objects
						Alphamap[y, x, z] = BiomeMap[x, y, z];
						
						if (BiomeMap[x, y, z] < min) {
							min = BiomeMap[x, y, z];
						}
						if (BiomeMap[x, y, z] > max) {
							max = BiomeMap[x, y, z];
						}
					}
				}
			}

			//todo Debug.Log(string.Format("Alpha -- min: {0} max: {1}", min, max));
		}

		/// <summary>
		/// Applies the calculated alphamap to the Terrain.
		/// </summary>
		public void ApplyAlphamap() {
			_terrain.terrainData.alphamapResolution = _terrain.terrainData.heightmapResolution;
			_terrain.terrainData.SetAlphamaps(0, 0, Alphamap);
		}
		
		public void SetSplatPrototypes() {
			SetSplats();	

			SplatPrototype[] prototypes = new SplatPrototype[Splats.Length];
			
			for (int i = 0; i < Splats.Length; i++) {
				SplatDetailNode sd = Splats[i];
				SplatPrototype sp = new SplatPrototype();

				sp.metallic = 0;
				sp.smoothness = 0.2f;

				sp.tileOffset = sd.Offset;
				sp.tileSize = sd.Tiling;

				sp.normalMap = sd.Normal;
				sp.texture = sd.Diffuse;

				prototypes[i] = sp;
			}

			_terrain.terrainData.splatPrototypes = prototypes;
		}

		/// <summary>
		/// Continuation of <see cref="Paint"/>'s logic after the biome map
		/// has been calculated asynchronously or synchronously.
		/// </summary>
		private void PostCreateBiomeMap() {
			if (BiomeMap == null) {
				Debug.LogWarning("CalculateBiomeMap() failed to produce a non-null BiomeMap");
				return;
			}

			SetSplatPrototypes();
			SetAlphamap();
			ApplyAlphamap();
		}

		/// <summary>
		/// Fills <see cref="Splats"/> with the splat data it needs to render 
		/// terrain. Assumes <see cref="BiomeMap"/> has been calculated
		/// first.
		/// </summary>
		private void SetSplats() {
			if (BiomeMap == null)
				return;

			Splats = Combiner.GetConnectedBiomeNodes().SelectMany(b => b.GetSplatObjects()).ToArray();
		}
	}
}
