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
			public Texture Diffuse;
			public Texture Normal;
			public Vector2 Tiling = new Vector2(1, 1);
			public Vector2 Offset;
			public float Smoothness;
			public float Metallic;
			public float Blend;

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
		Material TerrainMaterial;
		Mesh Mesh;
		Vector3[] Vertices;
		Vector3[] Normals;
		int MeshResolution;

		/// <summary>
		/// Create a TerrainPaint object that paints the passed gameobject
		/// </summary>
		/// <param name="gameobject">Gameobject to paint</param>
		public TerrainPaint(GameObject gameobject) {
			TerrainObject = gameobject;

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
					if (GUILayout.Button("Edit Material")) {
						AddTextureWindow window = new AddTextureWindow(ref splat);
					} if (splat.Diffuse == null) {
						EditorGUILayout.HelpBox("This splat material does not have a selected diffuse texture.", MessageType.Warning);
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

		public Texture2D CreateAlphaMap(List<SplatSetting> settings) {
			Texture2D tex = new Texture2D(AlphaMapResolution, AlphaMapResolution);
			Color[] colors = new Color[AlphaMapResolution * AlphaMapResolution];

			int colorIdx = 0;
			for (int x = 0; x < AlphaMapResolution; x++) {
				for (int y = 0; y < AlphaMapResolution; y++) {
					MeshSample sample = SampleAt((float)y / (float)AlphaMapResolution, (float)x / (float)AlphaMapResolution);
					float height = sample.Height;
					float angle = sample.Angle;
					float[] weights = new float[settings.Count];

					for (int i = 0; i < settings.Count; i++) {
						SplatSetting splat = settings[i];

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

					var sum = weights.Sum();
					var l = weights.Length;
					for (int i = 0; i < l; i++) {
						weights[i] /= sum;
					}

					colors[colorIdx] = new Color(l > 0 ? weights[0] : 0f,
						l > 1 ? weights[1] : 0f,
						l > 2 ? weights[2] : 0f,
						l > 3 ? weights[3] : 0f);
					colorIdx++;
				}
			}

			tex.SetPixels(colors);
			tex.Apply();
			TerrainMaterial.SetTexture("_Control", tex);

			var len = settings.Count;
			if (len > 0) SetMaterialForSplatIndex(0, settings[0]);
			if (len > 1) SetMaterialForSplatIndex(1, settings[1]);
			if (len > 2) SetMaterialForSplatIndex(2, settings[2]);
			if (len > 3) SetMaterialForSplatIndex(3, settings[3]);

			bool test = false;
			if (test) {
				byte[] bytes = tex.EncodeToPNG();
				File.WriteAllBytes(Application.dataPath + "/Splat.png", bytes);
			}

			return tex;
		}

		/// <summary>
		/// Finds the height of the passed x and z values on the mesh 
		/// by raycasting. x and z should be normalized.
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