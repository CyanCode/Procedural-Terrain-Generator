using System;
using UnityEditor;
using UnityEngine;

namespace Terra.Source.Editor {
	[ExecuteInEditMode, CustomEditor(typeof(TerraConfig)), Serializable]
	public class TerrainSettingsEditor: UnityEditor.Editor {
		private TerraConfig Config => target as TerraConfig;

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