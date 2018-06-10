
using System.Collections.Generic;
using Terra.CoherentNoise;
using XNode;

namespace Terra.Graph.Noise {
	/// <summary>
	/// This class represents a node that outputs a Generator
	/// </summary>
	public abstract class AbsGeneratorNode: XNode.Node {
		[Output] public AbsGeneratorNode Output;

		public override object GetValue(NodePort port) {
			return this;
		}

		/// <summary>
		/// Called when a value has changed in the graph
		/// </summary>
		public void OnValueChange() {
			/**
			 * Perform a depth first search for any NoisePreviewNode
			 * that spans off of this Node
			 */
			Stack<Node> dfs = new Stack<Node>();
			Dictionary<Node, bool> visited = new Dictionary<Node, bool>();

			dfs.Push(this);

			while (dfs.Count != 0) {
				var node = dfs.Pop();

				//If we haven't visited this node yet
				if (!visited.ContainsKey(node) || !visited[node]) {
					visited[node] = true;
				}

				// Search neighbors
				foreach (var output in node.Outputs) {
					if (output != null) {
						for (int i = 0; i < output.ConnectionCount; i++) {
							Node outNode = output.GetConnection(i).node;

							if (outNode != null && (!visited.ContainsKey(outNode) || !visited[outNode])) {
								dfs.Push(outNode);
							}
						}
					}
				}

				var outputNode = node as NoisePreviewNode;
				if (outputNode != null) {
					outputNode.InvalidateTexture();
				}
			}
		}

		/// <summary>
		/// Convenience method for checking whether all of the 
		/// provided generator nodes have an accessible generator 
		/// attached to them.
		/// </summary>
		/// <param name="g1">Array of generator nodes to check</param>
		/// <returns>true if generators can be accessed in all nodes</returns>
		internal static bool HasAllGenerators(params AbsGeneratorNode[] gens) {
			foreach (AbsGeneratorNode agn in gens) {
				if (agn == null || agn.GetGenerator() == null) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Get the generator associated with this node. Is 
		/// Used to compute the final generator passed to the end 
		/// node and preview nodes.
		/// </summary>
		/// <returns>Generator</returns>
		public abstract Generator GetGenerator();

		/// <summary>
		/// Gets the name of this node to display in the 
		/// title.
		/// </summary>
		/// <returns>Title in string form</returns>
		public abstract string GetTitle();
	}
}
 