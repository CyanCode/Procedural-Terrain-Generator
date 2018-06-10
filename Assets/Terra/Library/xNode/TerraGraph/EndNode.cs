using Terra.CoherentNoise;
using Terra.Graph.Detail;
using Terra.Graph.Noise;
using UnityEngine;

namespace Terra.Graph {
	[CreateNodeMenu("End Generator")]
	public class EndNode: XNode.Node {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Noise;
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public GrassNode Grass;

		/// <summary>
		/// Returns the "final" generator attached to this 
		/// node's input
		/// </summary>
		public Generator GetFinalGenerator() {
			return GetInputValue<AbsGeneratorNode>("Noise").GetGenerator();
		}
	}
}