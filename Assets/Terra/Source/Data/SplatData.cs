using System;
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

		public PlacementType PlacementType;

		public float AngleMin = 5f;
		public float AngleMax = 25f;

		public float MinHeight = 0.25f;
		public float MaxHeight = 0.75f;
		public bool IsMaxHeight;
		public bool IsMinHeight;
	}

	[Serializable]
	public enum PlacementType {
		ElevationRange,
		Angle
	}
}
