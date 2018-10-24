using System;
using Terra;
using Terra.Structure;
using UnityEngine;

namespace UnityEditor.Terra {
	[ExecuteInEditMode, CustomEditor(typeof(TerraConfig)), Serializable]
	public class TerrainSettingsEditor: Editor {
		private TerraConfig Config {
			get {
				return target as TerraConfig;
			}
		}

		private TerraGUI _gui;

		void OnEnable() {
			if (_gui == null) {
				_gui = new TerraGUI(Config);
			}
		}

		public override void OnInspectorGUI() {
			if (!TerraConfig.IsInitialized)
				return;

			//Options toolbar
			_gui.Toolbar();

			switch (Config.EditorState.ToolbarSelection) {
				case ToolbarOptions.General:
					_gui.General();

					break;
				case ToolbarOptions.Maps:
					_gui.Maps();

					break;
				case ToolbarOptions.Biomes:
					_gui.Biomes();

					break;
				case ToolbarOptions.Details:
					_gui.Details();

					break;
			}

			//Preview button
			_gui.PreviewUpdate();

			_gui.Debug();
		}
	}
}