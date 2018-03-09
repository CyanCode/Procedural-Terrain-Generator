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
		private TerraGUI gui;

		void OnEnable() {
			manager = new GraphManager(Settings);
			Settings.Generator = manager.GetGraphGenerator();
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
					gui.Noise(manager);

					break;
				case TerraSettings.ToolbarOptions.Materials:
					gui.Material();

					break;
			}
		}
	}
}