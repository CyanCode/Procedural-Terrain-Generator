using CoherentNoise;
using System;

[Serializable]
[GraphContextMenuItem("Modifier", "Divide")]
public class DivideNode: AbstractTwoModNode {
	public DivideNode(int id, Graph parent) : base(id, parent) { }

	public override Generator GetGenerator() {
		return Generator1 / Generator2;
	}
}
