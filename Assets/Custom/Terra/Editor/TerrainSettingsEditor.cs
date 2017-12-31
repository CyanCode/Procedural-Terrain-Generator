using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	[ExecuteInEditMode]
	[CustomEditor(typeof(TerraSettings))]
	public class TerrainSettingsEditor: Editor {
		private TerraSettings Settings {
			get {
				return (TerraSettings)target;
			}
		}
		private GraphManager manager;

		void OnEnable() {
			manager = new GraphManager(Settings);
			Settings.Generator = manager.GetGraphGenerator();
		}

		public override void OnInspectorGUI() {
			//Options tab
			EditorGUILayout.Space();
			Settings.ToolbarSelection = (TerraSettings.ToolbarOptions)EditorGUIExtension.EnumToolbar(Settings.ToolbarSelection);

			switch (Settings.ToolbarSelection) {
				case TerraSettings.ToolbarOptions.General:
					//Tracked gameobject
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Tracked GameObject", EditorStyles.boldLabel);
					Settings.TrackedObject = (GameObject)EditorGUILayout.ObjectField(Settings.TrackedObject, typeof(GameObject), true);

					//Terrain settings
					string[] stringResOptions = { "32", "64", "128" };
					int[] resOptions = { 32, 64, 128 };

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Terrain Generation Settings", EditorStyles.boldLabel);
					Settings.GenerationRadius = EditorGUILayout.IntField("Gen Radius", Settings.GenerationRadius);
					Settings.ColliderGenerationExtent = EditorGUILayout.FloatField("Collider Gen Extent", Settings.ColliderGenerationExtent);
					TerraSettings.GenerationSeed = EditorGUILayout.IntField("Seed", TerraSettings.GenerationSeed);
					Settings.Length = EditorGUILayout.IntField("Length", Settings.Length);
					Settings.MeshResolution = EditorGUILayout.IntPopup("Mesh Resolution", Settings.MeshResolution, stringResOptions, resOptions);
					
					break;
				case TerraSettings.ToolbarOptions.Noise:
					EditorGUILayout.Space();

					if (Settings.SelectedFile != "") {
						if (manager.GraphFileCanBeRead(Settings.SelectedFile))
							if (manager.HasValidEndNode()) manager.OptionGraphOpenSuccess();
							else manager.MessageNoEndNode();
						else
							manager.OptionGraphOpenError();
					} else {
						manager.OptionIncorrectFileSelection();
					}

					break;
				case TerraSettings.ToolbarOptions.Materials:
					TerrainPaint.DisplayGUI(Settings);
					break;
			}
		}
	}
}