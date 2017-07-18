using UnityEngine;
using CoherentNoise;

/// <summary>
/// TerrainTile represents a Terrain gameobject in the scene. 
/// This class handles the instantiation of Terrain, noise application, 
/// position, and texture application.
/// </summary>
public class TerrainTile: MonoBehaviour {
	public Mesh Terrain { get; private set; }
	public Vector2 Position { get; private set; }

	private TerrainSettings Settings;

	void OnEnable() {
		if (Settings == null) Settings = FindObjectOfType<TerrainSettings>();
		if (Settings == null) {
			Debug.LogError("Cannot find a TerrainSettings object in the scene");
		}
	}
	
	public void UpdatePosition(Vector2 position) {
		Position = position;
		int len = Settings.Length;
		transform.position = new Vector3(position.x * len, 0f, position.y * len);
	}

	/// <summary>
	/// Creates a preview mesh from the passed generator and terrain settings.
	/// </summary>
	/// <param name="settings">Settings to apply to the mesh</param>
	/// <param name="gen">Generator</param>
	/// <returns>A preview mesh</returns>
	public static Mesh GetPreviewMesh(TerrainSettings settings, Generator gen) {
		Mesh mesh = new Mesh();

		int resolution = settings.MeshResolution;
		float length = settings.Length;

		Vector3[] vertices = new Vector3[resolution * resolution];
		for (int z = 0; z < resolution; z++) {
			float zPos = ((float)z / (resolution - 1) - .5f) * length;

			for (int x = 0; x < resolution; x++) {
				float xPos = ((float)x / (resolution - 1) - .5f) * length;
				float yPos = gen.GetValue(x * settings.Spread, z * settings.Spread, 0f) * settings.Amplitude;
				vertices[x + z * resolution] = new Vector3(xPos, yPos, zPos);
			}
		}

		Vector3[] normales = new Vector3[vertices.Length];
		for (int n = 0; n < normales.Length; n++)
			normales[n] = Vector3.up;

		Vector2[] uvs = new Vector2[vertices.Length];
		for (int v = 0; v < resolution; v++) {
			for (int u = 0; u < resolution; u++) {
				uvs[u + v * resolution] = new Vector2((float)u / (resolution - 1), (float)v / (resolution - 1));
			}
		}

		int nbFaces = (resolution - 1) * (resolution - 1);
		int[] triangles = new int[nbFaces * 6];
		int t = 0;
		for (int face = 0; face < nbFaces; face++) {
			int i = face % (resolution - 1) + (face / (resolution - 1) * resolution);

			triangles[t++] = i + resolution;
			triangles[t++] = i + 1;
			triangles[t++] = i;

			triangles[t++] = i + resolution;
			triangles[t++] = i + resolution + 1;
			triangles[t++] = i + 1;
		}

		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		return mesh;
	}

	/// <summary>
	/// Creates a Mesh with the length and resolution specified in 
	/// TerrainSettings. Applies the passed heights from calculated noise 
	/// values. This cannot be called off of the main thread
	/// </summary>
	/// <param name="position">Position to place Mesh in the tile grid</param>
	/// <param name="heights">Heights to apply to the mesh. This array must be equal in size 
	/// to the length of vertices on the mesh.</param>
	public void CreateMesh(Vector2 position, float[] heights) {
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.material = new Material(Shader.Find("Diffuse"));
		Terrain = gameObject.AddComponent<MeshFilter>().mesh;
		
		int res = Settings.MeshResolution;
		int len = Settings.Length;

		Vector3[] vertices = new Vector3[res * res];
		int heightIdx = 0;
		for (int z = 0; z < res; z++) {
			float zPos = ((float)z / (res - 1) - .5f) * len;

			for (int x = 0; x < res; x++) {
				float xPos = ((float)x / (res - 1) - .5f) * len; //problem with x+z*res
				vertices[x + z * res] = new Vector3(xPos, heights[heightIdx], zPos);
				heightIdx++;
			}
		}

		Vector3[] normales = new Vector3[vertices.Length];
		for (int n = 0; n < normales.Length; n++)
			normales[n] = Vector3.up;

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

		Terrain.vertices = vertices;
		Terrain.normals = normales;
		Terrain.uv = uvs;
		Terrain.triangles = triangles;

		UpdatePosition(position);
	}

	/// <summary>
	/// Finds the heights from the created noise graph editor
	/// returns the heights that can be applied to the mesh.
	/// 
	/// This method can be called asynchronously off of the main Thread.
	/// </summary>
	/// <param name="vertices">Vertices from Mesh in world space</param>
	/// <param name="spread">Option spread to stretch or squash the terrain</param>
	/// <param name="amplitude">Optional amplitude to multiply height values by</param>
	/// <returns>array of height values</returns>
	public float[] GetNoiseHeights(Vector2 position, float spread = 1f, float amplitude = 1f) {
		int res = Settings.MeshResolution;
		int len = Settings.Length;
		float[] heights = new float[res * res];

		for (int z = 0; z < res; z++) {
			float zPos = (((float)z / (res - 1) - .5f) * len) + (position.y * Settings.Length);

			for (int x = 0; x < res; x++) {
				float xPos = (((float)x / (res - 1) - .5f) * len) + (position.x * Settings.Length);
				heights[x + z * Settings.MeshResolution] = Settings.Generator.GetValue(xPos * spread, zPos * spread, 0f) * amplitude;
			}
		}

		return heights;
	}

	/// <summary>
	/// Applies the passed heights to the mesh. This cannot 
	/// be called off of the main thread.
	/// </summary>
	/// <param name="heights">Heights to apply</param>
	public void ApplyHeights(float[] heights) {
		Vector3[] vertices = Terrain.vertices;

		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] = new Vector3(vertices[i].x, heights[i], vertices[i].z);
		}

		Terrain.vertices = vertices;
		Terrain.RecalculateBounds();
		Terrain.RecalculateNormals();
		Terrain.RecalculateTangents();
	}
}
