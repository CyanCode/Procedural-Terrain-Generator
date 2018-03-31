using Terra.CoherentNoise;
using System;
using Terra.Nodes.Generation;
using UnityEditor;
using Assets.Terra.UNEB.Utility;
using UNEB;

namespace Terra.Nodes {
	[Serializable]
	[GraphContextMenuItem("Noise", "End")]
	public class EndNode: Node {
		private NodeInput InputGenerator;

		public override void Init() {
			base.Init();

			InputGenerator = AddInput("Generator");
			FitKnobs();

			NodeGraphEvent.OnAddedNode += NodeAdded;
		}

		/// <summary>
		/// Returns the "final" generator attached to this 
		/// node's input
		/// </summary>
		public Generator GetFinalGenerator() {
			if (InputGenerator != null && InputGenerator.HasOutputConnected() && 
				InputGenerator.GetOutput(0).ParentNode is AbstractGeneratorNode) {
				AbstractGeneratorNode agn = InputGenerator.GetOutput(0).ParentNode as AbstractGeneratorNode;
				return agn.GetGenerator();
			}

			return null;
		}

		public void NodeAdded(NodeGraph graph, Node node) {
			if (!(node is EndNode))
				return;

			int endNodeCount = 0;
			foreach (Node n in graph.nodes) {
				if (n is EndNode) {
					endNodeCount++;
					if (endNodeCount > 1)
						break;
				}
			}

			if (endNodeCount > 1) { //Most recently added Node is EndNode
				graph.Remove(node);
				EditorUtility.DisplayDialog("End Node", "There can only be one End node in the graph at a time", "Okay");
			}
		}
	}
}