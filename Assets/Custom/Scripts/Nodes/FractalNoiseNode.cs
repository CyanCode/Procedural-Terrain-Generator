using Assets.Code.Bon.Socket;
using System;
using UnityEngine;
using UnityEditor;
using CoherentNoise;
using CoherentNoise.Generation.Fractal;
using Assets.Code.Bon;

[Serializable]
[GraphContextMenuItem("Noise", "Fractal")]
public class FractalNoiseNode: AbstractGeneratorNode {
	[Serializable]
	private enum FractalNoises {
		Billow,
		Pink,
		Ridge
	}

	[SerializeField]
	private FractalNoises NoiseSelection = FractalNoises.Billow;

	[NonSerialized]
	private Graph Parent;

	[NonSerialized]
	private Rect LabelSeed;
	[NonSerialized]
	private Rect LabelLacunarity;
	[NonSerialized]
	private Rect LabelOctaveCount;
	[NonSerialized]
	private Rect LabelFrequency;
	[NonSerialized]
	private Rect LabelPersistence;

	[NonSerialized]
	private InputSocket InputSocketSeed;
	[NonSerialized]
	private InputSocket InputSocketLacunarity;
	[NonSerialized]
	private InputSocket InputSocketOctaveCount;
	[NonSerialized]
	private InputSocket InputSocketFrequency;
	[NonSerialized]
	private InputSocket InputSocketPersistence;

	public FractalNoiseNode(int id, Graph parent) : base(id, parent) {
		this.Parent = parent;

		LabelSeed = new Rect(6, 20, 90, BonConfig.SocketSize);
		LabelLacunarity = new Rect(6, 40, 90, BonConfig.SocketSize);
		LabelOctaveCount = new Rect(6, 60, 90, BonConfig.SocketSize);
		LabelFrequency = new Rect(6, 80, 90, BonConfig.SocketSize);
		LabelPersistence = new Rect(6, 100, 90, BonConfig.SocketSize);

		InputSocketSeed = new InputSocket(this, typeof(AbstractNumberNode));
		InputSocketLacunarity = new InputSocket(this, typeof(AbstractNumberNode));
		InputSocketOctaveCount = new InputSocket(this, typeof(AbstractNumberNode));
		InputSocketFrequency = new InputSocket(this, typeof(AbstractNumberNode));
		InputSocketPersistence = new InputSocket(this, typeof(AbstractNumberNode));

		InputSocketLacunarity.SetDirectInputNumber(2.17f, false);
		InputSocketOctaveCount.SetDirectInputNumber(6, false);
		InputSocketFrequency.SetDirectInputNumber(1, false);
		InputSocketPersistence.SetDirectInputNumber(0.5f, false);

		Sockets.Add(InputSocketSeed);
		Sockets.Add(InputSocketLacunarity);
		Sockets.Add(InputSocketOctaveCount);
		Sockets.Add(InputSocketFrequency);
		Sockets.Add(InputSocketPersistence);

		Height = 140;
		SocketTopOffsetInput = 20;
	}

	public override void OnGUI() {
		EditorGUI.BeginChangeCheck();
		NoiseSelection = (FractalNoises)EditorGUI.EnumPopup(new Rect(6, 0, 90, BonConfig.SocketSize), NoiseSelection);
		if (EditorGUI.EndChangeCheck()) {
			EventManager.TriggerOnChangedNode(Parent, this);
		}

		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		GUI.Label(LabelSeed, "Seed");
		GUI.Label(LabelLacunarity, "Lacunarity");
		GUI.Label(LabelOctaveCount, "Octave Count");
		GUI.Label(LabelFrequency, "Frequency");
		GUI.Label(LabelPersistence, "Persistence");
	}

	public override Generator GetGenerator() {
		FractalNoiseBase fractal = null;
		float seed = AbstractNumberNode.GetInputNumber(InputSocketSeed);

		switch (NoiseSelection) {
			case FractalNoises.Billow:
				fractal = new BillowNoise(seed);
				break;
			case FractalNoises.Pink:
				fractal = new PinkNoise(seed);
				break;
			case FractalNoises.Ridge:
				fractal = new RidgeNoise(seed);
				break;
		}

		fractal.Lacunarity = AbstractNumberNode.GetInputNumber(InputSocketLacunarity);
		fractal.OctaveCount = (int)AbstractNumberNode.GetInputNumber(InputSocketOctaveCount);
		fractal.Frequency = AbstractNumberNode.GetInputNumber(InputSocketFrequency);
		
		return fractal;
	}

	public override void Update() {

	}
}