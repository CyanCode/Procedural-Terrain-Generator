using CoherentNoise;
using System;
using Terra.GraphEditor;
using Terra.GraphEditor.Sockets;
using Terra.Nodes.Generation;
using UnityEditor;
using UnityEngine;

namespace Terra.Nodes {
	[Serializable]
	[GraphContextMenuItem("Noise", "End")]
	public class EndNode: Node {
		[NonSerialized]
		private Rect LabelEnd;
		[NonSerialized]
		private InputSocket InputSocketGenerator;

		public EndNode(int id, Graph parent) : base(id, parent) {
			LabelEnd = new Rect(6, 0, 100, BonConfig.SocketSize);
			InputSocketGenerator = new InputSocket(this, typeof(AbstractGeneratorNode));
			Height = 40;

			Sockets.Add(InputSocketGenerator);
			EventManager.OnAddedNode += NodeAdded;
		}

		public override void OnGUI() {
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
			GUI.Label(LabelEnd, "Last Generator");
		}

		public override void Update() { }

		public Generator GetFinalGenerator() {
			Generator gen = AbstractGeneratorNode.GetInputGenerator(InputSocketGenerator);
			gen.ScaleShift(0f, 1f);

			return gen;
		}

		public void NodeAdded(Graph graph, Node node) {
			if (!(node is EndNode))
				return;

			int endNodeCount = 0;
			foreach (Node n in graph.GetNodes()) {
				if (n is EndNode) {
					endNodeCount++;
					if (endNodeCount > 1)
						break;
				}
			}

			if (endNodeCount > 1) { //Most recently added Node is EndNode
				graph.RemoveNode(node);
				EditorUtility.DisplayDialog("End Node", "There can only be one End node in the graph at a time", "Okay");
			}
		}
	}
}