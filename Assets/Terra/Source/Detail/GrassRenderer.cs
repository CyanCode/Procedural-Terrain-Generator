using System.Collections;
using System.Collections.Generic;
using Terra.Terrain;
using UnityEngine;

public static class GrassRenderer {
	private static Material _material;

	public static int Resolution = 128;
	public static Material Material {
		get {
			if (_material == null) {
				_material = GetGrassMaterial();
			}

			return _material;
		}
	}

	/// <summary>
	/// True if this GrassRenderer is currently calculating grass 
	/// data for a TerrainTile. Used for syncing up coroutines.
	/// </summary>
	private static bool CalculatingTile = false;

	/// <summary>
	/// Used for retrieving previously created mesh information
	/// </summary>
	private static Dictionary<TerrainTile, List<GameObject>> CachedMeshData = new Dictionary<TerrainTile, List<GameObject>>();
	private static LinkedList<TerrainTile> GenerationQueue = new LinkedList<TerrainTile>();

	private const string GRASS_SHADER_LOC = "Terra/GrassGeometry";
	private const string GRASS_CONTAINER_NAME = "Grass Meshes";

	static GrassRenderer() {
		//Watch for the addition of tiles
		TerraEvent.OnMeshColliderDidForm += (go, mc) => {
			TerrainTile t = go.GetComponent<TerrainTile>();

			if (TerraSettings.Instance.PlaceGrass && !CachedMeshData.ContainsKey(t) && !GenerationQueue.Contains(t)) {
				GenerationQueue.AddFirst(t);
			}
		};
	}

	/// <summary>
	/// Updates the internal generation queue by working on the 
	/// next available tile
	/// </summary>
	public static void Update() {
		UpdateMaterialData();

		if (!CalculatingTile && GenerationQueue.Count > 0) {
			TerrainTile t = GenerationQueue.Last.Value;
			GenerationQueue.RemoveLast();

			CalculateGrassMeshes(t);
		}
	}

	/// <summary>
	/// Returns true if this instance of GrassRenderer has already 
	/// computed data for placing grass on the passed tile.
	/// </summary>
	public static bool HasGrassData(TerrainTile tile) {
		return CachedMeshData.ContainsKey(tile);
	}

	/// <summary>
	/// Calculates data used for placing grass on terrain regardless of 
	/// whether or not this data has already been computed previously.
	/// Use <c>HasGrassData</c> to check if its already been computed.
	/// This method calculates grass meshes via a coroutine which calculates a 
	/// mesh and then waits for the next frame before continuing to the next.
	/// </summary>
	public static void CalculateGrassMeshes(TerrainTile tile) {
		GrassTile gt = new GrassTile(tile, TerraSettings.Instance.GrassStepLength);
		CalculatingTile = true;

		tile.StartCoroutine(gt.CalculateCells((data) => {
			List<GameObject> createdGos = new List<GameObject>(data.Count);
			GameObject parent = SetupGrassParent(tile);

			foreach (GrassTile.MeshData md in data) { 
				//Apply material to each Mesh
				if (md.vertices != null) {
					GameObject grassGO = new GameObject("Grass");
					grassGO.transform.parent = parent.transform;
					createdGos.Add(grassGO);

					var mf = grassGO.AddComponent<MeshFilter>();
					var mr = grassGO.AddComponent<MeshRenderer>();
					mr.material = Material;

					Mesh m = new Mesh();
					m.SetVertices(md.vertices);
					m.SetNormals(md.normals);
					m.SetIndices(md.indicies.ToArray(), MeshTopology.Points, 0);

					mf.mesh = m;
				}
			}

			CachedMeshData.Add(tile, createdGos);
			CalculatingTile = false;
		}));
	}

	/// <summary>
	/// Sets up the container that will contain all grass mesh 
	/// gameobjects
	/// </summary>
	/// <returns>Parent gameobject</returns>
	private static GameObject SetupGrassParent(TerrainTile tile) {
		//Setup parent gameobject
		GameObject parent = null;
		foreach (Transform trans in tile.transform) {
			if (trans.gameObject.name == GRASS_CONTAINER_NAME) {
				parent = trans.gameObject;
				break;
			}
		}
		if (parent == null) {
			parent = new GameObject(GRASS_CONTAINER_NAME);
			parent.transform.parent = tile.transform;
		}

		return parent;
	}

	/// <summary>
	/// Gets the grass material and assigns it the shader 
	/// located in resources
	/// </summary>
	/// <returns>Created material</returns>
	private static Material GetGrassMaterial() {
		Shader grassShader = Shader.Find(GRASS_SHADER_LOC);
		Material mat = new Material(grassShader);

		var sett = TerraSettings.Instance;
		mat.SetTexture("_MainTex", sett.GrassTexture);
		mat.SetFloat("_BillboardDistance", sett.BillboardDistance);
		mat.SetFloat("_GrassHeight", sett.GrassHeight);
		mat.SetFloat("_Cutoff", sett.ClipCutoff);

		return mat;
	}

