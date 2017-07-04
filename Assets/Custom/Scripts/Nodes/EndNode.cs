using Assets.Code.Bon;
using Assets.Code.Bon.Socket;
using CoherentNoise;
using CoherentNoise.Texturing;
using System;
using UnityEngine;

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

		Height = 140;
		Width = 120;
		SocketTopOffsetInput = 100;

		Sockets.Add(InputSocketGenerator);
	}

	public override void OnGUI() {
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		GUI.Label(LabelEnd, "Final Generator");
	}

	public override void Update() { }

	public Generator GetFinalGenerator() {
		return AbstractGeneratorNode.GetInputGenerator(InputSocketGenerator);
	}
}
