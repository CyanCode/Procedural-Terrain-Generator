using System;
using Terra.Graph.Biome;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph.Fields {
    [Serializable]
    public static class DetailField {
        public static void Show(ObjectDetailNodeEditor objectNode) {
            EditorGUI.BeginChangeCheck();

            ObjectDetailNode obj = objectNode.ObjectDetailNode;
            obj.Prefab = (GameObject)
                EditorGUILayout.ObjectField(new GUIContent("Tree"), obj.Prefab, typeof(GameObject), false);

            Show(obj, () => {
                //Translate
                obj.ShowTranslateFoldout = EditorGUILayout.Foldout(obj.ShowTranslateFoldout, "Translate");
                if (obj.ShowTranslateFoldout) {
                    obj.TranslationAmount = EditorGUILayout.Vector3Field("Translate", obj.TranslationAmount);

                    EditorGUILayout.BeginHorizontal();
                    obj.IsRandomTranslate = EditorGUILayout.Toggle("Random", obj.IsRandomTranslate);
                    if (GUILayout.Button("?", GUILayout.Width(25))) {
                        const string msg = "Optionally randomly translate the placed object. " +
                                            "Max and min extents for the random number generator can " +
                                            "be set.";
                        EditorUtility.DisplayDialog("Help - Random Translate", msg, "Close");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (obj.IsRandomTranslate) {
                        EditorGUI.indentLevel = 1;

                        obj.RandomTranslateExtents.Min = EditorGUILayout.Vector3Field("Min", obj.RandomTranslateExtents.Min);
                        obj.RandomTranslateExtents.Max = EditorGUILayout.Vector3Field("Max", obj.RandomTranslateExtents.Max);

                        FitMinMax(ref obj.RandomTranslateExtents.Min, ref obj.RandomTranslateExtents.Max);
                        EditorGUI.indentLevel = 0;
                    }
                }

                //Rotate
                obj.ShowRotateFoldout = EditorGUILayout.Foldout(obj.ShowRotateFoldout, "Rotate");
                if (obj.ShowRotateFoldout) {
                    obj.RotationAmount = EditorGUILayout.Vector3Field("Rotation", obj.RotationAmount);

                    EditorGUILayout.BeginHorizontal();
                    obj.IsRandomRotation = EditorGUILayout.Toggle("Random", obj.IsRandomRotation);
                    if (GUILayout.Button("?", GUILayout.Width(25))) {
                        const string msg = "Optionally randomly rotate the placed object. " +
                                            "Max and min extents for the random number generator can " +
                                            "be set.";
                        EditorUtility.DisplayDialog("Help - Random Rotate", msg, "Close");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (obj.IsRandomRotation) {
                        EditorGUI.indentLevel = 1;

                        obj.RandomRotationExtents.Min = EditorGUILayout.Vector3Field("Min", obj.RandomRotationExtents.Min);
                        obj.RandomRotationExtents.Max = EditorGUILayout.Vector3Field("Max", obj.RandomRotationExtents.Max);

                        FitMinMax(ref obj.RandomRotationExtents.Min, ref obj.RandomRotationExtents.Max);
                        EditorGUI.indentLevel = 0;
                    }
                }

                //Scale
                obj.ShowScaleFoldout = EditorGUILayout.Foldout(obj.ShowScaleFoldout, "Scale");
                if (obj.ShowScaleFoldout) {
                    obj.ScaleAmount = EditorGUILayout.Vector3Field("Scale", obj.ScaleAmount);

                    EditorGUILayout.BeginHorizontal();
                    obj.IsRandomScale = EditorGUILayout.Toggle("Random", obj.IsRandomScale);
                    if (GUILayout.Button("?", GUILayout.Width(25))) {
                        const string msg = "Optionally randomly scale the placed object. " +
                                            "Max and min extents for the random number generator can " +
                                            "be set.";
                        EditorUtility.DisplayDialog("Help - Random Scale", msg, "Close");
                    }
                    EditorGUILayout.EndHorizontal();

                    if (obj.IsRandomScale) {
                        obj.IsUniformScale = EditorGUILayout.Toggle("Scale Uniformly", obj.IsUniformScale);

                        EditorGUI.indentLevel = 1;

                        if (obj.IsUniformScale) {
                            obj.UniformScaleMin = EditorGUILayout.FloatField("Min", obj.UniformScaleMin);
                            obj.UniformScaleMax = EditorGUILayout.FloatField("Max", obj.UniformScaleMax);
                        } else {
                            obj.RandomScaleExtents.Min = EditorGUILayout.Vector3Field("Min", obj.RandomScaleExtents.Min);
                            obj.RandomScaleExtents.Max = EditorGUILayout.Vector3Field("Max", obj.RandomScaleExtents.Max);

                            FitMinMax(ref obj.RandomScaleExtents.Min, ref obj.RandomScaleExtents.Max);
                        }
                        EditorGUI.indentLevel = 0;
                    }
                }

            });

            if (EditorGUI.EndChangeCheck()) {
                objectNode.serializedObject.ApplyModifiedProperties();
            }
        }

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

        private static void Show(DetailNode obj, Action beforeProperties = null) {
            //General
            NodeEditorGUILayout.PortField(obj.GetInputPort("Constraint"));
            NodeEditorGUILayout.PortField(obj.GetInputPort("Modifier"));
            
            obj.DistributionType = (DetailNode.Distribution) EditorGUILayout.EnumPopup("Distribution", obj.DistributionType);
            if (obj.DistributionType == DetailNode.Distribution.PoissonDisc) { 
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

        /// <summary>
        /// Fits the min and max values so that the min Vector3's 
        /// components never exceed the max's.
        /// 
        /// If min > max
        ///   min = max
        /// </summary>
        /// <param name="min">Minimum vector</param>
        /// <param name="max">Maximum vector</param>
        private static void FitMinMax(ref Vector3 min, ref Vector3 max) {
            if (min.x > max.x || min.y > max.y || min.z > max.z) {
                min = new Vector3(min.x > max.x ? max.x : min.x,
                    min.y > max.y ? max.y : min.y,
                    min.z > max.z ? max.z : min.z);
            }
        }
    }
}
