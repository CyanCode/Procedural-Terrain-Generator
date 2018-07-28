using System;
using UnityEngine;

namespace Terra.Data {
	/// <summary>
	/// Represents a Biome and its various settings
	/// </summary>
	[Serializable]
	public class BiomeData {
		/// <summary>
		/// Detail information for this biome
		/// </summary>
		public DetailData Details;

		/// <summary>
		/// Name of this Biome
		/// </summary>
		public string Name = "";

		/// <summary>
		/// Height constraints if enabled
		/// </summary>
		public Constraint HeightConstraint = new Constraint(0, 1);

		/// <summary>
		/// Angle constraints if enabled
		/// </summary>
		public Constraint AngleConstraint = new Constraint(0, 90);

		/// <summary>
		/// Temperature constraint if enabled
		/// </summary>
		public Constraint TemperatureConstraint = new Constraint(0, 1f);

		/// <summary>
		/// Moisture map constraint if enabled
		/// </summary>
		public Constraint MoistureConstraint = new Constraint(0, 1f);

		/// <summary>
		/// Will this biome only appear between constrained 
		/// heights?
		/// </summary>
		public bool IsHeightConstrained = false;

		/// <summary>
		/// Will this biome only appear between constrained 
		/// angles?
		/// </summary>
		public bool IsAngleConstrained = false;

		/// <summary>
		/// Is this biome constrained by the temperature map?
		/// </summary>
		public bool IsTemperatureConstrained = false;

		/// <summary>
		/// Is this biome constrained by the moisture map?
		/// </summary>
		public bool IsMoistureConstrained = false;

		/// <summary>
		/// Display preview texture in editor?
		/// </summary>
		public bool ShowPreviewTexture = false;

		/// <summary>
		/// Preview texture that has possibly been previously calculated
		/// </summary>
		public Texture2D CachedPreviewTexture = null;

		/// <summary>
		/// "Color" assigned to this biome. Used for editor previewing
		/// </summary>
		public Color Color = default(Color);

		/// <summary>
		/// Create a preview texture for the passed list of biomes by 
		/// coloring biomes that pass constraints.
		/// </summary>
		public static Texture2D GetPreviewTexture(int width, int height, float zoom = 1f) {
			Texture2D tex = new Texture2D(width, height);

			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					BiomeData b = TerraSettings.Instance.GetBiomeAt(i / zoom, j / zoom);
					tex.SetPixel(i, j, b == null ? Color.black : b.Color);
				}
			}

			tex.Apply();
			return tex;
		}
	}
}
