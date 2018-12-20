using UnityEngine;
using XNode;

namespace Terra.Editor.Graph {
	[CreateNodeMenu("Biomes/Splat Object")]
	public class SplatObjectNode: Node {
		[Output]
		public SplatObjectNode Output;

		public Texture2D Diffuse;
		public Texture2D Normal;

		public Vector2 Tiling = new Vector2(10, 10);
		public Vector2 Offset = new Vector2(10, 10);

		public override object GetValue(NodePort port) {
			return this;
		}
	}
}