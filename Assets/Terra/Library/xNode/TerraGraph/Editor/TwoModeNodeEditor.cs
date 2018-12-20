using Terra.Graph.Noise.Modifier;
using UnityEngine;
using UnityEditor;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditor(typeof(AbsTwoModNode))]
	public class TwoModeNodeEditor: NodeEditor {
		public override Color GetTint() {
			return Constants.TintModifier;
		}
	}
}