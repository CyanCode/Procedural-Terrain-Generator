using System;
using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	[ExecuteInEditMode, CustomEditor(typeof(TerraSettings)), Serializable]
	public class TerrainSettingsEditor: Editor {
		internal TerraSettings Settings {
			get {
				return (TerraSettings)target;
			}
		}

		private TerraGUI gui;

		void OnEnable() {
			if (gui == null) {
				gui = new TerraGUI(Settings);
			}
		}

		public override void OnInspectorGUI() {
			//Options toolbar
			gui.Toolbar();

			switch (Settings.ToolbarSelection) {
				case TerraSettings.ToolbarOptions.General:
					gui.General();

					break;
				case TerraSettings.ToolbarOptions.Noise:
					gui.Noise();

					break;
				case TerraSettings.ToolbarOptions.Materials:
					gui.Material();

					break;
				case TerraSettings.ToolbarOptions.ObjectPlacement:
					gui.ObjectPlacement();

					break;
			}
		}
	}
}