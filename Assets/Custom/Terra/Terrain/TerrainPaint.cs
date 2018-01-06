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
			//Using Textures
			public Texture2D Diffuse;
			public Texture2D Normal;
			public Vector2 Tiling = new Vector2(1, 1);
			public Vector2 Offset;

			public float Smoothness;
			public float Metallic;
			public float Blend = 30f;

			public PlacementType PlacementType;

			public int Angle;

			public float MinRange;
			public float MaxRange;
			public bool IsMaxHeight;
			public bool IsMinHeight;

			public float Precision = 0.9f;
		}
		public enum PlacementType {
			ElevationRange
			//Angle,
		}

		public int AlphaMapResolution = 128;

		private GameObject TerrainObject;
		private List<SplatSetting> SplatSettings;

		//Terrain/Mesh
		private Material TerrainMaterial;
		private Mesh Mesh;
		private Vector3[] Vertices;
		private Vector3[] Normals;
		private int MeshResolution;
		
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

			//Use custom material instead
			settings.UseCustomMaterial = EditorGUILayout.Toggle("Custom Material", settings.UseCustomMaterial);

			if (settings.UseCustomMaterial) {
				settings.CustomMaterial = (Material)EditorGUILayout.ObjectField("material", settings.CustomMaterial, typeof(Material), false);
				return;
			}

			//Use textures
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
					//EditorGUILayout.LabelField((splat.PlacementType == PlacementType.Angle ? "Angled" : "Elevation") + " Material " + (i + 1));
					EditorGUILayout.LabelField("Elevation Material " + (i + 1));
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
						new AddTextureWindow(ref splat);
					}

					EditorGUILayout.Space();

					//Blend factor
					splat.Blend = EditorGUILayout.FloatField("Blend Amount", splat.Blend);

					//GUI for different types
					splat.PlacementType = (PlacementType)EditorGUILayout.EnumPopup("Placement Type", splat.PlacementType);
					switch (splat.PlacementType) {
						//case PlacementType.Angle:
						//	splat.Angle = EditorGUILayout.IntSlider("Angle", splat.Angle, 0, 90);
						//	break;
						case PlacementType.ElevationRange:
							if (!splat.IsMaxHeight)
								splat.MaxRange = EditorGUILayout.FloatField("Max Height", splat.MaxRange);
							if (!splat.IsMinHeight)
								splat.MinRange = EditorGUILayout.FloatField("Min Height", splat.MinRange);

							//Checkboxes for infinity & -infinity heights
							EditorGUI.BeginChangeCheck();
							if (splat.IsMaxHeight || !settings.IsMaxHeightSelected) splat.IsMaxHeight = EditorGUILayout.Toggle("Is Highest Material", splat.IsMaxHeight);
							if (EditorGUI.EndChangeCheck())
								settings.IsMaxHeightSelected = splat.IsMaxHeight;

							EditorGUI.BeginChangeCheck();
							if (splat.IsMinHeight || !settings.IsMinHeightSelected) splat.IsMinHeight = EditorGUILayout.Toggle("Is Lowest Material", splat.IsMinHeight);
							if (EditorGUI.EndChangeCheck())
								settings.IsMinHeightSelected = splat.IsMinHeight; 

							//if (splat.MinRange > splat.MaxRange) splat.MinRange = splat.MaxRange;
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
			for (int i = 0; i < Mathf.CeilToInt(SplatSettings.Count / 4f); i++)
				maps.Add(new Texture2D(AlphaMapResolution, AlphaMapResolution));
			
			for (int x = 0; x < AlphaMapResolution; x++) {
				for (int y = 0; y < AlphaMapResolution; y++) {
					MeshSample sample = SampleAt(y / (float)AlphaMapResolution, x / (float)AlphaMapResolution);
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
				
				textures[i / 4].SetPixel(x, y, new Color(r, g, b, a));
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
			//float angle = sample.Angle;
			float[] weights = new float[SplatSettings.Count];

			for (int i = 0; i < SplatSettings.Count; i++) {
				SplatSetting splat = SplatSettings[i];

				float min = splat.IsMinHeight ? float.MinValue : splat.MinRange;
				float max = splat.IsMaxHeight ? float.MaxValue : splat.MaxRange;

				switch (splat.PlacementType) {
					//case PlacementType.Angle:
					//	if (Math.Abs(angle - splat.Angle) < splat.Precision)
					//		weights[i] = 0f; //TODO: Fix
					//	break;
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

		void ApplySplatmapsToShaders(List<Texture2D> splats) {
			int len = SplatSettings.Count;
			MeshRenderer mr = TerrainObject.GetComponent<MeshRenderer>();
			Material toSet = TerrainMaterial;

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
		/// <param name="toSet"></param>
		/// <param name="mat">Material to apply</param>
		void SetMaterialForSplatIndex(int index, SplatSetting splat, Material toSet) {
			//Main Texture
			toSet.SetTexture("_Splat" + index, splat.Diffuse);
			toSet.SetTextureScale("_Splat" + index, splat.Tiling);
			toSet.SetTextureOffset("_Splat" + index, splat.Offset);

			//Normal Texture
			toSet.SetTexture("_Normal" + index, splat.Normal);
			toSet.SetTextureScale("_Normal" + index, splat.Tiling);
			toSet.SetTextureOffset("_Normal" + index, splat.Offset);

			//Metallic / Smoothness information
			toSet.SetFloat("_Metallic" + index, splat.Metallic);
			toSet.SetFloat("_Smoothness" + index, splat.Smoothness);
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