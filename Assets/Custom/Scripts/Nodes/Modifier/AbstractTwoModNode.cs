using Assets.Code.Bon;
using Assets.Code.Bon.Socket;
using CoherentNoise;
using System;
using UnityEngine;

public abstract class AbstractTwoModNode: AbstractGeneratorNode {
	public Generator Generator1 {
		get {
			return GetInputGenerator(InputSocketGenerator1);
		}
	}
	public Generator Generator2 {
		get {
			return GetInputGenerator(InputSocketGenerator2);
		}
	}

	[NonSerialized]
	private Rect LabelGenerator1;
	[NonSerialized]
	private Rect LabelGenerator2;

	[NonSerialized]
	private InputSocket InputSocketGenerator1;
	[NonSerialized]
	private InputSocket InputSocketGenerator2;

	public AbstractTwoModNode(int id, Graph parent) : base(id, parent) {
		LabelGenerator1 = new Rect(6, 0, 90, BonConfig.SocketSize);
		LabelGenerator2 = new Rect(6, 20, 90, BonConfig.SocketSize);

		InputSocketGenerator1 = new InputSocket(this, typeof(AbstractGeneratorNode));
		InputSocketGenerator2 = new InputSocket(this, typeof(AbstractGeneratorNode));

		Sockets.Add(InputSocketGenerator1);
		Sockets.Add(InputSocketGenerator2);
		Height = 60;
	}

	/// <summary>
	/// Creates GUI elements that all two modifier noise nodes share:
	/// <list type="">
	/// <item>Generator 1</item>
	/// <item>Generator 2</item>
	/// The last label ends at y position 20. 40 Should be used for the next element.
	/// The height should also changed accordingly (default is 60.)
	/// </list>
	/// </summary>
	public override void OnGUI() {
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;

		GUI.Label(LabelGenerator1, "Generator 1");
		GUI.Label(LabelGenerator2, "Generator 2");
	}

	public override void Update() {}
}
