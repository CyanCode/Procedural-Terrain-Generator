using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Displacement;
using UnityEngine;

namespace Terra.Graph.Generators.Modifiers {
	[CreateNodeMenu(AbsTwoModNode.MENU_PARENT_NAME + "Scale")]
	public class ScaleNode: AbsGeneratorNode {
		//[Output] public AbsGeneratorNode Out;
		[Input] public AbsGeneratorNode Generator;

		[Input] public Vector3 Factor = Vector3.one;
		[Input] public float Amount = 1f;
		[Input] public bool Uniform;

		public override Generator GetGenerator() {
			if (!HasAllGenerators(GetInputValue<AbsGeneratorNode>("Generator"))) {
				return null;
			}

			var gen = GetInputValue<AbsGeneratorNode>("Generator").GetGenerator();

			if (Uniform) {
				Vector3 fact = new Vector3(Amount, Amount, Amount);
				return new Scale(gen, fact);
			} else {
				return new Scale(gen, Factor);
			}
		}

		public override string GetTitle() {
			return "Scale";
		}

		float Max(Vector3 v) {
			return Mathf.Max(v.x, v.y, v.z);
		}
	}
}