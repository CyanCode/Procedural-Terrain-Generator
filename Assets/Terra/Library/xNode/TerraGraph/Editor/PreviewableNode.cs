using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Editor.Graph {
	public abstract class PreviewableNode: NodeEditor {
		public Texture2D PreviewTexture = null;
		public int PreviewTextureSize = 100;

		private bool IsDropdown;

		private const int NODE_PADDING = 6;

		public void TextureField() {
			IsDropdown = EditorGUILayout.Foldout(IsDropdown, "Preview");
			if (IsDropdown) {
				if (PreviewTexture != null) {
					var ctr = EditorGUILayout.GetControlRect(false, PreviewTextureSize);
					ctr.x = PreviewTextureSize * 0.25f;
					ctr.width = PreviewTextureSize;

					EditorGUI.DrawPreviewTexture(ctr, PreviewTexture);
				}

				if (GUILayout.Button("Update Preview")) {
					PreviewTexture = DidRequestTextureUpdate();
				}
			}
		}

		/// <summary>
		/// Called if the user pressed the "Update Preview" button
		/// </summary>
		public abstract Texture2D DidRequestTextureUpdate();
	}
}