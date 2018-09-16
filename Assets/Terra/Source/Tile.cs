using System;
using UnityEngine;
using Terra.Data;

namespace Terra.Terrain {
	/// <summary>
	///	Tile represents a Terrain gameobject in the scene. 
	///	This class handles the instantiation of Terrain, noise, 
	///	position, texture, and detail application.
	/// </summary>
	[ExecuteInEditMode]
	public class Tile: MonoBehaviour, ISerializationCallbackReceiver {
		private TerraSettings _settings { get { return TerraSettings.Instance; } }

		[SerializeField]
		private TilePaint _painter;
		[SerializeField]
		private TileMesh _meshManager;

		[HideInInspector]
		public bool IsColliderDirty = false;

		/// <summary>
		/// Position of this Tile in the grid of Tiles
		/// </summary>
		public GridPosition GridPosition { get; private set; }

		/// <summary>
		/// Create and manage mesh(es) attached to this Tile. This 
		/// provides an interface for creating and showing meshes of 
		/// varying resolutions.
		/// </summary>
		public TileMesh MeshManager {
			get {
				if (_meshManager == null) {
					_meshManager = new TileMesh(this, GetLodLevel());
				}

				return _meshManager;
			}
			set {
				_meshManager = value;
			}
		}

		/// <summary>
		/// Handles "painting" of this Tile through a splatmap that is 
		/// applied to each MeshRenderer.
		/// </summary>
		public TilePaint Painter {
			get {
				if (_painter == null) {
					_painter = new TilePaint(this);
				}

				return _painter;
			}
			set {
				_painter = value;
			}
		}

		/// <summary>
		/// The LOD level for this Tile. This value can change if 
		/// <see cref="GridPosition"/> is modified.
		/// </summary>
		public LodData.LodLevel LodLevel {
			get {
				return _settings.Generator.Lod.GetLevelForRadius((int)GridPosition.Distance(new GridPosition(0, 0)));
			}
		}

		/// <summary>
		/// Creates a gameobject with an attached Tile component and 
		/// places it in the scene. This is a convienence method and is not required 
		/// for correct tile creation.
		/// </summary>
		/// <param name="name">Name of the created gameobject</param>
		/// <returns>The attached Tile component</returns>
		public static Tile CreateTileGameobject(string name) {
			GameObject go = new GameObject(name);
			Tile tt = go.AddComponent<Tile>();

			return tt;
		}

		/// <summary>
		/// Fully constructs this Tile. This includes creating a Mesh, painting 
		/// the terrain, and adding details (grass, objects, etc.)
		/// 
		/// By default, calculating heights is done off of the main thread but 
		/// can be disabled.
		/// </summary>
		/// <param name="onComplete">Called after all calculations have completed. 
		/// <see cref="onComplete"/>Can be null if the result is not needed.</param>
		/// <param name="async">Perform mesh computation asynchronously</param>
		public void Generate(Action onComplete, bool async = true) {
			if (async) {
				MeshManager.CreateHeightmapAsync(() => {
					MeshManager.CreateMesh();
					PostCreateMeshGenerate();

					if (onComplete != null) {
						onComplete();
					}
				});
			} else {
				MeshManager.CreateHeightmap();
				MeshManager.CreateMesh();
				PostCreateMeshGenerate();

				if (onComplete != null) {
					onComplete();
				}
			}
		}

		/// <summary>
		/// Updates this TerrainTiles position by taking a Vector2 where 
		/// the x and y values are integers on a grid. Internally the x and y values 
		/// are multiplied by the Length of the mesh specified in TerraSettings
		/// </summary> 
		/// <param name="position">Position to set the Tile to (ie [1,0])</param>
		/// <param name="transformInScene">Move this Tile's gameobject to match position change?</param>
		public void UpdatePosition(GridPosition position, bool transformInScene = true) {
			GridPosition = position;

			//Update TileMesh LOD level
			MeshManager.LodLevel = GetLodLevel();

			if (transformInScene) {
				int len = _settings.Generator.Length;
				transform.position = new Vector3(position.X * len, 0f, position.Z * len);
			}
		} 

		/// <summary>
		/// Get the MeshFilter attached to this gameobject. If one doesn't 
		/// exist, it is added and returned.
		/// </summary>
		public MeshFilter GetMeshFilter() {
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null) {
				mf = gameObject.AddComponent<MeshFilter>();
			}

			return mf;
		}

		/// <summary>
		/// Get the MeshRenderer attached to this gameobject. If one doesn't 
		/// exist, it is added and returned.
		/// </summary>
		public MeshRenderer GetMeshRenderer() {
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null) {
				mr = gameObject.AddComponent<MeshRenderer>();
			}

