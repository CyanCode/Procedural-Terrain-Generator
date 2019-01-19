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

        private static object _lock = new object();

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
                lock (_lock) {
                    BiomeMap = Combiner.Sampler.GetBiomeMap(TerraConfig.Instance, _gridPosition, _resolution);
                }

				MTDispatch.Instance().Enqueue(() => {
				    if (BiomeMap == null) {
				        Debug.LogWarning("CalculateBiomeMap() failed to produce a non-null BiomeMap");
				        return;
				    }

				    SetSplatPrototypes();
				    SetAlphamap();
				    ApplyAlphamap();

                    if (onComplete != null) {
						onComplete();
					}
				});
			});
		}

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

			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
                    float[] weights = GetSplatWeights(x, y, biomes, resolution);

                    //Normalize weights before assigning to Alphamap
                    float sum = weights.Sum();
                    for (var i = 0; i < weights.Length; i++) {
                        Alphamap[y, x, i] = weights[i] / sum;
                    }
				}
			}
            
//            //todo remove
            if (_tile.GridPosition.X == 1 && _tile.GridPosition.Z == 0) {
                Debug.Log("Angle: " + angleRec.GetMinMax());
                Debug.Log("Height: " + heightRec.GetMinMax());

//                MTDispatch.Instance().Enqueue(() => {
//                    MathUtil.WriteDebugTexture(BiomeMap, Application.dataPath + "/biome.jpg");
//                });
            }
//            //todo remove
        }

        MinMaxRecorder angleRec = new MinMaxRecorder();
        MinMaxRecorder heightRec = new MinMaxRecorder();
        private float[] GetSplatWeights(int x, int y, BiomeNode[] biomes, int resolution) {
            float[] weights = new float[Splats.Length];
            Vector2 norm = new Vector2(x / (float)resolution, y / (float)resolution);
            Vector2 world = MathUtil.NormalToWorld(_tile.GridPosition, norm);

            float height = _terrain.terrainData.GetInterpolatedHeight(norm.x, norm.y) /
                           TerraConfig.Instance.Generator.Amplitude;
            float angle = Vector3.Angle(Vector3.up,
                              _terrain.terrainData.GetInterpolatedNormal(norm.x, norm.y)) / 90;
            angleRec.Register(angle);
            heightRec.Register(height);
            for (int z = 0; z < biomes.Length; z++) {
                SplatDetailNode[] splats = biomes[z].GetSplatInputs();

                float[] splatsWeights = new float[splats.Length];
                float curMax = 1f;
                float oldMax = 1f;
                float sum = 0f;

                for (var i = 0; i < splats.Length; i++) {
                    //If no constraint, display
                    SplatDetailNode splat = splats[i];
                    ConstraintNode cons = splat.GetConstraintValue();
                    if (cons == null) {
                        splatsWeights[i] = BiomeMap[x, y, z];
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

                for (int i = 0; i < splats.Length; i++) {
                    float val = splatsWeights[i];
                    float remapped = MathUtil.Map(val, 0f, oldMax, 0f, curMax);

                    if (sum >= 1f) {
                        //No remaining weight to assign, terminate
                        break;
                    }
                    if (i == splats.Length - 1) {
                        //All remaining weight goes to last idx 
                        splatsWeights[i] = 1 - sum;
                        break;
                    }

                    sum += remapped;
                    splatsWeights[i] = remapped;
                    oldMax = curMax;
                    curMax = 1 - remapped;
                }

                for (int i = 0; i < splats.Length; i++) {
                    splatsWeights[i] *= BiomeMap[x, y, z];
                }

                for (int i = 0; i < splatsWeights.Length; i++) {
                    weights[i + z] = splatsWeights[i];
                }
            }

            return weights;
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
