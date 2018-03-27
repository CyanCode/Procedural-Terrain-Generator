using Assets.Terra.NodeEditor.Editor;
using Terra.GraphEditor;
using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	[ExecuteInEditMode]
	[CustomEditor(typeof(TerraSettings))]
	public class TerrainSettingsEditor: Editor {
		internal TerraSettings Settings {
			get {
				return (TerraSettings)target;
			}
		}
		private NoiseGraphManager noiseManager;
		private MaterialGraphManager materialManager;
		private TerraGUI gui;

		void OnEnable() {
			noiseManager = new NoiseGraphManager(Settings);
			materialManager = new MaterialGraphManager(Settings);
			Settings.NoiseGenerator = noiseManager.GetGraphGenerator();
			gui = new TerraGUI(Settings);
		}

		public override void OnInspectorGUI() {
			//Options tab
			gui.Toolbar();

			switch (Settings.ToolbarSelection) {
				case TerraSettings.ToolbarOptions.General:
					gui.General();

					break;
				case TerraSettings.ToolbarOptions.Noise:
					gui.Noise(noiseManager);

					break;
				case TerraSettings.ToolbarOptions.Materials:
					gui.Material(materialManager);

					break;
				case TerraSettings.ToolbarOptions.ObjectPlacement:
					gui.ObjectPlacement();

					break;
			}
		}
	}
}