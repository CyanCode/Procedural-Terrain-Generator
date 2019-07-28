using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(GrassDetailNode))]
	public class GrassNodeEditor : PreviewableNodeEditor {
		public GrassDetailNode GrassDetailNode {
			get {
				return (GrassDetailNode)target;
			}
		}
        
	    public override void OnBodyGUI() {
            NodeEditorGUILayout.PortField(GrassDetailNode.GetOutputPort("Output"));
			DetailField.Show(this);
	        PreviewField.Show(GrassDetailNode);
        }

		public override void ShouldShowPreviewGenerator() { }

		public override Color GetTint() {
			return EditorColors.TintValue;
		}

		public override string GetTitle() {
			return "Grass";
		}
	}
}