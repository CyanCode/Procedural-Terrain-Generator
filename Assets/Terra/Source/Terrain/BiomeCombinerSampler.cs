using System;
using System.Collections.Generic;
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
			Texture2D tex = new Texture2D(size, size);
			BiomeNode[,] map = GetBiomeMap(size);

			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					BiomeNode b = map[x, y];

					if (b == null) {
						tex.SetPixel(x, y, Color.black);
						continue;
					}

					tex.SetPixel(x, y, b.PreviewColor);
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
		public MinMaxResult CalculateMinMax(int resolution) {
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

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="position">Position of this biome map in the grid of tiles</param>
		/// <param name="length">Length of a tile</param>
		/// <param name="spread">Divide x & z coordinates of the polled position by this number</param>
		/// <param name="resolution">Resolution of map</param>
		/// <param name="remapResolution">Resolution of the remap</param>
		/// <returns></returns>
		public BiomeNode[,] GetBiomeMap(GridPosition position, int length, float spread, int resolution, int remapResolution = 128) {
			BiomeNode[] connected = _combiner.GetConnectedBiomeNodes();

			BiomeNode[,] nodes = new BiomeNode[resolution, resolution];
			List<float[,]> biomeValues = new List<float[,]>(nodes.Length);

			if (_cachedMinMax == null) {
				_cachedMinMax = CalculateMinMax(remapResolution);
			}

			//Gather each biome's values
			foreach (BiomeNode biome in connected) {
				float[,,] biomeVals = biome.GetMapValues(position, resolution, spread, length);
				float[,] weighted = biome.GetWeightedValues(biomeVals, _cachedMinMax.Value.Min, _cachedMinMax.Value.Max);
				biomeValues.Add(weighted);
			}

			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					float limit = _combiner.Mix == BiomeCombinerNode.MixMethod.MAX ? 
						float.NegativeInfinity : float.PositiveInfinity;
					BiomeNode maxMinBiome = null;

					for (int z = 0; z < connected.Length; z++) {
						float val = biomeValues[z][x, y];

						if (_combiner.Mix == BiomeCombinerNode.MixMethod.MAX && val > limit) {
							limit = val;
							maxMinBiome = connected[z];
						}
						if (_combiner.Mix == BiomeCombinerNode.MixMethod.MIN && val < limit) {
							limit = val;
							maxMinBiome = connected[z];
						}
					}

					nodes[x, y] = maxMinBiome;
				}
			}

			return nodes;
		}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="config">TerraConfig instance for pulling length & spread</param>
		/// <param name="position">Position of in Terra grid units of this map</param>
		/// <param name="resolution">Resolution of thsi biome map</param>
		public BiomeNode[,] GetBiomeMap(TerraConfig config, GridPosition position, int resolution) {
			return GetBiomeMap(position, config.Generator.Length, config.Generator.Spread, resolution, config.Generator.RemapResolution);
		}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="resolution">Resolution of map</param>
		private BiomeNode[,] GetBiomeMap(int resolution) {
			return GetBiomeMap(new GridPosition(0, 0), 1, resolution, resolution);
		}
	}
}