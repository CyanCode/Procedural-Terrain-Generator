using Terra.Graph;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditor(typeof(EndNode))]
	class EndNodeEditor: NodeEditor {
		private readonly int NODE_WIDTH = 150;
	
		public override int GetWidth() {
			return NODE_WIDTH;
		}
	}
}
