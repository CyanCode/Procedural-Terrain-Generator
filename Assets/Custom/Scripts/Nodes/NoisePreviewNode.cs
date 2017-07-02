using Assets.Code.Bon;
using Assets.Code.Bon.Socket;
using CoherentNoise;
using CoherentNoise.Texturing;
using System;
using System.IO;
using UnityEngine;

[Serializable]
[GraphContextMenuItem("Noise", "Preview")]
public class NoisePreviewNode: Node {
	[NonSerialized]
	private Rect LabelGenerator;
	[NonSerialized]
	private InputSocket InputSocketGenerator;
	[NonSerialized]
	private bool TextureNeedsUpdating = false;
	[NonSerialized]
	private Texture2D Texture = null;

	public NoisePreviewNode(int id, Graph parent) : base(id, parent) {
		LabelGenerator = new Rect(6, 100, 90, BonConfig.SocketSize);
		InputSocketGenerator = new InputSocket(this, typeof(AbstractGeneratorNode));

		Height = 140;
		SocketTopOffsetInput = 100;

		Sockets.Add(InputSocketGenerator);
		EventManager.OnChangedNode += NodeUpdated;
		EventManager.OnAddedNode += NodeUpdated;
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

	public void NodeUpdated(Graph graph, Node node) {
		if (InputSocketGenerator.CanGetResult()) {
			TextureNeedsUpdating = true;
		}
	}

	private Texture2D GetNoiseTexture() {
		Generator generator = AbstractGeneratorNode.GetInputGenerator(InputSocketGenerator);
		Texture2D PreviewTexture = TextureMaker.MonochromeTexture(88, 88, generator) as Texture2D;

		bool Debug = false;
		if (Debug) {
			byte[] bytes = PreviewTexture.EncodeToPNG();
			File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bytes);
		}

		return PreviewTexture;
	}
}
