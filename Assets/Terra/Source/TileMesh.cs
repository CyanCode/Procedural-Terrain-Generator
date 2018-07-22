using System;
using System.Collections.Generic;
using System.Threading;
using Terra.Data;
using UnityEngine;
using Object = System.Object;

namespace Terra.Terrain {
	/// <summary>
	/// Represents the terrain mesh attached to a Tile
	/// </summary>
	public class TileMesh {
		/// <summary>
		/// Resolution of this mesh
		/// </summary>
		public Resolution MeshResolution { get; private set; }

		/// <summary>
		/// List of meshes that have already been computed. Sorted by 
		/// their resolutions.
		/// </summary>
		public SortedList<int, MeshData> ComputedMeshes { get; private set; }

		/// <summary>
		/// Tile using this TileMesh
		/// </summary>
		private Tile _tile;

		private readonly Object _asyncMeshLock = new Object();
		
		/// <summary>
		/// Constructs a new TileMesh instance.
		/// </summary>
		/// <param name="tile">Tile to attach mesh to</param>
		/// <param name="res">Resolution of the mesh (default 128)</param>
		public TileMesh(Tile tile, Resolution res = Resolution.High) {
			_tile = tile;
			MeshResolution = res;

			ComputedMeshes = new SortedList<int, MeshData>(3);
		}

		/// <summary>
		/// Creates a mesh of resolution <see cref="MeshResolution"/> 
		/// The result of this method is cached in <see cref="ComputedMeshes"/> 
		/// and overwrites the old mesh if one is already cached.
		/// 
		/// This method can only be called asynchronously when 
		/// <see cref="addToScene"/> is false. See <see cref="CreateMeshAsync"/>
		/// </summary>
		/// <param name="addToScene">Optionally disable adding the Mesh directly to the scene</param>
		public MeshData CreateMesh(bool addToScene = true) {
			Vector3[] vertices = new Vector3[(int)MeshResolution * (int)MeshResolution];

			for (int x = 0; x < (int)MeshResolution; x++) {
				for (int z = 0; z < (int)MeshResolution; z++) {
					Vector2 localXZ = PositionToLocal(x, z);
					Vector2 worldXZ = LocalToWorld(localXZ.x, localXZ.y);

					lock (_asyncMeshLock) {
						float y = HeightAt(worldXZ.x, worldXZ.y);

						vertices[x + z * (int)MeshResolution] = new Vector3(worldXZ.x, y, worldXZ.y);
					}
				}
			}

			MeshData md = MeshDataFromVertices(vertices);
			if (ComputedMeshes.ContainsKey((int)MeshResolution)) {
				ComputedMeshes.Remove((int)MeshResolution);
			}

			ComputedMeshes.Add((int)MeshResolution, md);

			if (addToScene) {
				var mf = _tile.GetMeshFilter();
				_tile.GetMeshRenderer();

				mf.sharedMesh = md.Mesh;
			}

			return md;
		}

		/// <summary>
		/// Creates a mesh of resolution <see cref="MeshResolution"/> 
		/// The result of this method is cached in <see cref="ComputedMeshes"/> 
		/// and overwrites the old mesh if one is already cached.
		/// 
		/// The mesh is computed asynchronously in this method. To create the mesh 
		/// on the main thread call <see cref="CreateMesh"/> instead.
		/// </summary>
		/// <param name="onComplete">Callback method that is executed once computation 
		/// has finished. Called on the main thread.</param>
		/// <param name="addToScene">Optionally disable adding the Mesh directly to the scene</param>
		public void CreateMeshAsync(Action<MeshData> onComplete, bool addToScene = true) {
			ThreadPool.QueueUserWorkItem(d => {
				MeshData md = CreateMesh(false);

				MTDispatch.Instance().Enqueue(() => { //Main Thread
					if (addToScene) {
						var mf = _tile.GetMeshFilter();
						_tile.GetMeshRenderer();

						mf.sharedMesh = md.Mesh;
					}

					onComplete(md);
				});
			});
		}

		/// <summary>
		/// Whether this TileMesh has already computed a mesh 
		/// with the passed resolution.
		/// </summary>
		/// <param name="res">resolution to check</param>
		public bool HasComputedResolution(Resolution res) {
			return ComputedMeshes.ContainsKey((int)res);
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
				Vector3 normal = new Vector3 {
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
		/// Transforms the passed x and z incrementors into local coordinates 
		/// of a Mesh plane.
		/// </summary>
		/// <param name="x">x position to transform</param>
		/// <param name="z">z position to transform</param>
		/// <returns></returns>
		public Vector2 PositionToLocal(int x, int z) {
			float length = TerraSettings.Instance.Generator.Length;

			float xLocal = ((float)x / ((int)MeshResolution - 1) - .5f) * length;
			float zLocal = ((float)z / ((int)MeshResolution - 1) - .5f) * length;

			return new Vector2(xLocal, zLocal);
		}

		/// <summary>
		/// Converts local X and Z coordinates to <see cref="Tile"/> world 
		/// coordinates.
		/// </summary>
		/// <param name="localX">Local x coordinate on mesh</param>
		/// <param name="localZ">Local z coordinate on mesh</param>
		/// <returns>World X and Z coordinates</returns>
		public Vector2 LocalToWorld(float localX, float localZ) {
			float worldX = localX + (_tile.Position.x * (int)MeshResolution);
			float worldZ = localZ + (_tile.Position.y * (int)MeshResolution);

			return new Vector2(worldX, worldZ);
		}

		/// <summary>
		/// Polls the Generator from <see cref="TerraSettings.HeightMapData"/> and 
		/// returns the height value found at [x, 0, z]. This method applies the 
		/// amplitude and spread from <see cref="TerraSettings"/> to the result.
		/// </summary>
		/// <param name="worldX">World x coordinate</param>
		/// <param name="worldZ">World z coordinate</param>
		/// <returns>height</returns>
		public float HeightAt(float worldX, float worldZ) {
			var sett = TerraSettings.Instance;
			var amp = sett.Generator.Amplitude;
			var spread = sett.Generator.Spread;
			var gen = sett.HeightMapData.Generator;
		
			//CoherentNoise considers Z to be up & down
			return gen.GetValue(worldX * spread, worldZ * spread, 0) * amp;
		}

		/// <summary>
		/// Creates the rest of the mesh by referencing the passed vertices 
		/// for normals, UVS, and triangles.
		/// </summary>
		/// <param name="vertices">Vertices of mesh</param>
		/// <returns>Filled MeshData</returns>
		private MeshData MeshDataFromVertices(Vector3[] vertices) {
			int res = (int)MeshResolution;

			//UVs
			Vector2[] uvs = new Vector2[vertices.Length];
			for (int v = 0; v < res; v++) {
				for (int u = 0; u < res; u++) {
					uvs[u + v * res] = new Vector2((float)u / (res - 1), (float)v / (res - 1));
				}
			}

			//Triangles
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

			//Normals
			Vector3[] normals = new Vector3[vertices.Length];
			CalculateNormalsManaged(vertices, ref normals, triangles);

			MeshData md = new MeshData();
			md.Triangles = triangles;
			md.Vertices = vertices;
			md.Normals = normals;
			md.Uvs = uvs;

			return md;
		}
	}

	/// <summary>
	/// Enumeration of the three different levels of detail 
	/// a TileMesh can have. Low, medium, and high which 
	/// each correspond to a different mesh resolution.
	/// </summary>
	public enum Resolution : int {
		Low = 32, 
		Medium = 64,
		High = 128
	}
}
