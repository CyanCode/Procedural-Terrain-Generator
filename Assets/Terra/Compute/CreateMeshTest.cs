using UnityEngine;
using System.Linq;

public class CreateMeshTest : MonoBehaviour {
	public ComputeShader shader;

	// Use this for initialization
	void Start () {
		const int res = 128;
		const int resSqr = res * res;
		const float len = 500;
		//float spread = 1f / (Settings.Spread * Settings.MeshResolution);

		Vector2[] xyPos = new Vector2[resSqr];
		for (int z = 0; z < res; z++) {
			float zPos = ((float)z / (res - 1) - .5f) * len;

			for (int x = 0; x < res; x++) {
				float xPos = ((float)x / (res - 1) - .5f) * len; //problem with x+z*res

				xyPos[x + z * res] = new Vector2(xPos, zPos);
			}
		}

		Vector3[] normals = new Vector3[resSqr];
		for (int n = 0; n < normals.Length; n++)
			normals[n] = Vector3.up;

		Vector2[] uvs = new Vector2[resSqr];
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
		

		ComputeBuffer vertBuffer = new ComputeBuffer(resSqr, sizeof(float) * 8, ComputeBufferType.Default);
		ComputeBuffer heightBuffer = new ComputeBuffer(resSqr, sizeof(float), ComputeBufferType.Default);
		vertBuffer.SetData(xyPos);

		int kernalHandle = shader.FindKernel("CSMain");
		shader.SetBuffer(kernalHandle, "MeshVert", vertBuffer);
		shader.SetBuffer(kernalHandle, "Heights", heightBuffer);
		shader.Dispatch(kernalHandle, 8, 1, 1);

		float[] heights = new float[resSqr];
		Vector3[] vertices = new Vector3[resSqr];
		heightBuffer.GetData(heights);

		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] = new Vector3(xyPos[i].x, heights[i], xyPos[i].y);
		}

		MeshFilter mf = gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>();

		mf.mesh.vertices = vertices;
		mf.mesh.triangles = triangles;
		mf.mesh.normals = normals;
		mf.mesh.uv = uvs;
		mf.mesh.RecalculateNormals();

		vertBuffer.Dispose();
		heightBuffer.Dispose();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
