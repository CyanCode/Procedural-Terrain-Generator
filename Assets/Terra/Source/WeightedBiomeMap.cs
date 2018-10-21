using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terra.Data;
using Terra.Terrain;
using UnityEngine;

namespace Assets.Terra.Source {
	/// <summary>
	/// Represents a 2D map of biomes and their weights for 
	/// a set <see cref="Tile"/>
	/// </summary>
	[Serializable]
	public class WeightedBiomeMap: ISerializationCallbackReceiver {
		private TerraSettings _settings { get { return TerraSettings.Instance; } }

		[SerializeField]
		private Tile _tile;
		[SerializeField]
		private int _resolution;
		[SerializeField]
		private int _heightmapResolution;
		[SerializeField]
		private float[,] _heightmap;

		/// <summary>
		/// A 2D map of biome/weight pairs where each index contains 
		/// a list of biomes and their respective weights. To populate 
		/// this structure, call <see cref="CreateMap"/>
		/// </summary>
		public KeyValuePair<BiomeData[], float[]>[,] WeightedMap { get; private set; }

		/// <summary>
		/// A set (no duplicates) of biomes that were found while 
		/// calling <see cref="CreateMap"/>
		/// </summary>
		public List<BiomeData> WeightedBiomeSet { get; private set; }

		/// <summary>
		/// Resolution of the biome map
		/// </summary>
		public int Resolution {
			get { return _resolution; }
			set { _resolution = value; }
		}

		/// <summary>
		/// Readies a weighted map for creation and pulls heightmap 
		/// information from the passed <see cref="Tile"/>
		/// </summary>
		/// <param name="tile">Tile to refernce heightmap from</param>
		/// <param name="resolution">Resolution of the map (must be lower 
		/// than or equal to heightmap resolution)</param>
		public WeightedBiomeMap(Tile tile, int resolution) {
			_tile = tile;
			_resolution = resolution;
			_heightmap = _tile.MeshManager.Heightmap;
			_heightmapResolution = _tile.MeshManager.HeightmapResolution;
		}

		/// <summary>
		/// Readies a weighted map for creation and pulls heightmap 
		/// information from the passed <see cref="TileMesh"/>
		/// </summary>
		/// <param name="tileMesh">TileMesh to reference heightmap from</param>
		/// <param name="resolution">Resolution of the map (must be lower 
		/// than or equal to heightmap resolution)</param>
		public WeightedBiomeMap(TileMesh tileMesh, int resolution) {
			_resolution = resolution;
			_tile = null;
			_heightmapResolution = tileMesh.HeightmapResolution;
			_heightmap = tileMesh.Heightmap;
		}

		public void CreateMap() {
			WeightedMap = new KeyValuePair<BiomeData[], float[]>[_resolution, _resolution];
			WeightedBiomeSet = new List<BiomeData>();

			for (int x = 0; x < _resolution; x++) {
				for (int z = 0; z < _resolution; z++) {
					List<BiomeData> biomes = new List<BiomeData>();
					List<float> weights = new List<float>();

					//Collect weight of every biome at this coordinate
					for (var i = 0; i < _settings.BiomesData.Count; i++) {
						BiomeData b = _settings.BiomesData[i];
						float weight = GetWeight(b, x, z);

						if (weight > 0.0001f) {
							biomes.Add(b);
							weights.Add(weight);

							if (i != 0) {
								weights[i - 1] = 1 - weight;
								weights[i] = weight;
							}
						}
					}

					//Normalize weights
					float sum = weights.Sum();
					for (var i = 0; i < weights.Count; i++) {
						weights[i] /= sum;
					}

					WeightedMap[x, z] = new KeyValuePair<BiomeData[], float[]>(biomes.ToArray(), weights.ToArray());

					//Add to biome set
					biomes.ForEach(b => { 
						if (!WeightedBiomeSet.Exists(existing => ReferenceEquals(existing, b))) {
							WeightedBiomeSet.Add(b);
						}
					});
				}
			}
		}

		/// <summary>
		/// Calculates the "weight", value between 0 and 1 which represents 
		/// how much a biome appears. The <see cref="Tile"/> that was used 
		/// when constructing must have a heightmap.
		/// </summary>
		/// <param name="biome">Biome to check</param>
		/// <param name="x">X location in heightmap</param>
		/// <param name="z">Z location in heightmap</param>
		public float GetWeight(BiomeData biome, int x, int z) {
			BiomeData b = biome;

			var tm = _settings.TemperatureMapData;
			var mm = _settings.MoistureMapData;

			if (b.IsTemperatureConstrained && !tm.HasGenerator()) return 0;
			if (b.IsMoistureConstrained && !mm.HasGenerator()) return 0;

			//Calculate x and z offsets for checking heightmap
			int offset = _resolution / _heightmapResolution;
			x *= offset;
			z *= offset;

			//Calculate height and world x/z positions
			var local = TileMesh.PositionToLocal(x, z, _resolution);
			var world = TileMesh.LocalToWorld(_tile == null ? new GridPosition() : _tile.GridPosition, local.x, local.y);
			var wx = world.x;
			var wz = world.y;

			//Establish and clamp sampled values between 1 & 0
			var height = _heightmap[x, z];
			var temp = tm.GetValue(wx, wz);
			var moisture = mm.GetValue(wx, wz);

			height = Mathf.Clamp01(height);
			temp = Mathf.Clamp01(temp);
			moisture = Mathf.Clamp01(moisture);

			//If no constraints return 1f
			if (!b.IsHeightConstrained && !b.IsTemperatureConstrained && !b.IsMoistureConstrained) {
				return 1f;
			}

			//Which map constraints fit the passed value
			bool passHeight = b.IsHeightConstrained && (b.HeightConstraint.Fits(height) || b.HeightConstraint.FitsMinMax(height));
			bool passTemp = b.IsTemperatureConstrained && (b.TemperatureConstraint.Fits(temp) || b.TemperatureConstraint.FitsMinMax(temp));
			bool passMoisture = b.IsMoistureConstrained && (b.MoistureConstraint.Fits(moisture) || b.MoistureConstraint.FitsMinMax(moisture));

			float blend = _settings.Generator.BiomeBlendAmount;

			//Calculate weight given to each map
			float weight = 0;
			int count = 0;
			if (passHeight) {
				weight += b.HeightConstraint.Weight(height, blend);
				count++;
			}
			if (passTemp) {
				weight += b.TemperatureConstraint.Weight(temp, blend);
				count++;
			}
			if (passMoisture) {
				weight += b.MoistureConstraint.Weight(moisture, blend);
				count++;
			}

			return weight / count;
		}

