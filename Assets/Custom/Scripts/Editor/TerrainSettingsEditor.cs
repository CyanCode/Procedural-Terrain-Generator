using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainSettings))]
public class TerrainSettingsEditor: Editor {
	private TerrainSettings Settings {
		get {
			return (TerrainSettings)target;
		}
	}

	public override void OnInspectorGUI() {
		//Options tab
		EditorGUILayout.Space();
		Settings.ToolbarSelection = (TerrainSettings.ToolbarOptions)EditorGUIExtension.EnumToolbar(Settings.ToolbarSelection);

		switch (Settings.ToolbarSelection) {
			case TerrainSettings.ToolbarOptions.General:
				//Tracked gameobject
				EditorGUILayout.Space();
				Settings.TrackedObject = (GameObject)EditorGUILayout.ObjectField(Settings.TrackedObject, typeof(GameObject), true);

				//Terrain settings
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Terrain Generation Settings", EditorStyles.boldLabel);
				Settings.GenerationRadius = EditorGUILayout.IntField("Gen Radius", Settings.GenerationRadius);
				Settings.GenerationSeed = EditorGUILayout.IntField("Seed", Settings.GenerationSeed);
				Settings.Height = EditorGUILayout.IntField("Height", Settings.Height);
				Settings.Length = EditorGUILayout.IntField("Length", Settings.Length);

				//Heightmap / Alphamap
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Heightmap & Alphamap Resolution", EditorStyles.boldLabel);
				int resolution = EditorGUILayout.IntField("Resolution", Settings.HeightmapResolution);
				Settings.HeightmapResolution = resolution;
				Settings.AlphamapResolution = resolution;

				break;
			case TerrainSettings.ToolbarOptions.Noise:
				EditorGUILayout.Space(); 
				if (GUILayout.Button("Open Noise Editor")) {
					//Open editor
				}

				break;
			case TerrainSettings.ToolbarOptions.Materials:
				break;
		}
	}
}
