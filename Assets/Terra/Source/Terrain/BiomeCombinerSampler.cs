using System;
using System.Collections.Generic;
using System.Linq;
using Terra.CoherentNoise;
using Terra.Graph.Biome;
using Terra.Structures;
using Terra.Util;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class BiomeCombinerSampler {
		[SerializeField]
		private MinMaxResult? _cachedMinMax;
		[SerializeField]
		private BiomeCombinerNode _combiner;

        private static object _minMaxLock = new object();
        private static object _weightLock = new object();
        private static object _valueLock = new object();

		public BiomeCombinerSampler(BiomeCombinerNode combiner) {
			_combiner = combiner;
		}

		public Texture2D GetPreviewTexture(int size) {
			BiomeNode[] nodes = _combiner.GetConnectedBiomeNodes();
			Texture2D tex = new Texture2D(size, size);
			float[,,] map = GetBiomeMap(size);

			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					Color final = new Color(0, 0, 0);

					for (int z = 0; z < nodes.Length; z++) {
						float weight = map[x, y, z];
						BiomeNode biome = nodes[z];

						final += biome.PreviewColor * weight;
					}

					tex.SetPixel(x, y, final);
				}
			}

			tex.Apply();
			return tex;
		}

        /// <summary>
        /// Samples the passed biomemap at the passed normalized 
        /// x & y coordinates.
        /// </summary>
        /// <param name="x">normalized x coordinate</param>
        /// <param name="y">normalized y coordinate</param>
        /// <returns></returns>
        public float[] GetBiomeWeightsInterpolated(float[,,] map, float x, float y) {
            int res = map.GetLength(0);
            int sx = Mathf.Clamp(Mathf.RoundToInt(x * res), 0, res - 1);
            int sy = Mathf.Clamp(Mathf.RoundToInt(y * res), 0, res - 1);

            float[] result = new float[map.GetLength(2)];
            for (int i = 0; i < result.Length; i++) {
                result[i] = map[sx, sy, i];
            }

            return result;
        }

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="position">Position of this biome map in the grid of tiles</param>
		/// <param name="length">Length of a tile</param>
		/// <param name="spread">Divide x & z coordinates of the polled position by this number</param>
		/// <param name="resolution">Resolution of map</param>
		/// <returns></returns>
		public float[,,] GetBiomeMap(GridPosition position, int length, float spread, int resolution) {
			BiomeNode[] connected = _combiner.GetConnectedBiomeNodes();

			float[,,] biomeMap = new float[resolution, resolution, connected.Length];
			List<float[,]> weightedBiomeValues = new List<float[,]>(biomeMap.Length);

            //Gather each biome's values
            lock (_valueLock) {
                for (var i = 0; i < connected.Length; i++) {
                    BiomeNode biome = connected[i];
                    BiomeNode.BiomeMapResult biomeResults = biome.GetMapsValues(position, resolution, spread, length);
                    if (_cachedMinMax == null) {
                        _cachedMinMax = CalculateMinMax(TerraConfig.Instance.Generator.RemapResolution);
                    }

                    float[,] weighted = biome.GetWeightedValues(biomeResults.Values, _cachedMinMax.Value.Min, _cachedMinMax.Value.Max);
                    weightedBiomeValues.Add(weighted);
                }
            }

            lock (_weightLock) {
                for (int x = 0; x < resolution; x++) {
                    for (int y = 0; y < resolution; y++) {
                        float[] map = CalculateBiomeWeightsAt(x, y, connected, weightedBiomeValues);

                        for (int z = 0; z < map.Length; z++) {
                            biomeMap[x, y, z] = map[z];
                        }
                    }
                }
            }

			return biomeMap;
		}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="config">TerraConfig instance for pulling length & spread</param>
		/// <param name="position">Position of in Terra grid units of this map</param>
		/// <param name="resolution">Resolution of thsi biome map</param>
		public float[,,] GetBiomeMap(TerraConfig config, GridPosition position, int resolution) {
			return GetBiomeMap(position, config.Generator.Length, config.Generator.Spread, resolution);
		}

        /// <summary>
        /// Finds the lowest and highest <see cref="Constraint"/>s in connected 
        /// biome nodes.
        /// </summary>
        /// <returns></returns>
        public MinMaxResult GetMaskConstraintMinMax() {
            float min = 1;
            float max = 0;

            foreach (BiomeNode node in _combiner.GetConnectedBiomeNodes()) {
                Vector2[] constraints = {
                    node.HeightmapMinMaxMask,
                    node.MoistureMinMaxMask,
                    node.TemperatureMinMaxMask
                };

                foreach (Vector2 cons in constraints) {
                    if (cons.x < min) {
                        min = cons.x;
                    }
                    if (cons.y > max) {
                        max = cons.y;
                    }
                }
            }

            return new MinMaxResult(min, max);
        }

        /// <summary>
        /// Gets the min/max values for all connected biomes
        /// </summary>
        /// <returns></returns>
        private MinMaxResult GetBiomesMinMax() {
            MinMaxRecorder recorder = new MinMaxRecorder();
            int res = TerraConfig.Instance.Generator.RemapResolution;
            
            List<Generator> generators = new List<Generator>();
            foreach (BiomeNode b in _combiner.GetConnectedBiomeNodes()) {
                Generator g1 = b.HeightmapGenerator.GetGenerator();
                Generator g2 = b.TemperatureGenerator.GetGenerator();
                Generator g3 = b.MoistureGenerator.GetGenerator();

                if (g1 != null) {
                    generators.Add(g1);
                }
                if (g2 != null) {
                    generators.Add(g2);
                }
                if (g3 != null) {
                    generators.Add(g3);
                }
            }

            for (int x = 0; x < res; x++) {
                for (int y = 0; y < res; y++) {
                    foreach (Generator g in generators) {
                        recorder.Register(g.GetValue(x / (float)res, y / (float)res, 0));
                    }
                }
            }

            return recorder.GetMinMax();
        }

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="resolution">Resolution of map</param>
		private float[,,] GetBiomeMap(int resolution) {
			return GetBiomeMap(new GridPosition(0, 0), 1, resolution, resolution);
		}

		private MinMaxResult CalculateMinMax(int resolution) {
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

            lock (_minMaxLock) {
                foreach (BiomeNode biome in _combiner.GetConnectedBiomeNodes()) {
                    MinMaxResult minMax = biome.CalculateMinMax(resolution);

                    if (minMax.Min < min) {
                        min = minMax.Min;
                    }
                    if (minMax.Min > max) {
                        max = minMax.Max;
                    }
                }
            }

			return new MinMaxResult(min, max);
		}

		private float[] CalculateBiomeWeightsAt(int x, int y, BiomeNode[] connected, List<float[,]> individualWeights) {
			if (_combiner.Mix == BiomeCombinerNode.MixMethod.MAX || _combiner.Mix == BiomeCombinerNode.MixMethod.MIN) {
				//Order from min->max or max->min
				var selection = individualWeights
					.Select((biomeWeights, index) => new { biomeWeights, index });

				var ordered = _combiner.Mix == BiomeCombinerNode.MixMethod.MAX ?
					selection.OrderBy(obj => obj.biomeWeights[x, y])
					.ToArray() :
					selection.OrderByDescending(obj => obj.biomeWeights[x, y])
					.ToArray();

				//Assign weights
				float[] weights = new float[connected.Length];

				weights[ordered[0].index] = ordered[0].biomeWeights[x, y];
				if (ordered.Length > 1) {
					weights[ordered[1].index] = 1 - ordered[0].biomeWeights[x, y];
				}

				return weights;
			}

			if (_combiner.Mix == BiomeCombinerNode.MixMethod.ADD) {
				float[] weights = new float[connected.Length];
				float curMax = 1f;
				float oldMax = 1f;
				float sum = 0f;

				//Biomes at the end of "connected" array overlay previous biomes
				for (int i = 0; i < connected.Length; i++) {
					float val = individualWeights[i][x, y];
					float remapped = MathUtil.Map(val, 0f, oldMax, 0f, curMax);

					if (sum >= 1f) {
						//No remaining weight to assign, terminate
						break;
					}
					if (i == connected.Length - 1) {
						//All remaining weight goes to last idx 
						weights[i] = 1 - sum;
						break;
					}

					sum += remapped;
					weights[i] = remapped;
					oldMax = curMax;
					curMax = 1 - remapped;
				}

				return weights;
			}

			return new float[connected.Length];
		}
	}
}