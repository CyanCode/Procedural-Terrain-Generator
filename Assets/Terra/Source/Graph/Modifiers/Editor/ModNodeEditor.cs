using Terra.Graph.Generators.Modifiers;
using UnityEngine;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(AbsTwoModNode))]
	abstract class ModNodeEditor: PreviewableNodeEditor {
		public override Color GetTint() {
			return EditorColors.TintModifier;
		}
	}
}