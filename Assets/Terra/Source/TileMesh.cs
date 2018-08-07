﻿using System;
using System.Collections.Generic;
using System.Threading;
using Terra.Data;
using UnityEngine;
using System.Linq;
using System.Xml.XPath;
using Object = System.Object;

namespace Terra.Terrain {
	/// <summary>
	/// Represents the terrain mesh attached to a Tile
	/// </summary>
	[Serializable]
	public class TileMesh: ISerializationCallbackReceiver {
		/// <summary>
		/// Resolution of this mesh
		/// </summary>
		public Resolution MeshResolution { get; private set; }

		/// <summary>
		/// Resolution of the heightmap
		/// </summary>
		public int HeightmapResolution { get; private set; }

		/// <summary>
		/// List of meshes that have already been computed and
		/// their resolutions.
		/// </summary>
		public List<KeyValuePair<int, MeshData>> ComputedMeshes { get; private set; }

		/// <summary>
		/// The heightmap that is used when creating a mesh. Updating 
		/// <see cref="LodLevel"/> does not update <see cref="Heightmap"/>. 
		/// Call <see cref="CreateHeightmap"/> or <see cref="CreateHeightmapAsync"/> 
		/// instead.
		/// </summary>
		public float[,] Heightmap { get; private set; }

		public LodData.LodLevel LodLevel { 
			get { return _lodLevel; }
			set {
				_lodLevel = value;
				HeightmapResolution = value.MapResolution;
				MeshResolution = (Resolution)value.MeshResolution;
			}
		}

		/// <summary>
		/// The mesh that corresponds with the set <see cref="MeshResolution"/> if 
		/// it has already been computed. Call <see cref="CreateMesh"/> to create or 
		/// <see cref="HasCreatedMeshAtResolution"/> to check if it has already been created.
		/// </summary>
		public Mesh ActiveMesh {
			get {
				if (ComputedMeshes.Exists(kvp => kvp.Key == (int)MeshResolution)) {
					return ComputedMeshes.Find(kvp => kvp.Key == (int)MeshResolution).Value.Mesh;
				}

				return null;
			}
		}

		/// <summary>
		/// Internal <see cref="LodLevel"/>
		/// </summary>
		[SerializeField]
		private LodData.LodLevel _lodLevel;

		/// <summary>
		/// Tile using this TileMesh
		/// </summary>
		[SerializeField]
		private Tile _tile;

		/// <summary>
		/// If this TileMesh needs to update its generator
		/// </summary>
		private bool _genNeedsUpdating = true;

		private readonly object _asyncMeshLock = new object();
		
		/// <summary>
		/// Constructs a new TileMesh instance
		/// </summary>
		/// <param name="tile">Tile to attach mesh to</param>
		/// <param name="lodLevel">LOD level to reference when creating heightmap and mesh</param>
		public TileMesh(Tile tile, LodData.LodLevel lodLevel) {
			_tile = tile;
			_lodLevel = lodLevel; 

			ComputedMeshes = new List<KeyValuePair<int, MeshData>>(3);
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
		
			if (_genNeedsUpdating) {
				sett.HeightMapData.UpdateGenerator();
				_genNeedsUpdating = false;
			}

			return sett.HeightMapData.GetValue(worldX / spread, worldZ / spread) * amp;
		}

		/// <summary>
		/// Creates a heightmap of resolution <see cref="HeightmapResolution"/>. If a 
		/// <see cref="Heightmap"/> of the same resolution or higher has already been 
		/// created, this method does nothing.
		/// A heightmap is 2D array of floats that represents the Y values (or heights) 
		/// of to-be created vertices in 3D space.
		/// </summary>
		public void CreateHeightmap() {
			if (Heightmap != null && (int)Math.Sqrt(Heightmap.Length) >= HeightmapResolution)
				return;

			Heightmap = new float[HeightmapResolution, HeightmapResolution];
			for (int x = 0; x < HeightmapResolution; x++) {
				for (int z = 0; z < HeightmapResolution; z++) {
					Vector2 localXZ = PositionToLocal(x, z, HeightmapResolution);
					Vector2 worldXZ = LocalToWorld(localXZ.x, localXZ.y);

					lock (_asyncMeshLock) {
						Heightmap[x, z] = HeightAt(worldXZ.x, worldXZ.y);
					}
				}
			}
		}

