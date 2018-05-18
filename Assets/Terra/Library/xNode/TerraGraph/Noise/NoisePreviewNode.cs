using Terra.CoherentNoise.Texturing;
using Terra.Graph.Noise.Generation;
using UnityEngine;
using XNode;

namespace Terra.Graph.Noise {
	[CreateNodeMenu("Preview")]
	public class NoisePreviewNode: XNode.Node {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Input;

		public Texture PreviewTexture { get; private set; }
		public bool TextureNeedsUpdating { get; private set; }

		public override void OnCreateConnection(NodePort from, NodePort to) {
			base.OnCreateConnection(from, to);

			UpdateTexture();
		}

		public override void OnRemoveConnection(NodePort port) {
			base.OnRemoveConnection(port);

			PreviewTexture = null;
		}

		/// <summary>
		/// Queues the current texture to be regenerated
		/// </summary>
		public void InvalidateTexture() {
			TextureNeedsUpdating = true;
		}

		/// <summary>
		/// Checks whether there is an active preview Texture instance
		/// </summary>
		public bool HasTexture() {
			return PreviewTexture != null;
		}

		/// <summary>
		/// Generates a noise texture for the currently attached InputSocket
		/// </summary>
		/// <returns>If an error occured while retrieving the generator, 
		/// null is returned. Otherwise the Texture is returned.</returns>
		public void UpdateTexture() {
			AbsGeneratorNode g = GetInputValue<AbsGeneratorNode>("Input");

			if (g != null && g.GetGenerator() != null) {
				PreviewTexture = TextureMaker.MonochromeTexture(100, 100, g.GetGenerator());
			}

			TextureNeedsUpdating = false;
		}
	}
}