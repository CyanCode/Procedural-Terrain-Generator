using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Terrain;

namespace Terra.Graph.Noise.Generation {
	[CreateNodeMenu(MENU_PARENT_NAME + "Billow")]
	public class BillowNoiseNode: AbsFractalNoiseNode {
		[Input] public float Persistence = 1f;

		public override Generator GetGenerator() {
			BillowNoise noise = new BillowNoise(TerraSettings.GenerationSeed);
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