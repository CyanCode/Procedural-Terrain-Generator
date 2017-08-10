using Terra.Terrain;
using UnityEditor;
using UnityEngine;

namespace Terra.Terrain {
	public class AddTextureWindow: EditorWindow {
		private TerrainPaint.SplatSetting Splat;
		private bool DiffuseSelected = false;

		/// <summary>
		/// Cached Empty Texture2D used by GetEmptyTexture() method
		/// </summary>
		private Texture2D Empty;

		public AddTextureWindow(ref TerrainPaint.SplatSetting splat) {
			titleContent = new GUIContent("Add Splat Material");
			maxSize = new Vector2(200, 280);
			minSize = maxSize;
			Splat = splat;
			ShowUtility();
		}

		void OnGUI() {
			GUILayout.BeginArea(new Rect(8, 8, 200 - 16, 500));
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			//Diffuse Map
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Diffuse", GUILayout.Width(60));
			Splat.Diffuse = (Texture2D)EditorGUILayout.ObjectField(
				Splat.Diffuse == null ? GetEmptyTexture() : GetDiffuseTexture(),
				typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
			if (!DiffuseSelected) Splat.Diffuse = null;
			EditorGUILayout.EndVertical();

			//Normal Map
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Normal", GUILayout.Width(60));

			EditorGUI.BeginChangeCheck();
			Splat.Normal = (Texture2D)EditorGUILayout.ObjectField(
				Splat.Normal == null ? GetEmptyTexture() : Splat.Normal,
				typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();

			//Tiling & Offset
			EditorGUILayout.Space();
			Splat.Tiling = EditorGUILayout.Vector2Field("Tiling", Splat.Tiling);
			Splat.Offset = EditorGUILayout.Vector2Field("Offset", Splat.Offset);

			//Metallic & Smoothness
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Metallic");
			EditorGUI.indentLevel = 1;
			GUILayout.Space(-5);
			Splat.Metallic = EditorGUILayout.Slider(Splat.Metallic, 0f, 1f);

			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField("Smoothness");
			EditorGUI.indentLevel = 1;
			GUILayout.Space(-5);
			Splat.Smoothness = EditorGUILayout.Slider(Splat.Smoothness, 0f, 1f);

			EditorGUI.indentLevel = 0;
			if (GUILayout.Button("Add Texture")) {
				Close();
			}

			EditorGUILayout.EndVertical();
			GUILayout.EndArea();
		}

		/// <summary>
		/// Gets a white, empty texture that can be displayed in a GUI
		/// </summary>
		/// <returns>Cached empty texture</returns>
		Texture2D GetEmptyTexture() {
			DiffuseSelected = false;
			if (Empty == null) {
				Empty = new Texture2D(1, 1);
				Empty.SetPixel(0, 0, Color.gray);
				Empty.Apply();
			}

			return Empty;
		}

		/// <summary>
		/// Gets the diffuse texture associated with the current splat 
		/// material. Sets DiffuseSelected to true.
		/// </summary>
		/// <returns>Diffuse texture</returns>
		Texture GetDiffuseTexture() {
			DiffuseSelected = true;
			return Splat.Diffuse;
		}
	}
}