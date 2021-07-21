using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terra.CoherentNoise;
using Terra.Graph;
using Terra.Graph.Biome;
using Terra.Source;
using Terra.Source.Util;
using Terra.Structures;
using Terra.Util;
using UnityEngine;
using Object = System.Object;

namespace Terra.Terrain {
	[Serializable]
	public class BiomeSampler {
		[SerializeField]
		private MinMaxResult? _cachedMinMax;
		[SerializeField]
		private BiomeNode[] _biomes;

        private static object _minMaxLock = new object();
        private static object _weightLock = new object();
        private static object _valueLock = new object();

		public BiomeSampler(BiomeNode[] biomes) {
			_biomes = biomes;
		}

		public Texture2D GetPreviewTexture(int size, float spread) {
			Texture2D tex = new Texture2D(size, size);
			int[,] map = GetBiomeMap(GridPosition.Zero, 1, spread, size);

			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					tex.SetPixel(x, y, _biomes[map[x, y]].PreviewColor);
				}
			}

			tex.Apply();
			return tex;
		}

		/// <summary>
		/// Samples the passed biomemap at the passed normalized 
		/// x & y coordinates.
		/// </summary>
		/// <param name="map">biome map</param>
		/// <param name="x">normalized x coordinate</param>
		/// <param name="y">normalized y coordinate</param>
		/// <returns></returns>
		public int GetBiomeAtInterpolatedCoords(int[,] map, float x, float y) {
            int res = map.GetLength(0);
            int sx = Mathf.Clamp(Mathf.RoundToInt(x * res), 0, res - 1);
            int sy = Mathf.Clamp(Mathf.RoundToInt(y * res), 0, res - 1);

            return map[sx, sy];
        }

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="position">Position of this biome map in the grid of tiles</param>
		/// <param name="length">Length of a tile</param>
		/// <param name="spread">Divide x & z coordinates of the polled position by this number</param>
		/// <param name="resolution">Resolution of map</param>
		/// <returns>
        /// Index of a biome enabled at each x, y coordinate. The index corresponds 
        /// to the order of biomes defined in the node graph. e.g. an index of 0
        /// represents the first biome.
        /// </returns>
		public int[,] GetBiomeMap(GridPosition position, int length, float spread, int resolution) {
			List<float[,]> allBiomeWeights = new List<float[,]>(_biomes.Length);

            //Gather each biome's weights
            lock (_valueLock) {
                for (var i = 0; i < _biomes.Length; i++) {
                    BiomeNode biome = _biomes[i];
                    allBiomeWeights.Add(biome.GetWeights(position, resolution, spread, length));
                }
            }

			// Choose biome to display
			int[,] biomeMap = new int[resolution, resolution];
			BlendStrategy blendStrategy = TerraConfig.Instance.Graph.GetEndNode().BlendStrategy;
			MathUtil.LoopXY(resolution, (x, y) => {
				int toShow = 0;

				switch (blendStrategy) {
					case BlendStrategy.RANDOM:
						toShow = GetShownBiomeRandom(x, y, allBiomeWeights);
						break;
					case BlendStrategy.ORDERED:
						toShow = GetShownBiomeOrdered(x, y, allBiomeWeights);
						break;
				}

				biomeMap[x, y] = toShow;
			});

			return biomeMap;
		}

		private int GetShownBiomeOrdered(int x, int y, List<float[,]> allBiomeWeights) {
			float prevWeight = -1f;
			int selectedBiome = 0;

			
			for (int i = 0; i < allBiomeWeights.Count; i++) {
				float weight = allBiomeWeights[i][x, y];
				if (weight >= prevWeight) {
					prevWeight = weight;
					selectedBiome = i;
                }
			}

			return selectedBiome;
		}

		private int GetShownBiomeRandom(int x, int y, List<float[,]> allBiomeWeights) {
			float weightsSum = 0f;
			for (int i = 0; i < allBiomeWeights.Count; i++) {
				weightsSum += allBiomeWeights[i][x, y];
			}

			float last = 0f;
			float chosenBiome = UnityEngine.Random.Range(0, weightsSum);
			for (int i = 0; i < allBiomeWeights.Count; i++) {
				float weight = allBiomeWeights[i][x, y];

				if (last < chosenBiome && chosenBiome <= weight + last) {
					return i;
				}

				last += weight;
			}

			return 0;
		}

		private List<float[,]> SortToMatchBiomeOrder(List<float[,]> allBiomes) {
			int[] order = TerraConfig.Instance.Graph.GetEndNode().BiomeOrder;
			if (order.Length != allBiomes.Count) return allBiomes;
			
			List<float[,]> sorted = new List<float[,]>(allBiomes.Count);
			sorted.AddRange(order.Select(i => allBiomes[i]));

			return sorted;
		}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="config">TerraConfig instance for pulling length & spread</param>
		/// <param name="position">Position of in Terra grid units of this map</param>
		/// <param name="resolution">Resolution of this biome map</param>
		public int[,] GetBiomeMap(TerraConfig config, GridPosition position, int resolution) {
			return GetBiomeMap(position, config.Generator.Length, config.Generator.Spread, resolution);
		}

		/// <summary>
		/// Generates a map of constructed from connected biome nodes. Instead of
		/// returning a biome map of size [resolution, resolution], it returns one
		/// of size [resolution + kernel size, resolution + kernel size] where
		/// kernel size is defined in <code>BlurUtils.GetGaussianKernelSize()</code>
		/// </summary>
		/// <param name="config"></param>
		/// <param name="position"></param>
		/// <param name="resolution"></param>
		/// <param name="deviation"></param>
		/// <returns></returns>
		public int[,] GetGaussianBlurrableBiomeMap(TerraConfig config, GridPosition position, int resolution, int deviation) {
			int[,] biomeMap = GetBiomeMap(config, position, resolution);
			int kernelSize = BlurUtils.GetGaussianKernelSize(deviation);
			int newRes = resolution + kernelSize;
			int[,] blurrableBiomeMap = new int[newRes,newRes];
			
			// Fill in area surrounding the center biome map
		}

        /// <summary>
        /// Finds the lowest and highest <see cref="Constraint"/>s in connected 
        /// biome nodes.
        /// </summary>
        /// <returns></returns>
        public MinMaxResult GetMaskConstraintMinMax() {
            float min = 1;
            float max = 0;

            foreach (BiomeNode node in _biomes) {
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
        //private MinMaxResult GetBiomesMinMax() {
        //    MinMaxRecorder recorder = new MinMaxRecorder();
        //    int res = TerraConfig.Instance.Generator.RemapResolution;
            
        //    List<Generator> generators = new List<Generator>();
        //    foreach (BiomeNode b in _combiner.GetConnectedBiomeNodes()) {
        //        Generator g1 = b.HeightmapGenerator.GetGenerator();
        //        Generator g2 = b.TemperatureGenerator.GetGenerator();
        //        Generator g3 = b.MoistureGenerator.GetGenerator();

        //        if (g1 != null) {
        //            generators.Add(g1);
        //        }
        //        if (g2 != null) {
        //            generators.Add(g2);
        //        }
        //        if (g3 != null) {
        //            generators.Add(g3);
        //        }
        //    }

        //    for (int x = 0; x < res; x++) {
        //        for (int y = 0; y < res; y++) {
        //            foreach (Generator g in generators) {
        //                recorder.Register(g.GetValue(x / (float)res, y / (float)res, 0));
        //            }
        //        }
        //    }

        //    return recorder.GetMinMax();
        //}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="resolution">Resolution of map</param>
		private int[,] GetBiomeMap(int resolution) {
			return GetBiomeMap(new GridPosition(0, 0), 1, resolution, resolution);
		}

		private MinMaxResult CalculateMinMax(int resolution) {
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

            lock (_minMaxLock) {
                foreach (BiomeNode biome in _biomes) {
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

		//private float[] CalculateBiomeWeightsAt(int x, int y, BiomeNode[] connected, List<float[,]> individualWeights) {
		//	if (_combiner.Mix == BiomeCombinerNode.MixMethod.MAX || _combiner.Mix == BiomeCombinerNode.MixMethod.MIN) {
		//		//Order from min->max or max->min
		//		var selection = individualWeights
		//			.Select((biomeWeights, index) => new { biomeWeights, index });

		//		var ordered = _combiner.Mix == BiomeCombinerNode.MixMethod.MAX ?
		//			selection.OrderBy(obj => obj.biomeWeights[x, y])
		//			.ToArray() :
		//			selection.OrderByDescending(obj => obj.biomeWeights[x, y])
		//			.ToArray();

		//		//Assign weights
		//		float[] weights = new float[connected.Length];

		//		weights[ordered[0].index] = ordered[0].biomeWeights[x, y];
		//		if (ordered.Length > 1) {
		//			weights[ordered[1].index] = 1 - ordered[0].biomeWeights[x, y];
		//		}

		//		return weights;
		//	}

		//	if (_combiner.Mix == BiomeCombinerNode.MixMethod.ADD) {
		//		float[] weights = new float[connected.Length];
		//		float curMax = 1f;
		//		float oldMax = 1f;
		//		float sum = 0f;

		//		//Biomes at the end of "connected" array overlay previous biomes
		//		for (int i = 0; i < connected.Length; i++) {
		//			float val = individualWeights[i][x, y];
		//			float remapped = MathUtil.Map(val, 0f, oldMax, 0f, curMax);

		//			if (sum >= 1f) {
		//				//No remaining weight to assign, terminate
		//				break;
		//			}
		//			if (i == connected.Length - 1) {
		//				//All remaining weight goes to last idx 
		//				weights[i] = 1 - sum;
		//				break;
		//			}

		//			sum += remapped;
		//			weights[i] = remapped;
		//			oldMax = curMax;
		//			curMax = 1 - remapped;
		//		}

		//		return weights;
		//	}

		//	return new float[connected.Length];
		//}
	}
}