using UnityEngine;

public class Tile {
	/// <summary>
	/// Whether the tile is "active" in the scene or not. If active, 
	/// the tile is rendered in the world. If inactive, it is not rendered 
	/// but the mesh data is still cached (if it was already computed)
	/// </summary>
	public bool isActive {
		get { return isActive; }
		set {
			isActive = value;
			mesh.displayMesh = value;
		}
	}

	/// <summary>
	/// Sets the position of the tile in world space
	/// </summary>
	public Vector2 tilePosition {
		get { return tilePosition; }
		set {
			this.mesh.GetMeshGameObject().transform.position = new Vector3(value.x, 0, value.y);
		}
	}

	private int seed;
	private float gain;
	private TerrainMesh mesh;

	public Tile(int seed, float gain) {
		this.seed = seed;
		this.gain = gain;
	}

	public Tile(float gain) {
		this.seed = 1337;
		this.gain = gain;
	}

	public void CreateTerrainTile(int resolution, int zSize, int xSize) {
		this.mesh = new TerrainMesh(resolution, zSize, xSize);
	}

	public void ApplyNoise(FastNoise noise) {
		Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; i++) {
			float x = vertices[i].x;
			float z = vertices[i].z;
			float y = noise.GetNoise(x, z) * gain;

			vertices[i] = new Vector3(x, y, z);
		}

		SetVertices(vertices);
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

	private void SetVertices(Vector3[] vertices) {
		mesh.vertices = vertices;
	}
}
