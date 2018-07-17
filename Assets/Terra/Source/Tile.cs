using System;
using UnityEngine;
using Terra.CoherentNoise;
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
		public DetailManager Details { get; private set; }

		public TileMesh Mesh;
		
		private TerraSettings _settings;

		void OnEnable() {
			Details = new DetailManager(this);
			//Mesh = new TileMesh(this);

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
		/// Creates a preview mesh from the passed generator and terrain settings.
		/// </summary>
		/// <param name="settings">Settings to apply to the mesh</param>
		/// <param name="gen">Optional generator to pull values from</param>
		/// <returns>A preview mesh</returns>
		public static Mesh GetPreviewMesh(TerraSettings settings, Generator gen) {
			MeshData md = TileMesh.CreateRawMesh(settings, new Vector2(0, 0), gen);

			Mesh mesh = new Mesh();
			mesh.vertices = md.Vertices;
			mesh.normals = md.Normals;
			mesh.uv = md.Uvs;
			mesh.triangles = md.Triangles;

			return mesh;
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
		/// Creates a Mesh with the length and resolution specified in 
		/// TerraSettings. Applies heights found in the noise generator graph 
		/// specified in TerraSettings.
		/// <list type="bullet">
		/// <item><description>Cannot be called off of the main thread</description></item>
		/// <item><description>Does not generate a MeshCollider, call <code>GenerateCollider</code> instead.</description></item>
		/// <item><description>Creates and attaches a MeshRenderer</description></item>
		/// </list>
		/// </summary>
		/// <param name="position">Position to place Mesh in the tile grid</param>
		/// <param name="renderOnCreation">If true, the attached MeshRenderer will be enabled after the mesh has been formed. 
		/// Otherwise, the attached MeshRenderer will be disabled by default.</param>
		public void CreateMesh(Vector2 position, bool renderOnCreation = true) {
			CreateMesh(position, _settings.Generator.Graph.GetEndGenerator(), renderOnCreation);
		}

		/// <summary>
		/// Creates a Mesh with the length and resolution specified in 
		/// TerraSettings. Applies heights found in the passed Generator param.
		/// To generate a mesh off of the main thread, use <see cref="CreateRawMesh(Vector2, Generator)"/>
		/// <list type="bullet">
		/// <item><description>Cannot be called off of the main thread</description></item>
		/// <item><description>Does not generate a MeshCollider, call <code>GenerateCollider</code> instead.</description></item>
		/// <item><description>Creates and attaches a MeshRenderer</description></item>
		/// </list>
		/// </summary>
		/// <param name="position">Position to place Mesh in the tile grid</param>
		/// <param name="generator">CoherentNoise generator used for applying noise values</param>
		/// <param name="renderOnCreation">If true, the attached MeshRenderer will be enabled after the mesh has been formed. 
		/// Otherwise, the attached MeshRenderer will be disabled by default.</param>
		public void CreateMesh(Vector2 position, Generator generator, bool renderOnCreation = true) {
			TerraEvent.TriggerOnMeshWillForm(gameObject);

			MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
			renderer.material = new Material(Shader.Find("Diffuse"));
			renderer.enabled = renderOnCreation;
			Terrain = gameObject.AddComponent<MeshFilter>().mesh;

			MeshData md = CreateRawMesh(position, generator);
			Terrain.vertices = md.Vertices;
			Terrain.triangles = md.Triangles;
			Terrain.uv = md.Uvs;
			Terrain.normals = md.Normals;

			UpdatePosition(position);
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
		/// Creates a raw mesh from the length specified in TerraSettings and from the 
		/// passed resolution. The passed vertices are directly passed into the MeshData 
		/// structure. This method is useful for dynamically changing the resolution of 
		/// the mesh at runtime (such as setting the LOD level).
		/// </summary>
		/// <param name="heightmap">vertices in world space to apply to the mesh</param>
		/// <param name="res">resolution of the heightmap</param>
		public MeshData CreateRawMesh(Vector3[] vertices, int res) {
			float len = _settings.Generator.Length;
			float spread = 1f / (_settings.Generator.Spread * _settings.Generator.MeshResolution);

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
			mesh.Triangles = triangles;
			mesh.Vertices = vertices;
			mesh.Normals = normals;
			mesh.Uvs = uvs;

			return mesh;
		}

		/// <summary>
		/// Creates a "raw" mesh from the passed generator and settings specified 
		/// in <c>TerraSettings</c>. This method can be executed off the main thread.
		/// 
		/// Because of Unity's incompatibility with multithreading, a <c>MeshData</c> 
		/// struct is returned with contents of the generated Mesh, instead of using the 
		/// <c>Mesh</c> class.
		/// </summary>
		/// <param name="position">Position in tile grid, used for polling generator</param>
		/// <param name="gen">Generator to apply</param>
		/// <returns>triangles, vertices, normals, and UVs of the generated mesh</returns>
		public MeshData CreateRawMesh(Vector2 position, Generator gen) {
			return TileMesh.CreateRawMesh(_settings, position, gen);
		}


		public Vector3 GetPositionAt(int xPos, int zPos, int resolution) {
			return TileMesh.GetPositionAt(xPos, zPos, resolution, _settings, _settings.Generator.Graph.GetEndGenerator(), Position);
		}

		/// <summary>
		/// Renders the passed MeshData as a Mesh in the scene 
		/// by creating a MeshRenderer and MeshFilter if they do 
		/// not exist already.
		/// </summary>
		/// <param name="data">Mesh information to apply</param>
		public void RenderRawMeshData(MeshData data) {
			if (gameObject.GetComponent<MeshRenderer>() == null) {
				MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
				renderer.material = new Material(Shader.Find("Diffuse"));
			}
			if (gameObject.GetComponent<MeshFilter>() == null) {
				Terrain = gameObject.AddComponent<MeshFilter>().mesh;
			}

			Terrain.vertices = data.Vertices;
			Terrain.triangles = data.Triangles;
			Terrain.uv = data.Uvs;
			Terrain.normals = data.Normals;
		}

		/// <summary>
		/// Add a MeshFilter to this gameobject if one does not yet exist.
		/// </summary>
		public MeshFilter AddMeshFilter() {
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null) {
				mf = gameObject.AddComponent<MeshFilter>();
			}

			return mf;
		}

		/// <summary>
		/// Add a MeshRenderer to this gameobject if one does not yet exist.
		/// </summary>
		public MeshRenderer AddMeshRenderer() {
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null) {
				mr = gameObject.AddComponent<MeshRenderer>();
			}

			return mr;
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