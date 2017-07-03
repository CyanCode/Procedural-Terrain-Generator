using CoherentNoise;
using System;
using CoherentNoise.Generation.Voronoi;

[Serializable]
[GraphContextMenuItem("Noise/Voroni", "Pits")]
public class VoronoiPitsNode: AbstractVoronoiNoiseNode {
	public VoronoiPitsNode(int id, Graph parent) : base(id, parent) { }

	public override Generator GetGenerator() {
		VoronoiPits2D noise = new VoronoiPits2D(12); //TODO: Implement static seed
		noise.Frequency = Frequency;
		noise.Period = (int)Period;

		return noise;
	}
}
