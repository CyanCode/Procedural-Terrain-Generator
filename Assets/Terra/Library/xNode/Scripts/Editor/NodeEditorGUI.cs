﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    /// <summary> Contains GUI methods </summary>
    public partial class NodeEditorWindow {
        public NodeGraphEditor graphEditor;
        private List<UnityEngine.Object> selectionCache;

        private void OnGUI() {
            Event e = Event.current;
            Matrix4x4 m = GUI.matrix;
            if (graph == null) return;
            graphEditor = NodeGraphEditor.GetEditor(graph);

            Controls();

            DrawGrid(position, zoom, panOffset);
            DrawConnections();
            DrawDraggedConnection();
            DrawNodes();
            DrawBox();
            DrawTooltip();

            GUI.matrix = m;
        }

        public static void BeginZoomed(Rect rect, float zoom) {
            GUI.EndClip();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
            Vector4 padding = new Vector4(0, 22, 0, 0);
            padding *= zoom;
            GUI.BeginClip(new Rect(-((rect.width * zoom) - rect.width) * 0.5f, -(((rect.height * zoom) - rect.height) * 0.5f) + (22 * zoom),
                rect.width * zoom,
                rect.height * zoom));
        }

        public static void EndZoomed(Rect rect, float zoom) {
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector3 offset = new Vector3(
                (((rect.width * zoom) - rect.width) * 0.5f),
                (((rect.height * zoom) - rect.height) * 0.5f) + (-22 * zoom) + 22,
                0);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        }

        public void DrawGrid(Rect rect, float zoom, Vector2 panOffset) {

            rect.position = Vector2.zero;

            Vector2 center = rect.size / 2f;
            Texture2D gridTex = graphEditor.GetGridTexture();
            Texture2D crossTex = graphEditor.GetSecondaryGridTexture();

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }

        public void DrawBox() {
            if (currentActivity == NodeActivity.DragGrid) {
                Vector2 curPos = WindowToGridPosition(Event.current.mousePosition);
                Vector2 size = curPos - dragBoxStart;
                Rect r = new Rect(dragBoxStart, size);
                r.position = GridToWindowPosition(r.position);
                r.size /= zoom;
                Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));
            }
        }

        public static bool DropdownButton(string name, float width) {
            return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
        }

        /// <summary> Show right-click context menu for selected nodes </summary>
        public void ShowNodeContextMenu() {
            GenericMenu contextMenu = new GenericMenu();
            // If only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is XNode.Node) {
                XNode.Node node = Selection.activeObject as XNode.Node;
                contextMenu.AddItem(new GUIContent("Move To Top"), false, () => {
                    int index;
                    while ((index = graph.nodes.IndexOf(node)) != graph.nodes.Count - 1) {
                        graph.nodes[index] = graph.nodes[index + 1];
                        graph.nodes[index + 1] = node;
                    }
                });
            }

            contextMenu.AddItem(new GUIContent("Duplicate"), false, DublicateSelectedNodes);
            contextMenu.AddItem(new GUIContent("Remove"), false, RemoveSelectedNodes);

            // If only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is XNode.Node) {
                XNode.Node node = Selection.activeObject as XNode.Node;
                AddCustomContextMenuItems(contextMenu, node);
            }

            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        /// <summary> Show right-click context menu for current graph </summary>
        void ShowGraphContextMenu() {
            GenericMenu contextMenu = new GenericMenu();
            Vector2 pos = WindowToGridPosition(Event.current.mousePosition);
            for (int i = 0; i < nodeTypes.Length; i++) {
                Type type = nodeTypes[i];

                //Get node context menu path
                string path = graphEditor.GetNodePath(type);
                if (path == null) continue;

                contextMenu.AddItem(new GUIContent(path), false, () => {
                    CreateNode(type, pos);
                });
            }
            contextMenu.AddSeparator("");
            contextMenu.AddItem(new GUIContent("Preferences"), false, () => OpenPreferences());
            AddCustomContextMenuItems(contextMenu, graph);
            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        void AddCustomContextMenuItems(GenericMenu contextMenu, object obj) {
            KeyValuePair<ContextMenu, System.Reflection.MethodInfo>[] items = GetContextMenuMethods(obj);
            if (items.Length != 0) {
                contextMenu.AddSeparator("");
                for (int i = 0; i < items.Length; i++) {
                    KeyValuePair<ContextMenu, System.Reflection.MethodInfo> kvp = items[i];
                    contextMenu.AddItem(new GUIContent(kvp.Key.menuItem), false, () => kvp.Value.Invoke(obj, null));
                }
            }
        }

        /// <summary> Draw a bezier from startpoint to endpoint, both in grid coordinates </summary>
        public void DrawConnection(Vector2 startPoint, Vector2 endPoint, Color col) {
            startPoint = GridToWindowPosition(startPoint);
            endPoint = GridToWindowPosition(endPoint);

            switch (NodeEditorPreferences.GetSettings().noodleType) {
                case NodeEditorPreferences.NoodleType.Curve:
                    Vector2 startTangent = startPoint;
                    if (startPoint.x < endPoint.x) startTangent.x = Mathf.LerpUnclamped(startPoint.x, endPoint.x, 0.7f);
                    else startTangent.x = Mathf.LerpUnclamped(startPoint.x, endPoint.x, -0.7f);

                    Vector2 endTangent = endPoint;
                    if (startPoint.x > endPoint.x) endTangent.x = Mathf.LerpUnclamped(endPoint.x, startPoint.x, -0.7f);
                    else endTangent.x = Mathf.LerpUnclamped(endPoint.x, startPoint.x, 0.7f);
                    Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, col, null, 4);
                    break;
                case NodeEditorPreferences.NoodleType.Line:
                    Handles.color = col;
                    Handles.DrawAAPolyLine(5, startPoint, endPoint);
                    break;
                case NodeEditorPreferences.NoodleType.Angled:
                    Handles.color = col;
                    if (startPoint.x <= endPoint.x - (50 / zoom)) {
                        float midpoint = (startPoint.x + endPoint.x) * 0.5f;
                        Vector2 start_1 = startPoint;
                        Vector2 end_1 = endPoint;
                        start_1.x = midpoint;
                        end_1.x = midpoint;
                        Handles.DrawAAPolyLine(5, startPoint, start_1);
                        Handles.DrawAAPolyLine(5, start_1, end_1);
                        Handles.DrawAAPolyLine(5, end_1, endPoint);
                    } else {
                        float midpoint = (startPoint.y + endPoint.y) * 0.5f;
                        Vector2 start_1 = startPoint;
                        Vector2 end_1 = endPoint;
                        start_1.x += 25 / zoom;
                        end_1.x -= 25 / zoom;
                        Vector2 start_2 = start_1;
                        Vector2 end_2 = end_1;
                        start_2.y = midpoint;
                        end_2.y = midpoint;
                        Handles.DrawAAPolyLine(5, startPoint, start_1);
                        Handles.DrawAAPolyLine(5, start_1, start_2);
                        Handles.DrawAAPolyLine(5, start_2, end_2);
                        Handles.DrawAAPolyLine(5, end_2, end_1);
                        Handles.DrawAAPolyLine(5, end_1, endPoint);
                    }
                    break;
            }
        }

        /// <summary> Draws all connections </summary>
        public void DrawConnections() {
            foreach (XNode.Node node in graph.nodes) {
                //If a null node is found, return. This can happen if the nodes associated script is deleted. It is currently not possible in Unity to delete a null asset.
                if (node == null) continue;

                foreach (XNode.NodePort output in node.Outputs) {
                    //Needs cleanup. Null checks are ugly
                    if (!portConnectionPoints.ContainsKey(output)) continue;
                    Vector2 from = _portConnectionPoints[output].center;
                    for (int k = 0; k < output.ConnectionCount; k++) {

                        XNode.NodePort input = output.GetConnection(k);
                        if (input == null) continue; //If a script has been updated and the port doesn't exist, it is removed and null is returned. If this happens, return.
                        if (!input.IsConnectedTo(output)) input.Connect(output);
                        if (!_portConnectionPoints.ContainsKey(input)) continue;
                        Vector2 to = _portConnectionPoints[input].center;
                        Color connectionColor = graphEditor.GetTypeColor(output.ValueType);
                        DrawConnection(from, to, connectionColor);
                    }
                }
            }
        }

        private void DrawNodes() {
            Event e = Event.current;
            if (e.type == EventType.Layout) {
                selectionCache = new List<UnityEngine.Object>(Selection.objects);
            }
            if (e.type == EventType.Repaint) {
                portConnectionPoints.Clear();
                nodeWidths.Clear();
            }

            //Active node is hashed before and after node GUI to detect changes
            int nodeHash = 0;
            System.Reflection.MethodInfo onValidate = null;
            if (Selection.activeObject != null && Selection.activeObject is XNode.Node) {
                onValidate = Selection.activeObject.GetType().GetMethod("OnValidate");
                if (onValidate != null) nodeHash = Selection.activeObject.GetHashCode();
            }

            BeginZoomed(position, zoom);

            Vector2 mousePos = Event.current.mousePosition;

            if (e.type != EventType.Layout) {
                hoveredNode = null;
                hoveredPort = null;
            }

            List<UnityEngine.Object> preSelection = preBoxSelection != null ? new List<UnityEngine.Object>(preBoxSelection) : new List<UnityEngine.Object>();

            //Save guiColor so we can revert it
            Color guiColor = GUI.color;
            for (int n = 0; n < graph.nodes.Count; n++) {
                // Skip null nodes. The user could be in the process of renaming scripts, so removing them at this point is not advisable.
                if (graph.nodes[n] == null) continue;
                if (n >= graph.nodes.Count) return;
                XNode.Node node = graph.nodes[n];

                NodeEditor nodeEditor = NodeEditor.GetEditor(node);
                NodeEditor.portPositions = new Dictionary<XNode.NodePort, Vector2>();

                //Get node position
                Vector2 nodePos = GridToWindowPositionNoClipped(node.position);

                GUILayout.BeginArea(new Rect(nodePos, new Vector2(nodeEditor.GetWidth(), 4000)));

                bool selected = selectionCache.Contains(graph.nodes[n]);

                if (selected) {
                    GUIStyle style = new GUIStyle(NodeEditorResources.styles.nodeBody);
                    GUIStyle highlightStyle = new GUIStyle(NodeEditorResources.styles.nodeHighlight);
                    highlightStyle.padding = style.padding;
                    style.padding = new RectOffset();
                    GUI.color = nodeEditor.GetTint();
                    GUILayout.BeginVertical(new GUIStyle(style));
                    GUI.color = NodeEditorPreferences.GetSettings().highlightColor;
                    GUILayout.BeginVertical(new GUIStyle(highlightStyle));
                } else {
                    GUIStyle style = NodeEditorResources.styles.nodeBody;
                    GUI.color = nodeEditor.GetTint();
                    GUILayout.BeginVertical(new GUIStyle(style));
                }

                GUI.color = guiColor;
                EditorGUI.BeginChangeCheck();

                //Draw node contents
                nodeEditor.OnNodeGUI();

                //Apply
                nodeEditor.serializedObject.ApplyModifiedProperties();

                //If user changed a value, notify other scripts through onUpdateNode
                if (EditorGUI.EndChangeCheck()) {
                    if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                }

                if (e.type == EventType.Repaint) {
                    nodeWidths.Add(node, nodeEditor.GetWidth());

                    foreach (var kvp in NodeEditor.portPositions) {
                        Vector2 portHandlePos = kvp.Value;
                        portHandlePos += node.position;
                        Rect rect = new Rect(portHandlePos.x - 8, portHandlePos.y - 8, 16, 16);
                        portConnectionPoints.Add(kvp.Key, rect);
                    }
                }

                GUILayout.EndVertical();
                if (selected) GUILayout.EndVertical();

                if (e.type != EventType.Layout) {
                    //Check if we are hovering this node
                    Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
                    Rect windowRect = new Rect(nodePos, nodeSize);
                    if (windowRect.Contains(mousePos)) hoveredNode = node;

                    //If dragging a selection box, add nodes inside to selection
                    if (currentActivity == NodeActivity.DragGrid) {
                        Vector2 startPos = GridToWindowPositionNoClipped(dragBoxStart);
                        Vector2 size = mousePos - startPos;
                        if (size.x < 0) { startPos.x += size.x; size.x = Mathf.Abs(size.x); }
                        if (size.y < 0) { startPos.y += size.y; size.y = Mathf.Abs(size.y); }
                        Rect r = new Rect(startPos, size);
                        if (windowRect.Overlaps(r)) preSelection.Add(node);
                    }

                    //Check if we are hovering any of this nodes ports
                    //Check input ports
                    foreach (XNode.NodePort input in node.Inputs) {
                        //Check if port rect is available
                        if (!portConnectionPoints.ContainsKey(input)) continue;
                        Rect r = GridToWindowRect(portConnectionPoints[input]);
                        if (r.Contains(mousePos)) hoveredPort = input;
                    }
                    //Check all output ports
                    foreach (XNode.NodePort output in node.Outputs) {
                        //Check if port rect is available
                        if (!portConnectionPoints.ContainsKey(output)) continue;
                        Rect r = GridToWindowRect(portConnectionPoints[output]);
                        if (r.Contains(mousePos)) hoveredPort = output;
                    }
                }

                GUILayout.EndArea();
            }

            if (e.type != EventType.Layout && currentActivity == NodeActivity.DragGrid) Selection.objects = preSelection.ToArray();
            EndZoomed(position, zoom);

            //If a change in hash is detected in the selected node, call OnValidate method. 
            //This is done through reflection because OnValidate is only relevant in editor, 
            //and thus, the code should not be included in build.
            if (nodeHash != 0) {
                if (onValidate != null && nodeHash != Selection.activeObject.GetHashCode()) onValidate.Invoke(Selection.activeObject, null);
            }
        }

        private void DrawTooltip() {
            if (hoveredPort != null) {
                Type type = hoveredPort.ValueType;
                GUIContent content = new GUIContent();
                content.text = type.PrettyName();
                if (hoveredPort.IsStatic && hoveredPort.IsOutput) {
                    object obj = hoveredPort.node.GetValue(hoveredPort);
                    content.text += " = " + (obj != null ? obj.ToString() : "null");
                }
                Vector2 size = NodeEditorResources.styles.tooltip.CalcSize(content);
                Rect rect = new Rect(Event.current.mousePosition - (size), size);
                EditorGUI.LabelField(rect, content, NodeEditorResources.styles.tooltip);
                Repaint();
            }
        }
    }
}