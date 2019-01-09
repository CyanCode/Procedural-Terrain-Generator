using System;
using Terra.Graph.Biome;
using Terra.Structures;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Terra.Graph.Fields {
    [Serializable]
    public static class DetailField {
        public static void Show(TreeNodeEditor treeNode) {
            EditorGUI.BeginChangeCheck();

            TreeDetailNode treeDetail = treeNode.TreeDetailNode;

            treeDetail.Prefab = (GameObject)
                EditorGUILayout.ObjectField(new GUIContent("Tree"), treeDetail.Prefab, typeof(GameObject), false);

            Show(treeDetail, () => {
                treeDetail.RandomRotation = EditorGUILayout.Toggle("Random Rotation", treeDetail.RandomRotation);
                treeDetail.BendFactor = EditorGUILayout.FloatField("Bend Factor", treeDetail.BendFactor);
            });

            if (EditorGUI.EndChangeCheck()) {
                treeNode.serializedObject.ApplyModifiedProperties();
            }
        }

        public static void Show(GrassNodeEditor grassNode) {
            EditorGUI.BeginChangeCheck();

            GrassDetailNode grassDetail = grassNode.GrassDetailNode;

            grassDetail.RenderMode = (DetailRenderMode) EditorGUILayout.EnumPopup("Render Mode", grassDetail.RenderMode);

            if (grassDetail.RenderMode == DetailRenderMode.VertexLit) {
                grassDetail.Prefab = (GameObject)
                    EditorGUILayout.ObjectField(new GUIContent("Prefab"), grassDetail.Prefab, typeof(GameObject), false);
            } else {
                grassDetail.Texture = (Texture2D)
                    EditorGUILayout.ObjectField(new GUIContent("Texture"), grassDetail.Texture, typeof(Texture2D), false);
            }

            Show(grassDetail, () => {
                if (grassDetail.RenderMode != DetailRenderMode.VertexLit) {
                    grassDetail.HealthyColor = EditorGUILayout.ColorField("Healthy Color", grassDetail.HealthyColor);
                    grassDetail.DryColor = EditorGUILayout.ColorField("Dry Color", grassDetail.DryColor);
                }
            });
            
            if (EditorGUI.EndChangeCheck()) {
                grassNode.serializedObject.ApplyModifiedProperties();
            }
        }

        private static void Show(DetailObjectNode obj, Action beforeProperties = null) {
            //General
            NodeEditorGUILayout.PortField(obj.GetInputPort("Constraint"));
            NodeEditorGUILayout.PortField(obj.GetInputPort("Modifier"));
            
            obj.DistributionType = (DetailObjectNode.Distribution) EditorGUILayout.EnumPopup("Distribution", obj.DistributionType);
            if (obj.DistributionType == DetailObjectNode.Distribution.PoissonDisc) { 
                obj.Spread = EditorGUIExtension.MinMaxFloatField("Spread", obj.Spread, 1f, 50f);
            } else {
                obj.UniformResolution = EditorGUIExtension.MinMaxIntField("Resolution", obj.UniformResolution, 2, 1024);
            }

            obj.MaxObjects = EditorGUILayout.IntField("Max Objects", obj.MaxObjects);

            if (beforeProperties != null)
                beforeProperties();

            EditorGUIExtension.DrawMinMax("Width Scale", ref obj.WidthScale.x, ref obj.WidthScale.y);
            EditorGUIExtension.DrawMinMax("Height Scale", ref obj.HeightScale.x, ref obj.HeightScale.y);

            if (obj.MaxObjects < 1)
                obj.MaxObjects = 1;
        }
    }
}
