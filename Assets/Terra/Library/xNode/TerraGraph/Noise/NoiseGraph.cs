using System;
using Terra.CoherentNoise;
using UnityEngine;
using XNode;

namespace Terra.Graph.Noise {
    /// <summary> Defines a noise graph that can be created as an asset using the Terra dropdown.</summary>
    [Serializable, CreateAssetMenu(fileName = "New Noise Graph", menuName = "Terra/Noise Graph")]
    public class NoiseGraph : XNode.NodeGraph { 
		/// <summary>
		/// Returns the generator assigned to the EndNode if 
		/// one exists.
		/// </summary>
		public Generator GetEndGenerator() {
			foreach (Node n in nodes) {
				if (n is EndNode) {
					return ((EndNode)n).GetFinalGenerator();
				}	
			}

			return null;
		}
	}
}