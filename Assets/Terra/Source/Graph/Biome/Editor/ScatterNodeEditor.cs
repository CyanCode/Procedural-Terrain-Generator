using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
    [CustomNodeEditor(typeof(ScatterNode))]
    public class ScatterNodeEditor : PreviewableNodeEditor {
        private ScatterNode _scatter {
            get {
                return (ScatterNode)target;
            }
        }

        public override Color GetTint() {
            return EditorColors.TintModifier;
        }

        public override string GetTitle() {
            return "Scatter";
        }

        public override void OnBodyGUI() {
            NodeEditorGUILayout.PortField(_scatter.GetOutputPort("Output"));
            NodeEditorGUILayout.PortField(_scatter.GetInputPort("DetailNode"));

            //Object count min max
            EditorGUI.BeginChangeCheck();

            EditorGUIExtension.DrawMinMax("Distance", ref _scatter.DistanceMin, ref _scatter.DistanceMax);

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void ShouldShowPreviewGenerator() { }
    }
}