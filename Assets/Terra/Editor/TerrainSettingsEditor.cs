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
		private TerrainPreview Preview;

		void OnEnable() {
			manager = new GraphManager(Settings);
			Settings.Generator = manager.GetGraphGenerator();
			gui = new TerraGUI(Settings);

			if (Preview == null)
				Preview = new TerrainPreview(Settings);
			if (Settings.DisplayPreview)
				Preview.SetVisible(true);
		}

		void OnDisable() {
			if (Preview != null && Settings.DisplayPreview)
				Preview.SetVisible(false);
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