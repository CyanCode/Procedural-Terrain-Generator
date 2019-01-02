using Terra.Structures;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
	[CreateNodeMenu("Biomes/Objects/Splat")]
	public class SplatDetailNode: Node {
		[Output]
		public SplatDetailNode Output;

		public Texture2D Diffuse;
		public Texture2D Normal;

		public Vector2 Tiling = new Vector2(10, 10);
		public Vector2 Offset = new Vector2(0, 0);

		public bool IsHeightConstrained = false;
		public bool IsAngleConstrained = false;

		public Constraint HeightConstraint = new Constraint(0, 1);
		public Constraint AngleConstraint = new Constraint(0, 90);

		public ConstraintMixMethod MixMethod = ConstraintMixMethod.AND;

		public override object GetValue(NodePort port) {
			return this;
		}

		public bool ShouldShowAt(float height, float angle) {
			bool fitsHeight = false;
			bool fitsAngle = false;

			//No constraints, should show splat everywhere
			if (!IsHeightConstrained && !IsAngleConstrained) {
				return true;
			}

			if (IsHeightConstrained && HeightConstraint.Fits(height)) {
				fitsHeight = true;
			} 
			if (IsAngleConstrained && AngleConstraint.Fits(angle)) {
				fitsAngle = true;
			}

			if (MixMethod == ConstraintMixMethod.AND && fitsHeight && fitsAngle) {
				return true;
			}
			if (MixMethod == ConstraintMixMethod.OR && (fitsHeight || fitsAngle)) {
				return true;
			}

			return false;
		}
	}
}