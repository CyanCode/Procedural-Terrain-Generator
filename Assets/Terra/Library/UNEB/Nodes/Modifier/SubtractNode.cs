using Terra.CoherentNoise;
using System;
using Assets.Terra.UNEB.Utility;

namespace Terra.Nodes.Modifier {
	[Serializable]
	[GraphContextMenuItem("Modifier", "Subtract")]
	public class SubtractNode: AbstractTwoModNode {
		public override Generator GetGenerator() {
			return Generator1 - Generator2;
		}

		public override string GetName() {
			return "Subtract";
		}
	}
}