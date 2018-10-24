using System;
using Terra.Terrain;

namespace Terra.Structure {
	public struct Neighborhood {
		public Tile Up;
		public Tile Right;
		public Tile Down;
		public Tile Left;

		public Neighborhood(Tile up, Tile right, Tile down, Tile left) {
			Up = up;
			Right = right;
			Down = down;
			Left = left;
		}

		/// <summary>
		/// Creates a <see cref="Neighborhood"/> with the passed array of Tiles 
		/// representing up, right, down, and left in that order.
		/// </summary>
		/// <param name="tiles"></param>
		public Neighborhood(Tile[] tiles) {
			if (tiles.Length != 4) {
				throw new ArgumentException("A neighborhood can only be created with an array of length 4.");
			}

			Up = tiles[0];
			Right = tiles[1];
			Down = tiles[2];
			Left = tiles[3];
		}
	}
}
