using CoherentNoise;
using System;

[Serializable]
[GraphContextMenuItem("Modifier", "Subtract")]
public class SubtractNode: AbstractTwoModNode {
	public SubtractNode(int id, Graph parent) : base(id, parent) { }

	public override Generator GetGenerator() {
		return Generator1 - Generator2;
	}
}
