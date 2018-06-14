using Terra.CoherentNoise;
using Terra.Graph.Noise;
using UnityEngine;

namespace Terra.Graph {
	[CreateNodeMenu("End Generator")]
	public class EndNode: XNode.Node {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Noise;

		/// <summary>
		/// Returns the "final" generator attached to this 
		/// node's input
		/// </summary>
		public Generator GetFinalGenerator() {
			return GetInputValue<AbsGeneratorNode>("Noise").GetGenerator();
		}
	}
}