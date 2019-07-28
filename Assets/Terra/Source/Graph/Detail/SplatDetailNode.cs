using System;
using Terra.Structures;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
	[Serializable, CreateNodeMenu("Biomes/Objects/Splat")]
	public class SplatDetailNode: Node {
		[Output]
		public SplatDetailNode Output;

	    [Input(ShowBackingValue.Never, ConnectionType.Override)]
	    public ConstraintNode Constraint;

        public Texture2D Diffuse;
		public Texture2D Normal;

		public Vector2 Tiling = new Vector2(10, 10);
		public Vector2 Offset = new Vector2(0, 0);

		public ConstraintMixMethod MixMethod = ConstraintMixMethod.AND;

        public float Blend = 0.05f;

		public override object GetValue(NodePort port) {
			return this;
		}

		public bool ShouldPlaceAt(float x, float y, float height, float angle) {
            var cons = GetConstraintValue();
            if (cons == null) {
                return true;
            }

            return cons.ShouldPlaceAt(x, y, height, angle);
		}

        public ConstraintNode GetConstraintValue() {
            return GetInputValue<ConstraintNode>("Constraint");
        }
	}
}