		/// <summary>
		/// Creates a heightmap of resolution <see cref="HeightmapResolution"/> asynchronously. 
		/// If a <see cref="Heightmap"/> of the same resolution or higher has already been 
		/// created, this method does nothing.
		/// A heightmap is 2D array of floats that represents the Y values (or heights) 
		/// of to-be created vertices in 3D space.
		/// </summary>
		/// <param name="onComplete">Called when the heightmap has been created</param>
		public void CreateHeightmapAsync(Action onComplete) {
			ThreadPool.QueueUserWorkItem(d => { //Worker thread
				CreateHeightmap();

				MTDispatch.Instance().Enqueue(onComplete);
			});
		}

		/// <summary>
		/// Check if the <see cref="Heightmap"/> has been created and if 
		/// the resolution is equal to or a lower square of the passed 
		/// <see cref="resolution"/>.
		/// </summary>
		/// <param name="resolution">Resolution to check</param>
		public bool HasHeightmapForResolution(int resolution) {
			return Heightmap != null && (int)Math.Sqrt(Heightmap.Length) <= resolution;
		}

		/// <summary>
		/// Sets <see cref="Heightmap"/> to null.
		/// </summary>
		public void ClearHeightmap() {
			Heightmap = null;
		}

		/// <summary>
		/// Creates a mesh by reading the <see cref="Heightmap"/> and 
		/// caches the result. If a mesh of the same resolution has already 
		/// been created, it is returned and (optionally) added to the scene. 
		/// In order to construct a mesh, the <see cref="HeightmapResolution"/> 
		/// must be greater than or equal to the <see cref="MeshResolution"/>.
		/// </summary>
		/// <param name="addToScene">Optionally disable adding the Mesh directly to the scene</param>
		public MeshData CreateMesh(bool addToScene = true) {
			MeshData computed = FindComputedForResolution((int)MeshResolution);
			if (computed != default(MeshData)) {
				return computed;
			}

			Vector3[] vertices = new Vector3[(int)MeshResolution * (int)MeshResolution];
			int increment = HeightmapResolution / (int)MeshResolution;

			for (int x = 0; x < (int)MeshResolution; x += increment) {
				for (int z = 0; z < (int)MeshResolution; z += increment) {
					Vector2 localXZ = PositionToLocal(x, z, (int)MeshResolution);

					lock (_asyncMeshLock) {
						float y = Heightmap[x, z];
						vertices[x / increment + z / increment * (int)MeshResolution] = new Vector3(localXZ.x, y, localXZ.y);
					}
				}
			}

			MeshData md = MeshDataFromVertices(vertices);
			ComputedMeshes.Add(new KeyValuePair<int, MeshData>((int)MeshResolution, md));

			if (addToScene) {
				var mf = _tile.GetMeshFilter();
				_tile.GetMeshRenderer();

				mf.sharedMesh = md.Mesh;
			}

			return md;
		}

		/// <summary>
		/// Whether this TileMesh has already computed a mesh 
		/// with the passed resolution.
		/// </summary>
		/// <param name="res">resolution to check</param>
		public bool HasCreatedMeshAtResolution(Resolution res) {
			return ComputedMeshes.Exists(kvp => kvp.Key == (int)res);
		}

