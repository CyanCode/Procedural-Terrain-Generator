using Terra.Graph.Noise;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
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

		public override Color GetTint() {
			return Constants.TintNoise;
		}
	}
}