using Terra.CoherentNoise;

namespace Terra.Graph.Generators.Modifiers {
	[CreateNodeMenu(MENU_PARENT_NAME + "Round")]
	public class RoundNode: AbsModNode {
		// ReSharper disable once MemberCanBePrivate.Global
		// ReSharper disable once UnassignedField.Global
		public float Cutoff = 0.5f;

		public override Generator GetGenerator() {
			return new RoundGenerator(GetGenerator1(), Cutoff);
		}

		public override string GetTitle() {
			return "Round";
		}

		private class RoundGenerator: Generator {
			private readonly Generator _generator;
			private readonly float _cutoff;

			public RoundGenerator(Generator generator, float cutoff) {
				_generator = generator;
				_cutoff = cutoff;
			}

			public override float GetValue(float x, float y, float z) {
				return _generator.GetValue(x, y, z) > _cutoff ? 1f : 0f;
			}
		}
	}
}
