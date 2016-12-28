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
			mesh.displayMesh = value;
		}
	}

	/// <summary>
	/// Sets the position of the tile in world space
	/// </summary>
	public Vector2 tilePosition {
		get {
			return tilePosition;
		} set {
			this.mesh.gameobject.transform.position = new Vector3(value.x, 0, value.y);
		}
	}

	/// <summary>
	/// The internal mesh
	/// </summary>
	public TerrainMesh mesh;

	private int seed;
	private float gain;
	
	public Tile(int seed, float gain) {
		this.seed = seed;
		this.gain = gain;
	}

	public Tile(float gain) {
		this.seed = 1337;
		this.gain = gain;
	}

	public void CreateTerrainTile(int resolution, float zSize, float xSize, float xPos, float zPos) {
		mesh = new TerrainMesh(resolution, zSize, xSize);
		mesh.CreateTerrainMesh(xPos, zPos);
	}

	public void ApplyNoise(FastNoise noise) {
		MeshFilter[] filters = mesh.filters;

		for (int i = 0; i < filters.Length; i++) {
			Vector3[] vertices = filters[i].mesh.vertices;

			for (int j = 0; j < vertices.Length; j++) {
				Vector3 worldVert = filters[i].transform.TransformPoint(vertices[j]);
				float y = noise.GetNoise(worldVert.x, worldVert.z) * gain;

				vertices[j] = new Vector3(vertices[j].x, y, vertices[j].z);
			}

			filters[i].mesh.vertices = vertices;
			filters[i].mesh.RecalculateNormals();
			filters[i].gameObject.AddComponent<MeshCollider>();
		}
	}

	public void ApplyNoise() {
		FastNoise noise = new FastNoise(seed);

		noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		noise.SetFrequency(.004f);
		noise.SetFractalOctaves(5);
		noise.SetFractalLacunarity(2);
		noise.SetFractalGain(0.5f);

		ApplyNoise(noise);
	}
}
