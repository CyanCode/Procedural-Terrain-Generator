using UnityEngine;

namespace Terra.Terrain {
	public class TilePaint {
		/// <summary>
		/// List of control texture maps. 1 control map holds placement 
		/// information for up to 4 splat maps. This is a cached result 
		/// from calling <see cref="UpdateTextures"/>
		/// </summary>
		public Texture2D[] Controls;

		/// <summary>
		/// List of textures that are placed (splatted) onto the terrain.
		/// This is a cached result from calling <see cref="UpdateTextures"/>
		/// </summary>
		public Texture2D[] Splats;

		private readonly Tile _tile;

		public TilePaint(Tile tile) {
			_tile = tile;
		}

		/// <summary>
		/// TODO Summary
		/// </summary>
		public void Paint() {
			ApplyDefaultMaterial();
		}

		/// <summary>
		/// Updates the textures that are fed into the splatmapping shader. 
		/// These includes <see cref="Controls"/> and <see cref="Splats"/>.
		/// </summary>
		public void UpdateTextures() {
			//use TileMesh to pull preexisting mesh information
			//where to store heightmap..
		}

		/// <summary>
		/// Applies the Unity default material to the terrain if one is not 
		/// already applied.
		/// </summary>
		private void ApplyDefaultMaterial() {
			if (_tile.GetMeshRenderer() == null || _tile.GetMeshRenderer().sharedMaterial != null)
				return;

			Shader s = Shader.Find("Diffuse");
			if (s == null) {
				Debug.Log("Failed to find default shader when creating terrain");
				return;
			}

			Material material = new Material(s);
			_tile.GetMeshRenderer().sharedMaterial = material;
		}
	}
}
