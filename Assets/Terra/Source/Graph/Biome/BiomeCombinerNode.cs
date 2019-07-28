using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Terrain;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
	[CreateNodeMenu("Biomes/Combiner")]
	public class BiomeCombinerNode: PreviewableNode, ISerializationCallbackReceiver {
		[Serializable]
		public enum MixMethod {
			MAX = 0,
			MIN = 1,
			ADD = 2
		}

		[Output]
		public NodePort Output;

		/// <summary>
		/// Used by the editor for checking which frame added a port
		/// </summary>
		public bool DidAddPort;

		public BiomeCombinerSampler Sampler;
		public MixMethod Mix;

		private LinkedList<NodePort> _activePorts;
		[SerializeField]
		private NodePort[] _serializedActivePorts;

		protected override void Init() {
			if (_activePorts == null) {
				_activePorts = new LinkedList<NodePort>();
			}
			if (Sampler == null) {
				Sampler = new BiomeCombinerSampler(this);
			}

			//Add first biome instance port
			if (GetInstanceInputs().Length < 1) {
				_activePorts.AddLast(AddInstanceInput(typeof(BiomeNode), ConnectionType.Override, "Biome 1"));
				DidAddPort = true;
			}
		} 

		public override void OnCreateConnection(NodePort from, NodePort to) {
			int portCount = GetInstanceInputs().Length + 1;
			_activePorts.AddLast(AddInstanceInput(typeof(BiomeNode), ConnectionType.Override, "Biome " + portCount));
			DidAddPort = true;
		}

		public override void OnRemoveConnection(NodePort port) {
			if (_activePorts.Last != null) {
				RemoveInstancePort(_activePorts.Last.Value);
				_activePorts.RemoveLast();
			}
		}

		public override object GetValue(NodePort port) {
			return this;
		}

		public override Texture2D DidRequestTextureUpdate() {
			return Sampler.GetPreviewTexture(PreviewTextureSize);
		}

		public BiomeNode[] GetConnectedBiomeNodes() {
			NodePort[] ports = GetInstanceInputs();
			List<BiomeNode> biomes = new List<BiomeNode>(ports.Length);

			foreach (NodePort p in ports) {
				BiomeNode input = p.GetInputValue<BiomeNode>();
				if (p.IsConnected && input != null) {
					biomes.Add(input);
				}
			}

			return biomes.ToArray();
		}

		public NodePort[] GetInstanceInputs() {
			List<NodePort> ports = new List<NodePort>(2);

			foreach (NodePort p in InstanceInputs) {
				ports.Add(p);
			}

			return ports.ToArray(); 
		}

		public void OnBeforeSerialize() {
			_serializedActivePorts = _activePorts.ToArray();
		}

		public void OnAfterDeserialize() {
			if (_serializedActivePorts != null) {
				_activePorts = new LinkedList<NodePort>();
				
				foreach (var port in _serializedActivePorts) {
					_activePorts.AddLast(port);
				}
			}
		}
	}
}