using System.Collections.Generic;
using Terra.CoherentNoise;
using Terra.Graph.Noise;
using Terra.Graph;

namespace Terra.Graph {
	[CreateNodeMenu("End Generator")]
	public class EndNode: XNode.Node {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] 
		public AbsGeneratorNode Noise;

		[Input(ShowBackingValue.Never)]
		public BiomeNode[] Biomes;

		/// <summary>
		/// Returns the "final" generator attached to this 
		/// node's input
		/// </summary>
		public Generator GetFinalGenerator() {
			var iv = GetInputValue<AbsGeneratorNode>("Noise");
			return iv == null ? null : iv.GetGenerator();
		}

		/// <summary>
		/// Get the biomes attached to this node's input
		/// </summary>
		/// <returns></returns>
		public BiomeNode[] GetBiomes() {
			return GetInputValues<BiomeNode>("Biomes");
		}


	}
}