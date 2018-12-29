using System;
using System.IO;
using System.Linq;
using System.Text;
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
		public SplatObjectNode[] Splats;

		/// <summary>
		/// Standard unity terrain representation of an alphamap. Call <see cref="SetAlphamap"/> 
		/// to fill.
		/// See <see cref="https://docs.unity3d.com/ScriptReference/TerrainData.SetAlphamaps.html"/>
		/// </summary>
		public float[,,] Alphamap { get; private set; }

		public float[,,] BiomeMap;

		private UnityEngine.Terrain _terrain {
			get {
				return _tile.GetComponent<UnityEngine.Terrain>();
			}
		}

		[SerializeField]
		private Tile _tile;
		[SerializeField]
		private BiomeCombinerNode _combiner;
		[SerializeField]
		private int _resolution;
		[SerializeField]
		private GridPosition _gridPosition;
		
		public TilePaint(Tile tile) {
			_tile = tile;

			//Get biome combiner from terraconfig
			_combiner = TerraConfig.Instance.Graph.GetBiomeCombiner();
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
					BiomeMap = _combiner.Sampler.GetBiomeMap(TerraConfig.Instance, _gridPosition, _resolution);

					MTDispatch.Instance().Enqueue(() => {
						PostCreateBiomeMap();

						if (onComplete != null) {
							onComplete();
						}
					});
				});
			} else {
				BiomeMap = _combiner.Sampler.GetBiomeMap(TerraConfig.Instance, _gridPosition, _resolution);
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
			BiomeNode[] connected = _combiner.GetConnectedBiomeNodes();

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

		public void NormalizeAlphamap() {
			MinMaxResult minMax = MathUtil.MinMax(Alphamap);
			Alphamap = MathUtil.Map01(Alphamap, minMax.Min, minMax.Max);
		}

		/// <summary>
		/// Applies the calculated alphamap to the Terrain.
		/// </summary>
		public void ApplyAlphamap() {
			NormalizeAlphamap();
			_terrain.terrainData.alphamapResolution = _terrain.terrainData.heightmapResolution;
			_terrain.terrainData.SetAlphamaps(0, 0, Alphamap);
		}
		
		public void SetSplatPrototypes() {
			SetSplats();	

			SplatPrototype[] prototypes = new SplatPrototype[Splats.Length];
			
			for (int i = 0; i < Splats.Length; i++) {
				SplatObjectNode sd = Splats[i];
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
		/// Calculates the weights of each splat texture within this 
		/// <see cref="BiomeMap"/>. 
		/// </summary>
		/// <param name="biomes">List of biomes at some point on the Tile</param>
		/// <param name="biomeWeights">List of weights associated with the passed biomes</param>
		/// <param name="height">Height at some point on the Tile</param>
		/// <param name="angle">Angle at some point on the Tile</param>
		/// <returns>Weight of each texture in the order they appear in <see cref="BiomeMap"/></returns>
		private float[] GetTextureWeights(BiomeData[] biomes, float[] biomeWeights, float height, float angle) {
			int totalSplatCount = Splats.Length;
			float[] weights = new float[totalSplatCount];
			
			height = Mathf.Clamp01(height);

			//TODO most likely causing issues with terrain painting
			//this is the only different code between here and WeightedBiomeMap.GetTexturePreview()
			for (int bi = 0; bi < biomes.Length; bi++) {
				BiomeData b = biomes[bi];
				float biomeWeight = biomeWeights[bi];

				//Length of SplatData in previous biomes
				int prevBiomeSplatCount = 0;
				for (int i = 0; i < bi; i++) {
					prevBiomeSplatCount += biomes[i].Details.SplatsData.Count;
				}
				
				//Calculate individual "splat weight"
				for (int si = 0; si < b.Details.SplatsData.Count; si++) {
					int weightIndex = prevBiomeSplatCount + si;
					SplatData splat = b.Details.SplatsData[si];

					//Check whether this SplatData fits required height and angle constraints
					bool passHeight = splat.ConstrainHeight && splat.HeightConstraint.Fits(height) || !splat.ConstrainHeight;
					bool passAngle = splat.ConstrainAngle && splat.AngleConstraint.Fits(angle) || !splat.ConstrainAngle;

					if (!passHeight && !passAngle) {
						continue;
					}

					//If it passes height or angle constraints what are the texturing weights
					float weight = 0;
					int count = 0;
					if (passHeight) {
						weight += splat.HeightConstraint.Weight(height, splat.Blend);
						count++;
					}
					if (passAngle) {
						weight += splat.AngleConstraint.Weight(angle, splat.Blend);
						count++;
					}
					
					weight /= count;
					weight *= biomeWeight;

					//Blend the bottom with the top
					if (weightIndex > 0) {
						weights[weightIndex - 1] = 1 - weight;
						weights[weightIndex] = weight;
					} else {
						weights[weightIndex] = 1f;
					}
				}
			}

			//Normalize weights
			float sum = weights.Sum();
			for (var i = 0; i < weights.Length; i++) {
				weights[i] /= sum;
			}

			return weights;
		}

		/// <summary>
		/// Fills <see cref="Splats"/> with the splat data it needs to render 
		/// terrain. Assumes <see cref="BiomeMap"/> has been calculated
		/// first.
		/// </summary>
		private void SetSplats() {
			if (BiomeMap == null)
				return;

			Splats = _combiner.GetConnectedBiomeNodes().SelectMany(b => b.GetSplatObjects()).ToArray();
		}
	}
}
