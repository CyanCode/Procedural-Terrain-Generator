using Terra.CoherentNoise;
using System;
using Terra.CoherentNoise.Generation.Voronoi;
using Terra.Terrain;
using Assets.Terra.UNEB.Utility;

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise/Voronoi", "Pits")]
	public class VoronoiPitsNode: AbstractVoronoiNoiseNode {
		public override Generator GetGenerator() {
			VoronoiPits2D noise = new VoronoiPits2D(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Period = (int)Period;

			return noise;
		}

		public override string GetName() {
			return "Voronoi Pits";
		}
	}
}