using System;
using UnityEngine;

namespace Terra.Structure {
	/// <summary>
	/// Represents a Biome and its various settings
	/// </summary>
	[Serializable]
	public class BiomeData {
		[Serializable]
		public enum ConstraintMixMethod {
			AND, OR
		}

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

		/// <summary>
		/// Does the passed <see cref="Tile"/> pass all of the constraints 
		/// for this biome? 
		/// </summary>
		/// <param name="t">Tile to compare with</param>
		/// <param name="x">X coordinate of the heightmap</param>
		/// <param name="z">Z coordinate of the heightmap</param>
		/// <returns></returns>
		//public bool TileFitsConstraints(Tile t, int resolution, int x, int z) {
		//	var tm = _settings.TemperatureMapData;
		//	var mm = _settings.MoistureMapData;

		//	if (IsTemperatureConstrained && !tm.HasGenerator()) return false;
		//	if (IsMoistureConstrained && !mm.HasGenerator()) return false;

		//	var height = t.MeshManager.Heightmap[x, z];
		//	var local = t.MeshManager.PositionToLocal(x, z, _res);
		//	var world = t.MeshManager.LocalToWorld(local.x, local.y);
		//	var wx = world.x;
		//	var wz = world.y;

		//	bool passHeight = b.IsHeightConstrained && b.HeightConstraint.Fits(height) || !b.IsHeightConstrained;
		//	bool passTemp = b.IsTemperatureConstrained && b.TemperatureConstraint.Fits(tm.GetValue(wx, wz)) || !b.IsTemperatureConstrained;
		//	bool passMoisture = b.IsMoistureConstrained && b.MoistureConstraint.Fits(mm.GetValue(wx, wz)) || !b.IsMoistureConstrained;

		//	if (passHeight && passTemp && passMoisture) {
		//		chosen = b;
		//	}
		//}
		//TODO remove ^
// //todo remove
//		/// <summary>
//		/// Create a preview texture for the passed list of biomes by 
//		/// coloring biomes that pass constraints.
//		/// </summary>
//		public static Texture2D GetPreviewTexture(int width, int height, float zoom = 1f) {
//			Texture2D tex = new Texture2D(width, height);
//
//			for (int i = 0; i < width; i++) {
//				for (int j = 0; j < height; j++) {
//					BiomeData b = TerraSettings.Instance.GetBiomeAt(i / zoom, j / zoom);
//					tex.SetPixel(i, j, b == null ? Color.black : b.Color);
//				}
//			}
//
//			tex.Apply();
//			return tex;
//		}
	}
}
