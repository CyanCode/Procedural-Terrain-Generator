using Terra.CoherentNoise.Generation.Voronoi;

namespace Terra.Graph.Generators.Voronoi {
	[CreateNodeMenu(MENU_PARENT_NAME + "Valleys")]
	public class VoronoiValleysNode: AbsVoronoiNoiseNode {
		public override CoherentNoise.Generator GetGenerator() {
			VoronoiValleys2D noise = new VoronoiValleys2D(TerraConfig.Instance.Seed);
			noise.Frequency = Frequency;
			noise.Period = (int)Period;

			return noise;
		}

		public override string GetTitle() {
			return "Voronoi Valleys";
		}
	}
}
