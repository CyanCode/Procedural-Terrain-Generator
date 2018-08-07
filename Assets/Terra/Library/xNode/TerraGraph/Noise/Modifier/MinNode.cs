using System;
using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Combination;

namespace Terra.Graph.Noise.Modifier {
	[CreateNodeMenu(MENU_PARENT_NAME + "Min")]
	public class MinNode: AbsTwoModNode {
		public override Generator GetGenerator() {
			if (!HasBothGenerators()) {
				return null;
			}

			return new Min(GetGenerator1(), GetGenerator2());
		}

		public override string GetTitle() {
			return "Min";
		}

		public override float GetMaxValue() {
			return Math.Max(Generator1.GetMaxValue(), Generator2.GetMaxValue());
		}

		public override float GetMinValue() {
			return Math.Min(Generator1.GetMinValue(), Generator2.GetMinValue());
		}
	}
}