		public BiomeData[] BiomesAt(int x, int z) {
			return WeightedMap[x, z].Key;
		}

		public float[] WeightsAt(int x, int z) {
			return WeightedMap[x, z].Value;
		}

		public Texture2D GetPreviewTexture() {
			Texture2D tex = new Texture2D(_resolution, _resolution);

			for (int x = 0; x < _resolution; x++) {
				for (int z = 0; z < _resolution; z++) {
					KeyValuePair<BiomeData[], float[]> biomeWeights = WeightedMap[x, z];

					Color overall = new Color(0, 0, 0);
					for (int i = 0; i < biomeWeights.Key.Length; i++) {
						Color biomeColor = biomeWeights.Key[i].Color;
						float weight = biomeWeights.Value[i];

						if (i == 0) {
							overall = biomeColor;
						} else {
							overall = Color.Lerp(biomeWeights.Key[i - 1].Color, biomeColor, weight);
						}
					}

					tex.SetPixel(x, z, overall);
				}
			}

			tex.Apply();
			return tex;
		}

		public KeyValuePair<BiomeData[], float[]> this[int x, int z] {
			set { WeightedMap[x, z] = value; }
			get { return WeightedMap[x, z]; }
		}

		#region Serialization

		[Serializable]
		private struct BiomeIndexWeightPair {
			public int[] BiomeIndicies;
			public float[] Weights;

			public BiomeIndexWeightPair(int[] indicies, float[] weights) {
				BiomeIndicies = indicies;
				Weights = weights;
			}
		}

		[SerializeField, HideInInspector]
		private BiomeIndexWeightPair[] _serializedBiomeWeights;

		[SerializeField, HideInInspector]
		private BiomeData[] _serializedBiomes;

		public void OnBeforeSerialize() {
			//Biome map
			//TODO Write test for biome serialization
			if (WeightedMap == null || WeightedMap.Length <= 0) 
				return;
			
			_serializedBiomeWeights = new BiomeIndexWeightPair[WeightedMap.Length];

			for (int x = 0; x < _resolution; x++) {
				for (int z = 0; z < _resolution; z++) {
					KeyValuePair<BiomeData[], float[]> weightedBiomes = WeightedMap[x, z];
					int[] indices = new int[weightedBiomes.Key.Length];

					//Serialize each biome by referencing some index within WeightedBiomeSet
					for (int i = 0; i < indices.Length; i++) {
						int existIdx = WeightedBiomeSet.FindIndex(p => ReferenceEquals(p, weightedBiomes.Key[i]));

						if (existIdx == -1) { //Does not exist, add
							WeightedBiomeSet.Add(weightedBiomes.Key[i]);
							existIdx = WeightedBiomeSet.Count - 1;
						}

						indices[i] = existIdx;
					}

					var pair = new BiomeIndexWeightPair(indices, weightedBiomes.Value);
					_serializedBiomeWeights[x + z * _resolution] = pair;
				}
			}

			_serializedBiomes = WeightedBiomeSet.ToArray();
		}

		public void OnAfterDeserialize() {
			if (_serializedBiomeWeights != null && _serializedBiomes != null) {
				WeightedBiomeSet = _serializedBiomes.ToList();
				WeightedMap = new KeyValuePair<BiomeData[], float[]>[_resolution, _resolution];

				for (int x = 0; x < _resolution; x++) {
					for (int z = 0; z < _resolution; z++) {
						BiomeIndexWeightPair biomeWeightPair = _serializedBiomeWeights[x + z * _resolution];
						BiomeData[] reconstructedBiomes = new BiomeData[biomeWeightPair.BiomeIndicies.Length];

						for (int i = 0; i < biomeWeightPair.BiomeIndicies.Length; i++) {
							int index = biomeWeightPair.BiomeIndicies[i];
							reconstructedBiomes[i] = _serializedBiomes[index];
						}

						var pair = new KeyValuePair<BiomeData[], float[]>(reconstructedBiomes, biomeWeightPair.Weights);
						WeightedMap[x, z] = pair;
					}
				}
			}
		}

		#endregion
	}
}
