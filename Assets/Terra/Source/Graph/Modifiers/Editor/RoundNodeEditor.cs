using Terra.Graph.Fields;
using Terra.Graph.Generators.Modifiers;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(RoundNode))]
	class RoundNodeEditor: ModNodeEditor {
		private RoundNode Round {
			get {
				return (RoundNode)target;
			}
		}

		public override void OnBodyGUI() {
			//Draw default except for cutoff and preview
			base.OnBodyGUI(false, new[] { "Cutoff" });

			SerializedProperty prop = serializedObject.FindProperty("Cutoff");
			Rect ctrl = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
			var label = EditorGUI.BeginProperty(ctrl, new GUIContent("Cutoff"), prop);

			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUI.Slider(ctrl, label, prop.floatValue, 0f, 1f);
			if (EditorGUI.EndChangeCheck()) {
				prop.floatValue = newValue;
			}
			EditorGUI.EndProperty();

			PreviewField.Show(Round);
		}

		public override void ShouldShowPreviewGenerator() { }
	}
}