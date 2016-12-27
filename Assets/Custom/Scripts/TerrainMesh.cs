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
			displayMesh = value;

			if (value) {
				if (gameobject) gameobject.SetActive(false);
			} else {
				if (gameobject) gameobject.SetActive(true);
			}
		}
	}
	public MeshFilter[] filters {
		get { 
			return gameobject.GetComponentsInChildren<MeshFilter>();
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
		if (gameobject) gameobject.GetComponent<MeshRenderer>().material = mat;
	}

	public void ApplyMaterial() {
		ApplyMaterial(Resources.Load<Material>("Default"));
	}

    public void CreateTerrainMesh(float xPos, float zPos) {
		this.gameobject = new GameObject();
		List<GameObject> cells = new List<GameObject>(resolution * resolution);

		for (int i = 0; i < resolution; i++) { //Rows
			for (int j = 0; j < resolution; j++) { //Cols
				GameObject cell = new GameObject();
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