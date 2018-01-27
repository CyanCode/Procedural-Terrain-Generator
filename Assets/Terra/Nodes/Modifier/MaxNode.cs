using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Combination;
using System;
using Terra.GraphEditor;

namespace Terra.Nodes.Modifier {
	[Serializable]
	[GraphContextMenuItem("Modifier", "Max")]
	public class MaxNode: AbstractTwoModNode {
		public MaxNode(int id, Graph parent) : base(id, parent) { }

		public override Generator GetGenerator() {
			return new Max(Generator1, Generator2);
		}
	}
}