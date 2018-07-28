using System;
using UnityEditor;
using UnityEngine;

namespace Terra.Data {
	/// <summary>
	/// Container for data relating to the state of the TerraSettingsEditor
	/// </summary>
	[Serializable]
	public class EditorStateData {
		public ToolbarOptions ToolbarSelection = ToolbarOptions.General;

		public bool DisplayPreview = false;
		public bool IsAdvancedFoldout = false;

		//Biomes
		public bool ShowBiomePreview = false;
		public float BiomePreviewZoom = 25f;
		public Texture2D BiomePreview = null;
		public bool ShowWhittakerInfo = false;

		public float InspectorWidth { get { return EditorGUIUtility.currentViewWidth; } }

		private float lastInspectorWidth = 0f;

		public bool DidResize(float currentWidth) {
			return Math.Abs(lastInspectorWidth - currentWidth) > 0.1f;
		}
	}

	[Serializable]
	public enum ToolbarOptions {
		General = 0,
		Maps = 1,
		Biomes = 2,
		Details = 3
	}
}