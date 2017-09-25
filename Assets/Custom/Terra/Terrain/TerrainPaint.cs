using System.IO;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using System.Collections.Generic;

namespace Terra.Terrain {
	[ExecuteInEditMode]
	public class TerrainPaint {
		public class MeshSample {
			readonly public float Height;
			readonly public float Angle;

			public MeshSample(float height = 0f, float angle = 0f) {
				Height = height;
				Angle = angle;
			}
		}
		[Serializable]
		public class SplatSetting {
			public Texture2D Diffuse;
			public Texture2D Normal;
			public Vector2 Tiling = new Vector2(1, 1);
			public Vector2 Offset;

			public float Smoothness;
			public float Metallic;
			public float Blend = 5f;

			public PlacementType PlacementType;

			public int Angle;

			public float MinRange;
			public float MaxRange;

			public float Precision = 0.9f;
		}
		public enum PlacementType {
			ElevationRange,
			Angle,
		}

		public int AlphaMapResolution = 128; //128

		GameObject TerrainObject;
		List<SplatSetting> SplatSettings;

		Material TerrainMaterial;
		Mesh Mesh;
		Vector3[] Vertices;
		Vector3[] Normals;
		int MeshResolution;
		

		/// <summary>
		/// Create a TerrainPaint object that paints the passed gameobject
		/// </summary>
		/// <param name="gameobject">Gameobject to paint</param>
		public TerrainPaint(GameObject gameobject, List<SplatSetting> splatSettings) {
			TerrainObject = gameobject;
			SplatSettings = splatSettings;

			const string path = "Nature/Terrain/Standard";
			TerrainMaterial = TerrainObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find(path));

			Mesh = TerrainObject.GetComponent<MeshFilter>().sharedMesh;
			Vertices = Mesh.vertices;
			Normals = Mesh.normals;
			MeshResolution = (int)Math.Sqrt(Mesh.vertexCount);
		}

