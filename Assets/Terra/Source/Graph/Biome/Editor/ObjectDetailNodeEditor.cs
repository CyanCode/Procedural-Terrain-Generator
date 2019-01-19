using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
    [CustomEditor(typeof(ObjectDetailNode))]
    public class ObjectDetailNodeEditor : PreviewableNodeEditor {
        public ObjectDetailNode ObjectDetailNode {
            get {
                return (ObjectDetailNode)target;
            }
        }

        public override void OnBodyGUI() {
            NodeEditorGUILayout.PortField(ObjectDetailNode.GetOutputPort("Output"));
            DetailField.Show(this);
            PreviewField.Show(ObjectDetailNode);
        }

        public override Color GetTint() {
            return EditorColors.TintValue;
        }

        public override string GetTitle() {
            return "Grass";
        }
        public override void ShouldShowPreviewGenerator() { }
    }
}