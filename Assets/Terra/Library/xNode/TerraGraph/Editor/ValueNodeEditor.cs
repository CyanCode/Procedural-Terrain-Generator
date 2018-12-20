using Terra.Graph.Value;
using UnityEngine;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditorAttribute(typeof(ValueNode))]
	public class ValueNodeEditor: NodeEditor {
		public override Color GetTint() {
			return Constants.TintValue;
		}
	}
}