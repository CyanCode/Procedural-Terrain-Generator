using Terra.CoherentNoise.Texturing;
using System;
using Terra.Nodes.Generation;
using UnityEngine;
using UNEB;
using Assets.Terra.UNEB.Utility;

namespace Terra.Nodes {
	[Serializable]
	[GraphContextMenuItem("", "Preview")]
	public class NoisePreviewNode: Node {
		public bool TextureNeedsUpdating;
		
		private Texture Texture;
		private NodeInput InputGenerator;

		private float startHeight = 0f;
		private float heightExpanded {
			get {
				return startHeight + 82f;
			}
		}
		private float heightContracted {
			get {
				return startHeight;
			}
		}

		public override void Init() {
			base.Init();

			InputGenerator = AddInput("Generator");
			FitKnobs();

			name = "Preview";
			startHeight = bodyRect.height;
			bodyRect.width -= 38f;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();

			if (TextureNeedsUpdating) {
				Texture = GetNoiseTexture();
				TextureNeedsUpdating = false;
			}

			if (Texture != null) {
				GUI.DrawTexture(new Rect(6, kHeaderHeight + 6, 90, 90), Texture);
				bodyRect.height = heightExpanded;
			} else {
				bodyRect.height = heightContracted;
			}
		}

		/// <summary>
		/// Generates a noise texture for the currently attached InputSocket
		/// </summary>
		/// <returns>If an error occured while retrieving the generator, 
		/// null is returned. Otherwise the Texture is returned.</returns>
		private Texture GetNoiseTexture() {
			if (!InputGenerator.HasOutputConnected()) {
				return null;
			}

			AbstractGeneratorNode genNode = (AbstractGeneratorNode) InputGenerator.GetOutput(0).ParentNode;
			if (genNode == null || genNode.GetGenerator() == null) {
				return null;
			} else {
				Texture PreviewTexture = TextureMaker.MonochromeTexture(100, 100, genNode.GetGenerator());
				return PreviewTexture;
			}
		}
	}
}