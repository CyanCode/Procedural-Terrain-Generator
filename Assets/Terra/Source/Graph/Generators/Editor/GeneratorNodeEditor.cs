using System.Collections.Generic;
using Terra.CoherentNoise;
using Terra.Graph.Fields;
using Terra.Graph.Generators;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(AbsGeneratorNode))]
	class GeneratorNodeEditor: PreviewableNodeEditor {
		private AbsGeneratorNode _generator {
			get {
				return (AbsGeneratorNode)target;
			}
		}

		public override void OnBodyGUI() {
			EditorGUI.BeginChangeCheck();
			OnBodyGUI(true);
			if (EditorGUI.EndChangeCheck()) {
				_generator.OnValueChange();
			}
		}

		public override string GetTitle() {
			return ((AbsGeneratorNode)target).GetTitle();
		}

		public override void ShouldShowPreviewGenerator() {
			PreviewField.Show(_generator);
		}

		public override Color GetTint() {
			return EditorColors.TintNoise;
		}
	}
}