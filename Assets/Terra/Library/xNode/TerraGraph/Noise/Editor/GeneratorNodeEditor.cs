using Terra.Graph.Noise;
using UnityEditor;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditor(typeof(AbsGeneratorNode))]
	class GeneratorNodeEditor: NodeEditor {
		public override void OnBodyGUI() {
			EditorGUI.BeginChangeCheck();
			base.OnBodyGUI();
			if (EditorGUI.EndChangeCheck()) {
				((AbsGeneratorNode)target).OnValueChange();
			}
		}

		public override string GetTitle() {
			return ((AbsGeneratorNode)target).GetTitle();
		}
	}
}