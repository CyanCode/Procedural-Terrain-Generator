using Terra.Graph.Noise;
using UnityEngine;
using XNode;

namespace Terra.Graph.Detail {
	public class GrassNode: XNode.Node {
		[Output] public GrassNode Output;

		[Input(ShowBackingValue.Never, ConnectionType.Override)] public AbsGeneratorNode Mask;
		[Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public float Influence = 0.5f;

		public Texture2D Texture;

		//TODO Height, Angle, Vertical shift, Density

		public override object GetValue(NodePort port) {
			return this;
		}
	}
}