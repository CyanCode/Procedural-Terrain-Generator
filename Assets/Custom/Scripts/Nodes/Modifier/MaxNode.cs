using CoherentNoise;
using CoherentNoise.Generation.Combination;
using System;

[Serializable]
[GraphContextMenuItem("Modifier", "Max")]
public class MaxNode: AbstractTwoModNode {
	public MaxNode(int id, Graph parent) : base(id, parent) { }

	public override Generator GetGenerator() {
		return new Max(Generator1, Generator2);
	}
}
