using System;
using System.Collections;
using System.Collections.Generic;
using Terra.Terrain;

namespace Terra.Structures {
	public struct Neighborhood : IEnumerable<Tile> {
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

		/// <summary>
		/// Enumerate through all non-null Tiles in this neighborhood
		/// </summary>
		public IEnumerator<Tile> GetEnumerator() {
			Tile[] all = { Up, Right, Down, Left };

			foreach(Tile t in all) {
				if (t != null) {
					yield return t;
				}
			}
		}

		/// <summary>
		/// Enumerate through all non-null Tiles in this neighborhood
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
