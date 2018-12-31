using Terra.CoherentNoise;

namespace Terra.Graph.Generators.Modifiers {
	public abstract class AbsModNode: AbsGeneratorNode {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Generator1;

		internal const string MENU_PARENT_NAME = "Modifier/";

		/// <summary>
		/// Gets the generator assigned to the generator 1 input
		/// </summary>
		/// <returns>Generator if found</returns>
		protected Generator GetGenerator1() {
			return GetInputValue<AbsGeneratorNode>("Generator1").GetGenerator();
		}


	}
}