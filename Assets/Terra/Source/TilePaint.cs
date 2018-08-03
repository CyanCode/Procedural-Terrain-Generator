using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terra.Data;
using Terra.Terrain.Util;
using UnityEngine;
using UnityEngine.Profiling;

namespace Terra.Terrain {
	[Serializable]
	public class TilePaint: ISerializationCallbackReceiver {
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
		public SplatData[] Splats;

		public BiomeData[,] BiomeMap;

		[SerializeField]
		private readonly Tile _tile;

		public TilePaint(Tile tile) {
			_tile = tile;
		}

		/// <summary>
		/// TODO Summary
		/// </summary>
		public void Paint() {
			CalculateBiomeMap();
			ApplyDefaultMaterial();
		}

		/// <summary>
		/// Updates the textures that are fed into the splatmapping shader. 
		/// These includes <see cref="Controls"/> and <see cref="Splats"/>.
		/// </summary>
		public void GatherTextures() {
			SetSplatData();
			SetControlTextures();
		}

		/// <summary>
		/// Calculates <see cref="BiomeMap"/> if it is null.
		/// </summary>
		public void CalculateBiomeMap() {
			if (BiomeMap != null)
				return;

			BiomeMap = _tile.GetBiomeMap(_tile.LodLevel.SplatmapResolution);
		}

		/// <summary>
		/// Fills <see cref="Splats"/> with the splat data it needs to render 
		/// terrain. Assumes <see cref="CalculateBiomeMap"/> has been called 
		/// first.
		/// </summary>
		private void SetSplatData() {
			if (BiomeMap == null)
				return;

			//TODO measure speed 
			List<SplatData> data = new List<SplatData>();
			for (int x = 0; x < BiomeMap.Length; x++) {
				for (int z = 0; z < BiomeMap.Length; z++) {
					BiomeData biome = BiomeMap[x, z];

					foreach (SplatData sd in biome.Details.SplatsData) {
						if (!data.Exists(t => t.Equals(sd))) {
							data.Add(sd);
						}
					}
				}
			}

			Splats = data.ToArray();
		}

		/// <summary>
		/// Fills <see cref="Controls"/> with the textures it needs to render 
		/// terrain. Assumes <see cref="CalculateBiomeMap"/> has been called 
		/// first.
		/// </summary>
		private void SetControlTextures() {
			//Ensure correct shader is set
			SetFirstPassShader(true);

			//Set amount of required maps
			List<Texture2D> maps = new List<Texture2D>();
			int splatRes = _tile.LodLevel.SplatmapResolution;

			for (int i = 0; i < Mathf.CeilToInt(Splats.Length / 4f); i++)
				maps.Add(new Texture2D(splatRes, splatRes));

			//Sample weights and fill in textures
			int incrementer = _tile.MeshManager.HeightmapResolution / splatRes;
			for (int x = 0; x < splatRes; x += incrementer) {
				for (int z = 0; z < splatRes; z += incrementer) {
					float height = _tile.MeshManager.Heightmap[x, z];
					float angle = GetSteepness(x, z) * 90;
					
					AddWeightsToTextures(CalculateWeights(height, angle), ref maps, x, z); //consider switching x & z
				}
			}
			
			//Apply set pixel values to textures
			maps.ForEach(t => t.Apply());
		}

