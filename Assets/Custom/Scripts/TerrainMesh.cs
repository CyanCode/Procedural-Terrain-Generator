using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TerrainMesh contains a collection of many smaller meshes 
/// that are placed adjacent to each other. This allows many 
/// different materials to be displayed at the same time.
/// 
/// TerrainMesh will statically batch internal meshes for
/// increases in performance.
/// </summary>
public class TerrainMesh {
	public bool displayMesh {
		set {
			if (value) {
				if (gameobject) gameobject.SetActive(false);
			} else {
				if (gameobject) gameobject.SetActive(true);
			}
		}
	}
	public MeshFilter[] filters {
		get {
			return gameobject.GetComponentsInChildren<MeshFilter>(true);
		}
	}
	public GameObject gameobject;

	private int resolution;
	private float cellWidth;
	private float cellLength;

	/// <summary>
	/// Creates a new TerrainMesh with the specified resolution, cell width, and cell length.
	/// </summary>
	/// <param name="resolution">
	/// How many mesh cells to create along the length of the terrain. This is simply the resolution
	/// of the whole mesh so therefore the total internal mesh cells will be the resolution squared.
	/// </param>
	/// <param name="cellWidth">The width of internal (individual) cells</param>
	/// <param name="cellLength">The length of internal (individual) cells</param>
	public TerrainMesh(int resolution, float cellWidth, float cellLength) {
		this.resolution = resolution >= 2 ? resolution : 2;
		this.cellWidth = cellWidth;
		this.cellLength = cellLength;
	}

	public TerrainMesh() {
		resolution = 10;
		cellWidth = 1f;
		cellLength = 1f;
	}

	public void SetParentGameObject(GameObject parent) {
		gameobject.transform.parent = parent.transform;
	}

	public void ApplyMaterial(Material mat) {
		if (gameobject) {
			foreach (Transform obj in gameobject.GetComponentsInChildren<Transform>()) {
				MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
				if (renderer != null) renderer.material = mat;
			}
		}
	}

	public void ApplyDefaultMaterial() {
		ApplyMaterial(Resources.Load<Material>("Default"));
	}

	/// <summary>
	/// Creates a flat terrain mesh at the specified x and z positions. 
	/// The mesh created follows the assigned resolution specified in the 
	/// constructor. This does not render the mesh, rather just creates it. 
	/// To render, call the Render method.
	/// </summary>
	/// <param name="xPos">x position to create terrain at</param>
	/// <param name="zPos">z position to create terrain at</param>
	public void CreateTerrainMesh(float xPos, float zPos) {
		this.gameobject = new GameObject();
		List<GameObject> cells = new List<GameObject>(resolution * resolution);

		for (int i = 0; i < resolution; i++) { //Rows
			for (int j = 0; j < resolution; j++) { //Cols
				GameObject cell = new GameObject();
				cell.SetActive(false);
				Mesh mesh = CreateSquare(cellWidth, cellLength);

				cell.AddComponent<MeshFilter>().mesh = mesh;
				cell.AddComponent<MeshRenderer>();

				cell.transform.position = new Vector3(cellWidth * i, 0f, cellLength * j);
				cells.Add(cell);
			}
		}

		//Add each cell as child of container gameobject
		cells.ForEach(cell => cell.transform.parent = this.gameobject.transform);

		//Set x & z pos
		this.gameobject.transform.position = new Vector3(xPos, 0f, zPos);
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