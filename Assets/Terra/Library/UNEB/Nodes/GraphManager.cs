using System;
using Terra.CoherentNoise;
using Terra.Nodes;
using UNEB;
using UnityEngine;

namespace Assets.Terra.Nodes {
	/// <summary>
	/// Handles interactions between the editor and play mode for 
	/// computing graphs.
	/// </summary>
	[Serializable]
	public class GraphManager {
#if UNITY_EDITOR
		public NodeGraph Graph;
#endif
		[SerializeField]
		private EndNode EndNode;

		public GraphManager() {
			#if UNITY_EDITOR
			NodeGraphEvent.OnNodeChanged += OnNodeChanged;
			#endif
		}

#if UNITY_EDITOR
		/// <summary>
		/// Updates the internal EndNode based on the currently assigned graph
		/// </summary>
		public void Update() {
			if (Graph != null) {
				foreach (Node n in Graph.nodes) {
					if (n is EndNode) {
						EndNode = n as EndNode;
					}
				}
			}
		}

		/// <summary>
		/// Called when a node changes in the graph
		/// </summary>
		/// <param name="node"></param>
		private void OnNodeChanged(Node node) {
			Update();
		}
#endif

		/// <summary>
		/// Finds and returns the generator attached to the "End Node" 
		/// if it exists.
		/// </summary>
		/// <returns>Generator or null</returns>
		public Generator GetEndGenerator() {
			if (EndNode != null) {
				return EndNode.GetFinalGenerator();
			}

			return null;
		}
	}
}
