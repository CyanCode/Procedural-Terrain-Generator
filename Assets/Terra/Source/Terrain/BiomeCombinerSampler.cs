using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Graph.Biome;
using Terra.Structures;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class BiomeCombinerSampler { //todo rename to biomecombinersampler
		[Serializable]
		public struct MinMaxResult {
			public float Min;
			public float Max;

			public MinMaxResult(float min, float max) {
				Min = min;
				Max = max;
			}
		}

		[SerializeField]
		private MinMaxResult? _cachedMinMax;
		[SerializeField]
		private BiomeCombinerNode _combiner;

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
		/// Calculates the min and max values for all connected 
		/// biomes within this BiomeCombinerNode.
		/// </summary>
		/// <param name="resolution">The resolution of the generator polling</param>
		/// <returns></returns>
		public MinMaxResult GetMinMax(int resolution) {
			float min = Single.PositiveInfinity;
			float max = Single.NegativeInfinity;

			foreach (BiomeNode b in _combiner.GetConnectedBiomeNodes()) {
				float[,,] vals = b.GetMapValues(GridPosition.Zero, resolution, resolution, 1);

				for (int x = 0; x < resolution; x++) {
					for (int y = 0; y < resolution; y++) {
						for (int z = 0; z < 3; z++) {
							float val = vals[x, y, z];

							if (val < min) {
								min = val;
							}
							if (val > max) {
								max = val;
							}
						}
					}
				}
			}

			return new MinMaxResult(min, max);
		}

		public MinMaxResult GetMinMax(float[] values) {
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			foreach (float val in values) {
				if (val < min) {
					min = val;
				}
				if (val > max) {
					max = val;
				}
			}

			return new MinMaxResult(min, max);
		}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="position">Position of this biome map in the grid of tiles</param>
		/// <param name="length">Length of a tile</param>
		/// <param name="spread">Divide x & z coordinates of the polled position by this number</param>
		/// <param name="resolution">Resolution of map</param>
		/// <param name="remapResolution">Resolution of the remap</param>
		/// <returns></returns>
		public float[,,] GetBiomeMap(GridPosition position, int length, float spread, int resolution, int remapResolution = 128) {
			BiomeNode[] connected = _combiner.GetConnectedBiomeNodes();

			float[,,] biomeMap = new float[resolution, resolution, connected.Length];
			List<float[,]> weightedBiomeValues = new List<float[,]>(biomeMap.Length);

			if (_cachedMinMax == null) {
				_cachedMinMax = GetMinMax(remapResolution);
			}

			//Gather each biome's values
			foreach (BiomeNode biome in connected) {
				float[,,] biomeVals = biome.GetMapValues(position, resolution, spread, length);
				float[,] weighted = biome.GetWeightedValues(biomeVals, _cachedMinMax.Value.Min, _cachedMinMax.Value.Max);
				weightedBiomeValues.Add(weighted);
			}

			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					float[] map = GetBiomeWeights(x, y, connected, weightedBiomeValues);

					for (int z = 0; z < map.Length; z++) {
						biomeMap[x, y, z] = map[z];
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
			return GetBiomeMap(position, config.Generator.Length, config.Generator.Spread, resolution, config.Generator.RemapResolution);
		}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="resolution">Resolution of map</param>
		private float[,,] GetBiomeMap(int resolution) {
			return GetBiomeMap(new GridPosition(0, 0), 1, resolution, resolution);
		}

		private float[] GetBiomeWeights(int x, int y, BiomeNode[] connected, List<float[,]> individualWeights) {
			if (_combiner.Mix == BiomeCombinerNode.MixMethod.MAX || _combiner.Mix == BiomeCombinerNode.MixMethod.MIN) {
				float limit = _combiner.Mix == BiomeCombinerNode.MixMethod.MAX ?
					float.NegativeInfinity : float.PositiveInfinity;
				int minMaxIdx = 0;

				float[] weights = new float[connected.Length];

				for (int z = 0; z < connected.Length; z++) { //Do not select min/max from all biomes, only currently iterating ones (z)
					float val = individualWeights[z][x, y];
					
					if (_combiner.Mix == BiomeCombinerNode.MixMethod.MAX && val > limit) {
						limit = val;
						minMaxIdx = z;

						if (z == 1) {
							Console.WriteLine("");
						}
					}
					if (_combiner.Mix == BiomeCombinerNode.MixMethod.MIN && val < limit) {
						limit = val;
						minMaxIdx = z;
					}
				}

				weights[minMaxIdx] = 1f;
				return weights;
			}

			if (_combiner.Mix == BiomeCombinerNode.MixMethod.ADD) {
				float[] weights = new float[connected.Length];

				//Order by weight ascending
				var ordered = individualWeights
					.Select((biomeWeights, index) => new { biomeWeights, index })
					.OrderBy(obj => obj.biomeWeights[x, y])
					.ToArray();

				weights[ordered[0].index] = ordered[0].biomeWeights[x, y];
				if (ordered.Length > 1) {
					weights[ordered[1].index] = 1 - ordered[0].biomeWeights[x, y];
				}

				return weights;
			}

			return new float[connected.Length];
		}

		private float[] Normalize(float[] values) {
			//Determine min/max
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			foreach (float val in values) {
				if  (val < min) {
					min = val;
				}
				if (val > max) {
					max = val;
				}
			}

			for (int i = 0; i < values.Length; i++) {
				values[i] = (values[i] - min) / (max - min);
			}

			return values;
		}
	}
}