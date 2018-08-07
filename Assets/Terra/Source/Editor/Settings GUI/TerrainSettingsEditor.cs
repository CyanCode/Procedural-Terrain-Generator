using System;
using Terra.Data;
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
			if (!TerraSettings.IsInitialized)
				return;

			//Options toolbar
			gui.Toolbar();

			switch (Settings.EditorState.ToolbarSelection) {
				case ToolbarOptions.General:
					gui.General();

					break;
				case ToolbarOptions.Maps:
					gui.Maps();

					break;
				case ToolbarOptions.Biomes:
					gui.Biomes();

					break;
				case ToolbarOptions.Details:
					gui.Details();

					break;
			}

			//Preview button
			gui.PreviewUpdate();

			gui.Debug();
		}
	}
}