using System.Collections.Generic;
using UnityEngine;

public class Tile {
	/// <summary>
	/// Whether the tile is "active" in the scene or not. If active, 
	/// the tile is rendered in the world. If inactive, it is not rendered 
	/// but the mesh data is still cached (if it was already computed)
	/// </summary>
	public bool isActive {
		get {
			return isActive;
		} set {
			if (gameobject) gameobject.SetActive(!value);
		}
	}

	/// <summary>
	/// MeshFilters attached to the individual gameobjects. 
	/// This will return fewer MeshFilters once the child gameobjects 
	/// have been combined.
	/// </summary>
	public MeshFilter[] filters {
		get {
			return gameobject.GetComponentsInChildren<MeshFilter>(true);
		}
	}

	/// <summary>
	/// Sets the position of the tile in world space
	/// </summary>
	public Vector2 tilePosition {
		get {
			return tilePosition;
		}
		set {
			gameobject.transform.position = new Vector3(value.x, 0, value.y);
		}
	}

	/// <summary>
	/// Internal gameobject that acts as the parent for all child gameobjects
	/// </summary>
	public GameObject gameobject;

	private int Resolution;
	private float CellWidth;
	private float CellLength;
	private int Seed;
	private float Gain;

	public Tile(int seed, float gain, int resolution, float cellWidth, float cellLength) {
		this.Seed = seed;
		this.Gain = gain;
		this.Resolution = resolution;
		this.CellLength = cellLength;
		this.CellWidth = cellWidth;
	}

	public Tile(float gain, int resolution, float cellWidth, float cellLength) {
		this.Seed = 1337;
		this.Gain = gain;
		this.Resolution = resolution;
		this.CellLength = cellLength;
		this.CellWidth = cellWidth;
	}

	/// <summary>
	/// Sets the passed gameobject as the parent gameobject 
	/// for the mesh.
	/// </summary>
	/// <param name="parent">Gameobject to make the parent</param>
	public void SetParentGameObject(GameObject parent) {
		gameobject.transform.parent = parent.transform;
	}

	/// <summary>
	/// Applies the passed material to the entire mesh
	/// </summary>
	/// <param name="mat">Material to apply</param>
	public void ApplyMaterial(Material mat) {
		if (gameobject) {
			foreach (Transform obj in gameobject.GetComponentsInChildren<Transform>(true)) {
				MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
				if (renderer != null) renderer.material = mat;
			}
		}
	}

	/// <summary>
	/// Applies the default (white) material to the entire mesh
	/// </summary>
	public void ApplyDefaultMaterial() {
		ApplyMaterial(Resources.Load<Material>("Default"));
	}

	/// <summary>
	/// Applies the passed material settings to the entire mesh. 
	/// It's suggested that this method is called <b>after</b> applying 
	/// noise to the mesh.
	/// </summary>
	/// <param name="settings">Settings to apply to the mesh</param>
	public void ApplyMaterialSettings(MaterialSetting settings) {
		foreach (MeshFilter filter in filters) {
			MeshRenderer render = filter.gameObject.GetComponent<MeshRenderer>();

			if (render != null) {
				//Find normal angle
				float dot = Vector3.Dot(filter.mesh.normals[0], Vector3.forward);
				float angle = Mathf.Abs(dot) * 100;

				//Find height
				float height = filter.gameObject.transform.position.y;

				render.material = settings.GetMaterial(height, angle);
			}
		}
	}



	/// <summary>
	/// Creates a flat terrain mesh at the specified x and z positions. 
	/// The mesh created follows the assigned resolution specified in the 
	/// constructor. This does not render the mesh, rather just creates it. 
	/// To render, call the Render method.
	/// </summary>
	/// <param name="xPos">x position to create terrain at</param>
	/// <param name="zPos">z position to create terrain at</param>
	public void CreateTerrainTile(float xPos, float zPos) {
		this.gameobject = new GameObject();
		List<GameObject> cells = new List<GameObject>(Resolution * Resolution);

		for (int i = 0; i < Resolution; i++) { //Rows
			for (int j = 0; j < Resolution; j++) { //Cols
				GameObject cell = new GameObject();
				cell.SetActive(false);
				Mesh mesh = CreateSquare(CellWidth, CellLength);

				cell.AddComponent<MeshFilter>().mesh = mesh;
				cell.AddComponent<MeshRenderer>();

				cell.transform.position = new Vector3(CellWidth * i, 0f, CellLength * j);
				cells.Add(cell);
			}
		}

		//Add each cell as child of container gameobject
		cells.ForEach(cell => cell.transform.parent = this.gameobject.transform);

		//Set x & z pos
		this.gameobject.transform.position = new Vector3(xPos, 0f, zPos);
	}
	
