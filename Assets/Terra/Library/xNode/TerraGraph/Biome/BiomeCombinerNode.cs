using System.Collections.Generic;
using System.Linq;
using Terra.Graph.Noise.Modifier;
using XNode;

namespace Terra.Editor.Graph {
	[CreateNodeMenu("Biomes/Combiner")]
	public class BiomeCombinerNode: Node {
		private NodePort _lastInputPort;

		protected override void Init() {
			//Add first biome instance port
			if (GetInstanceInputs().Length < 1) {
				AddInstanceInput(typeof(BiomeNode), ConnectionType.Override, "Biome 1");
			}
		}

		public override void OnCreateConnection(NodePort from, NodePort to) {
			int portCount = GetInstanceInputs().Length + 1;
			_lastInputPort = AddInstanceInput(typeof(BiomeNode), ConnectionType.Override, "Biome " + portCount);
		}

		public override void OnRemoveConnection(NodePort port) {
			RemoveInstancePort(_lastInputPort);
		}

		public NodePort[] GetInstanceInputs() {
			List<NodePort> ports = new List<NodePort>(2);

			foreach (NodePort p in InstanceInputs) {
				ports.Add(p);
			}

			return ports.ToArray(); 
		}
	}
}