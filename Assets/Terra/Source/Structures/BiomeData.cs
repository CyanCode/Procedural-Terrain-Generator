using System;
using UnityEngine;

namespace Terra.Structure {
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
		/// Temperature constraint if enabled
		/// </summary>
		public Constraint TemperatureConstraint = new Constraint(0, 1f);

		/// <summary>
		/// Moisture map constraint if enabled
		/// </summary>
		public Constraint MoistureConstraint = new Constraint(0, 1f);

		/// <summary>
		/// When are constraints considered fulfilled? Either when all constraints 
		/// are met or when any are.
		/// </summary>
		public ConstraintMixMethod MixMethod = ConstraintMixMethod.AND;

		/// <summary>
		/// Will this biome only appear between constrained 
		/// heights?
		/// </summary>
		public bool IsHeightConstrained = false;

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
	}
}
