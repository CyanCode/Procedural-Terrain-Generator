using Terra.Graph.Noise;

namespace Terra.Nodes.Generation {
	public abstract class AbsVoronoiNoiseNode: AbsGeneratorNode {
		[Input] public float Frequency;
		[Input] public float Period;

		protected const string MENU_PARENT_NAME = "Terrain/Noise/Voronoi/";
	}
}