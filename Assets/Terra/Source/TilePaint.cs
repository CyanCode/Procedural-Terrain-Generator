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

		private Tile _tile;

		public TilePaint(Tile tile) {
			_tile = tile;
		}

		/// <summary>
		/// TODO Summary
		/// </summary>
		public void Paint() {
			
		}

		/// <summary>
		/// Updates the textures that are fed into the splatmapping shader. 
		/// These includes <see cref="Controls"/> and <see cref="Splats"/>.
		/// </summary>
		public void UpdateTextures() {
			//use TileMesh to pull preexisting mesh information
			//where to store heightmap..
		}
	}
}
