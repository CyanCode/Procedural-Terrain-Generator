using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Graph;
using UnityEngine;
using XNode;

namespace Terra.Graph {
	[CreateNodeMenu("Biomes/Combiner")]
	public class BiomeCombinerNode: PreviewableNode {
		[Serializable]
		public enum MixMethod {
			MAX = 0,
			MIN = 1
		}

		[Output]
		public NodePort Output;
		public MixMethod Mix;
		
		[SerializeField]
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

		public override object GetValue(NodePort port) {
			return this;
		}

		public override Texture2D DidRequestTextureUpdate() {
			return GetPreviewTexture(PreviewTextureSize);
		}

		public Texture2D GetPreviewTexture(int size) {
			Texture2D tex = new Texture2D(size, size);
			BiomeNode[,] map = GetBiomeMap(size);

			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					BiomeNode b = map[x, y];

					if (b == null) {
						tex.SetPixel(x, y, Color.black);
						continue;
					}

					tex.SetPixel(x, y, b.PreviewColor);
				}
			}
			
			tex.Apply();
			return tex;
		}

		/// <summary>
		/// Generates a map of biomes constructed from connected biome nodes.
		/// </summary>
		/// <param name="resolution">Resolution of map</param>
		/// <returns></returns>
		public BiomeNode[,] GetBiomeMap(int resolution) {
			BiomeNode[] connected = GetConnectedBiomeNodes();

			BiomeNode[,] nodes = new BiomeNode[resolution, resolution];
			List<float[,]> biomeValues = new List<float[,]>(nodes.Length);

			//Gather each biome's values
			foreach (BiomeNode biome in connected) {
				biomeValues.Add(biome.GetNormalizedValues(resolution));
			}

			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					float limit = Mix == MixMethod.MAX ? float.NegativeInfinity : float.PositiveInfinity;
					BiomeNode maxMinBiome = null;

					for (int z = 0; z < connected.Length; z++) {
						float val = biomeValues[z][x, y];

						if (Mix == MixMethod.MAX && val > limit) {
							limit = val;
							maxMinBiome = connected[z];
						} 
						if (Mix == MixMethod.MIN && val < limit) {
							limit = val;
							maxMinBiome = connected[z];
						}
					}

					nodes[x, y] = maxMinBiome;
				}
			}

			return nodes;
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
	}
}