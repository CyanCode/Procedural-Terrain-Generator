using Assets.Code.Bon;
using Assets.Code.Bon.Socket;
using CoherentNoise;
using CoherentNoise.Generation.Combination;
using System;
using UnityEngine;

[Serializable]
[GraphContextMenuItem("Modifier", "Mask")]
public class MaskNode: AbstractTwoModNode {
	[NonSerialized]
	private Rect LabelMaskGenerator;
	[NonSerialized]
	private InputSocket InputSocketMaskGenerator;

	public MaskNode(int id, Graph parent) : base(id, parent) {
		LabelMaskGenerator = new Rect(6, 40, 90, BonConfig.SocketSize);
		InputSocketMaskGenerator = new InputSocket(this, typeof(AbstractGeneratorNode));
		Sockets.Add(InputSocketMaskGenerator);

		Height = 80;
	}

	public override Generator GetGenerator() {
		Generator BlendGenerator = GetInputGenerator(InputSocketMaskGenerator);
		return new Blend(Generator1, Generator2, BlendGenerator);
	}

	public override void OnGUI() {
		base.OnGUI();
		GUI.Label(LabelMaskGenerator, "Mask");
	}
}
