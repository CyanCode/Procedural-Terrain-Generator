using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Terrain;

namespace Terra.Graph.Noise.Generation {
	[CreateNodeMenu(MENU_PARENT_NAME + "Ridge")]
	public class RidgeNoiseNode: AbsFractalNoiseNode {
		[Input] public float Exponent = 1f;
		[Input] public float Offset = 1f;
		[Input] public float Gain = 2f;

		public override Generator GetGenerator() {
			RidgeNoise noise = new RidgeNoise(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;

			noise.Exponent = Exponent;
			noise.Offset = Offset;
			noise.Gain = Gain;

			return noise;
		}

		public override string GetTitle() {
			return "Ridge";
		}
	}
}
