using System.IO;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Terra.Terrain.Util;

namespace Terra.Terrain {
	[ExecuteInEditMode]
	public class TerrainPaint {
		[Serializable]
		public class SplatSetting {
			public Texture2D Diffuse;
			public Texture2D Normal;
			public Vector2 Tiling = new Vector2(1, 1);
			public Vector2 Offset;

			public float Smoothness;
			public float Metallic;
			public float Blend = 30f;

			public PlacementType PlacementType;

			public float AngleMin = 5f;
			public float AngleMax = 25f;

			public float MinRange;
			public float MaxRange;
			public bool IsMaxHeight;
			public bool IsMinHeight;
		}
		public enum PlacementType {
			ElevationRange,
			Angle
		}

		public int AlphaMapResolution = 128;

		private GameObject TerrainObject;
		private List<SplatSetting> SplatSettings;
		private TerraSettings Settings;

		//Terrain/Mesh
		private MeshSampler Sampler;
		private Material TerrainMaterial;
		private Mesh Mesh;
		
		/// <summary>
		/// Create a TerrainPaint object that paints the passed gameobject. For this to 
		/// work, the passed gameobject must have the following components:
		/// MeshFilter, MeshRenderer, and TerrainTile
		/// </summary>
		/// <param name="gameobject">Gameobject to paint</param>
		public TerrainPaint(GameObject gameobject) {
			TerrainObject = gameobject;
			Settings = TerraSettings.Instance;
			SplatSettings = Settings.SplatSettings;
			

			SetFirstPassShader();
			Mesh = TerrainObject.GetComponent<MeshFilter>().sharedMesh;

			int res = (int)Math.Sqrt(Mesh.vertexCount);
			Sampler = new MeshSampler(Mesh.normals, Mesh.vertices, res);
		}

		/// <summary>
		/// Creates splatmaps which are (by default) applied to terrain splat shaders.
		/// </summary>
		/// <param name="applySplats">When true, the generated splat is automatically applied to the terrain. 
		/// Otherwise, it can be applied by calling ApplySplatmapsToShader</param>
		/// <param name="debug">When true, the generated alpha maps are saved to the disk. 
		/// They're located at [Your project's root dir]/SplatImages/</param>
		/// <returns>Created alphamap textures</returns>
		public List<Texture2D> GenerateSplatmaps(bool applySplats = true) {
			//Ensure correct shader is set
			SetFirstPassShader(true);

			//Set amount of required maps
			List<Texture2D> maps = new List<Texture2D>();
			for (int i = 0; i < Mathf.CeilToInt(SplatSettings.Count / 4f); i++)
				maps.Add(new Texture2D(AlphaMapResolution, AlphaMapResolution));
			
			//Sample weights and fill in textures
			for (int x = 0; x < AlphaMapResolution; x++) {
				for (int y = 0; y < AlphaMapResolution; y++) {
					MeshSampler.MeshSample sample = Sampler.SampleAt(y / (float)AlphaMapResolution, x / (float)AlphaMapResolution);
					AddWeightsToTextures(CalculateWeights(sample), ref maps, y, x);
				}
			}

			if (TerraDebug.WRITE_SPLAT_TEXTURES) {
				TerrainTile tile = TerrainObject.GetComponent<TerrainTile>();

				string tileName = tile != null ?
					"Tile[" + tile.Position.x + "_" + tile.Position.y + "]" :
					"Tile[0_0]";
				string folderPath = Application.dataPath + "/SplatImages/";
				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				for (var i = 0; i < maps.Count; i++) {
					byte[] bytes = maps[i].EncodeToPNG();
					string name = "Splat" + i + "_" + tileName + ".png";
					File.WriteAllBytes(folderPath + name, bytes);
				}
			}

			//Apply set pixel values to textures
			maps.ForEach(t => t.Apply());
			if (applySplats) {
				ApplySplatmapsToShaders(maps);
			}

			return maps;
		}

