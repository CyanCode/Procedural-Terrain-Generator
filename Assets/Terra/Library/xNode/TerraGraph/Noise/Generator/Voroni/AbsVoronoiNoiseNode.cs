using Terra.Graph.Noise;

namespace Terra.Nodes.Generation {
	public abstract class AbsVoronoiNoiseNode: AbsGeneratorNode {
		[Input] public float Frequency;
		[Input] public float Period;

		protected const string MENU_PARENT_NAME = "Terrain/Noise/Voronoi/";

		public override float GetMaxValue() {
			return 1;
		}

		public override float GetMinValue() {
			return -1;
		}
	}
}