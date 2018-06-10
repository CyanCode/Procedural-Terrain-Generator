using Terra.Graph.Detail;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditor(typeof(GrassNode))]
	public class GrassNodeEditor: NodeEditor {
		private GrassNode Node {
			get {
				return ((GrassNode)target);
			}
		}
		private SerializedProperty Influence {
			get {
				return serializedObject.FindProperty("Influence");
			}
		}
		private SerializedProperty Mask {
			get {
				return serializedObject.FindProperty("Mask");
			}
		}
		private SerializedProperty Output {
			get {
				return serializedObject.FindProperty("Output");
			}
		}

		public override void OnBodyGUI() {
			NodeEditorGUILayout.PropertyField(Output);

			//Display mask and influence
			NodeEditorGUILayout.PropertyField(Mask);

			EditorGUI.BeginChangeCheck();
			NodeEditorGUILayout.PropertyField(Influence);
			if (EditorGUI.EndChangeCheck()) {
				//Clamp Influence between 0 & 1
				Influence.floatValue = Mathf.Clamp01(Influence.floatValue);
			}
			
			Node.Texture = (Texture2D) EditorGUILayout.ObjectField("Texture", Node.Texture, typeof(Texture2D), false);
		}

		public override string GetTitle() {
			return "Grass";
		}
	}
}