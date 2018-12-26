using System;
using UnityEngine;

namespace Terra.Structures {
	/// <summary>
	/// Represents a position in the grid of <see cref="Tile"/>s
	/// </summary>
	[Serializable]
	public struct GridPosition {
		public static GridPosition Zero {
			get {
				return new GridPosition(0, 0);
			}
		}

		public int X;
		public int Z;

		/// <summary>
		/// The list of neighboring <see cref="GridPosition"/>s ordered 
		/// as Top, Right, Bottom, Left.
		/// </summary>
		public GridPosition[] Neighbors {
			get {
				return new[] {
						new GridPosition(X, Z + 1),
						new GridPosition(X + 1, Z),
						new GridPosition(X, Z - 1),
						new GridPosition(X - 1, Z)
					};
			}
		}

		/// <summary>
		/// Create a GridPosition at the passed X and Z coordinates
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="z">Z coordinate</param>
		public GridPosition(int x, int z) {
			X = x;
			Z = z;
		}

		/// <summary>
		/// Creates a GridPosition from the passed XZ position in world space 
		/// and the length of a grid tile.
		/// </summary>
		/// <param name="worldXz">world x and z positions</param>
		/// <param name="length">length of grid tile</param>
		public GridPosition(Vector2 worldXz, float length) {
			int x = Mathf.RoundToInt(worldXz.x / length);
			int z = Mathf.RoundToInt(worldXz.y / length);

			X = x;
			Z = z;
		}

		/// <summary>
		/// Creates a GridPosition from the passed gameobject's position
		/// and the length of a grid tile.
		/// </summary>
		/// <param name="gameObject">Gameobject in the world</param>
		/// <param name="length">length of grid tile</param>
		public GridPosition(GameObject gameObject, float length) : 
			this(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z), length) { }

		/// <summary>
		/// The distance between <see cref="p"/> and this <see cref="GridPosition"/>
		/// </summary>
		public float Distance(GridPosition p) {
			float x = this.X - p.X;
			float z = this.Z - p.Z;

			return Mathf.Sqrt((x * x) + (z * z));
		}

		public static bool operator ==(GridPosition p1, GridPosition p2) {
			return p1.X == p2.X && p1.Z == p2.Z;
		}

		public static bool operator !=(GridPosition p1, GridPosition p2) {
			return !(p1 == p2);
		}

		public bool Equals(GridPosition other) {
			return X == other.X && Z == other.Z;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is GridPosition && Equals((GridPosition)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (X * 397) ^ Z;
			}
		}

		public override string ToString() {
			return "[" + X + ", " + Z + "]";
		}
	}
}