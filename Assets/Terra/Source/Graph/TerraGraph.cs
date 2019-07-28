using System;
using Terra.CoherentNoise;
using Terra.Graph.Biome;
using UnityEngine;
using XNode;

namespace Terra.Graph {
    /// <summary> Defines a noise graph that can be created as an asset using the Terra dropdown.</summary>
    [Serializable, CreateAssetMenu(fileName = "New Graph", menuName = "Terra/Graph")]
    public class TerraGraph : NodeGraph { 
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

		public BiomeCombinerNode GetBiomeCombiner() {
			foreach (Node n in nodes) {
				if (n is BiomeCombinerNode) {
					return (BiomeCombinerNode)n;
				}
			}

			return null;
		}
	}
}