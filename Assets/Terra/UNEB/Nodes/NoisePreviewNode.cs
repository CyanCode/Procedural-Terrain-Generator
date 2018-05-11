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
		public bool TextureNeedsUpdating = true;
		
		[SerializeField]
		private Texture Texture;
		[SerializeField]
		private NodeInput InputGenerator;

		private float startHeight = 20f;
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

		public override string GetName() {
			return "Preview";
		}

		public override void Init() {
			base.Init();

			InputGenerator = AddInput("Generator");
			InputGenerator.name = "Generator";
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
				SetNameText("");
			} else {
				bodyRect.height = heightContracted;
				SetNameText("Generator");
			}

			NodeGraphEvent.OnNodeChanged -= OnGraphUpdate;
			NodeGraphEvent.OnNodeChanged += OnGraphUpdate;
		}

		private void OnGraphUpdate(Node n) {
			TextureNeedsUpdating = true;
		}

		private void SetNameText(string name) {
			if (InputGenerator != null) {
				InputGenerator.name = name;
			}
		}

		/// <summary>
		/// Generates a noise texture for the currently attached InputSocket
		/// </summary>
		/// <returns>If an error occured while retrieving the generator, 
		/// null is returned. Otherwise the Texture is returned.</returns>
		private Texture GetNoiseTexture() {
			if (InputGenerator == null || !InputGenerator.HasOutputConnected()) {
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