using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
    [CustomNodeEditor(typeof(ObjectDetailNode))]
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
            return "Object";
        }

        public override void ShouldShowPreviewGenerator() { }
    }
}