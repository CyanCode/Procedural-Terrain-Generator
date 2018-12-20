using Terra.Graph;
using UnityEngine;
using UnityEditor;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(AbsTwoModNode))]
	public class TwoModeNodeEditor: NodeEditor {
		public override Color GetTint() {
			return Constants.TintModifier;
		}
	}
}