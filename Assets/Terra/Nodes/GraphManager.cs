using System;
using Terra.CoherentNoise;
using Terra.Nodes;
using Terra.Terrain;
using UNEB;

namespace Assets.Terra.Nodes {
	[Serializable]
	public class GraphManager {
		private TerraSettings Settings;	
		private NodeGraph Graph {
			get {
				return Settings.LoadedGraph;
			}
		}

		public GraphManager(TerraSettings settings) {
			Settings = settings;
		}

		/// <summary>
		/// Finds and returns the generator attached to the "End Node" 
		/// if it exists.
		/// </summary>
		/// <returns>Generator or null</returns>
		public Generator GetEndGenerator() {
			foreach (Node n in Graph.nodes) {
				if (n is EndNode) {
					return (n as EndNode).GetFinalGenerator();
				}
			}

			return null;
		}
	}
}
