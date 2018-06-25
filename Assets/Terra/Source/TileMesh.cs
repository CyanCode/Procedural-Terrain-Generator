using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Terrain {
	/// <summary>
	/// Represents the terrain mesh attached to a Tile
	/// </summary>
	class TileMesh {
		private Tile Tile;
		
		public TileMesh(Tile tile) {
			Tile = tile;
		}

		/// <summary>
		/// Sets this mesh's resolution based on the passed level of 
		/// detail. The associated heightmap is computed if it hasn't 
		/// been already. Otherwise, the cached heightmap is used.
		/// </summary>
		/// <param name="level"></param>
		public void SetActiveLOD(LOD level) {
			
		}
	}

	/// <summary>
	/// Enumeration of the three different levels of detail 
	/// a TileMesh can have. Low, medium, and high.
	/// </summary>
	public enum LOD {
		LOW, MEDIUM, HIGH
	}

	/// <summary>
	/// Handles assigning LOD levels to individual TerrainTiles 
	/// and changing the resolution at runtime.
	/// </summary>
	public class LODLevel {
		private Tile Tile;
		private List<Level> Levels;

		public LODLevel(Tile tile) {
			Tile = tile;
			Levels = new List<Level>();
		}

		public LODLevel(Tile tile, List<Level> levels) {
			Tile = tile;
			Levels = levels;
		}

		public LODLevel(Tile tile, Level level) : this(tile) {
			Levels.Add(level);
		}

		/// <summary>
		/// Sets the available LOD levels of this Tile
		/// </summary>
		/// <param name="levels"></param>
		public void SetLODLevels(List<Level> levels) {
			Levels = levels;
		}

		/// <summary>
		/// Adds a new LOD level to this Tile
		/// </summary>
		/// <param name="level"></param>
		public void AddLODLevel(Level level) {
			Levels.Add(level);
		}

		/// <summary>
		/// Activates an LOD Level and applies the changes to the 
		/// assigned Tile. If the vertex map hasn't already been 
		/// computed for the requested LOD level, it is computed.
		/// </summary>
		/// <param name="level">level to activate</param>
		/// <exception cref="LevelNotSetException">Thrown when the passed level hasn't been set</exception>
		public void ActivateLODLevel(int level) {
			Level l = GetLevel(level);
			if (l == null) {
				throw new LevelNotSetException();
			}

			if (!l.HasHeightmap()) {
				PrecomputeLODLevel(l.LevelNum);
			}

			var md = Tile.CreateRawMesh(l.VertexMap, l.Resolution);
			Tile.RenderRawMeshData(md);
		}

		/// <summary>
		/// Precomputes the necessary information needed by the 
		/// passed LOD level to change the resolution of the 
		/// current Tile. Consider calling this before 
		/// <see cref="ActivateLODLevel(int)"></see> as computing a heightmap 
		/// can take time. This method is thread safe.
		/// </summary>
		/// <param name="level">level to precompute</param>
		/// <exception cref="LevelNotSetException">Thrown when the passed level hasn't been set</exception>
		public void PrecomputeLODLevel(int level) {
			Level l = GetLevel(level);
			if (l == null) {
				throw new LevelNotSetException();
			}

			//Setup vertex map
			l.VertexMap = new Vector3[l.Resolution * l.Resolution];
			for (int x = 0; x < l.Resolution; x++) {
				for (int z = 0; z < l.Resolution; z++) {
					l.VertexMap[x + z * l.Resolution] = Tile.GetPositionAt(x, z, l.Resolution);
				}
			}
		}

		/// <summary>
		/// Finds the LOD level that is internally stored and 
		/// returns it if its found.
		/// </summary>
		/// <param name="level">level to search for</param>
		/// <returns>Level instance if found, null otherwise</returns>
		public Level GetLevel(int level) {
			foreach (Level l in Levels) {
				if (l.LevelNum == level) {
					return l;
				}
			}

			return null;
		}

		/// <summary>
		/// Container class for information level of detail data. 
		/// Contains the resolution of the heightmap and the cached 
		/// heightmap if one is available.
		/// </summary>
		public class Level {
			public int Resolution = 128;

			/// <summary>
			/// Array of Vector3 positions in world space that each represent 
			/// a single vertex. Indicies should be accessed using the following 
			/// equation: x + z * resolution = position
			/// </summary>
			public Vector3[] VertexMap = null;

			public int LevelNum;

			public Level(int resolution, int level) {
				Resolution = resolution;
				LevelNum = level;
			}

			public bool HasHeightmap() {
				return VertexMap != null;
			}
		}

		/// <summary>
		/// This exception occurs when a numerical LOD level 
		/// is passed but this LODLevel instance hasn't been assigned 
		/// the passed level.
		/// </summary>
		public class LevelNotSetException: Exception {
			public LevelNotSetException(string message) : base(message) { }

			public LevelNotSetException() : base("A LOD level was not set for the passed number") { }
		}
	}
}
