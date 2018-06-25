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

		private TerraSettings Settings;

		public struct MeshData {
			public Vector3[] vertices;
			public Vector3[] normals;
			public Vector2[] uvs;
			public int[] triangles;
		}

		void OnEnable() {
			Details = new DetailManager(this);

			Settings = TerraSettings.Instance;
			if (Settings == null) {
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
			if (tt.Settings == null) tt.Settings = FindObjectOfType<TerraSettings>();
			if (tt.Settings == null) {
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
			MeshData md = CreateRawMesh(settings, new Vector2(0, 0), gen);

			Mesh mesh = new Mesh();
			mesh.vertices = md.vertices;
			mesh.normals = md.normals;
			mesh.uv = md.uvs;
			mesh.triangles = md.triangles;

			return mesh;
		}

		/// <summary>
		/// Polls the passed Generator for a value at the passed x / z position. 
		/// Applies spread and amplitude to computation.
		/// </summary>
		/// <param name="xPos">X position to get value at</param>
		/// <param name="zPos">Z position to get value at</param>
		/// <param name="settings">Settings instance for amplitude and spread</param>
		/// <param name="gen">Generator to get value from</param>
		/// <returns></returns>
		public static float PollGenerator(float xPos, float zPos, TerraSettings settings, Generator gen) {
			float spread = 1f / (settings.Spread * settings.MeshResolution);
			return gen.GetValue(xPos * spread, zPos * spread, 0f) * settings.Amplitude;
		}

        /// <summary>
		/// Polls the passed Generator for a value at the passed x / z position. 
		/// Applies spread and amplitude from TerraSettings global instance to computation.
		/// </summary>
		/// <param name="xPos">X position to get value at</param>
		/// <param name="zPos">Z position to get value at</param>
		/// <returns>height value if available, float default otherwise (0.0)</returns>
		public static float PollGenerator(float xPos, float zPos) {
            var sett = TerraSettings.Instance;
            if (sett != null && sett.Graph.GetEndGenerator() != null) {
                return PollGenerator(xPos, zPos, sett, sett.Graph.GetEndGenerator());
            }

            return default(float);
        }

        /// <summary>
        /// Updates this TerrainTiles position by taking a Vector2 where 
        /// the x and y values are integers on a grid. Internally the x and y values 
        /// are multiplied by the Length of the mesh specified in TerraSettings
        /// </summary>
        /// <param name="position">Position to set the Tile to (ie [1,0])</param>
        public void UpdatePosition(Vector2 position) {
			Position = position;
			int len = Settings.Length;
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
			CreateMesh(position, Settings.Graph.GetEndGenerator(), renderOnCreation);
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
			mr.sharedMaterial = Settings.CustomMaterial;
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
            float len = Settings.Length;
            float spread = 1f / (Settings.Spread * Settings.MeshResolution);

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
            CalculateNormalsManaged(vertices, normals, triangles);

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
			return CreateRawMesh(Settings, position, gen);
		}

		/// <summary>
		/// Static version of <see cref="CreateRawMesh(Vector2, Generator)"/>
		/// Creates a "raw" mesh from the passed generator and settings specified 
		/// in <c>TerraSettings</c>. This method can be executed off the main thread.
		/// 
		/// Because of Unity's incompatibility with multithreading, a <see cref="MeshData"></see> 
		/// struct is returned with contents of the generated Mesh, instead of using the 
		/// <see cref="Mesh"></see> class.
		/// </summary>
		/// <param name="position">Position in tile grid, used for polling generator</param>
		/// <param name="gen">Generator to apply</param>
		/// <returns>triangles, vertices, normals, and UVs of the generated mesh</returns>
		public static MeshData CreateRawMesh(TerraSettings settings, Vector2 position, Generator gen) {
			int res = settings.MeshResolution;
			float len = settings.Length;
			float spread = 1f / (settings.Spread * settings.MeshResolution);

			Vector3[] vertices = new Vector3[res * res];
			for (int z = 0; z < res; z++) {
				for (int x = 0; x < res; x++) {
                    vertices[x + z * res] = GetPositionAt(x, z, res, settings, gen, position);
				}
			}

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
			CalculateNormalsManaged(vertices, normals, triangles);

			MeshData mesh = new MeshData();
			mesh.triangles = triangles;
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uvs = uvs;

			return mesh;
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
            return GetPositionAt(xPos, zPos, resolution, Settings, Settings.Graph.GetEndGenerator(), Position);
        }

        /// <summary>
        /// Static version of <see cref="GetPositionAt(int, int, int)"/>
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetPositionAt(int xPos, int zPos, int resolution, TerraSettings settings, Generator gen, Vector2 position) {
            float amp = settings.Amplitude;
            float spread = settings.Spread;
            float length = settings.Length;

            float worldX = ((float)xPos / (resolution - 1) - .5f) * length;
            float worldZ = ((float)zPos / (resolution - 1) - .5f) * length;
            float worldY = gen.GetValue(((position.x * length) + xPos) * spread,
                ((position.y * length) + zPos) * spread, 0f) * settings.Amplitude;

            return new Vector3(worldX, worldY, worldZ);
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

		/// <summary>
		/// Calculates normals for the passed vertices and triangles in a method 
		/// similar to Unity's Mesh.RecalculateNormals method.
		/// </summary>
		/// <seealso cref="https://forum.unity.com/threads/procedural-mesh-and-normals-closed-solved.354900/"/>
		/// <param name="verts">vertices of mesh</param>
		/// <param name="normals">normals array to fill</param>
		/// <param name="tris">triangles from mesh</param>
		static void CalculateNormalsManaged(Vector3[] verts, Vector3[] normals, int[] tris) {
			for (int i = 0; i < tris.Length; i += 3) {
				int tri0 = tris[i];
				int tri1 = tris[i + 1];
				int tri2 = tris[i + 2];

				Vector3 vert0 = verts[tri0];
				Vector3 vert1 = verts[tri1];
				Vector3 vert2 = verts[tri2];
				Vector3 normal = new Vector3() {
					x = vert0.y * vert1.z - vert0.y * vert2.z - vert1.y * vert0.z + vert1.y * vert2.z + vert2.y * vert0.z - vert2.y * vert1.z,
					y = -vert0.x * vert1.z + vert0.x * vert2.z + vert1.x * vert0.z - vert1.x * vert2.z - vert2.x * vert0.z + vert2.x * vert1.z,
					z = vert0.x * vert1.y - vert0.x * vert2.y - vert1.x * vert0.y + vert1.x * vert2.y + vert2.x * vert0.y - vert2.x * vert1.y
				};

				normals[tri0] += normal;
				normals[tri1] += normal;
				normals[tri2] += normal;
			}

			for (int i = 0; i < normals.Length; i++) {
				Vector3 norm = normals[i];
				float invlength = 1.0f / (float)System.Math.Sqrt(norm.x * norm.x + norm.y * norm.y + norm.z * norm.z);

				normals[i].x = norm.x * invlength;
				normals[i].y = norm.y * invlength;
				normals[i].z = norm.z * invlength;
			}
		}
	}
}