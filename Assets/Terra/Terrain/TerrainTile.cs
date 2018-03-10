using UnityEngine;
using Terra.CoherentNoise;
using System.Collections.Generic;

namespace Terra.Terrain {
	/// <summary>
	///	TerrainTile represents a Terrain gameobject in the scene. 
	///	This class handles the instantiation of Terrain, noise application, 
	///	position, and texture application.
	/// </summary>
	public class TerrainTile: MonoBehaviour {
		public Mesh Terrain;
		public Vector2 Position { get; private set; }

		[HideInInspector]
		public bool IsColliderDirty = false;

		private TerraSettings Settings;
		private TerrainPaint Paint;

		void OnEnable() {
			if (Settings == null) Settings = FindObjectOfType<TerraSettings>();
			if (Settings == null) {
				Debug.LogError("Cannot find a TerraSettings object in the scene");
			}
		}

		/// <summary>
		/// Creates a gameobject with an attached TerrainTile component and 
		/// places it in the scene. This is a convienence method and is not required 
		/// for correct tile creation.
		/// </summary>
		/// <param name="name">Name of the created gameobject</param>
		/// <returns>The attached TerrainTile component</returns>
		public static TerrainTile CreateTileGameobject(string name) {
			GameObject go = new GameObject(name);
			return go.AddComponent<TerrainTile>();
		}

		/// <summary>
		/// Creates a preview mesh from the passed generator and terrain settings.
		/// </summary>
		/// <param name="settings">Settings to apply to the mesh</param>
		/// <param name="gen">Optional generator to pull values from</param>
		/// <returns>A preview mesh</returns>
		public static Mesh GetPreviewMesh(TerraSettings settings, Generator gen) {
			Mesh mesh = new Mesh();

			int res = settings.MeshResolution;
			float len = settings.Length;
			float spread = 1f / (settings.Spread * settings.MeshResolution);

			Vector3[] vertices = new Vector3[res * res];
			for (int z = 0; z < res; z++) {
				float zPos = ((float)z / (res - 1) - .5f) * len;

				for (int x = 0; x < res; x++) {
					float xPos = ((float)x / (res - 1) - .5f) * len;
					float yPos = gen == null ? 0 : PollGenerator(xPos, zPos, settings, gen);

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
		/// Polls the passed Generator for a value at the passed x / y position. 
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
		/// Updates this TerrainTiles position by taking a Vector2 where 
		/// the x and y values are integers on a grid. Internally the x and y values 
		/// are multiplied by the Length of the mesh specified in TerraSettings
		/// </summary>
		/// <param name="position">Position to set the TerrainTile to (ie [1,0])</param>
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
			CreateMesh(position, Settings.Generator, renderOnCreation);
		}

		/// <summary>
		/// Creates a Mesh with the length and resolution specified in 
		/// TerraSettings. Applies heights found in the passed Generator param.
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

			int res = Settings.MeshResolution;
			float len = Settings.Length;
			float spread = 1f / (Settings.Spread * Settings.MeshResolution);

			Vector3[] vertices = new Vector3[res * res];
			for (int z = 0; z < res; z++) {
				float zPos = ((float)z / (res - 1) - .5f) * len;

				for (int x = 0; x < res; x++) {
					float xPos = ((float)x / (res - 1) - .5f) * len; //problem with x+z*res
					float yPos = generator.GetValue(((position.x * len) + xPos) * spread,
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
		}

		/// <summary>
		/// Generates and applies new MeshCollider for the tile if no collider 
		/// exists currently or <code>IsColliderDirty</code> is true.
		/// </summary>
		public void GenerateCollider() {
			if (gameObject.GetComponent<MeshCollider>() == null || IsColliderDirty) {
				MeshCollider collider = gameObject.AddComponent<MeshCollider>();
				collider.sharedMesh = Terrain;

				TerraEvent.TriggerOnMeshDidForm(gameObject, Terrain);
			}
		}

		/// <summary>
		/// Applies the SplatSettings specified in TerraSettings to this 
		/// TerrainTile. A TerrainPaint instance is created if it didn't exist 
		/// already, and is returned.
		/// </summary>
		/// <returns>TerrainTile instance</returns>
		public TerrainPaint ApplySplatmap() {
			TerraEvent.TriggerOnSplatmapWillCalculate(gameObject);
			if (Paint == null)
				Paint = new TerrainPaint(gameObject, Settings.SplatSettings);

			List<Texture2D> maps = Paint.GenerateSplatmaps();
			maps.ForEach(m => TerraEvent.TriggerOnSplatmapDidCalculate(gameObject, m));

			return Paint;
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
		/// Applies the passed Material to this TerrainTile by setting the 
		/// material assigned to the MeshRenderer component.
		/// </summary>
		/// <param name="mat">Material to apply</param>
		public void ApplyMaterial(Material mat) {
			MeshRenderer mr = GetComponent<MeshRenderer>();
			mr.sharedMaterial = mat;
		}
	}
}