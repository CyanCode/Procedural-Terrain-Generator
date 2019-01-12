using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(EndNode))]
	class EndNodeEditor: NodeEditor {
		private readonly int NODE_WIDTH = 150;
	
		public override int GetWidth() {
			return NODE_WIDTH;
		}

		public override string GetTitle() {
			return "End";
		}

		public override Color GetTint() {
			return EditorColors.TintEnd;
		}
	}
}
