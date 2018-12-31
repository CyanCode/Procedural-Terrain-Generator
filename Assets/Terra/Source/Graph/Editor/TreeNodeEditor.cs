using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(TreeNode))]
	public class TreeNodeEditor: PreviewableNodeEditor {
		private TreeNode _treeNode {
			get {
				return (TreeNode)target;
			}
		}

	    public override void OnBodyGUI() {
            NodeEditorGUILayout.PortField(_treeNode.GetOutputPort("Output"));
			PlaceableObjectField.Show(this);
	        PreviewField.Show(_treeNode);
        }

		public override void ShouldShowPreviewGenerator() { }

		public override Color GetTint() {
			return Constants.TintValue;
		}

		public override string GetTitle() {
			return "Tree";
		}
	}
}