using System;
using System.Collections.Generic;
using Terra.CoherentNoise;
using UnityEngine;

namespace Terra.Terrain {
	/// <summary>
	/// Represents the terrain mesh attached to a Tile
	/// </summary>
	public class TileMesh {
		/// <summary>
		/// Resolution of this mesh
		/// </summary>
		public int MeshResolution { get; private set; }

		/// <summary>
		/// List of existing mesh resolutions sorted by key 
		/// (numerical representation of the resolution)
		/// </summary>
		private SortedList<int, MeshData> ExistingResolutions = new SortedList<int, MeshData>();

		/// <summary>
		/// Tile using this TileMesh
		/// </summary>
		private Tile Tile;
		
		/// <inheritdoc />
		public TileMesh(Tile tile, Resolution res = Resolution.HIGH) : this(tile, (int)res) { }

		/// <summary>
		/// Constructs a new TileMesh instance.
		/// </summary>
		/// <param name="tile">Tile to attach mesh to</param>
		/// <param name="res">Resolution of the mesh (default 128)</param>
		public TileMesh(Tile tile, int res = 128) {
			Tile = tile;
			MeshResolution = res;
		}

		/// <summary>
		/// Computes a mesh with the passed resolution regardless 
		/// of whether this resolution mesh has already been computed 
		/// or not. Once the MeshData has been computed it is cached.
		/// </summary>
		public void ComputeMesh() {
			if (ExistingResolutions.ContainsKey(MeshResolution)) {
				//Remove existing MeshData if it already exists
				ExistingResolutions.Remove(MeshResolution);
			}

			MeshData md = new MeshData();
			//TODO: Get generator at point on Tile (implement in get generator at point in the tile class)
			

			//l.VertexMap = new Vector3[l.Resolution * l.Resolution];
			//for (int x = 0; x < l.Resolution; x++) {
			//	for (int z = 0; z < l.Resolution; z++) {
			//		l.VertexMap[x + z * l.Resolution] = Tile.GetPositionAt(x, z, l.Resolution);
			//	}
			//}
		}

		/// <summary>
		/// Sets the mesh resolution to use when creating new meshes
		/// </summary>
		/// <param name="res">resolution to set</param>
		public void SetResolution(Resolution level) {
			MeshResolution = (int)level;
		}

		/// <summary>
		/// Sets the mesh resolution to use when creating new meshes
		/// </summary>
		/// <param name="res">resolution to set</param>
		public void SetResolution(int res) {
			MeshResolution = res;
		}

		/// <summary>
		/// Whether this TileMesh has already computed a mesh 
		/// with the passed resolution.
		/// </summary>
		/// <param name="res">resolution to check</param>
		public bool HasComputedResolution(Resolution res) {
			return HasComputedResolution((int)res);
		}

		/// <summary>
		/// Whether this TileMesh has already computed a mesh 
		/// with the passed resolution.
		/// </summary>
		/// <param name="res">resolution to check</param>
		public bool HasComputedResolution(int res) {
			return ExistingResolutions.ContainsKey(res);
		}

		/// <summary>
		/// Calculates normals for the passed vertices and triangles in a method 
		/// similar to Unity's Mesh.RecalculateNormals method.
		/// </summary>
		/// <seealso cref="https://forum.unity.com/threads/procedural-mesh-and-normals-closed-solved.354900/"/>
		/// <param name="verts">vertices of mesh</param>
		/// <param name="normals">normals array to fill</param>
		/// <param name="tris">triangles from mesh</param>
		public static void CalculateNormalsManaged(Vector3[] verts, ref Vector3[] normals, int[] tris) {
			for (int i = 0; i < tris.Length; i += 3) {
				int tri0 = tris[i];
				int tri1 = tris[i + 1];
				int tri2 = tris[i + 2];

				Vector3 vert0 = verts[tri0];
				Vector3 vert1 = verts[tri1];
				Vector3 vert2 = verts[tri2];
				Vector3 normal = new Vector3() {
					x = vert0.y * vert1.z - vert0.y * vert2.z - vert1.y * vert0.z + vert1.y * vert2.z + vert2.y * vert0.z - vert2.y * vert1.z,
					y = -vert0.x * vert1.z + vert0.x * vert2.z + vert1.x * vert0.z - vert1.x * vert2.z - vert2.x * vert0.z + vert2.x * vert1.z,
					z = vert0.x * vert1.y - vert0.x * vert2.y - vert1.x * vert0.y + vert1.x * vert2.y + vert2.x * vert0.y - vert2.x * vert1.y
				};

				normals[tri0] += normal;
				normals[tri1] += normal;
				normals[tri2] += normal;
			}

			for (int i = 0; i < normals.Length; i++) {
				Vector3 norm = normals[i];
				float invlength = 1.0f / (float)Math.Sqrt(norm.x * norm.x + norm.y * norm.y + norm.z * norm.z);

				normals[i].x = norm.x * invlength;
				normals[i].y = norm.y * invlength;
				normals[i].z = norm.z * invlength;
			}
		}

