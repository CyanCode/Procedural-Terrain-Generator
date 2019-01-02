using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Fractal;

namespace Terra.Graph.Generators {
	[CreateNodeMenu(MENU_PARENT_NAME + "Billow")]
	public class BillowNoiseNode: AbsFractalNoiseNode {
		[Input] public float Persistence = 1f;

		public override Generator GetGenerator() {
			BillowNoise noise = new BillowNoise(TerraConfig.Instance.Seed);
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;
			noise.Persistence = Persistence;

			return noise;
		}

		public override string GetTitle() {
			return "Billow";
		}
	}
}