	/// <summary>
	/// Applies the specified noise settings to the mesh.
	/// </summary>
	/// <param name="noise">FastNoise object to apply</param>
	public void ApplyNoise(FastNoise noise) {
		for (int i = 0; i < filters.Length; i++) {
			Vector3[] vertices = filters[i].mesh.vertices;

			for (int j = 0; j < vertices.Length; j++) {
				Vector3 worldVert = filters[i].transform.TransformPoint(vertices[j]);
				float y = noise.GetNoise(worldVert.x, worldVert.z) * Gain;

				vertices[j] = new Vector3(vertices[j].x, y, vertices[j].z);
			}

			filters[i].mesh.vertices = vertices;
			filters[i].mesh.RecalculateNormals();
			filters[i].gameObject.AddComponent<MeshCollider>();
		}
	}

	/// <summary>
	/// Applies default noise settings to the mesh.
	/// </summary>
	public void ApplyNoise() {
		FastNoise noise = new FastNoise(Seed);

		noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		noise.SetFrequency(.004f);
		noise.SetFractalOctaves(5);
		noise.SetFractalLacunarity(2);
		noise.SetFractalGain(0.5f);

		ApplyNoise(noise);
	}

	/// <summary>
	/// This takes all changes made to the mesh (creation, material application, noise, etc.) 
	/// and combines them into a single mesh. The combined mesh increases performance and is 
	/// made active, rendering it in the scene.
	/// 
	/// Old individual meshes are discarded in this process, freeing memory. Because of this,
	/// this operation cannot be undone.
	/// </summary>
	public void Render() {
		//Collect all material types
		Dictionary<string, Material> materialTypes = new Dictionary<string, Material>();

		foreach (Transform child in gameobject.GetComponentsInChildren<Transform>(true)) {
			MeshRenderer renderer = child.GetComponent<MeshRenderer>();
			if (renderer != null && !materialTypes.ContainsKey(renderer.material.name))
				materialTypes.Add(renderer.material.name, renderer.material);
		}

		//Collect meshes of the same type
		foreach (KeyValuePair<string, Material> mat in materialTypes) {
			List<CombineInstance> toCombine = new List<CombineInstance>();

			foreach (Transform child in gameobject.GetComponentsInChildren<Transform>(true)) {
				MeshRenderer renderer = child.GetComponent<MeshRenderer>();

				if (renderer != null && renderer.material.name == mat.Key) {
					GameObject go = child.gameObject;
					go.SetActive(false);

					CombineInstance comb = new CombineInstance();
					comb.mesh = go.GetComponent<MeshFilter>().mesh;
					comb.transform = go.transform.localToWorldMatrix;
					toCombine.Add(comb);
				}
			}

			//Combine
			Mesh combinedMesh = new Mesh();
			combinedMesh.CombineMeshes(toCombine.ToArray());

			//Add to container G.O.
			GameObject container = new GameObject();
			container.SetActive(true);
			container.AddComponent<MeshFilter>().mesh = combinedMesh;
			container.AddComponent<MeshCollider>();
			container.AddComponent<MeshRenderer>().material = mat.Value;
			container.transform.parent = gameobject.transform;
		}
	}

	/// <summary>
	/// Creates a square with the passed width and height.
	/// Note: This does not recalculate normals.
	/// </summary>
	/// <returns>A mesh that is a simple square</returns>
	private Mesh CreateSquare(float width, float length) {
		Mesh m = new Mesh();
		width /= 2;
		length /= 2;

		m.vertices = new Vector3[] {
			new Vector3(-width, 0f, -length),
			new Vector3(width, 0f, -length),
			new Vector3(width, 0f, length),
			new Vector3(-width, 0f, length)
		};
		m.uv = new Vector2[] {
			new Vector2 (0, 0),
			 new Vector2 (0, 1),
			 new Vector2(1, 1),
			 new Vector2 (1, 0)
		};
		m.triangles = new int[] { 2, 1, 0, 3, 2, 0 };

		return m;
	}
}
