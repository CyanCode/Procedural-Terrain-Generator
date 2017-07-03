using CoherentNoise;
using System;

[Serializable]
[GraphContextMenuItem("Modifier", "Multiply")]
public class MultiplyNode: AbstractTwoModNode {
	public MultiplyNode(int id, Graph parent) : base(id, parent) { }

	public override Generator GetGenerator() {
		return Generator1 * Generator2;
	}
}
