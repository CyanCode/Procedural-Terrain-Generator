using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Combination;
using UnityEngine;

namespace Terra.Graph.Noise.Modifier {
	[CreateNodeMenu(AbsTwoModNode.MENU_PARENT_NAME + "Mask")]
	public class MaskNode: AbsGeneratorNode {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Generator1;
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Generator2;
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Mask;

		public override Generator GetGenerator() {
			var genNode1 = GetInputValue<AbsGeneratorNode>("Generator1");
			var genNode2 = GetInputValue<AbsGeneratorNode>("Generator2");
			var blendNode = GetInputValue<AbsGeneratorNode>("Mask");

			if (!HasAllGenerators(genNode1, genNode2, blendNode)) {
				return null;
			}

			var g1 = genNode1.GetGenerator();
			var g2 = genNode2.GetGenerator();
			var mask = blendNode.GetGenerator();

			return new Blend(g1, g2, mask);
		}

		public override string GetTitle() {
			return "Mask";
		}
	}
}
 