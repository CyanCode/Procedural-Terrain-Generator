using UnityEditor;
using UnityEngine;

namespace Terra.Graph.Fields {
	public static class PreviewField {
		public static void Show(PreviewableNode node) {
			node.IsPreviewDropdown = EditorGUILayout.Foldout(node.IsPreviewDropdown, "Preview");

			if (node.IsPreviewDropdown) {
				if (node.PreviewTexture != null) {
					var ctr = EditorGUILayout.GetControlRect(false, node.PreviewTextureSize);
					ctr.x = node.PreviewTextureSize * 0.25f;
					ctr.width = node.PreviewTextureSize;

					EditorGUI.DrawPreviewTexture(ctr, node.PreviewTexture);
				}

				if (GUILayout.Button("Update Preview")) {
					node.PreviewTexture = node.DidRequestTextureUpdate();
				}
			}
		}
	}
}