		/// <summary>
		/// Polls the passed Generator for a value at the passed x / z position. 
		/// Applies spread and amplitude to computation.
		/// </summary>
		/// <param name="xPos">X position to get value at</param>
		/// <param name="zPos">Z position to get value at</param>
		/// <param name="settings">Settings instance for amplitude and spread</param>
		/// <param name="gen">Generator to get value from</param>
		/// <returns></returns>
		public static float PollGenerator(float xPos, float zPos, TerraSettings settings, Generator gen) {
			float spread = 1f / (settings.Generator.Spread * settings.Generator.MeshResolution);
			return gen.GetValue(xPos * spread, zPos * spread, 0f) * settings.Generator.Amplitude;
		}

		/// <summary>
		/// Polls the passed Generator for a value at the passed x / z position. 
		/// Applies spread and amplitude from TerraSettings global instance to computation.
		/// </summary>
		/// <param name="xPos">X position to get value at</param>
		/// <param name="zPos">Z position to get value at</param>
		/// <returns>height value if available, float default otherwise (0.0)</returns>
		public static float PollGenerator(float xPos, float zPos) {
			var sett = TerraSettings.Instance;
			if (sett != null && sett.Generator.Graph.GetEndGenerator() != null) {
				return PollGenerator(xPos, zPos, sett, sett.Generator.Graph.GetEndGenerator());
			}

			return default(float);
		}

		/// <summary>
		/// Static version of <see cref="Terra.Terrain.Tile.CreateRawMesh(UnityEngine.Vector2,Terra.CoherentNoise.Generator)"/>
		/// Creates a "raw" mesh from the passed generator and settings specified 
		/// in <c>TerraSettings</c>. This method can be executed off the main thread.
		/// 
		/// Because of Unity's incompatibility with multithreading, a <see cref="MeshData"></see> 
		/// struct is returned with contents of the generated Mesh, instead of using the 
		/// <see cref="Terra.Terrain.Tile.Mesh"></see> class.
		/// </summary>
		/// <param name="position">Position in tile grid, used for polling generator</param>
		/// <param name="gen">Generator to apply</param>
		/// <returns>triangles, vertices, normals, and UVs of the generated mesh</returns>
		public static MeshData CreateRawMesh(TerraSettings settings, Vector2 position, Generator gen) {
			int res = settings.Generator.MeshResolution;
			float len = settings.Generator.Length;
			float spread = 1f / (settings.Generator.Spread * settings.Generator.MeshResolution);

			Vector3[] vertices = new Vector3[res * res];
			for (int z = 0; z < res; z++) {
				for (int x = 0; x < res; x++) {
					vertices[x + z * res] = GetPositionAt(x, z, res, settings, gen, position);
				}
			}

			Vector2[] uvs = new Vector2[vertices.Length];
			for (int v = 0; v < res; v++) {
				for (int u = 0; u < res; u++) {
					uvs[u + v * res] = new Vector2((float)u / (res - 1), (float)v / (res - 1));
				}
			}

			int nbFaces = (res - 1) * (res - 1);
			int[] triangles = new int[nbFaces * 6];
			int t = 0;
			for (int face = 0; face < nbFaces; face++) {
				int i = face % (res - 1) + (face / (res - 1) * res);

				triangles[t++] = i + res;
				triangles[t++] = i + 1;
				triangles[t++] = i;

				triangles[t++] = i + res;
				triangles[t++] = i + res + 1;
				triangles[t++] = i + 1;
			}

			Vector3[] normals = new Vector3[vertices.Length];
			TileMesh.CalculateNormalsManaged(vertices, ref normals, triangles);

			MeshData mesh = new MeshData();
			mesh.triangles = triangles;
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uvs = uvs;

			return mesh;
		}

		/// <summary>
		/// Static version of <see cref="GetPositionAt(int, int, int)"/>
		/// </summary>
		/// <returns></returns>
		public static Vector3 GetPositionAt(int xPos, int zPos, int resolution, TerraSettings settings, Generator gen, Vector2 position) {
			float amp = settings.Generator.Amplitude;
			float spread = settings.Generator.Spread;
			float length = settings.Generator.Length;

			float worldX = ((float)xPos / (resolution - 1) - .5f) * length;
			float worldZ = ((float)zPos / (resolution - 1) - .5f) * length;
			float worldY = gen.GetValue(((position.x * length) + xPos) * spread,
				               ((position.y * length) + zPos) * spread, 0f) * amp;

			return new Vector3(worldX, worldY, worldZ);
		}
	}

	/// <summary>
	/// Enumeration of the three different levels of detail 
	/// a TileMesh can have. Low, medium, and high which 
	/// each correspond to a different mesh resolution.
	/// </summary>
	public enum Resolution : int {
		LOW = 32, 
		MEDIUM = 64,
		HIGH = 128
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
