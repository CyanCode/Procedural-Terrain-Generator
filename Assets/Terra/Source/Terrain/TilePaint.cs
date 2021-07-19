using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Terra.Graph.Biome;
using Terra.Source;
using Terra.Structures;
using Terra.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Terra.Terrain {
	[Serializable]
	public class TilePaint {
        private struct AngleHeights {
            public float[,] Heights;
            public float[,] Angles;

            public AngleHeights(float[,] heights, float[,] angles) {
                Heights = heights;
                Angles = angles;
            }
        }

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
		/// Standard unity terrain representation of an alphamap. Call <see cref="CalculateAlphamap"/> 
		/// to fill.
		/// See <see cref="https://docs.unity3d.com/ScriptReference/TerrainData.SetAlphamaps.html"/>
		/// </summary>
		public float[,,] Alphamap { get; private set; }

		public BiomeNode[] Biomes;

		private BiomeSampler _biomeSampler;
		
		private UnityEngine.Terrain _terrain;
		
        /// <summary>
        /// Since terrain height & angle interpolated methods cannot be accessed 
        /// off the main thread, they must be precomputed
        /// </summary>
        private AngleHeights _angleHeightResults;

        [SerializeField]
		private Tile _tile;
		[SerializeField]
		private int _resolution;
		[SerializeField]
		private GridPosition _gridPosition;
		
        private static object _biomeMapLock = new object();
        private static object _alphamapLock = new object();

        public TilePaint(Tile tile) {
			_tile = tile;

			//Get biome combiner from terraconfig
			Biomes = TerraConfig.Instance.Graph.GetEndNode().GetBiomes();
			_biomeSampler = new BiomeSampler(Biomes);
		    _resolution = TerraConfig.Instance.Generator.SplatmapResolution - 1;
			_gridPosition = _tile.GridPosition;
            _terrain = _tile.GetComponent<UnityEngine.Terrain>();

            if (_terrain == null) {
                Debug.LogError("TilePaint's passed Tile must have an attached Terrain component");
            }
        }

	    /// <summary>
	    /// Paints this Tile according to <see cref="TerraConfig"/> asynchronously
	    /// </summary>
	    /// <param name="biomeMap">Biomemap result from <see cref="GetBiomeMap"/></param>
	    /// <param name="onComplete">Optional callback called after painting the terrain</param>
	    public IEnumerator PaintAsync(int[,] biomeMap, Action onComplete) {
			if (biomeMap == null) {
				Debug.LogWarning("CalculateBiomeMap() failed to produce a non-null BiomeMap");
				yield break;
			}

            int res = TerraConfig.Instance.Generator.SplatmapResolution - 1;
            PrecomputeAngleHeights(res);
			SetTerrainLayers(biomeMap);

            bool madeAm = false;
            TerraConfig.Instance.Worker.Enqueue(() => CalculateAlphamap(biomeMap, res), () => madeAm = true);
            while (!madeAm)
                yield return null;

            ApplyAlphamap();
            onComplete();
		}

	    /// <summary>
	    /// Paints this Tile according to <see cref="TerraConfig"/> synchronously
	    /// </summary>
	    /// <param name="biomeMap">Biomemap result from <see cref="GetBiomeMap"/></param>
        public void Paint(int[,] biomeMap) {
            if (biomeMap == null) {
                Debug.LogWarning("CalculateBiomeMap() failed to produce a non-null BiomeMap");
                return;
            }

            int res = TerraConfig.Instance.Generator.SplatmapResolution - 1;
            PrecomputeAngleHeights(res);
            SetTerrainLayers(biomeMap);
            
            CalculateAlphamap(biomeMap, res);

            ApplyAlphamap();
        }

        public int[,] GetBiomeMap() {
            lock (_biomeMapLock) {
                return _biomeSampler.GetBiomeMap(TerraConfig.Instance, _gridPosition, _resolution);
            }
        }

        /// <summary>
        /// Calculates the <see cref="Alphamap"/> for this <see cref="Tile"/>. 
        /// </summary>
        public void CalculateAlphamap(int[,] biomeMap, int resolution) { 
			//Initialize Alphamap structure
			Alphamap = new float[resolution, resolution, Splats.Length];

			//Sample weights and fill in textures
			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
                    float[] weights = GetSplatWeights(x, y, biomeMap, resolution);

                    //Normalize weights before assigning to Alphamap
                    float sum = weights.Sum();
                    for (var i = 0; i < weights.Length; i++) {
                        Alphamap[y, x, i] = weights[i] / sum;
                    }
				}
			}
        }

	    /// <summary>
        /// Applies the calculated alphamap to the Terrain.
        /// </summary>
        public void ApplyAlphamap() {
            GenerationData gen = TerraConfig.Instance.Generator;
            int res = gen.SplatmapResolution - 1;
            int maxResPerFrame = TerraConfig.Instance.Generator.CoroutineRes;
            int splatCount = Alphamap.GetLength(2);
            _terrain.terrainData.alphamapResolution = res;

//            if (gen.UseCoroutines && Application.isPlaying && res > maxResPerFrame) {
//                _tile.StartCoroutine(ApplyAlphamapCoroutine(res, maxResPerFrame, splatCount));
//            } else {
                _terrain.terrainData.SetAlphamaps(0, 0, Alphamap);
//            }
		}
		
		public void SetTerrainLayers(int[,] biomeMap) {
			SetSplats(biomeMap);

			TerrainLayer[] layers = new TerrainLayer[Splats.Length];
			
			for (int i = 0; i < Splats.Length; i++) {
				SplatDetailNode sd = Splats[i];
				TerrainLayer layer = new TerrainLayer();

				layer.metallic = 0;
				layer.smoothness = 0.2f;

				layer.tileOffset = sd.Offset;
				layer.tileSize = sd.Tiling;

				layer.normalMapTexture = sd.Normal;
				layer.diffuseTexture = sd.Diffuse;

				layers[i] = layer;
			}

			_terrain.terrainData.terrainLayers = layers;
		}

        private IEnumerator ApplyAlphamapCoroutine(int res, int maxResPerFrame, int splatCount) {
            int resFactor = res / maxResPerFrame;
            int subResolution = res / resFactor;

            //Loop through first chunk of the resolution
            for (int ix = 0; ix < resFactor; ix++) {
                for (int iy = 0; iy < resFactor; iy++) {
                    int xPlus1 = ix == resFactor - 1 ? 1 : 0;
                    int yPlus1 = iy == resFactor - 1 ? 1 : 0;

                    float[,,] subheights = new float[subResolution + yPlus1, subResolution + xPlus1, splatCount];

                    //Copy alphamap into new subdivision array
                    for (int x = 0; x < subResolution + xPlus1; x++) {
                        for (int y = 0; y < subResolution + yPlus1; y++) {
                            int thisAmX = ix * subResolution + x;
                            int thisAmY = iy * subResolution + y;

                            for (int z = 0; z < splatCount; z++) {
                                subheights[y, x, z] = Alphamap[thisAmY, thisAmX, z];
                            }
                        }
                    }

                    //Set alphamap for this subsection
                    _terrain.terrainData.SetAlphamaps(subResolution * ix, subResolution * iy, subheights);

                    //Wait for next frame
                    yield return null;
                }
            }
        }

        private void PrecomputeAngleHeights(int resolution) {
            float[,] heights = new float[resolution, resolution];
            float[,] angles = new float[resolution, resolution];

            for (int x = 0; x < resolution; x++) {
                for (int y = 0; y < resolution; y++) {
                    Vector2 norm = new Vector2(x / (float)resolution, y / (float)resolution);
                    float height = _terrain.terrainData.GetInterpolatedHeight(norm.x, norm.y) /
                                   TerraConfig.Instance.Generator.Amplitude;
                    float angle = Vector3.Angle(Vector3.up,
                                      _terrain.terrainData.GetInterpolatedNormal(norm.x, norm.y)) / 90;

                    heights[x, y] = height;
                    angles[x, y] = angle;
                }
            }

            _angleHeightResults = new AngleHeights(heights, angles);
        }

	    private float[] GetSplatWeights(int x, int y, int[,] biomeMap, int resolution) {
	        float[] weights = new float[Splats.Length];
	        Vector2 norm = new Vector2(x / (float)resolution, y / (float)resolution);
	        Vector2 world = MathUtil.NormalToWorld(_tile.GridPosition, norm);

	        int splatOffset = 0;
            float height = _angleHeightResults.Heights[x, y];
            float angle = _angleHeightResults.Angles[x, y];

	        for (int z = 0; z < Biomes.Length; z++) {
	            SplatDetailNode[] splats = Biomes[z].GetSplatInputs();
	            if (splats == null) {
		            continue;
	            }

	            float[] splatsWeights = new float[splats.Length];
	            float curMax = 1f;
	            float oldMax = 1f;
	            float sum = 0f;

	            for (var i = 0; i < splats.Length; i++) {
	                //If no constraint, display
	                SplatDetailNode splat = splats[i];
	                ConstraintNode cons = splat.GetConstraintValue();
	                if (cons == null) {
		                splatsWeights[i] = biomeMap[x, y] == z ? 1f : 0f;
	                    continue;
	                }

	                //Check whether this SplatData fits required height and angle constraints
	                bool passHeight = cons.ConstrainHeight && cons.HeightConstraint.Fits(height) || !cons.ConstrainHeight;
	                bool passAngle = cons.ConstrainAngle && cons.AngleConstraint.Fits(angle) || !cons.ConstrainAngle;

	                if (!passHeight && !passAngle || !splat.ShouldPlaceAt(world.x, world.y, height, angle)) {
	                    continue;
	                }

	                //If it passes height or angle constraints what are the texturing weights
	                float weight = 0;
	                int count = 0;

	                if (passHeight && !passAngle) {
	                    weight += cons.HeightConstraint.Weight(height, splat.Blend);
	                    count++;
	                }
	                if (passAngle) {
	                    weight += cons.AngleConstraint.Weight(angle, splat.Blend);
	                    count++;
	                }

	                splatsWeights[i] = weight / count;
	            }

	            // for (int i = 0; i < splats.Length; i++) {
	            //     float val = splatsWeights[i];
	            //     float remapped = MathUtil.Map(val, 0f, oldMax, 0f, curMax);
	            //
	            //     if (sum >= 1f) {
	            //         //No remaining weight to assign, terminate
	            //         break;
	            //     }
	            //     if (i == splats.Length - 1) {
	            //         //All remaining weight goes to last idx 
	            //         splatsWeights[i] = 1 - sum;
	            //         break;
	            //     }
	            //
	            //     sum += remapped;
	            //     splatsWeights[i] = remapped;
	            //     oldMax = curMax;
	            //     curMax = 1 - remapped;
	            // }

	            for (int i = 0; i < splatsWeights.Length; i++) {
	                weights[i + splatOffset] = splatsWeights[i];
	            }

	            splatOffset += splats.Length;
	        }

	        return weights;
	    }

	    /// <summary>
		/// Fills <see cref="Splats"/> with the splat data it needs to render 
		/// terrain. Assumes <see cref="BiomeMap"/> has been calculated
		/// first.
		/// </summary>
		private void SetSplats(int[,] biomeMap) {
			if (biomeMap == null)
				return;

            Splats = Biomes
                .Where(b => b.GetSplatInputs() != null)
                .SelectMany(b => b.GetSplatInputs())
                .ToArray();
        }
	}
}
