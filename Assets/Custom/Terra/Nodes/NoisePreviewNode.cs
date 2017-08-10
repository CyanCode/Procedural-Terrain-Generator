using Terra.CoherentNoise;
using Terra.CoherentNoise.Texturing;
using System;
using Terra.GraphEditor;
using Terra.GraphEditor.Sockets;
using Terra.Nodes.Generation;
using UnityEngine;

namespace Terra.Nodes {
	[Serializable]
	[GraphContextMenuItem("", "Preview")]
	public class NoisePreviewNode: Node {
		[NonSerialized]
		private Rect LabelGenerator;
		[NonSerialized]
		private InputSocket InputSocketGenerator;
		[NonSerialized]
		private bool TextureNeedsUpdating;
		[NonSerialized]
		private Texture Texture;

		public NoisePreviewNode(int id, Graph parent) : base(id, parent) {
			LabelGenerator = new Rect(6, 100, 90, BonConfig.SocketSize);
			InputSocketGenerator = new InputSocket(this, typeof(AbstractGeneratorNode));

			Height = 140;
			SocketTopOffsetInput = 100;

			Sockets.Add(InputSocketGenerator);
			EventManager.OnChangedNode += NodeUpdated;
			EventManager.OnAddedNode += NodeUpdated;
			EventManager.OnLinkEdge += NodeUpdated;
			EventManager.OnFocusGraph += GraphFocused;
		}

		public override void OnGUI() {
			if (TextureNeedsUpdating) {
				Texture = GetNoiseTexture();
				TextureNeedsUpdating = false;
			}

			if (Texture != null)
				GUI.DrawTexture(new Rect(6, 0, 88, 88), Texture);

			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
			GUI.Label(LabelGenerator, "Generator");
		}

		public override void Update() {

		}

		private void NodeUpdated(Graph graph, Node node) {
			if (InputSocketGenerator.CanGetResult()) {
				TextureNeedsUpdating = true;
			}
		}

		private void NodeUpdated(Graph graph, Edge edge) {
			NodeUpdated(graph, (Node)null);
		}

		private void GraphFocused(Graph graph) {
			NodeUpdated(graph, (Node)null);
		}

		private Texture GetNoiseTexture() {
			Generator noise = AbstractGeneratorNode.GetInputGenerator(InputSocketGenerator);
			Texture PreviewTexture = TextureMaker.MonochromeTexture(100, 100, noise);

			return PreviewTexture;
		}
	}
}