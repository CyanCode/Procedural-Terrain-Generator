using Terra.Structures;
using UnityEditor;
using UnityEngine;

namespace Terra.Graph.Fields {
	public static class PreviewField {
		private static int PreviewTextureSize = 100;
		private static int ExportTextureSize = 500;
		private static int ExportSpread = 1;

		public static void Show(PreviewableNode node, bool showExport = false) {
			node.IsPreviewDropdown = EditorGUILayout.Foldout(node.IsPreviewDropdown, "Preview");

			if (node.IsPreviewDropdown) {
				if (node.PreviewTexture != null) {
					var ctr = EditorGUILayout.GetControlRect(false, PreviewTextureSize);
					ctr.x = PreviewTextureSize * 0.25f;
					ctr.width = PreviewTextureSize;

					EditorGUI.DrawPreviewTexture(ctr, node.PreviewTexture);
				}

				if (GUILayout.Button("Update Preview")) {
					node.PreviewTexture = node.DidRequestTextureUpdate(PreviewTextureSize, PreviewTextureSize);
				}
				if (showExport && GUILayout.Button("Export Preview")) {
					string path = Application.dataPath + "/TerraPreview.png";
					MathUtil.WriteTexture(node.DidRequestTextureUpdate(ExportTextureSize, ExportSpread), path);
					Debug.Log("Exported preview texture to " + path);
				}
			}
		}
	}
}
