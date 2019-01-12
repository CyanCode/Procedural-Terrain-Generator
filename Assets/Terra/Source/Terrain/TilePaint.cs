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
	    /// <param name="onComplete">Optional callback called after painting the terrain</param>
	    public void Paint(Action onComplete = null) {
			ThreadPool.QueueUserWorkItem(d => { //Worker thread 
				BiomeMap = Combiner.Sampler.GetBiomeMap(TerraConfig.Instance, _gridPosition, _resolution);

				MTDispatch.Instance().Enqueue(() => {
					PostCreateBiomeMap();

					if (onComplete != null) {
						onComplete();
					}
				});
			});
		}

//        void SetSplatWeightFor(BiomeNode[] biomes, int x, int y, float height, float angle) {
//            SplatDetailNode[] splats = biomes.SelectMany(bn => bn.GetSplatInputs()).ToArray();
//            float[] weights = new float[splats.Length];
//            float sum = 0f;
//
//            //Biomes at the end of "connected" array overlay previous biomes
//            int splatIdx = 0;
//            for (int i = 0; i < biomes.Length; i++) {
//                float bweight = BiomeMap[x, y, i];
//                SplatDetailNode[] inputs = biomes[i].GetSplatInputs();
//
//                //Calculate individual "splat weight"
//                int offsetSize = splatIdx + inputs.Length;
//                for (int si = splatIdx; si < offsetSize; si++) {
//                    if (sum >= 1f) {
//                        //No remaining weight to assign, terminate
//                        break;
//                    }
//                    if (i == inputs.Length - 1) {
//                        //All remaining weight goes to last idx 
//                        weights[i] = 1 - sum;
//                        break;
//                    }
//
//                    sum += remapped;
//                    weights[i] = remapped;
//                }
//
//                splatIdx += inputs.Length;
//            }
//
//            return weights;
//        }

        /// <summary>
        /// Calculates the <see cref="Alphamap"/> for this <see cref="Tile"/>. 
        /// </summary>
        public void SetAlphamap() { 
			//Initialize Alphamap structure
			int resolution = _tile.GetLodLevel().SplatResolution;
			Alphamap = new float[resolution, resolution, Splats.Length];

			//Sample weights and fill in textures
			BiomeNode[] biomes = Combiner.GetConnectedBiomeNodes();
            Constraint minMaxMask = Combiner.Sampler.GetMaskConstraintMinMax().ToConstraint();

			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

            //todo remove
            float mi = float.PositiveInfinity;
            float ma = float.NegativeInfinity;
            //todo remove

			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
                    Vector2 norm = new Vector2(x / (float)resolution, y / (float)resolution);
				    Vector2 world = MathUtil.NormalToWorld(_tile.GridPosition, norm);

				    float height = _terrain.terrainData.GetInterpolatedHeight(norm.x, norm.y) /
				                   TerraConfig.Instance.Generator.Amplitude;
				    float angle = Vector3.Angle(Vector3.up,
				        _terrain.terrainData.GetInterpolatedNormal(norm.x, norm.y));
                    //limit the height of splat to height of biome
                    //find the max and min transition height for all biomes
                    //add method to biomecombinersampler?

                    for (int z = 0; z < biomes.Length; z++) {
                        SplatDetailNode[] splats = biomes[z].GetSplatInputs();

                        for (var i = 0; i < splats.Length; i++) {
                            //If no constraint, display
                            SplatDetailNode splat = splats[i];
                            ConstraintNode cons = splat.GetConstraintValue();
                            if (cons == null) {
                                Alphamap[y, x, i + z] = BiomeMap[x, y, z];
                                continue;
                            }

                            //Constrain Height to this biome
                            cons.HeightConstraint = cons.HeightConstraint.Clamp(minMaxMask);

                            if (splat.ShouldPlaceAt(world.x, world.y, height, angle)) {



                                //Check whether this SplatData fits required height and angle constraints
                                bool passHeight = cons.ConstrainHeight && cons.HeightConstraint.Fits(height) || !cons.ConstrainHeight;
                                bool passAngle = cons.ConstrainAngle && cons.AngleConstraint.Fits(angle) || !cons.ConstrainAngle;

                                if (!passHeight && !passAngle) {
                                    continue;
                                }

                                //If it passes height or angle constraints what are the texturing weights
                                float weight = 0;
                                int count = 0;
                                if (passHeight) {
                                    weight += cons.HeightConstraint.Weight(height, TerraConfig.Instance.Generator.BiomeBlendAmount);
                                    count++;
                                }
                                if (passAngle) {
                                    float w = cons.AngleConstraint.Weight(angle, 0.1f);
                                    //w /= 90;
                                    //todo remove
                                    if (w < mi) {
                                        mi = w;
                                    }
                                    if (w > ma) {
                                        ma = w;
                                    }
                                    //todo remove

                                    weight += w;
                                    count++;
                                }

                                weight /= count;
                                weight *= BiomeMap[x, y, z];

                                //Blend the bottom with the top
                                if (i + z > 0) {
                                    Alphamap[y, x, i + z - 1] = 1 - weight;
                                    Alphamap[y, x, i + z] = weight;
                                } else {
                                    Alphamap[y, x, i + z] = 1f;
                                }
                            }
                        }

                        if (BiomeMap[x, y, z] < min) {
                            min = BiomeMap[x, y, z];
                        }
                        if (BiomeMap[x, y, z] > max) {
                            max = BiomeMap[x, y, z];
                        }
                    }

                   // SetSplatWeightFor(biomes, x, y, height, angle);
				}
			}
            //todo remove
            if (_tile.GridPosition.X == 1 && _tile.GridPosition.Z == 0) {
                Debug.Log("min " + mi + "    max " + ma);
            }
            //todo remove
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

            Splats = Combiner.GetConnectedBiomeNodes()
                .Where(b => b.GetSplatInputs() != null)
                .SelectMany(b => b.GetSplatInputs())
                .ToArray();
        }
	}
}
