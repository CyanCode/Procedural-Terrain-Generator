using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Combination;
using System;
using Assets.Terra.UNEB.Utility;

namespace Terra.Nodes.Modifier {
	[Serializable]
	[GraphContextMenuItem("Modifier", "Min")]
	public class MinNode: AbstractTwoModNode {
		public override Generator GetGenerator() {
			return new Min(Generator1, Generator2);
		}

		public override string GetName() {
			return "Min";
		}
	}
}