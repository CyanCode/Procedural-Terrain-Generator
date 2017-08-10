using Terra.CoherentNoise;
using System;
using Terra.CoherentNoise.Generation.Voronoi;
using Terra.GraphEditor;
using Terra.Terrain;

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise/Voroni", "Pits")]
	public class VoronoiPitsNode: AbstractVoronoiNoiseNode {
		public VoronoiPitsNode(int id, Graph parent) : base(id, parent) { }

		public override Generator GetGenerator() {
			VoronoiPits2D noise = new VoronoiPits2D(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Period = (int)Period;

			return noise;
		}
	}
}