using Terra.CoherentNoise;
using System;
using Terra.CoherentNoise.Generation.Voronoi;
using Terra.Terrain;
using Assets.Terra.UNEB.Utility;

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise/Voronoi", "Valleys")]
	public class VoronoiValleysNode: AbstractVoronoiNoiseNode {
		public override Generator GetGenerator() {
			VoronoiValleys2D noise = new VoronoiValleys2D(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Period = (int)Period;

			return noise;
		}

		public override string GetName() {
			return "Voronoi Valleys";
		}
	}
}