		/// <summary>
		/// Calculates the weights that can be used to create a splatmap 
		/// based on the passed sample and splats.
		/// </summary>
		/// <param name="sample">Sample to base calculation on</param>
		/// <param name="splat">Splat setting to base calculation on</param>
		/// <returns>Weight values in the same order of the </returns>
		float[] CalculateWeights(MeshSampler.MeshSample sample) {
			float height = sample.Height;
			float angle = sample.Angle;
			float[] weights = new float[SplatSettings.Count];

			var orderMap = new Dictionary<PlacementType, int>() {
				{ PlacementType.ElevationRange, 0 },
				{ PlacementType.Angle, 1 }
			};
			List<SplatSetting> ordered = SplatSettings
			.OrderBy(s => orderMap[s.PlacementType]) //Order elevation before angle
			//.OrderBy(s => s.MinRange)                //Order lower ranges 
			//.OrderBy(s => s.IsMinHeight)             //Order min height first
			.ToList();

			for (int i = 0; i < SplatSettings.Count; i++) {
				SplatSetting splat = ordered[i];

				float min = splat.IsMinHeight ? float.MinValue : splat.MinRange;
				float max = splat.IsMaxHeight ? float.MaxValue : splat.MaxRange;

				switch (splat.PlacementType) {
					case PlacementType.Angle:
						if (angle > splat.AngleMin && angle < splat.AngleMax) {
							float factor = Mathf.Clamp01(((angle - splat.AngleMin) ) / splat.Blend);
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

		void AddWeightsToTextures(float[] weights, ref List<Texture2D> textures, int x, int y) {
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
		/// Applies the passed splat textures to the terrain. Because only 4 alphamaps can be 
		/// applied at one time, multiple splat textures must be passed at one time if the amount 
		/// of supplied textures is > 4. The GenerateSplatmaps function takes care of this for you and 
		/// its result can be passed as the splats parameter.
		/// </summary>
		/// <param name="splats">Splatmap textures to apply to the shaders and MeshRenderer</param>
		public void ApplySplatmapsToShaders(List<Texture2D> splats) {
			int len = SplatSettings.Count;
			MeshRenderer mr = TerrainObject.GetComponent<MeshRenderer>();
			Material toSet = TerrainMaterial;

			if (Settings.UseTessellation) {
				SetTessellation(toSet);
			}

			for (var i = 0; i < splats.Count; i++) {
				const int off = 4; //Offset for splat textures

				if (i != 0) { //Insert new Material/AddPass shader
					const string fpLoc = "Hidden/TerrainEngine/Splatmap/Standard-AddPass";
					Material mat = new Material(Shader.Find(fpLoc));
					toSet = mat;
					
					mr.sharedMaterials = mr.sharedMaterials.Concat(new Material[] { toSet }).ToArray();
				}

				toSet.SetTexture("_Control", splats[i]);
				toSet.SetTexture("_MainTex", splats[0]);
				toSet.SetColor("_Color", Color.black);
				
				if (i * off < len) SetMaterialForSplatIndex(0, SplatSettings[i * off], toSet);
				if (i * off + 1 < len) SetMaterialForSplatIndex(1, SplatSettings[i * off + 1], toSet);
				if (i * off + 2 < len) SetMaterialForSplatIndex(2, SplatSettings[i * off + 2], toSet);
				if (i * off + 3 < len) SetMaterialForSplatIndex(3, SplatSettings[i * off + 3], toSet);
			}
		}

		/// <summary>
		/// Sets the terrain splat texture at the passed index to the same 
		/// information provided in the passed material.
		/// </summary>
		/// <param name="index">Splat index to apply material to (0 - 3)</param>
		/// <param name="splat"></param>
		/// <param name="mat">Material to apply</param>
		void SetMaterialForSplatIndex(int index, SplatSetting splat, Material mat) {
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
		}

		/// <summary>
		/// Sets tessellation values from TerraSettings into 
		/// the passed Material's shader
		/// </summary>
		/// <param name="mat">Material to assign values to</param>
		void SetTessellation(Material mat) {
			const string tess = "_TessValue";
			const string tessMin = "_TessMin";
			const string tessMax = "_TessMax";

			mat.SetFloat(tess, Settings.TessellationAmount);
			mat.SetFloat(tessMin, Settings.TessellationMinDistance);
			mat.SetFloat(tessMax, Settings.TessellationMaxDistance);
		}

		/// <summary>
		/// Sets up the first pass shader, applies it to a material, 
		/// and applies it to the MeshRenderer.
		/// </summary>
		/// <param name="overwrite">When enabled, the correct first pass shader is 
		/// applied to a material regardless of whether or not one is already set.</param>
		void SetFirstPassShader(bool overwrite = false) {
			string path = Settings.UseTessellation ? "Terra/TerrainFirstPassTess" : "Terra/TerrainFirstPass";

			if (TerrainMaterial == null) {
				TerrainMaterial = TerrainObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find(path));
			} else if (overwrite) {
				TerrainMaterial.shader = Shader.Find(path);
			}
		}
	}
}