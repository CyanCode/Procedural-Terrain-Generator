using Assets.Code.Bon.Socket;
using System;
using UnityEngine;
using Assets.Code.Bon;

public abstract class AbstractVoronoiNoiseNode: AbstractGeneratorNode {
	public float Frequency {
		get {
			return AbstractNumberNode.GetInputNumber(InputSocketFrequency);
		}
	}
	public float Period {
		get {
			return AbstractNumberNode.GetInputNumber(InputSocketPeriod);
		}
	}

	[NonSerialized]
	private Rect LabelFrequency;
	[NonSerialized]
	private Rect LabelPeriod;

	[NonSerialized]
	private InputSocket InputSocketFrequency;
	[NonSerialized]
	private InputSocket InputSocketPeriod;

	public AbstractVoronoiNoiseNode(int id, Graph parent) : base(id, parent) {
		LabelFrequency = new Rect(6, 0, 90, BonConfig.SocketSize);
		LabelPeriod = new Rect(6, 20, 90, BonConfig.SocketSize);

		InputSocketFrequency = new InputSocket(this, typeof(AbstractNumberNode));
		InputSocketPeriod = new InputSocket(this, typeof(AbstractNumberNode));

		InputSocketFrequency.SetDirectInputNumber(1, false);
		InputSocketPeriod.SetDirectInputNumber(0, false);

		Sockets.Add(InputSocketFrequency);
		Sockets.Add(InputSocketPeriod);

		Height = 60;
	}

	/// <summary>
	/// Creates GUI elements that all fractal noise nodes share:
	/// <list type="">
	/// <item>Frequency</item>
	/// <item>Lacunarity</item>
	/// <item>Octave Count</item>
	/// The last label ends at y position 20. 40 Should be used for the next element.
	/// </list>
	/// </summary>
	public override void OnGUI() {
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;

		GUI.Label(LabelFrequency, "Frequency");
		GUI.Label(LabelPeriod, "Period");
	}

	public override void Update() {}
}