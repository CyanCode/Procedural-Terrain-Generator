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
			public Texture MainTexture;
			public Texture NormalTexture;
			public Vector2 Tiling = new Vector2(1, 1);
			public Vector2 Offset;
			public float Smoothness;
			public float Metallic;

			public PlacementType PlacementType;

			public int Angle;

			public float MinRange;
			public float MaxRange;

			public float Impact = 1f;
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
		public static void DisplayGUI(TerrainSettings settings) {
			EditorGUILayout.Space();

			if (settings.SplatSettings != null) {
				for (int i = 0; i < settings.SplatSettings.Count; i++) {
					SplatSetting splat = settings.SplatSettings[i];

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("X", GUILayout.Height(16), GUILayout.Width(18))) {
						settings.SplatSettings.RemoveAt(i);
						i--;
						continue;
					}
					EditorGUILayout.LabelField((splat.PlacementType == PlacementType.Angle ? "Angled" : "Elevation") + " Texture " + (i + 1));
					EditorGUILayout.EndHorizontal();

					splat.MainTexture = (Texture)EditorGUILayout.ObjectField("Main Texture", splat.MainTexture, typeof(Texture), true, GUILayout.Height(16));
					splat.NormalTexture = (Texture)EditorGUILayout.ObjectField("Normal Texture", splat.NormalTexture, typeof(Texture), true, GUILayout.Height(16));
					splat.Metallic = EditorGUILayout.Slider("Metallic", splat.Metallic, 0f, 1f);
					splat.Smoothness = EditorGUILayout.Slider("Smoothness", splat.Smoothness, 0f, 1f);
					splat.Tiling = EditorGUILayout.Vector2Field("Tiling", splat.Tiling);
					splat.Offset = EditorGUILayout.Vector2Field("Offset", splat.Offset);

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

					splat.Impact = EditorGUILayout.FloatField("Impact", splat.Impact);

					EditorGUILayout.Separator();
				}
			}

			if (GUILayout.Button("Add Material")) {
				if (settings.SplatSettings == null)
					settings.SplatSettings = new List<SplatSetting>();

				settings.SplatSettings.Add(new SplatSetting());
			}
		}

		public void CreateAlphaMap(List<SplatSetting> settings) {
			Texture2D Tex = new Texture2D(AlphaMapResolution, AlphaMapResolution);
			Color[] Colors = new Color[AlphaMapResolution * AlphaMapResolution];

			int colorIdx = 0;
			for (int x = 0; x < AlphaMapResolution; x++) {
				for (int y = 0; y < AlphaMapResolution; y++) {
					MeshSample sample = SampleAt((float)y / (float)AlphaMapResolution, (float)x / (float)AlphaMapResolution);
					float height = sample.Height;
					float angle = sample.Angle;
					float[] weights = new float[settings.Count];
					var blend = 10f; //TODO: Actually put in settings

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
										float factor = Mathf.Clamp01((blend - (height - splat.MinRange)) / blend);
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

					Colors[colorIdx] = new Color(l > 0 ? weights[0] : 0f,
						l > 1 ? weights[1] : 0f,
						l > 2 ? weights[2] : 0f,
						l > 3 ? weights[3] : 0f);
					colorIdx++;
				}
			}

			Tex.SetPixels(Colors);
			Tex.Apply();
			TerrainMaterial.SetTexture("_Control", Tex);

			var len = settings.Count;
			if (len > 0) SetMaterialForSplatIndex(0, settings[0]);
			if (len > 1) SetMaterialForSplatIndex(1, settings[1]);
			if (len > 2) SetMaterialForSplatIndex(2, settings[2]);
			if (len > 3) SetMaterialForSplatIndex(3, settings[3]);

			bool test = false;
			if (test) {
				byte[] bytes = Tex.EncodeToPNG();
				File.WriteAllBytes(Application.dataPath + "/Splat.png", bytes);
			}
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
			TerrainMaterial.SetTexture("_Splat" + index, splat.MainTexture);
			TerrainMaterial.SetTextureScale("_Splat" + index, splat.Tiling);
			TerrainMaterial.SetTextureOffset("_Splat" + index, splat.Offset);

			//Normal Texture
			TerrainMaterial.SetTexture("_Normal" + index, splat.NormalTexture);
			TerrainMaterial.SetTextureScale("_Normal" + index, splat.Tiling);
			TerrainMaterial.SetTextureOffset("_Normal" + index, splat.Offset);

			//Metallic / Smoothness information
			TerrainMaterial.SetFloat("_Metallic" + index, splat.Metallic);
			TerrainMaterial.SetFloat("_Smoothness" + index, splat.Smoothness);
		}
	}
}