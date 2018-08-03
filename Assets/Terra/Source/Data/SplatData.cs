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

		public PlacementType PlacementType;

		public float AngleMin = 5f;
		public float AngleMax = 25f;

		public float MinHeight = 0.25f;
		public float MaxHeight = 0.75f;
		public bool IsMaxHeight;
		public bool IsMinHeight;

		public override bool Equals(object obj) {
			var data = obj as SplatData;
			return data != null &&
				   EqualityComparer<Texture2D>.Default.Equals(Diffuse, data.Diffuse) &&
				   EqualityComparer<Texture2D>.Default.Equals(Normal, data.Normal) &&
				   EqualityComparer<Vector2>.Default.Equals(Tiling, data.Tiling) &&
				   EqualityComparer<Vector2>.Default.Equals(Offset, data.Offset) &&
				   Smoothness == data.Smoothness &&
				   Metallic == data.Metallic &&
				   Blend == data.Blend &&
				   PlacementType == data.PlacementType &&
				   AngleMin == data.AngleMin &&
				   AngleMax == data.AngleMax &&
				   MinHeight == data.MinHeight &&
				   MaxHeight == data.MaxHeight &&
				   IsMaxHeight == data.IsMaxHeight &&
				   IsMinHeight == data.IsMinHeight;
		}

		public override int GetHashCode() {
			var hashCode = 1038440905;
			hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(Diffuse);
			hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(Normal);
			hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Tiling);
			hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Offset);
			hashCode = hashCode * -1521134295 + Smoothness.GetHashCode();
			hashCode = hashCode * -1521134295 + Metallic.GetHashCode();
			hashCode = hashCode * -1521134295 + Blend.GetHashCode();
			hashCode = hashCode * -1521134295 + PlacementType.GetHashCode();
			hashCode = hashCode * -1521134295 + AngleMin.GetHashCode();
			hashCode = hashCode * -1521134295 + AngleMax.GetHashCode();
			hashCode = hashCode * -1521134295 + MinHeight.GetHashCode();
			hashCode = hashCode * -1521134295 + MaxHeight.GetHashCode();
			hashCode = hashCode * -1521134295 + IsMaxHeight.GetHashCode();
			hashCode = hashCode * -1521134295 + IsMinHeight.GetHashCode();
			return hashCode;
		}
	}

	[Serializable]
	public enum PlacementType {
		ElevationRange,
		Angle
	}
}
