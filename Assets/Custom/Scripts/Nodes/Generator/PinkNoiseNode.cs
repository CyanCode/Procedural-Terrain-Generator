using UnityEngine;
using CoherentNoise;
using System;
using CoherentNoise.Generation.Fractal;
using Assets.Code.Bon.Socket;
using Assets.Code.Bon;

[Serializable]
[GraphContextMenuItem("Noise", "Pink")]
public class PinkNoiseNode: AbstractFractalNoiseNode {
	[NonSerialized]
	private Rect LabelPersistence;

	[NonSerialized]
	private InputSocket InputSocketPersistence;

	public PinkNoiseNode(int id, Graph parent) : base(id, parent) {
		LabelPersistence = new Rect(6, 60, 90, BonConfig.SocketSize);
		InputSocketPersistence = new InputSocket(this, typeof(AbstractNumberNode));
		InputSocketPersistence.SetDirectInputNumber(1f, false);
		Sockets.Add(InputSocketPersistence);

		Height = 100;
	}

	public override Generator GetGenerator() {
		PinkNoise noise = new PinkNoise(12); //TODO: Implement static seed
		noise.Frequency = Frequency;
		noise.Lacunarity = Lacunarity;
		noise.OctaveCount = OctaveCount;
		noise.Persistence = AbstractNumberNode.GetInputNumber(InputSocketPersistence);

		return noise;
	}

	public override void OnGUI() {
		base.OnGUI();

		GUI.Label(LabelPersistence, "Persistence");
	}
}
