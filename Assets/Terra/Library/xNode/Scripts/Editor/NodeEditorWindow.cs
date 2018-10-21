﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace XNodeEditor {
    [InitializeOnLoad]
    public partial class NodeEditorWindow : EditorWindow {
        public static NodeEditorWindow current;

        /// <summary> Stores node positions for all nodePorts. </summary>
        public Dictionary<XNode.NodePort, Rect> portConnectionPoints { get { return _portConnectionPoints; } }
        private Dictionary<XNode.NodePort, Rect> _portConnectionPoints = new Dictionary<XNode.NodePort, Rect>();
        public Dictionary<XNode.Node, float> nodeWidths { get { return _nodeWidths; } }
        private Dictionary<XNode.Node, float> _nodeWidths = new Dictionary<XNode.Node, float>();
        public XNode.NodeGraph graph;
        public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
        private Vector2 _panOffset;
        public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, 1f, 5f); Repaint(); } }
        private float _zoom = 1;

        void OnFocus() {
            AssetDatabase.SaveAssets();
            current = this;
        }

        partial void OnEnable();
        /// <summary> Create editor window </summary>
        public static NodeEditorWindow Init() {
            NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
            w.titleContent = new GUIContent("Node Editor");
            w.wantsMouseMove = true;
            w.Show();
            return w;
        }

        public void Save() {
            if (AssetDatabase.Contains(graph)) {
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
            } else SaveAs();
        }

        public void SaveAs() {
            string path = EditorUtility.SaveFilePanelInProject("Save NodeGraph", "NewNodeGraph", "asset", "");
            if (string.IsNullOrEmpty(path)) return;
            else {
                XNode.NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<XNode.NodeGraph>(path);
                if (existingGraph != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(graph, path);
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
            }
        }

        private void DraggableWindow(int windowID) {
            GUI.DragWindow();
        }

        public Vector2 WindowToGridPosition(Vector2 windowPosition) {
            return (windowPosition - (position.size * 0.5f) - (panOffset / zoom)) * zoom;
        }

        public Vector2 GridToWindowPosition(Vector2 gridPosition) {
            return (position.size * 0.5f) + (panOffset / zoom) + (gridPosition / zoom);
        }

        public Rect GridToWindowRect(Rect gridRect) {
            gridRect.position = GridToWindowPositionNoClipped(gridRect.position);
            return gridRect;
        }

        public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition) {
            Vector2 center = position.size * 0.5f;
            float xOffset = (center.x * zoom + (panOffset.x + gridPosition.x));
            float yOffset = (center.y * zoom + (panOffset.y + gridPosition.y));
            return new Vector2(xOffset, yOffset);
        }

        public void SelectNode(XNode.Node node, bool add) {
            if (add) {
                List<Object> selection = new List<Object>(Selection.objects);
                selection.Add(node);
                Selection.objects = selection.ToArray();
            } else Selection.objects = new Object[] { node };
        }

        public void DeselectNode(XNode.Node node) {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(node);
            Selection.objects = selection.ToArray();
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line) {
            XNode.NodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as XNode.NodeGraph;
            if (nodeGraph != null) {
                NodeEditorWindow w = GetWindow(typeof(NodeEditorWindow), false, "Node Editor", true) as NodeEditorWindow;
                w.wantsMouseMove = true;
                w.graph = nodeGraph;
                return true;
            }
            return false;
        }

		public static void TryOpen(XNode.NodeGraph graph) {
			NodeEditorWindow w = Init();
			w.wantsMouseMove = true;
			
			if (graph != null) {
				w.graph = graph;
			}
		}

        /// <summary> Repaint all open NodeEditorWindows. </summary>
        public static void RepaintAll() {
            NodeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
            for (int i = 0; i < windows.Length; i++) {
                windows[i].Repaint();
            }
        }
    }
}