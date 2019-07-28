using System;
using Terra;
using Terra.Structures;
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

            _gui.General();

			//Preview button
			_gui.PreviewUpdate();
		}
	}
}