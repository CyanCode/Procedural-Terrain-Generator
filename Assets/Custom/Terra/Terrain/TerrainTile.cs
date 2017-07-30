using UnityEngine;
using Terra.CoherentNoise;

namespace Terra.Terrain {
	/// <summary>
	/// TerrainTile represents a Terrain gameobject in the scene. 
	/// This class handles the instantiation of Terrain, noise application, 
	/// position, and texture application.
	/// </summary>
	public class TerrainTile: MonoBehaviour {
		public Mesh Terrain { get; private set; }
		public Vector2 Position { get; private set; }

		private TerrainSettings Settings;
		private TerrainPaint Paint;

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

			int res = settings.MeshResolution;
			float len = settings.Length;
			float spread = settings.Spread * (1f / settings.MeshResolution);

			Vector3[] vertices = new Vector3[res * res];
			for (int z = 0; z < res; z++) {
				float zPos = ((float)z / (res - 1) - .5f) * len;

				for (int x = 0; x < res; x++) {
					float xPos = ((float)x / (res - 1) - .5f) * len;
					float yPos = gen.GetValue(xPos * spread, zPos * spread, 0f) * settings.Amplitude;
					vertices[x + z * res] = new Vector3(xPos, yPos, zPos);
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
			float len = Settings.Length;
			float spread = Settings.Spread * (1f / Settings.MeshResolution);

			Vector3[] vertices = new Vector3[res * res];
			for (int z = 0; z < res; z++) {
				float zPos = ((float)z / (res - 1) - .5f) * len;

				for (int x = 0; x < res; x++) {
					float xPos = ((float)x / (res - 1) - .5f) * len; //problem with x+z*res
					float yPos = Settings.Generator.GetValue(((position.x * len) + xPos) * spread,
						((position.y * len) + zPos) * spread, 0f) * Settings.Amplitude;

					vertices[x + z * res] = new Vector3(xPos, yPos, zPos);
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
			Terrain.RecalculateNormals();

			UpdatePosition(position);

			MeshCollider collider = gameObject.AddComponent<MeshCollider>();
			collider.sharedMesh = Terrain;
		}
		
		/// <summary>
		/// Applies a splatmap to the terrain
		/// </summary>
		public void ApplySplatmap() {
			if (Paint == null) Paint = new TerrainPaint(gameObject);

			Paint.CreateAlphaMap(Settings.SplatSettings);
		}
	}
}