		/// <summary>
		/// Displays options for materials tab. Can only 
		/// be called from the OnGUI method
		/// </summary>
		/// <param name="settings">Settings instance for modifying values</param>
		public static void DisplayGUI(TerraSettings settings) {
			EditorGUILayout.Space();

			if (settings.SplatSettings != null) {
				for (int i = 0; i < settings.SplatSettings.Count; i++) {
					SplatSetting splat = settings.SplatSettings[i];

					//Surround each material w/ box
					GUIStyle boxStyle = new GUIStyle();
					boxStyle.padding = new RectOffset(3, 3, 3, 3);
					boxStyle.normal.background = GetWhiteTexture();
					EditorGUILayout.BeginVertical(boxStyle);
					
					//Close button / name
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("X", GUILayout.Height(16), GUILayout.Width(18))) {
						settings.SplatSettings.RemoveAt(i);
						i--;
						continue;
					}
					EditorGUILayout.LabelField((splat.PlacementType == PlacementType.Angle ? "Angled" : "Elevation") + " Material " + (i + 1));
					EditorGUILayout.EndHorizontal();

					//Material settings
					EditorGUILayout.Space();

					if (splat.Diffuse == null) {
						EditorGUILayout.HelpBox("This splat material does not have a selected diffuse texture.", MessageType.Warning);
					} else {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.BeginVertical();

						splat.Diffuse = (Texture2D)EditorGUILayout.ObjectField(splat.Diffuse,
							typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
						EditorGUILayout.LabelField("Diffuse", GUILayout.Width(60));
						EditorGUILayout.EndVertical();

						if (splat.Normal != null) {
							EditorGUILayout.BeginVertical();
							splat.Normal = (Texture2D)EditorGUILayout.ObjectField(splat.Normal,
								typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
							EditorGUILayout.LabelField("Normal", GUILayout.Width(60));
							EditorGUILayout.EndVertical();
						}

						EditorGUILayout.EndHorizontal();
					} if (GUILayout.Button("Edit Material")) {
						AddTextureWindow window = new AddTextureWindow(ref splat);
					}

					EditorGUILayout.Space();

					//GUI for different types
					splat.PlacementType = (PlacementType)EditorGUILayout.EnumPopup("Placement Type", splat.PlacementType);
					switch (splat.PlacementType) {
						case PlacementType.Angle:
							splat.Angle = EditorGUILayout.IntSlider("Angle", splat.Angle, 0, 90);
							break;
						case PlacementType.ElevationRange:
							splat.MaxRange = EditorGUILayout.FloatField("Max Height", splat.MaxRange);
							splat.MinRange = EditorGUILayout.FloatField("Min Height", splat.MinRange);

							if (splat.MinRange > splat.MaxRange) splat.MinRange = splat.MaxRange;
							break;
					}

					EditorGUILayout.EndVertical();
					EditorGUILayout.Separator();
				}
			}

			if (GUILayout.Button("Add Material")) {
				if (settings.SplatSettings == null)
					settings.SplatSettings = new List<SplatSetting>();

				settings.SplatSettings.Add(new SplatSetting());
			}
		}

		public List<Texture2D> CreateAlphaMaps(bool debug = false) {
			List<Texture2D> maps = new List<Texture2D>();
			for (int i = 0; i < SplatSettings.Count / 4; i++)
				maps.Add(new Texture2D(AlphaMapResolution, AlphaMapResolution));
			
			for (int x = 0; x < AlphaMapResolution; x++) {
				for (int y = 0; y < AlphaMapResolution; y++) {
					MeshSample sample = SampleAt((float)y / (float)AlphaMapResolution, (float)x / (float)AlphaMapResolution);
					AddWeightsToTextures(CalculateWeights(sample), ref maps, y, x);
				}
			}

			if (debug) {
				TerrainTile tile = TerrainObject.GetComponent<TerrainTile>();

				string tileName = "Tile[" + tile.Position.x + "_" + tile.Position.y + "]";
				string folderPath = Application.dataPath + "/SplatImages/";
				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				for (var i = 0; i < maps.Count; i++) {
					byte[] bytes = maps[i].EncodeToPNG();
					string name = "Splat" + i + "_" + tileName + ".png";
					File.WriteAllBytes(folderPath + name, bytes);
				}
			}

			maps.ForEach(t => t.Apply());
			ApplySplatmapsToShaders(maps);
			
			return maps;
		}

		void AddWeightsToTextures(float[] weights, ref List<Texture2D> textures, int x, int y) {
			int len = weights.Length;

			for (int i = 0; i < len; i += 4) {
				float r = weights[i];
				float g = i + 1 < len ? weights[i + 1] : 0f;
				float b = i + 2 < len ? weights[i + 2] : 0f;
				float a = i + 3 < len ? weights[i + 3] : 0f;
				
				textures[i].SetPixel(x, y, new Color(r, g, b, a));
			}
		}

		/// <summary>
		/// Calculates the weights that can be used to create a splatmap 
		/// based on the passed sample and splats.
		/// </summary>
		/// <param name="sample">Sample to base calculation on</param>
		/// <param name="splat">Splat setting to base calculation on</param>
		/// <returns>Weight values in the same order of the </returns>
		float[] CalculateWeights(MeshSample sample) {
			float height = sample.Height;
			float angle = sample.Angle;
			float[] weights = new float[SplatSettings.Count];

			for (int i = 0; i < SplatSettings.Count; i++) {
				SplatSetting splat = SplatSettings[i];

				switch (splat.PlacementType) {
					case PlacementType.Angle:
						if (Math.Abs(angle - splat.Angle) < splat.Precision)
							weights[i] = 0f; //TODO: Fix
						break;
					case PlacementType.ElevationRange:
						if (height > splat.MinRange && height < splat.MaxRange) {
							if (i > 0) { //Can blend up
								float factor = Mathf.Clamp01((splat.Blend - (height - splat.MinRange)) / splat.Blend);
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

		/// <summary>
		/// Finds the height of the passed x and z values on the mesh.
		/// </summary>
		/// <param name="x">Normalized x position to sample</param>
		/// <param name="z">Normalized z position to sample</param>
		/// <returns>Height if found, float.Nan otherwise</returns>
		MeshSample SampleAt(float x, float z) {
			float res = MeshResolution;
			int sampleLoc = Mathf.RoundToInt(Mathf.Clamp(x * res, 0f, res - 1)) +
				Mathf.RoundToInt(Mathf.Clamp(z * res, 0f, res - 1)) * MeshResolution;
			float height = Vertices[sampleLoc].y;
			float angle = Normals[sampleLoc].y;

			return new MeshSample(height, angle);
		}

		/// <summary>
		/// Applies materials in individual splat settings to the mesh 
		/// while taking into account multiple splatmap shaders. In 
		/// the case of materials exceeding 4, an AddPass shader will be 
		/// added to the mesh in multiples of 3.
		/// </summary>
		void SetMaterialsForSplats() {
			//Insert FirstPass shader
			const string fpLoc = "Nature/Terrain/Standard";
			MeshRenderer mr = TerrainObject.GetComponent<MeshRenderer>();
			Material mat = new Material(Shader.Find(fpLoc));
			mat.SetPass(1);

			for (int i = 0; i < SplatSettings.Count; i++) {
				//Need to insert new AddPass shader
				if (i != 0 && i % 4 == 0) {

				}

				SetMaterialForSplatIndex(i, SplatSettings[i]);
			}
		}

		void ApplySplatmapsToShaders(List<Texture2D> splats) {
			int len = SplatSettings.Count;

			for (var i = 0; i < splats.Count; i++) {
				const int off = 4; //Offset for splat textures
				TerrainMaterial.SetTexture("_Control", splats[i]);
				TerrainMaterial.SetTexture("_MainTex", splats[0]);
				TerrainMaterial.SetColor("_Color", Color.black);
				
				if (i * off < len) SetMaterialForSplatIndex(0, SplatSettings[0]);
				if (i * off + 1 < len) SetMaterialForSplatIndex(1, SplatSettings[1]);
				if (i * off + 2 < len) SetMaterialForSplatIndex(2, SplatSettings[2]);
				if (i * off + 3 < len) SetMaterialForSplatIndex(3, SplatSettings[3]);
			}
		}

		/// <summary>
		/// Sets the terrain splat texture at the passed index to the same 
		/// information provided in the passed material.
		/// </summary>
		/// <param name="index">Splat index to apply material to (0 - 3)</param>
		/// <param name="mat">Material to apply</param>
		void SetMaterialForSplatIndex(int index, SplatSetting splat) {
			//Main Texture
			TerrainMaterial.SetTexture("_Splat" + index, splat.Diffuse);
			TerrainMaterial.SetTextureScale("_Splat" + index, splat.Tiling);
			TerrainMaterial.SetTextureOffset("_Splat" + index, splat.Offset);

			//Normal Texture
			TerrainMaterial.SetTexture("_Normal" + index, splat.Normal);
			TerrainMaterial.SetTextureScale("_Normal" + index, splat.Tiling);
			TerrainMaterial.SetTextureOffset("_Normal" + index, splat.Offset);

			//Metallic / Smoothness information
			TerrainMaterial.SetFloat("_Metallic" + index, splat.Metallic);
			TerrainMaterial.SetFloat("_Smoothness" + index, splat.Smoothness);
		}

		/// <summary>
		/// Cached texture used by <code>GetWhiteTexture</code> method
		/// </summary>
		private static Texture2D WhiteTex;

		/// <summary>
		/// Gets a cached white texture that can be used for GUI
		/// </summary>
		/// <returns>All white Texture instance</returns>
		private static Texture2D GetWhiteTexture() {
			if (WhiteTex == null) {
				WhiteTex = new Texture2D(1, 1);
				WhiteTex.SetPixel(0, 0, new Color(230f / 255f, 230f / 255f, 230f / 255f));
				WhiteTex.Apply();
			}

			return WhiteTex;
		}
	}
}