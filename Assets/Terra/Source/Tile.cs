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
	public class Tile: MonoBehaviour {
		private TerraSettings _settings { get { return TerraSettings.Instance; } }

		/// <summary>
		/// Is this Tile used for in-editor previewing?
		/// </summary>
		internal bool IsPreviewTile = false;

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
		public TileMesh MeshManager;

		/// <summary>
		/// Handles "painting" of this Tile through a splatmap that is 
		/// applied to each MeshRenderer.
		/// </summary>
		public TilePaint Painter;

		void Awake() {
			MeshManager = new TileMesh(this, GetLodLevel());
			Painter = new TilePaint(this);
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
		public void UpdatePosition(GridPosition position) {
			GridPosition = position;

			//Update TileMesh LOD level
			MeshManager.LodLevel = GetLodLevel();

			int len = _settings.Generator.Length;
			transform.position = new Vector3(position.X * len, 0f, position.Z * len);
		} 

		/// <summary>
		/// Generates and applies new MeshCollider for the tile if no collider 
		/// exists currently or <code>IsColliderDirty</code> is true.
		/// </summary>
		public void GenerateCollider() {
			if (gameObject.GetComponent<MeshCollider>() == null || IsColliderDirty) {
				MeshCollider collider = gameObject.AddComponent<MeshCollider>();
				collider.sharedMesh = MeshManager.ActiveMesh;

				TerraEvent.TriggerOnMeshColliderDidForm(gameObject, collider);
			}
		}

		/// <summary>
		/// Applies the passed Material to this Tile by setting the 
		/// material assigned to the MeshRenderer component.
		/// </summary>
		/// <param name="mat">Material to apply</param>
		public void ApplyMaterial(Material mat) {
			MeshRenderer mr = GetComponent<MeshRenderer>();
			mr.sharedMaterial = mat;
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
		/// Finishes the <see cref="Generate"/> method after the 
		/// mesh has been created. This exists as a convenience as 
		/// a mesh can be created asynchronously or synchronously but 
		/// the logic afterwards is the same.
		/// </summary>
		private void PostCreateMeshGenerate() {

		}

		/// <summary>
		/// Gets the LOD level for this tile based off of its <see cref="GridPosition"/>.
		/// </summary>
		/// <returns>LOD level</returns>
		private LodData.LodLevel GetLodLevel() {
			int radius = (int)GridPosition.Distance(new GridPosition(0, 0));
			return _settings.Generator.Lod.GetLevelForRadius(radius);
		}
	}

	/// <summary>
	/// An implementation of <see cref="Mesh"/> that does not return 
	/// copies of its' parameters.
	/// </summary>
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
		private Vector3[] _vertices;
		private Vector3[] _normals;
		private Vector2[] _uvs;
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