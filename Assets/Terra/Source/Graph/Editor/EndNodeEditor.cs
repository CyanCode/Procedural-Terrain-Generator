using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Graph.Biome;
using Terra.Graph.Fields;
using Terra.Source.Graph.Editor.Fields;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
    [CustomNodeEditor(typeof(EndNode))]
    class EndNodeEditor : TerraNodeEditor {
        private readonly int NODE_WIDTH = 200;

        private EndNode En => (EndNode) target;

        private SerializedProperty _biomeOrderProperty;

        private ReorderableList _reorderableList;

        private void RenderBiomeOrderEditor() {
            BiomeNode[] biomes = En.GetBiomes();
            _biomeOrderProperty = serializedObject.FindProperty("BiomeOrder");

            if (_biomeOrderProperty.arraySize == biomes.Length) return;
            _biomeOrderProperty.ClearArray();

            for (int i = 0; i < biomes.Length; i++) {
                _biomeOrderProperty.arraySize++;
                _biomeOrderProperty.GetArrayElementAtIndex(i).intValue = i;
            }

            serializedObject.ApplyModifiedProperties();
            _reorderableList = new ReorderableList(
                serializedObject, _biomeOrderProperty, true, false, false, false
            ) {
                drawElementCallback = (rect, index, active, focused) => {
                    BiomeNode selectedBiome =
                        En.GetBiomes()[index];
                    string name = String.IsNullOrEmpty(selectedBiome.Name) ? "Unnamed Biome" : selectedBiome.Name;
                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, GetWidth() - 10, EditorGUIUtility.singleLineHeight),
                        name);
                }
            };
        }

        public override int GetWidth() {
            return NODE_WIDTH;
        }

        public override string GetTitle() {
            return "End";
        }

        public override Color GetTint() {
            return EditorColors.TintEnd;
        }

        public override void OnBodyGUI() {
            RenderBiomeOrderEditor();

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Biomes"), new GUIContent("Biomes"));
            En.BlendStrategy = (BlendStrategy) EditorGUILayout.EnumPopup("Blend Strategy", En.BlendStrategy);

            // Render order of biomes
            if (En.BlendStrategy == BlendStrategy.ORDERED && _reorderableList != null) {
                _reorderableList.DoLayoutList();
            }

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("HeightMap"), new GUIContent("HeightMap"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("TemperatureMap"),
                new GUIContent("TemperatureMap"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("MoistureMap"),
                new GUIContent("MoistureMap"));

            PreviewField.Show(En, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}