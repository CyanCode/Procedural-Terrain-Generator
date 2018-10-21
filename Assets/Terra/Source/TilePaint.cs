using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terra.Data;
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
		public SplatData[] Splats;

		public readonly WeightedBiomeMap BiomeMap;

		[SerializeField]
		private readonly Tile _tile;

		public TilePaint(Tile tile) {
			_tile = tile;
			BiomeMap = new WeightedBiomeMap(tile, _tile.LodLevel.SplatmapResolution);
		}

		/// <summary>
		/// TODO Summary
		/// </summary>
		public void Paint() {
			BiomeMap.CreateMap();

			if (BiomeMap == null) {
				Debug.LogWarning("CalculateBiomeMap() failed to produce a non-null BiomeMap");
				ApplyDefaultMaterial();
				return;
			}

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (TerraSettings.TerraDebug.SHOW_BIOME_DEBUG_TEXTURE) {
				ApplyDebugBiomeMap();
				return;
			}

			GatherTextures();
			ApplyControlMapsToShaders();
		}

		/// <summary>
		/// Updates the textures that are fed into the splatmapping shader. 
		/// These includes <see cref="Controls"/> and <see cref="Splats"/>.
		/// </summary>
		public void GatherTextures() {
			SetSplats();
			SetControlTextures();
		}

		/// <summary>
		/// Applies the passed splat textures to the terrain. Because only 4 alphamaps can be 
		/// applied at one time, multiple splat textures must be passed at one time if the amount 
		/// of supplied textures is > 4. The GenerateSplatmaps function takes care of this for you and 
		/// its result can be passed as the splats parameter.
		/// </summary>
		public void ApplyControlMapsToShaders() {
			SetFirstPassShader(true);
			
			int len = Splats.Length;
			MeshRenderer mr = _tile.GetMeshRenderer();
			Material toSet = mr.sharedMaterial;

			for (var i = 0; i < Controls.Length; i++) {
				const int off = 4; //Offset for splat textures

				if (i != 0) { //Insert new Material/AddPass shader
					const string fpLoc = "Hidden/TerrainEngine/Splatmap/Standard-AddPass";
					Material mat = new Material(Shader.Find(fpLoc));
					toSet = mat;

					mr.sharedMaterials = mr.sharedMaterials.Concat(new[] { toSet }).ToArray();
				}

				toSet.SetTexture("_Control", Controls[i]);
				toSet.SetTexture("_MainTex", Controls[0]);
				toSet.SetColor("_Color", Color.black);

				if (i * off < len) SetMaterialForSplatIndex(0, Splats[i * off], toSet);
				if (i * off + 1 < len) SetMaterialForSplatIndex(1, Splats[i * off + 1], toSet);
				if (i * off + 2 < len) SetMaterialForSplatIndex(2, Splats[i * off + 2], toSet);
				if (i * off + 3 < len) SetMaterialForSplatIndex(3, Splats[i * off + 3], toSet);
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
		/// Calculates the weights of each splat texture within this 
		/// <see cref="BiomeMap"/>. 
		/// </summary>
		/// <param name="biomes">List of biomes at some point on the Tile</param>
		/// <param name="biomeWeights">List of weights associated with the passed biomes</param>
		/// <param name="height">Height at some point on the Tile</param>
		/// <param name="angle">Angle at some point on the Tile</param>
		/// <returns>Weight of each texture in the order they appear in <see cref="BiomeMap"/></returns>
		float[] GetTextureWeights(BiomeData[] biomes, float[] biomeWeights, float height, float angle) {
			int totalSplatCount = Splats.Length;
			float[] weights = new float[totalSplatCount];
			
			height = Mathf.Clamp01(height);

			for (int bi = 0; bi < biomes.Length; bi++) {
				BiomeData b = biomes[bi];
				float biomeWeight = biomeWeights[bi];

				//Length of SplatData in previous biomes
				int prevBiomeSplatCount = 0;
				for (int i = 0; i < bi; i++) {
					prevBiomeSplatCount += biomes[i].Details.SplatsData.Count;
				}
				
				//Calculate individual "splat weight"
				for (int si = 0; si < b.Details.SplatsData.Count; si++) {
					int weightIndex = prevBiomeSplatCount + si;
					SplatData splat = b.Details.SplatsData[si];

					//Check whether this SplatData fits required height and angle constraints
					bool passHeight = splat.ConstrainHeight && splat.HeightConstraint.Fits(height) || !splat.ConstrainHeight;
					bool passAngle = splat.ConstrainAngle && splat.AngleConstraint.Fits(angle) || !splat.ConstrainAngle;

					if (!passHeight && !passAngle) {
						continue;
					}

					//If it passes height or angle constraints what are the texturing weights
					float weight = 0;
					int count = 0;
					if (passHeight) {
						weight += splat.HeightConstraint.Weight(height, splat.Blend);
						count++;
					}
					if (passAngle) {
						weight += splat.AngleConstraint.Weight(angle, splat.Blend);
						count++;
					}

					weight /= count;
					weight *= biomeWeight;

					//Blend the bottom with the top
					if (weightIndex > 0) {
						weights[weightIndex - 1] = 1 - weight;
						weights[weightIndex] = weight;
					} else {
						weights[weightIndex] = 1f;
					}

					
				}
			}

			//Normalize weights
			float sum = weights.Sum();
			for (var i = 0; i < weights.Length; i++) {
				weights[i] /= sum;
			}

			return weights;
		}

		/// <summary>
		/// TODO Summary
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		float GetSteepness(int x, int y) {
			int res = _tile.MeshManager.HeightmapResolution;
			float[,] heightmap = _tile.MeshManager.Heightmap;
			float height = heightmap[x, y];

			//Ensure x & y fall within heightmap
			if (x >= res - 1)
				x = res - 2;
			if (y >= res - 1)
				y = res - 2;

			// Compute the differentials by stepping over 1 in both directions.
			float dx = heightmap[x + 1, y] - height;
			float dy = heightmap[x, y + 1] - height;

			// The "steepness" is the magnitude of the gradient vector
			return 0.5f * Mathf.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>
		/// Fills <see cref="Splats"/> with the splat data it needs to render 
		/// terrain. Assumes <see cref="BiomeMap"/> has been calculated
		/// first.
		/// </summary>
		private void SetSplats() {
			if (BiomeMap == null)
				return;

			Splats = BiomeMap.WeightedBiomeSet.SelectMany(b => b.Details.SplatsData).ToArray();
		}

		/// <summary>
		/// Fills <see cref="Controls"/> with the textures it needs to render 
		/// terrain.
		/// </summary>
		private void SetControlTextures() {
			//Ensure correct shader is set
			SetFirstPassShader(true);
			
			//Non-duplicate list of biomes in this map
			List<Texture2D> maps = new List<Texture2D>();
			int splatRes = _tile.LodLevel.SplatmapResolution;
			for (int i = 0; i < Mathf.CeilToInt(Splats.Length / 4f); i++) {
				maps.Add(new Texture2D(splatRes, splatRes));
			}

			//Sample weights and fill in textures
			int incrementer = _tile.MeshManager.HeightmapResolution / splatRes;
			for (int x = 0; x < splatRes; x += incrementer) {
				for (int z = 0; z < splatRes; z += incrementer) {
					BiomeData[] biomesAt = BiomeMap.BiomesAt(x, z);
					float[] weightsAt = BiomeMap.WeightsAt(x, z);
					float height = _tile.MeshManager.Heightmap[x, z];
					float angle = GetSteepness(x, z) * 90;

					float[] weights = GetTextureWeights(biomesAt, weightsAt, height, angle);
					AddWeightsToTextures(weights, ref maps, x, z); //consider switching x & z
				}
			}

			//Apply set pixel values to textures
			maps.ForEach(t => t.Apply());
			Controls = maps.ToArray();

			WriteDebugTextures();
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
		/// Sets the terrain splat texture at the passed index to the same 
		/// information provided in the passed material.
		/// </summary>
		/// <param name="index">Splat index to apply material to (0 - 3)</param>
		/// <param name="splat"></param>
		/// <param name="mat">Material to apply</param>
		void SetMaterialForSplatIndex(int index, SplatData splat, Material mat) {
			//Main Texture
			mat.SetTexture("_Splat" + index, splat.Diffuse);
			mat.SetTextureScale("_Splat" + index, splat.Tiling);
			mat.SetTextureOffset("_Splat" + index, splat.Offset);

			//Normal Texture
			mat.SetTexture("_Normal" + index, splat.Normal);
			mat.SetTextureScale("_Normal" + index, splat.Tiling);
			mat.SetTextureOffset("_Normal" + index, splat.Offset);

			//Smoothness
			mat.SetFloat("_Smoothness" + index, splat.Smoothness);

			//Distance blending
			var sd = TerraSettings.Instance.ShaderData;
			mat.SetFloat("_TransitionDistance", sd.TransitionStart);
			mat.SetFloat("_TransitionFalloff", sd.TransitionFalloff);
			mat.SetFloat("_TilingMultiplier", sd.FarScaleMultiplier);
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

			if (mr.sharedMaterial == null || overwrite) {
				mr.sharedMaterial = new Material(Shader.Find(path));
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

				int max = TerraSettings.TerraDebug.MAX_TEXTURE_WRITE_COUNT;
				int length = Controls.Length > max ? max : Controls.Length;
				for (var i = 0; i < length; i++) {
					byte[] bytes = Controls[i].EncodeToPNG();
					string name = "Splat" + i + "_" + tileName + ".png";
					File.WriteAllBytes(folderPath + name, bytes);
				}
			}
#pragma warning restore CS0162 // Unreachable code detected
		}

		/// <summary>
		/// Applies a texture representing the weighted biome map onto 
		/// the <see cref="Tile"/>. Assumes <see cref="BiomeMap"/> has 
		/// already been created.
		/// </summary>
		private void ApplyDebugBiomeMap() {
			Texture2D preview = BiomeMap.GetPreviewTexture();
			Material m = new Material(Shader.Find("Standard"));
			m.SetTexture("_MainTex", preview);

			_tile.GetMeshRenderer().material = m;
		}
	}
}
