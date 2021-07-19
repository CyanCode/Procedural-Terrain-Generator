using System;
using Terra.CoherentNoise;
using UnityEngine;
using XNode;

namespace Terra.Graph {
    /// <summary> Defines a noise graph that can be created as an asset using the Terra dropdown.</summary>
    [Serializable, CreateAssetMenu(fileName = "New Graph", menuName = "Terra/Graph")]
    public class TerraGraph : NodeGraph {
		private EndNode _endNode;

		/// <summary>
		/// Returns the generator assigned to the EndNode if 
		/// one exists.
		/// </summary>
		public Generator GetEndGenerator() {
			EndNode node = GetEndNode();
			return node == null ? null : node.GetHeightMap();
		}

		public EndNode GetEndNode() {
			if (_endNode != null) {
				return _endNode;
            }

			foreach (Node n in nodes) {
				if (n is EndNode) {
					_endNode = (EndNode)n;
					return _endNode;
				}
			}

			return null;
		}
	}
}