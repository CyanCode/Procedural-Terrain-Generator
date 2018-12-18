using Terra.CoherentNoise;

namespace Terra.Graph.Noise.Modifier {
	[CreateNodeMenu(MENU_PARENT_NAME + "Arithmetic")]
	public class ArithmeticNode: AbsTwoModNode {
		public Operation operation = Operation.Add;
		public enum Operation { Add, Subtract, Multiply, Divide }

		public override Generator GetGenerator() {
			if (!HasBothGenerators()) {
				return null;
			}

			var g1 = GetGenerator1();
			var g2 = GetGenerator2();

			switch (operation) {
				case Operation.Add:
					return g1 + g2;
				case Operation.Subtract:
					return g1 - g2;
				case Operation.Multiply:
					return g1 * g2;
				case Operation.Divide:
					return g1 / g2;
				default:
					return null;
			}
		}

		public override string GetTitle() {
			return "Arithmetic";
		}
	}
}