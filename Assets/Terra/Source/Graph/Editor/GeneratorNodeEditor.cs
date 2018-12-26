using Terra.Graph.Generators;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(AbsGeneratorNode))]
	class GeneratorNodeEditor: NodeEditor {
		private AbsGeneratorNode _generator {
			get {
				return (AbsGeneratorNode)target;
			}
		}

		public override void OnBodyGUI() {
			EditorGUI.BeginChangeCheck();
			OnBodyGUI(PreviewableNode.FieldNames);
			if (EditorGUI.EndChangeCheck()) {
				_generator.OnValueChange(); 
			}

			PreviewField.Show(_generator);
		}

		public override string GetTitle() {
			return ((AbsGeneratorNode)target).GetTitle();
		}

		public override Color GetTint() {
			return Constants.TintNoise;
		}
	}
}