using Terra.Graph.Generators.Modifiers;
using UnityEditor;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(ScaleNode))]
	class ScaleNodeEditor: NodeEditor {
		private ScaleNode Sn {
			get {
				return (ScaleNode)target;
			}
		}

		public override void OnBodyGUI() {
			//Because we're not calling base.OnBodyGUI
			//change checks need to be performed manually
			EditorGUI.BeginChangeCheck();

			//IO
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Output"));
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Generator"));

			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Uniform"));

			if (Sn.Uniform) {
				NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Amount"));
			} else {
				NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Factor"));
			}

			if (EditorGUI.EndChangeCheck()) {
				Sn.OnValueChange();
			}
		}
	}
}