using UnityEngine;
using XNode;
using XNodeEditor;

namespace Terra.Editor.Graph {
	[CustomNodeEditor(typeof(BiomeCombinerNode))]
	public class BiomeCombinerNodeEditor: PreviewableNode {
		public override Texture2D DidRequestTextureUpdate() {
			throw new System.NotImplementedException();
		}

		public override void OnBodyGUI() {
			if (Event.current.type == EventType.MouseUp) {
				return;
			}

			//Draw Instance Ports
			BiomeCombinerNode bcn = (BiomeCombinerNode)target;
			NodePort[] ports = bcn.GetInstanceInputs();

			foreach (NodePort p in ports) {
				NodeEditorGUILayout.PortField(p);
			}
		}

		public override Color GetTint() {
			return Constants.TintBiome;
		}

		public override string GetTitle() {
			return "Biome Combiner";
		}
	}
}