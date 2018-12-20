using Terra.Graph.Noise;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditor(typeof(NoisePreviewNode))]
	class PreviewNodeEditor: NodeEditor {
		private NoisePreviewNode PreviewNode {
			get {
				return (NoisePreviewNode)target;
			}
		}

		private readonly int TEXTURE_PADDING = 16;
		private readonly int TEXTURE_HEIGHT_OFFSET = 55;
		private readonly int NODE_WIDTH = 150;

		private int EXPANDED_HEIGHT {
			get {
				return GetWidth() - (TEXTURE_PADDING * 2);
			}
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();

			//Update texture if needed
			if (PreviewNode.TextureNeedsUpdating) {
				PreviewNode.UpdateTexture();
			}

			//If there is a texture to display, display it.
			if (PreviewNode.HasTexture()) {
				int texExtents = GetWidth() - (TEXTURE_PADDING * 2);
				Rect dimensions = new Rect(TEXTURE_PADDING, TEXTURE_HEIGHT_OFFSET, texExtents, texExtents);

				//Expand node size and draw texture
				EditorGUILayout.LabelField("", GUILayout.Height(EXPANDED_HEIGHT));
				GUI.DrawTexture(dimensions, PreviewNode.PreviewTexture);
			}
		}

		public override int GetWidth() {
			return NODE_WIDTH;
		}

		public override string GetTitle() {
			return "Preview";
		}
	}
}
