using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Graph.Biome;
using Terra.Structures;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class BiomeCombinerSampler {
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
			for (var i = 0; i < connected.Length; i++) {
				BiomeNode biome = connected[i];
				BiomeNode.BiomeMapResult biomeResults = biome.GetMapsValues(position, resolution, spread, length);
				if (_cachedMinMax == null) {
					_cachedMinMax = CalculateMinMax(TerraConfig.Instance.Generator.RemapResolution);
				}

				float[,] weighted = biome.GetWeightedValues(biomeResults.Values, _cachedMinMax.Value.Min, _cachedMinMax.Value.Max);
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
			return GetBiomeMap(position, config.Generator.Length, config.Generator.Spread, resolution);
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

			foreach (BiomeNode biome in _combiner.GetConnectedBiomeNodes()) {
				MinMaxResult minMax = biome.CalculateMinMax(resolution);

				if (minMax.Min < min) {
					min = minMax.Min;
				}
				if (minMax.Min > max) {
					max = minMax.Max;
				}
			}

			return new MinMaxResult(min, max);
		}

		private float[] GetBiomeWeights(int x, int y, BiomeNode[] connected, List<float[,]> individualWeights) {
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