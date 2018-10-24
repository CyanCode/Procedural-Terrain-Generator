using System;
using UnityEditor;
using UnityEngine;

namespace Terra.Structure {
	/// <summary>
	/// Container for data relating to the state of the TerraSettingsEditor
	/// </summary>
	[Serializable]
	public class EditorStateData {
		public ToolbarOptions ToolbarSelection = ToolbarOptions.General;

		public bool IsLodFoldout = false;

		//Biomes
		public bool ShowBiomePreview = false;
		public float BiomePreviewZoom = 25f;
		public Texture2D BiomePreview = null;
		public bool ShowWhittakerInfo = false;

		public float InspectorWidth { get { return EditorGUIUtility.currentViewWidth; } }
	}

	[Serializable]
	public enum ToolbarOptions {
		General = 0,
		Maps = 1,
		Biomes = 2,
		Details = 3
	}
}