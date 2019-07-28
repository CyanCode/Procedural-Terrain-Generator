using Terra.Graph.Biome;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(SplatDetailNode))]
	public class SplatObjectNodeEditor: NodeEditor {
		private const int TEXTURE_PADDING = 8;
		private const int NODE_PADDING = 6;

		private SplatDetailNode Node {
			get {
				return (SplatDetailNode)target;
			}
		}

		public override void OnBodyGUI() {
			//Output
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Output"));

            NodeEditorGUILayout.PortField(Node.GetPort("Constraint"));

			//Textures
			SerializedProperty diffuse = serializedObject.FindProperty("Diffuse");
			SerializedProperty normal = serializedObject.FindProperty("Normal");

			int texDimens = GetWidth() / 2 - TEXTURE_PADDING - NODE_PADDING * 2;
			Rect ctrl = EditorGUILayout.GetControlRect(false, texDimens);
			ctrl.width = texDimens;

			//Diffuse & Normal
			EditorGUI.BeginProperty(ctrl, GUIContent.none, diffuse);
			EditorGUI.BeginChangeCheck();
			Object diffRef = EditorGUI.ObjectField(ctrl, diffuse.objectReferenceValue, typeof(Texture2D), false);
			if (EditorGUI.EndChangeCheck()) {
				diffuse.objectReferenceValue = diffRef;
			}
			EditorGUI.EndProperty();

			ctrl.x += texDimens + TEXTURE_PADDING;

			EditorGUI.BeginProperty(ctrl, GUIContent.none, normal);
			EditorGUI.BeginChangeCheck();
			Object normRef = EditorGUI.ObjectField(ctrl, normal.objectReferenceValue, typeof(Texture2D), false);
			if (EditorGUI.EndChangeCheck()) {
				normal.objectReferenceValue = normRef;
			}
			EditorGUI.EndProperty();

			//Labels
			GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.normal.textColor = Color.white;
			centeredStyle.alignment = TextAnchor.UpperCenter;

			ctrl = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
			ctrl.width = texDimens;

			EditorGUI.LabelField(ctrl, "Diffuse", centeredStyle);
			ctrl.x += texDimens + TEXTURE_PADDING;
			EditorGUI.LabelField(ctrl, "Normal", centeredStyle);

			//Tiling/Offset
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Tiling"));
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Offset"));

            //Blend
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Blend"));
		}

		public override string GetTitle() {
			return "Splat Object";
		}

		public override Color GetTint() {
			return EditorColors.TintValue;
		}
	}
}