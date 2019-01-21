using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    /// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>

    [CustomNodeEditor(typeof(XNode.Node))]
    public class NodeEditor : XNodeEditor.Internal.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, XNode.Node> {

        /// <summary> Fires every whenever a node was modified through the editor </summary>
        public static Action<XNode.Node> onUpdateNode;
        public static Dictionary<XNode.NodePort, Vector2> portPositions;

        /// <summary> Draws the node GUI.</summary>
        /// <param name="portPositions">Port handle positions need to be returned to the NodeEditorWindow </param>
        public void OnNodeGUI() {
	        SetEditorStyle();

            OnHeaderGUI();
//            if (target.IsExpanded)
            OnBodyGUI();

            ResetEditorStyle();
        }

        public virtual void OnHeaderGUI() {
            GUI.color = Color.white;
			string title = GetTitle();
            GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
//
//            const int buttonWidth = 25;
//            Rect ctrl = new Rect {
//                y = 15,
//                x = GetWidth() - buttonWidth - 5,
//                width = buttonWidth,
//                height = 8
//            };
//
//            var up = Resources.Load<Texture2D>("arrow_up");
//            var down = Resources.Load<Texture2D>("arrow_down");
//            var show = target.IsExpanded ? up : down;
//            if (GUI.Button(ctrl, show, GUIStyle.none)) {
//                target.IsExpanded = !target.IsExpanded;
//            }
        }

        /// <summary> Draws standard field editors for all public fields </summary>
        public virtual void OnBodyGUI() {
            string[] excludes = { "m_Script", "graph", "position", "ports" };
            portPositions = new Dictionary<XNode.NodePort, Vector2>();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            EditorGUIUtility.labelWidth = 84;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                if (excludes.Contains(iterator.name)) continue;
                NodeEditorGUILayout.PropertyField(iterator, true);
            }
        }

		public void OnBodyGUI(string[] excludes) {
			List<string> excludesInit = new List<string>();
			excludesInit.AddRange(excludes);
			excludesInit.AddRange(new[] { "m_Script", "graph", "position", "ports" });

			portPositions = new Dictionary<XNode.NodePort, Vector2>();

			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;
			EditorGUIUtility.labelWidth = 84;
			while (iterator.NextVisible(enterChildren)) {
				enterChildren = false;
				if (excludesInit.Contains(iterator.name)) continue;
				NodeEditorGUILayout.PropertyField(iterator, true);
			}
		}

        public virtual int GetWidth() {
            return 208;
        }

        public virtual Color GetTint() {
            Type type = target.GetType();
            if (NodeEditorWindow.nodeTint.ContainsKey(type)) return NodeEditorWindow.nodeTint[type];
            else return Color.white;
        }

		public virtual string GetTitle() {
			return target.name;
		}

        /// <summary>
        /// Sets the editor style for Terra's editor
        /// </summary>
        private void SetEditorStyle() {
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.foldoutPreDrop.normal.textColor = Color.white;
            EditorStyles.foldout.normal.textColor = Color.white;
            EditorStyles.foldout.active.textColor = Color.white;
            EditorStyles.foldout.onNormal.textColor = Color.white;
        }

        /// <summary>
        /// Resets the editor style to default
        /// </summary>
        private void ResetEditorStyle() {
            EditorStyles.label.normal.textColor = Color.black;
            EditorStyles.foldoutPreDrop.normal.textColor = Color.black;
            EditorStyles.foldout.normal.textColor = Color.black;
            EditorStyles.foldout.active.textColor = Color.black;
            EditorStyles.objectField.normal.textColor = Color.black;
            EditorStyles.foldout.onNormal.textColor = Color.black;
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeEditorAttribute : Attribute,
            XNodeEditor.Internal.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, XNode.Node>.INodeEditorAttrib {
            private Type inspectedType;
            /// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            /// <param name="contextMenuName">Path to the node</param>
            public CustomNodeEditorAttribute(Type inspectedType) {
                this.inspectedType = inspectedType;
            }

            public Type GetInspectedType() {
                return inspectedType;
            }
        }
    }
}