	/// <summary>
	/// Updates grass material information from information in 
	/// TerraSettings. 
	/// </summary>
	private static void UpdateMaterialData() {
		if (Material != null) {
			var sett = TerraSettings.Instance;
			Material.SetTexture("_MainTex", sett.GrassTexture);
			Material.SetFloat("_BillboardDistance", sett.BillboardDistance);
			Material.SetFloat("_GrassHeight", sett.GrassHeight);
			Material.SetFloat("_Cutoff", sett.ClipCutoff);
		}
	}

	private class GrassTile {
		public delegate void CalcFinished(List<MeshData> data);

		public struct MeshData {
			public List<Vector3> vertices;
			public List<Vector3> normals;
			public List<int> indicies;
		}

		private MeshCollider _mc;
		private MeshCollider MeshCollider {
			get {
				if (_mc == null) {
					_mc = Tile.GetComponent<MeshCollider>();

					if (_mc == null) {
						Debug.Log("GrassRenderer requires the passed TerrainTile have a MeshCollider");
					}
				}

				return _mc;
			}
		}

		/// <summary>
		/// Maximum amount of iterations CalculateCells is alotted before 
		/// waiting for the next frame.
		/// </summary>
		private const int MAX_ITERATIONS_PER_FRAME = 10000;

		/// <summary>
		/// Maxmimum amount of vertices a grass mesh is allowed.
		/// </summary>
		private const int MAX_VERTS_PER_MESH = 60000;

		private TerrainTile Tile;
		private float StepLength;


		public GrassTile(TerrainTile tile, float stepLength) {
			Tile = tile;
			StepLength = stepLength;
		}

		/// <summary>
		/// Calculates MeshData incrementally using a coroutine. 
		/// TerrainTile instance must have a MeshCollider attached to 
		/// the same gameobject
		/// </summary>
		/// <param name="onCalculated">Callback delegate when operations have finished</param>
		/// 
		public IEnumerator CalculateCells(CalcFinished onCalculated) {
			Random.InitState(TerraSettings.GenerationSeed);
			float variation = TerraSettings.Instance.GrassVariation;

			List<MeshData> data = new List<MeshData>();
			Bounds bounds = Tile.Terrain.bounds;
			Bounds worldBounds = Tile.GetComponent<MeshRenderer>().bounds;

			int res = TerraSettings.Instance.MeshResolution;
			float rayHeight = worldBounds.max.y + 5;
			float rayMaxLength = rayHeight - (worldBounds.min.y - 5);

			List<Vector3> verts = new List<Vector3>(MAX_VERTS_PER_MESH);
			List<Vector3> norms = new List<Vector3>(MAX_VERTS_PER_MESH);
			List<int> indicies = new List<int>(MAX_VERTS_PER_MESH);

			int idx = 0;
			int iterationCount = 0; //Tracks when to wait for next frame
			for (float x = worldBounds.min.x; x < worldBounds.max.x; x += StepLength) {
				for (float z = worldBounds.min.z; z < worldBounds.max.z; z += StepLength) {
					float varX = x + Random.Range(-variation, variation);
					float varZ = z + Random.Range(-variation, variation);

					Ray r = new Ray(new Vector3(varX, rayHeight, varZ), Vector3.down);
					RaycastHit hit;

					if (MeshCollider.Raycast(r, out hit, rayMaxLength) && CanPlaceAt(hit))  {
						verts.Add(new Vector3(varX, hit.point.y, varZ));
						norms.Add(hit.normal);
						indicies.Add(idx);
						idx++;
					}

					//If max verts met, flush to MeshData list
					if (verts.Count >= MAX_VERTS_PER_MESH) {
						MeshData d = new MeshData();
						d.vertices = verts;
						d.normals = norms;
						d.indicies = indicies;
						data.Add(d);

						//Clear existing data
						verts = new List<Vector3>();
						norms = new List<Vector3>();
						indicies = new List<int>();
						idx = 0;
					}

					//Possibly wait for next frame
					iterationCount++;
					if (iterationCount > MAX_ITERATIONS_PER_FRAME) {
						iterationCount = 0;
						yield return null; //Wait for next frame
					}
				}
			}

			//Pause before returning as we want to create mesh in a different frame
			yield return null;

			MeshData md = new MeshData();
			md.vertices = verts;
			md.normals = norms;
			md.indicies = indicies;
			data.Add(md);

			onCalculated(data);
		}

		/// <summary>
		/// Uses raycast hit information to check whether a grass 
		/// vertex can be placed at this location in the world. 
		/// Checks against grass height and angle information found 
		/// in TerraSettings.
		/// </summary>
		/// <param name="hit">Raycast hit</param>
		/// <returns>true if this vertex should be placed here</returns>
		private bool CanPlaceAt(RaycastHit hit) {
			float height = hit.point.y;
			float angle = Vector3.Angle(Vector3.up, hit.normal);
			TerraSettings set = TerraSettings.Instance;

			bool passesHeight = height >= set.GrassMinHeight && height <= set.GrassMaxHeight;
			bool passesAngle = angle >= set.GrassAngleMin && angle <= set.GrassAngleMax;

			if (set.GrassConstrainHeight && set.GrassConstrainAngle) {
				return passesHeight && passesAngle;
			} else if (set.GrassConstrainHeight) {
				return passesHeight;
			} else if (set.GrassConstrainAngle) {
				return passesAngle;
			} else {
				return true;
			}
		}
	}
}