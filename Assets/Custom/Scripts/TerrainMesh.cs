using UnityEngine;

public class TerrainMesh {
	public bool displayMesh {
		set {
			displayMesh = value;

			if (value) { //Remove meshObject from scene
				if (meshObj) Object.Destroy(meshObj);
			} else { //Add meshObject to scene again
				GetMeshGameObject();
			}
		}
	}
	public Vector3[] vertices {
		get {
			return mesh.vertices;
		}

		set {
			mesh.vertices = value;
		}
	}

    private Mesh mesh;
	private GameObject meshObj;
    private int resolution;
    private int zSize;
	private int xSize;

    public TerrainMesh(int resolution, int zSize, int xSize) {
        this.resolution = resolution >= 2 ? resolution : 2;
        this.zSize = zSize;
        this.xSize = xSize;
    }

    public TerrainMesh() {
        resolution = 10;
        zSize = 20;
        xSize = 20;
    }

	~TerrainMesh() {
		if (meshObj) Object.Destroy(meshObj);
	}

	public void ApplyMaterial(Material mat) {
		if (meshObj) meshObj.GetComponent<MeshRenderer>().material = mat;
	}

	public void ApplyMaterial() {
		ApplyMaterial(Resources.Load<Material>("Default"));
	}

    public GameObject GetMeshGameObject() {
		if (!mesh) {
			CreateMesh();
		} if (!meshObj) { 
			meshObj = new GameObject();
			meshObj.AddComponent<MeshFilter>().mesh = mesh;
			meshObj.AddComponent<MeshRenderer>();
			meshObj.AddComponent<MeshCollider>();

			ApplyMaterial();
		}

        return meshObj;
    }



	/// <summary>
	/// Creates a Mesh object with the specified resolution and 
	/// x / z sizes. The created Mesh is cached.
	/// </summary>
    private void CreateMesh() {
        this.mesh = new Mesh();
        this.mesh.Clear();

        float length = zSize;
        float width = xSize;
        int resX = resolution;
        int resZ = resolution;

        Vector3[] vertices = new Vector3[resX * resZ];
        for (int z = 0; z < resZ; z++) {
            float zPos = ((float)z / (resZ - 1) - .5f) * length;

            for (int x = 0; x < resX; x++) {
                float xPos = ((float)x / (resX - 1) - .5f) * width;
                vertices[x + z * resX] = new Vector3(xPos, 0f, zPos);
            }
        }

        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < normales.Length; n++)
            normales[n] = Vector3.up;

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int v = 0; v < resZ; v++) {
            for (int u = 0; u < resX; u++) {
                uvs[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
            }
        }

        int nbFaces = (resX - 1) * (resZ - 1);
        int[] triangles = new int[nbFaces * 6];
        int t = 0;
        for (int face = 0; face < nbFaces; face++) {
            int i = face % (resX - 1) + (face / (resZ - 1) * resX);

            triangles[t++] = i + resX;
            triangles[t++] = i + 1;
            triangles[t++] = i;

            triangles[t++] = i + resX;
            triangles[t++] = i + resX + 1;
            triangles[t++] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }
}
