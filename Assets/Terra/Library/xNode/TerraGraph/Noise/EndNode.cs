using Terra.CoherentNoise;
using Terra.Graph.Noise.Generation;
using UnityEngine;

namespace Terra.Graph.Noise {
	[CreateNodeMenu("End Generator")]
	public class EndNode: XNode.Node {
		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Input;

		/// <summary>
		/// Returns the "final" generator attached to this 
		/// node's input
		/// </summary>
		public Generator GetFinalGenerator() {
			return GetInputValue<AbsGeneratorNode>("Input").GetGenerator();
		}
	}
}