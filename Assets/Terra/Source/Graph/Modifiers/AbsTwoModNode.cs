using Terra.CoherentNoise;

namespace Terra.Graph.Generators.Modifiers {
	public abstract class AbsTwoModNode: AbsModNode {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Generator2;
		
		/// <summary>
		/// Checks if this two mod node has both generators set
		/// </summary>
		/// <returns>true if both generators 1 and 2 are set</returns>
		protected bool HasBothGenerators() {
			var g1 = GetInputValue<AbsGeneratorNode>("Generator1");
			var g2 = GetInputValue<AbsGeneratorNode>("Generator2");

			return HasAllGenerators(g1, g2);
		}

		/// <summary>
		/// Gets the generator assigned to the generator 2 input
		/// </summary>
		/// <returns>Generator if found</returns>
		protected Generator GetGenerator2() {
			return GetInputValue<AbsGeneratorNode>("Generator2").GetGenerator();
		}
	}
}