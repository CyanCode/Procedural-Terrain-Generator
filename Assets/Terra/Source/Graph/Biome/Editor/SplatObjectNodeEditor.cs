using System;
using Terra.Graph.Biome;
using UnityEditor;
using UnityEngine;
using XNodeEditor;
using Object = UnityEngine.Object;

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

			Rect MakeTexCtrl() {
				Rect diffTexCtrl = EditorGUILayout.GetControlRect(false, texDimens + TEXTURE_PADDING);
				diffTexCtrl.x = GetWidth() - texDimens - NODE_PADDING * 3;
				diffTexCtrl.width = texDimens;
				diffTexCtrl.height = texDimens;
				return diffTexCtrl;
			}

			//Diffuse
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Diffuse ");
			Object diffRef = EditorGUILayout.ObjectField(diffuse.objectReferenceValue, typeof(Texture2D), false);
			EditorGUILayout.EndHorizontal();

			if (diffuse.objectReferenceValue != null) {
				EditorGUI.DrawPreviewTexture(MakeTexCtrl(), (Texture) diffRef);
			}
			
			if (EditorGUI.EndChangeCheck()) {
				diffuse.objectReferenceValue = diffRef;
			}

			//Normal
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Normal ");
			Object normRef = EditorGUILayout.ObjectField(normal.objectReferenceValue, typeof(Texture2D), false);
			EditorGUILayout.EndHorizontal();
			
			if (normal.objectReferenceValue != null) {
				EditorGUI.DrawPreviewTexture(MakeTexCtrl(), (Texture) normRef);
			}
			
			if (EditorGUI.EndChangeCheck()) {
				normal.objectReferenceValue = normRef;
			}

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

		private int GetObjectPickerControlId(bool isDiffuse) {
			int id = EditorGUIUtility.GetControlID(FocusType.Passive) + Node.name.GetHashCode() + Node.position.GetHashCode();
			return isDiffuse ? id + 1 : id - 1;
		}
	}
}