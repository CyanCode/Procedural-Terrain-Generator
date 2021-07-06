namespace Terra.Graph.Generators.Voronoi {
	public abstract class AbsVoronoiNoiseNode: AbsGeneratorNode {
		[Input] public float Frequency = 1f;
		[Input] public float Period;

		protected const string MENU_PARENT_NAME = "Noise/Voronoi/";
	}
}