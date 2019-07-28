using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
    [CustomNodeEditor(typeof(PropagateNode))]
    public class PropagateNodeEditor : PreviewableNodeEditor {
        private PropagateNode _propagate {
            get {
                return (PropagateNode)target;
            }
        }

        public override Color GetTint() {
            return EditorColors.TintModifier;
        }

        public override string GetTitle() {
            return "Propagate";
        }

        public override void OnBodyGUI() {
            NodeEditorGUILayout.PortField(_propagate.GetOutputPort("Output"));
            NodeEditorGUILayout.PortField(_propagate.GetInputPort("DetailNode"));

            //Object count min max
            EditorGUI.BeginChangeCheck();

            EditorGUIExtension.DrawMinMax("Distance", ref _propagate.DistanceMin, ref _propagate.DistanceMax);
            EditorGUIExtension.DrawMinMax("Object Count", ref _propagate.ObjectCountMin, ref _propagate.ObjectCountMax);

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void ShouldShowPreviewGenerator() { }
    }
}