		/// <summary>
		/// Calculates normals for the passed vertices and triangles in a method 
		/// similar to Unity's Mesh.RecalculateNormals method.
		/// </summary>
		/// <seealso cref="https://forum.unity.com/threads/procedural-mesh-and-normals-closed-solved.354900/"/>
		/// <param name="verts">vertices of mesh</param>
		/// <param name="normals">normals array to fill</param>
		/// <param name="tris">triangles from mesh</param>
		public void CalculateNormalsManaged(Vector3[] verts, ref Vector3[] normals, int[] tris) {
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
		/// Transforms the passed x and z incrementors into local coordinates.
		/// </summary>
		/// <param name="x">x position to transform</param>
		/// <param name="z">z position to transform</param>
		/// <param name="resolution">resolution of structure (mesh or heightmap)</param>
		/// <returns></returns>
		public Vector2 PositionToLocal(int x, int z, int resolution) {
			float length = TerraSettings.Instance.Generator.Length;
			float xLocal = ((float)x / (resolution - 1) - .5f) * length;
			float zLocal = ((float)z / (resolution - 1) - .5f) * length;

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
			int length = TerraSettings.Instance.Generator.Length;
			float worldX = localX + (_tile.GridPosition.X * length);
			float worldZ = localZ + (_tile.GridPosition.Z * length);

			return new Vector2(worldX, worldZ);
		}

		/// <summary>
		/// Creates the rest of the mesh by referencing the passed vertices 
		/// for normals, UVs, and triangles.
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

		/// <summary>
		/// Find the <see cref="ComputedMeshes"/> that has the passed 
		/// resolution as a key.
		/// </summary>
		/// <param name="resolution">Resolution to search for</param>
		/// <returns>MeshData if found, default(MeshData) otherwise</returns>
		private MeshData FindComputedForResolution(int resolution) {
			return ComputedMeshes.Find(kvp => kvp.Key == resolution).Value;
		}

		#region Serialization

		/// <summary>
		/// One dimensional representation of the heightmap that 
		/// Unity can serialize.
		/// </summary>
		[SerializeField, HideInInspector]
		private float[] _serializedHeightmap;

		[SerializeField, HideInInspector]
		private int[] _serializedMeshResolutions;

		[SerializeField, HideInInspector]
		private MeshData[] _serializedMeshData;

		public void OnBeforeSerialize() {
			//Heightmap
			if (Heightmap != null) {
				_serializedHeightmap = new float[HeightmapResolution * HeightmapResolution];

				for (int x = 0; x < HeightmapResolution; x++) {
					for (int z = 0; z < HeightmapResolution; z++) {
						_serializedHeightmap[x + z * HeightmapResolution] = Heightmap[x, z];
					}
				}
			}

			//ComputedMeshes
			if (ComputedMeshes != null) {
				_serializedMeshResolutions = new int[ComputedMeshes.Count];
				_serializedMeshData = new MeshData[ComputedMeshes.Count];

				for (int i = 0; i < ComputedMeshes.Count; i++) {
					_serializedMeshResolutions[i] = ComputedMeshes[i].Key;
					_serializedMeshData[i] = ComputedMeshes[i].Value;
				}
			}
		}

		public void OnAfterDeserialize() {
			//Heightmap
			if (_serializedHeightmap != null) {
				Heightmap = new float[HeightmapResolution, HeightmapResolution];

				for (int x = 0; x < HeightmapResolution; x++) {
					for (int z = 0; z < HeightmapResolution; z++) {
						Heightmap[x, z] = _serializedHeightmap[x + z * HeightmapResolution];
					}
				}
			}

			//ComputedMeshes
			if (_serializedMeshResolutions != null && _serializedMeshData != null) {
				ComputedMeshes = new List<KeyValuePair<int, MeshData>>();
				
				for (int i = 0; i < _serializedMeshResolutions.Length; i++) {
					int key = _serializedMeshResolutions[i];
					MeshData value = _serializedMeshData[i];

					ComputedMeshes.Add(new KeyValuePair<int, MeshData>(key, value));
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Enumeration of the three different levels of detail 
	/// a TileMesh can have. Low, medium, and high which 
	/// each correspond to a different mesh resolution.
	/// </summary>
	[Serializable]
	public enum Resolution : int {
		Low = 32, 
		Medium = 64,
		High = 128
	}
}