			return mr;
		}

		/// <summary>
		/// Creates a map of biomes with the passed <see cref="resolution"/> and 
		/// the heightmap created in <see cref="MeshManager"/>.
		/// Points are polled along this <see cref="Tile"/>.
		/// </summary>
		/// <returns>BiomeData, null if no Heightmap has been created first</returns>
		//public BiomeData[,] GetBiomeMap(int resolution) {
		//	if (MeshManager == null || !MeshManager.HasHeightmapForResolution(resolution)) {
		//		Debug.LogWarning("Cannot create biome map without an available heightmap.");
		//		return null;
		//	}

		//	BiomeData[,] map = new BiomeData[resolution,resolution];
		//	int increment = MeshManager.HeightmapResolution / resolution;
			
		//	for (int x = 0; x < resolution; x += increment) {
		//		for (int z = 0; z < resolution; z += increment) {
		//			BiomeData chosen = null;
					
		//			foreach (BiomeData b in _settings.BiomesData) {
		//				var tm = _settings.TemperatureMapData;
		//				var mm = _settings.MoistureMapData;

		//				if (b.IsTemperatureConstrained && !tm.HasGenerator()) continue;
		//				if (b.IsMoistureConstrained && !mm.HasGenerator()) continue;

		//				var height = MeshManager.Heightmap[x, z];
		//				var local = MeshManager.PositionToLocal(x, z, resolution);
		//				var world = MeshManager.LocalToWorld(local.x, local.y);
		//				var wx = world.x;
		//				var wz = world.y;

		//				bool passHeight = b.IsHeightConstrained && b.HeightConstraint.Fits(height) || !b.IsHeightConstrained;
		//				bool passTemp = b.IsTemperatureConstrained && b.TemperatureConstraint.Fits(tm.GetValue(wx, wz)) || !b.IsTemperatureConstrained;
		//				bool passMoisture = b.IsMoistureConstrained && b.MoistureConstraint.Fits(mm.GetValue(wx, wz)) || !b.IsMoistureConstrained;

		//				if (passHeight && passTemp && passMoisture) {
		//					chosen = b;
		//				}
		//			}

		//			map[x / increment, z / increment] = chosen;
		//		}
		//	}

		//	return map;
		//}

		/// <summary>
		/// Finishes the <see cref="Generate"/> method after the 
		/// mesh has been created. This exists as a convenience as 
		/// a mesh can be created asynchronously or synchronously but 
		/// the logic afterwards is the same.
		/// </summary>
		private void PostCreateMeshGenerate() {
			Painter.Paint();
		}

		/// <summary>
		/// Gets the LOD level for this tile based off of its <see cref="GridPosition"/>.
		/// </summary>
		/// <returns>LOD level</returns>
		private LodData.LodLevel GetLodLevel() {
			int radius = (int)GridPosition.Distance(new GridPosition(0, 0));
			return _settings.Generator.Lod.GetLevelForRadius(radius);
		}

		#region Serialization

		[SerializeField]
		private GridPosition _serializedGridPosition;

		public void OnBeforeSerialize() {
			//Grid Position
			_serializedGridPosition = GridPosition;
		}

		public void OnAfterDeserialize() {
			//Grid Position
			GridPosition = _serializedGridPosition;
		}

		#endregion
	}

	/// <summary>
	/// An implementation of <see cref="Mesh"/> that does not return 
	/// copies of its' parameters.
	/// </summary>
	[Serializable]
	public struct MeshData {
		public Vector3[] Vertices { 
			get { return _vertices; }
			set {
				_vertices = value;
				_meshDirty = true;
			}
		}
		public Vector3[] Normals {
			get { return _normals; }
			set {
				_normals = value;
				_meshDirty = true;
			}
		}
		public Vector2[] Uvs {
			get { return _uvs; }
			set {
				_uvs = value;
				_meshDirty = true;
			}
		}
		public int[] Triangles {
			get { return _triangles; }
			set {
				_triangles = value;
				_meshDirty = true;
			}
		}

		//Private instances of public variables for 
		//allowing mesh reconstruction on value change
		[SerializeField]
		private Vector3[] _vertices;
		[SerializeField]
		private Vector3[] _normals;
		[SerializeField]
		private Vector2[] _uvs;
		[SerializeField]
		private int[] _triangles;

		/// <summary>
		/// The <see cref="Mesh"/> class representation of this MeshData. 
		/// Internally, the construction of this Mesh instance is done upon 
		/// access if any of the instance variables have changed.
		/// </summary>
		public Mesh Mesh {
			get {
				if (_mesh == null || _meshDirty) {
					_mesh = new Mesh {
						vertices = Vertices,
						normals = Normals,
						uv = Uvs,
						triangles = Triangles
					};
					_meshDirty = false;
				}

				return _mesh;
			}
		}

		public static bool operator ==(MeshData lhs, MeshData rhs) {
			return lhs._vertices == rhs._vertices && lhs._normals == rhs._normals &&
				lhs._uvs == rhs._uvs && lhs._triangles == rhs._triangles;
		}

		public static bool operator !=(MeshData lhs, MeshData rhs) {
			return !(lhs == rhs);
		}

		private Mesh _mesh;
		private bool _meshDirty;
	}
}