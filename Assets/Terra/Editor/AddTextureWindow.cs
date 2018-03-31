using UnityEditor;
using UnityEngine;

namespace Terra.Terrain {
	public class AddTextureWindow: EditorWindow {
		private TerrainPaint.SplatSetting Splat;

		public static AddTextureWindow Init(ref TerrainPaint.SplatSetting splat) {
			AddTextureWindow win = CreateInstance<AddTextureWindow>();
			win.titleContent = new GUIContent("Add Splat Material");
			win.maxSize = new Vector2(200, 280);
			win.minSize = win.maxSize;
			win.Splat = splat;
			win.ShowUtility();

			return win;
		}

		void OnGUI() {
			GUILayout.BeginArea(new Rect(8, 8, 200 - 16, 500));
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			//Diffuse Map
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Diffuse", GUILayout.Width(60));

			Splat.Diffuse = (Texture2D)EditorGUILayout.ObjectField(Splat.Diffuse,
				typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));

			EditorGUILayout.EndVertical();

			//Normal Map
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Normal", GUILayout.Width(60));

			Splat.Normal = (Texture2D)EditorGUILayout.ObjectField(Splat.Normal,
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
	}
}