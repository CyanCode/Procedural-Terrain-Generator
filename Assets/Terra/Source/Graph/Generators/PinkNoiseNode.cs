using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Fractal;

namespace Terra.Graph.Generators {
	[CreateNodeMenu(MENU_PARENT_NAME + "Pink")]
	public class PinkNoiseNode: AbsFractalNoiseNode {
		[Input] public float Persistence = 1f;

		public override Generator GetGenerator() {
			PinkNoise noise = new PinkNoise(TerraConfig.Instance.Seed);
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;
			noise.Persistence = Persistence;

			return noise;
		}

		public override string GetTitle() {
			return "Pink";
		}
	}
}