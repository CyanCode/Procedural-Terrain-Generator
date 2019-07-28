using Terra.Graph.Values;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(ValueNode))]
	public class ValueNodeEditor: NodeEditor {
		public override Color GetTint() {
			return EditorColors.TintValue;
		}
	}
}