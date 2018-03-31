using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Combination;
using System;
using Assets.Terra.UNEB.Utility;

namespace Terra.Nodes.Modifier {
	[Serializable]
	[GraphContextMenuItem("Modifier", "Max")]
	public class MaxNode: AbstractTwoModNode {
		public override Generator GetGenerator() {
			return new Max(Generator1, Generator2);
		}

		public override string GetName() {
			return "Max";
		}
	}
}