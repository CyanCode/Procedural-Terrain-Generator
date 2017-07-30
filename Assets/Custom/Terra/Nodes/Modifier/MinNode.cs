using CoherentNoise;
using CoherentNoise.Generation.Combination;
using System;
using Terra.GraphEditor;

namespace Terra.Nodes.Modifier {
	[Serializable]
	[GraphContextMenuItem("Modifier", "Min")]
	public class MinNode: AbstractTwoModNode {
		public MinNode(int id, Graph parent) : base(id, parent) { }

		public override Generator GetGenerator() {
			return new Min(Generator1, Generator2);
		}
	}
}