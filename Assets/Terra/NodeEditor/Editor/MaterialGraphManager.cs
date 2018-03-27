using System;
using System.Collections.Generic;
using System.IO;
using Terra.GraphEditor;
using Terra.Terrain;
using UnityEditor;
using UnityEngine;

namespace Assets.Terra.NodeEditor.Editor {
	public class MaterialGraphManager: GraphManager {
		public MaterialGraphManager(TerraSettings settings) : base(settings, Graph.GraphType.Material) { }

		public override void OptionGraphOpenSuccess() {
			string msg = "The node graph for this terrain is ready for use.";
			EditorGUILayout.HelpBox(msg, MessageType.Info);
			EditorGUILayout.LabelField("Selected File: " + Path.GetFileNameWithoutExtension(Settings.SelectedNoiseFile));

			if (GUILayout.Button("Edit Selected Graph")) {
				Open();
			}

			OptionDefault();

			EditorGUILayout.Space();

			Settings.Spread = EditorGUILayout.FloatField("Spread", Settings.Spread);
			Settings.Amplitude = EditorGUILayout.FloatField("Amplitude", Settings.Amplitude);

			EditorGUILayout.Space();
			if (Application.isEditor && Settings.DisplayPreview) {
				if (GUILayout.Button("Update Preview")) {
					//Generator gen = GetGraphGenerator();

					//if (gen != null) {
					//	//Settings.PreviewMesh = TerrainTile.GetPreviewMesh(Settings, gen);
					//	Settings.Preview.TriggerPreviewUpdate();
					//}
				}
			}
		}
	}
}
