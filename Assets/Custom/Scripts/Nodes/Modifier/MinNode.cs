using CoherentNoise;
using CoherentNoise.Generation.Combination;
using System;

[Serializable]
[GraphContextMenuItem("Modifier", "Min")]
public class MinNode: AbstractTwoModNode {
	public MinNode(int id, Graph parent) : base(id, parent) { }

	public override Generator GetGenerator() {
		return new Min(Generator1, Generator2);
	}
}
