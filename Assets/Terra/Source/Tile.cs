using System;
using UnityEngine;
using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.CoherentNoise.Texturing;
using Terra.Graph.Noise;
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

		void Update() {
			Details.Update();
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
			if (tt._settings == null) tt._settings = FindObjectOfType<TerraSettings>();
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
			mesh.vertices = md.vertices;
			mesh.normals = md.normals;
			mesh.uv = md.uvs;
			mesh.triangles = md.triangles;

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
			int len = _settings.Length;
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
			CreateMesh(position, _settings.Graph.GetEndGenerator(), renderOnCreation);
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
			Terrain.vertices = md.vertices;
			Terrain.triangles = md.triangles;
			Terrain.uv = md.uvs;
			Terrain.normals = md.normals;

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
			float len = _settings.Length;
			float spread = 1f / (_settings.Spread * _settings.MeshResolution);

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

		/// <summary>
		/// Polls the <see cref="Generator"/> found in <see cref="TerraSettings"/> and 
		/// retrieves the y height value and x & z world coordinates for the passed x 
		/// and z location. The x and z values are integers representing a local point on 
		/// the to-be-created mesh. Internally, these values are transformed into world 
		/// coordinates based on the passed resolution.
		/// </summary>
		/// <param name="xPos">mesh x position</param>
		/// <param name="zPos">mesh z position</param>
		/// <param name="resolution">resolution of this mesh</param>
		/// <returns></returns>
		public Vector3 GetPositionAt(int xPos, int zPos, int resolution) {
			return TileMesh.GetPositionAt(xPos, zPos, resolution, _settings, _settings.Graph.GetEndGenerator(), Position);
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

			Terrain.vertices = data.vertices;
			Terrain.triangles = data.triangles;
			Terrain.uv = data.uvs;
			Terrain.normals = data.normals;
		}
	}

	public struct MeshData {
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector2[] uvs;
		public int[] triangles;
	}

	/// <summary>
	/// Represents a "map" attached to a TerrainTile. Internally 
	/// this contains a texture for previewing, generator(s), and 
	/// ways to retrieve map data.
	/// </summary>
	[Serializable]
	public class TileMap {
		/// <summary>
		/// 2-Dimensional float array representation of this map. 
		/// </summary>
		public float[,] Map { get; private set; }

		/// <summary>
		/// Resolution of this TileMap
		/// </summary>
		public int Resolution { get; private set; }

		/// <summary>
		/// map data that is referenced when updating the internal map
		/// </summary>
		public TileMapData MapData;

		/// <summary>
		/// Tile instance attached to this TileMap
		/// </summary>
		private Tile _tile;

		/// <summary>
		/// Generator specified in <see cref="MapData"/>
		/// </summary>
		private Generator _generator {
			get {
				return MapData.Generator;
			}
		}

		/// <summary>
		/// Creates a new TileMap with the passed resolution used 
		/// as the resolution in <see cref="Map"/>
		/// </summary>
		/// <param name="tile">Tile</param>
		/// <param name="resolution"></param>
		public TileMap(Tile tile, int resolution, TileMapData mapData) {
			_tile = tile;
			Resolution = resolution;
			MapData = mapData;
		}

		/// <summary>
		/// Updates <see cref="Map"/> with values pulled from  <see cref="Generator"/>.
		/// The polled positions are in world coordinates supplied by the Tile passed 
		/// into the constructor. If <see cref="Generator"/> is null, nothing is updated.
		/// </summary>
		public void UpdateMap() {
			Map = new float[Resolution, Resolution];
			Generator gen = _generator;
			TerraSettings settings = TerraSettings.Instance;

			if (gen == null)
				return;

			for (int x = 0; x < Resolution; x++) {
				for (int y = 0; y < Resolution; y++) {
					Map[x, y] = PollGeneratorWorld(x, y, gen, settings.Spread, settings.Amplitude);
				}
			}
		}

		/// <summary>
		/// Polls <see cref="Generator"/> by passing in world coordinates 
		/// of the provided <see cref="Tile"/>. 
		/// </summary>
		/// <param name="xPos">Local x position</param>
		/// <param name="zPos">Local z position</param>
		/// <param name="generator">Generator to poll</param>
		/// <param name="spread">Optional horizontal stretch or squash of the X & Z coordinates</param>
		/// <param name="amplitude">Optional vertical stretch or squash of the Y coordinate</param>
		/// <returns>float value returned by Generator</returns>
		public float PollGeneratorWorld(int xPos, int zPos, Generator generator, float spread = 1f, float amplitude = 1f) {
			if (_generator == null)
				return default(float);

			spread = 1 / spread;
			int length = TerraSettings.Instance.Length;

			float x = (_tile.Position.x * length + xPos) * spread;
			float z = (_tile.Position.y * length + zPos) * spread;
			return generator.GetValue(x, z, 0f) * amplitude;
		}
	}
}