using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Voronoi;

namespace Terra.Graph.Generators.Voronoi {
	[CreateNodeMenu(MENU_PARENT_NAME + "Pits")]
	public class VoronoiPitsNode: AbsVoronoiNoiseNode {
		public override Generator GetGenerator() {
			VoronoiPits2D noise = new VoronoiPits2D(TerraConfig.Instance.Seed);
			noise.Frequency = Frequency;
			noise.Period = (int)Period;

			return noise;
		}

		public override string GetTitle() {
			return "Voronoi Pits";
		}
	}
}