using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Data {
	[Serializable]
	public class SplatData {
		public Texture2D Diffuse;
		public Texture2D Normal;
		public Vector2 Tiling = new Vector2(1, 1);
		public Vector2 Offset;

		public float Smoothness;
		public float Metallic;
		public float Blend = 30f;

		public bool ConstrainAngle = false;
		public Constraint AngleConstraint = new Constraint(0, 90);

		public bool ConstrainHeight = false;
		public Constraint HeightConstraint = new Constraint(0.25f, 0.75f);
		public bool IsMaxHeight = false;
		public bool IsMinHeight = false;
	}
}
