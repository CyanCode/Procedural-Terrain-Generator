using Terra.Graph.Generators.Modifiers;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(AbsTwoModNode))]
	public class TwoModeNodeEditor: NodeEditor {
		public override Color GetTint() {
			return Constants.TintModifier;
		}
	}
}