		float GetSteepness(int x, int y) {
			float[,] heightmap = _tile.MeshManager.Heightmap;
			float height = heightmap[x, y];

			//Ensure x & y fall within heightmap
			if (x >= heightmap.Length - 1)
				x = heightmap.Length - 2;
			if (y >= heightmap.Length - 1)
				y = heightmap.Length - 2;

			// Compute the differentials by stepping over 1 in both directions.
			float dx = heightmap[x + 1, y] - height;
			float dy = heightmap[x, y + 1] - height;

			// The "steepness" is the magnitude of the gradient vector
			return Mathf.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>
		/// Calculates the weights that can be used to create a splatmap 
		/// based on the passed sample and splats.
		/// </summary>
		/// <param name="height">Sampled height</param>
		/// <param name="angle">Sampled angle</param>
		/// <returns>Weight values in the same order of the </returns>
		float[] CalculateWeights(float height, float angle) {
			float[] weights = new float[Splats.Length];

			var orderMap = new Dictionary<PlacementType, int>() {
				{ PlacementType.ElevationRange, 0 },
				{ PlacementType.Angle, 1 }
			};
			List<SplatData> ordered = Splats
				.OrderBy(s => orderMap[s.PlacementType]) //Order elevation before angle
				//.OrderBy(s => s.MinRange)                //Order lower ranges 
				//.OrderBy(s => s.IsMinHeight)             //Order min height first
				.ToList();

			for (int i = 0; i < Splats.Length; i++) {
				SplatData splat = ordered[i];

				float min = splat.IsMinHeight ? float.MinValue : splat.MinHeight;
				float max = splat.IsMaxHeight ? float.MaxValue : splat.MaxHeight;

				switch (splat.PlacementType) {
					case PlacementType.Angle:
						if (angle > splat.AngleMin && angle < splat.AngleMax) {
							float factor = Mathf.Clamp01((angle - splat.AngleMin) / splat.Blend);
							weights[i] = factor;
						}

						break;
					case PlacementType.ElevationRange:
						if (height > min && height < max) {
							if (i > 0) { //Can blend up
								float factor = Mathf.Clamp01((splat.Blend - (height - min)) / splat.Blend);
								weights[i - 1] = factor;
								weights[i] = 1 - factor;
							} else {
								weights[i] = 1f;
							}
						}

						break;
				}
			}

			//Normalize weights
			float sum = weights.Sum();
			for (var i = 0; i < weights.Length; i++) {
				weights[i] /= sum;
			}

			return weights;
		}

		private void AddWeightsToTextures(float[] weights, ref List<Texture2D> textures, int x, int y) {
			int len = weights.Length;

			for (int i = 0; i < len; i += 4) {
				float r = weights[i];
				float g = i + 1 < len ? weights[i + 1] : 0f;
				float b = i + 2 < len ? weights[i + 2] : 0f;
				float a = i + 3 < len ? weights[i + 3] : 0f;

				textures[i / 4].SetPixel(x, y, new Color(r, g, b, a));
			}
		}

		/// <summary>
		/// Applies the Unity default material to the terrain if one is not 
		/// already applied.
		/// </summary>
		private void ApplyDefaultMaterial() {
			if (_tile.GetMeshRenderer() == null || _tile.GetMeshRenderer().sharedMaterial != null)
				return;

			Shader s = Shader.Find("Diffuse");
			if (s == null) {
				Debug.Log("Failed to find default shader when creating terrain");
				return;
			}

			Material material = new Material(s);
			_tile.GetMeshRenderer().sharedMaterial = material;
		}

		/// <summary>
		/// Sets up the first pass shader, applies it to a material, 
		/// and applies it to the MeshRenderer.
		/// </summary>
		/// <param name="overwrite">When enabled, the correct first pass shader is 
		/// applied to a material regardless of whether or not one is already set.</param>
		private void SetFirstPassShader(bool overwrite = false) {
			const string path = "Terra/TerrainFirstPass";
			var mr = _tile.GetMeshRenderer();

			if (mr.material == null || overwrite) {
				mr.material = new Material(Shader.Find(path));
			}
		}

		/// <summary>
		/// Writes debug textures to the file system if 
		/// <see cref="TerraSettings.TerraDebug.WRITE_SPLAT_TEXTURES"/> is 
		/// true.
		/// </summary>
		private void WriteDebugTextures() {
#pragma warning disable CS0162 // Unreachable code detected
			if (TerraSettings.TerraDebug.WRITE_SPLAT_TEXTURES) {
				string tileName = _tile != null ?
					"Tile[" + _tile.GridPosition.X + "_" + _tile.GridPosition.Z + "]" :
					"Tile[0_0]";
				string folderPath = Application.dataPath + "/SplatImages/";
				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				for (var i = 0; i < Controls.Length; i++) {
					byte[] bytes = Controls[i].EncodeToPNG();
					string name = "Splat" + i + "_" + tileName + ".png";
					File.WriteAllBytes(folderPath + name, bytes);
				}
			}
#pragma warning restore CS0162 // Unreachable code detected
		}

		#region Serialization

		[SerializeField, HideInInspector]
		private int[] _serializedBiomePoints;

		[SerializeField]
		private BiomeData[] _serializedBiomes;

		public void OnBeforeSerialize() {
			//Biome map
			if (BiomeMap != null) {
				_serializedBiomePoints = new int[BiomeMap.Length * BiomeMap.Length];
				List<BiomeData> biomeTypes = new List<BiomeData>();

				for (int x = 0; x < BiomeMap.Length; x++) {
					for (int z = 0; z < BiomeMap.Length; z++) {
						BiomeData b = BiomeMap[x, z];
						int existIdx = biomeTypes.FindIndex(p => p == b);

						if (existIdx == -1) { //Does not exist, add
							biomeTypes.Add(b);
							existIdx = biomeTypes.Count - 1;
						}

						_serializedBiomePoints[x + z * BiomeMap.Length] = existIdx;
					}
				}

				_serializedBiomes = biomeTypes.ToArray();
			}
		}

		public void OnAfterDeserialize() {
			if (_serializedBiomePoints != null && _serializedBiomes != null) {
				int length = (int)Math.Sqrt(_serializedBiomePoints.Length);
				BiomeMap = new BiomeData[length, length];

				for (int x = 0; x < BiomeMap.Length; x++) {
					for (int z = 0; z < BiomeMap.Length; z++) {
						int point = _serializedBiomePoints[x + z * length];
						BiomeMap[x, z] = _serializedBiomes[point];
					}
				}
			}
		}

		#endregion
	}
}
