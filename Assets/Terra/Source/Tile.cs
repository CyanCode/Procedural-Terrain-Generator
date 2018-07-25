using System;
using UnityEngine;
using Terra.Data;
using Terra.Terrain.Detail;

namespace Terra.Terrain {
	/// <summary>
	///	Tile represents a Terrain gameobject in the scene. 
	///	This class handles the instantiation of Terrain, noise application, 
	///	position, and texture application.
	/// </summary>
	public class Tile: MonoBehaviour {
		[HideInInspector]
		public bool IsColliderDirty = false;
		public Mesh Terrain { get; private set; }
		public Vector2 Position { get; private set; }

		public TileMesh Mesh;
		public TilePaint Painter;
		
		private TerraSettings _settings;

		void OnEnable() {
			Mesh = new TileMesh(this);
			Painter = new TilePaint(this);

			_settings = TerraSettings.Instance;
			if (_settings == null) {
				Debug.LogError("Cannot find a TerraSettings object in the scene");
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

			//Perform initilization before OnEnable
			if (tt._settings == null) tt._settings = TerraSettings.Instance;
			if (tt._settings == null) {
				Debug.LogError("Cannot find a TerraSettings object in the scene");
			}

			return tt;
		}

		/// <summary>
		/// Fully constructs this Tile. This includes creating a Mesh, painting 
		/// the terrain, and adding details (grass, objects, etc.)
		/// 
		/// By default, calculating heights is done off of the main thread but 
		/// can be disabled.
		/// </summary>
		/// <param name="onComplete">If <see cref="async"/> is true, <see cref="onComplete"/> 
		/// is called after all calculations have completed. onComplete can be null if the result 
		/// is not needed.</param>
		/// <param name="async">Perform mesh computation asynchronously</param>
		public void Generate(Action onComplete, bool async = true) {
			if (async) {
				Mesh.CreateMeshAsync(md => {
					PostCreateMeshGenerate();

					if (onComplete != null) {
						onComplete();
					}
				});
			} else {
				PostCreateMeshGenerate();
			}
		}

		/// <summary>
		/// Updates this TerrainTiles position by taking a Vector2 where 
		/// the x and y values are integers on a grid. Internally the x and y values 
		/// are multiplied by the Length of the mesh specified in TerraSettings
		/// </summary>
		/// <param name="position">Position to set the Tile to (ie [1,0])</param>
		public void UpdatePosition(Vector2 position) {
			Position = position;
			int len = _settings.Generator.Length;
			transform.position = new Vector3(position.x * len, 0f, position.y * len);
		}

		/// <summary>
		/// Generates and applies new MeshCollider for the tile if no collider 
		/// exists currently or <code>IsColliderDirty</code> is true.
		/// </summary>
		public void GenerateCollider() {
			if (gameObject.GetComponent<MeshCollider>() == null || IsColliderDirty) {
				MeshCollider collider = gameObject.AddComponent<MeshCollider>();
				collider.sharedMesh = Terrain;

				TerraEvent.TriggerOnMeshColliderDidForm(gameObject, collider);
			}
		}

		/// <summary>
		/// Applies the custom material specified in TerraSettings to the associated TerrainObject.
		/// </summary>
		/// <param name="mat">Custom material to apply</param>
		public void ApplyCustomMaterial() {
			TerraEvent.TriggerOnCustomMaterialWillApply(gameObject);

			MeshRenderer mr = GetComponent<MeshRenderer>();
			mr.sharedMaterial = _settings.CustomMaterial;
			TerraEvent.TriggerOnCustomMaterialDidApply(gameObject);
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
	}

	/// <summary>
	/// An implementation of <see cref="Mesh"/> that does not return 
	/// copies of its' parameters.
	/// </summary>
	public struct MeshData {
		public Vector3[] Vertices;
		public Vector3[] Normals;
		public Vector2[] Uvs;
		public int[] Triangles;

		/// <summary>
		/// The <see cref="Mesh"/> class representation of this MeshData. 
		/// Internally, the construction of this Mesh instance is only done 
		/// once as the result is cached after the first construction.
		/// </summary>
		public Mesh Mesh {
			get {
				if (_mesh == null) {
					_mesh = new Mesh {
						vertices = Vertices,
						normals = Normals,
						uv = Uvs,
						triangles = Triangles
					};
				}

				return _mesh;
			}
		}

		private Mesh _mesh;
	}
}