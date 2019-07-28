using Terra.Graph.Fields;
using Terra.Graph.Generators;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(AbsFractalNoiseNode))]
	class FractalNodeEditor: NodeEditor {
		private readonly int OCTAVE_CNT_MAX = 15;
		private readonly int OCTAVE_CNT_MIN = 1;

		private AbsFractalNoiseNode _fractal {
			get {
				return (AbsFractalNoiseNode)target;
			}
		}

		public override void OnBodyGUI() {
			EditorGUI.BeginChangeCheck();
			OnBodyGUI(PreviewableNode.FieldNames);
			if (EditorGUI.EndChangeCheck()) {
				//If OctaveCount falls outside of bounds, reset
				SerializedProperty ocProp = serializedObject.FindProperty("OctaveCount");
				int count = Mathf.Clamp(ocProp.intValue, OCTAVE_CNT_MIN, OCTAVE_CNT_MAX);
				serializedObject.FindProperty("OctaveCount").intValue = count;

				//Since we captured the BeginChangeCheck from AbsGeneratorNode
				//we have to call update OnValueChange() manually
				_fractal.OnValueChange();
			}

			PreviewField.Show(_fractal);
		}

		public override string GetTitle() {
			return _fractal.GetTitle();
		}

		public override Color GetTint() {
			return EditorColors.TintNoise;
		}
	}
}