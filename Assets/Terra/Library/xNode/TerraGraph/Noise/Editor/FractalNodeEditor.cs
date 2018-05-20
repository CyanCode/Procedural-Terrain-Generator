using Terra.Graph.Noise.Generation;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditor(typeof(AbsFractalNoiseNode))]
	class FractalNodeEditor: NodeEditor {
		private readonly int OCTAVE_CNT_MAX = 15;
		private readonly int OCTAVE_CNT_MIN = 1;

		public override void OnBodyGUI() {
			EditorGUI.BeginChangeCheck();
			base.OnBodyGUI();
			if (EditorGUI.EndChangeCheck()) {
				//If OctaveCount falls outside of bounds, reset
				SerializedProperty ocProp = serializedObject.FindProperty("OctaveCount");
				int count = Mathf.Clamp(ocProp.intValue, OCTAVE_CNT_MIN, OCTAVE_CNT_MAX);
				serializedObject.FindProperty("OctaveCount").intValue = count;

				//Since we captured the BeginChangeCheck from AbsGeneratorNode
				//we have to call update OnValueChange() manually
				AbsFractalNoiseNode fnn = ((AbsFractalNoiseNode)target);
				fnn.OnValueChange();
			}
		}
	}
}