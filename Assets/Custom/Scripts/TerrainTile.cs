using UnityEngine;
using System.Linq;
using CoherentNoise.Generation.Fractal;
using CoherentNoise;
using CoherentNoise.Generation;
using CoherentNoise.Generation.Combination;

/// <summary>
/// TerrainTile represents a Terrain gameobject in the scene. 
/// This class handles the instantiation of Terrain, noise application, 
/// position, and texture application.
/// </summary>
public class TerrainTile {
	public Terrain Terrain { get; private set; }
	public Vector2 Position { get; private set; }
	public bool Active {
		set {
			if (TerrainObject) TerrainObject.SetActive(value);
		}
	}

	private TerrainSettings Settings;
	private GameObject TerrainObject;
	
	public TerrainTile(Vector2 position, TerrainSettings settings) {
		this.Position = position;
		this.Settings = settings;
	}

	/// <summary>
	/// Internally creates a Terrain gameobject. This 
	/// does not apply any noise to the Terrain and cannot be called 
	/// off of the main thread.
	/// </summary>
	public void CreateTerrain() {
		TerrainData data = new TerrainData();
		data.heightmapResolution = Settings.HeightmapResolution;
		data.alphamapResolution = Settings.AlphamapResolution;
		data.size = new Vector3(Settings.Length, Settings.Height, Settings.Length);

		TerrainObject = Terrain.CreateTerrainGameObject(data);
		TerrainObject.transform.position = new Vector3(Position.x * Settings.Length, 0f, Position.y * Settings.Length);
		Terrain = TerrainObject.GetComponent<Terrain>();
	}

	/// <summary>
	/// Finds the heights from the passed CombinedNoise object and
	/// returns the heights that can be applied to the terrain.
	/// 
	/// This method can be called asynchronously off of the main Thread.
	/// </summary>
	/// <param name="noise">Noise to calculate</param>
	public float[,] GetNoiseHeights(Generator gen) {
		float[,] heightmap = new float[Settings.HeightmapResolution, Settings.HeightmapResolution];

		for (var zRes = 0; zRes < Settings.HeightmapResolution; zRes++) {
			for (var xRes = 0; xRes < Settings.HeightmapResolution; xRes++) {
				var xCoordinate = Position.x + (float)xRes / (Settings.HeightmapResolution - 1);
				var zCoordinate = Position.y + (float)zRes / (Settings.HeightmapResolution - 1);
			//	var height = (gen.GetValue(xCoordinate, zCoordinate, 0f) + 2f) / 2.5f;
				var height = gen.GetValue(xCoordinate, zCoordinate, 0f);

				heightmap[zRes, xRes] = height;
			}
		}

		return heightmap;
	}

	/// <summary>
	/// Finds the heights from the default FastNoise object and
	/// returns the heights that can be applied to the terrain.
	/// 
	/// This method can be called asynchronously off of the main Thread.
	/// </summary>
	/// <param name="noise">Noise to calculate</param>
	public float[,] GetNoiseHeights() {
		//Combine ridges with fractals
		RidgeNoise ridgeNoise = new RidgeNoise(Settings.GenerationSeed);
		ridgeNoise.ScaleShift(0f, 1f);
		ridgeNoise.Frequency = 0.1f;
		ridgeNoise.Gain = 0.6f;
		ridgeNoise.Offset = 0.8f;

		PinkNoise fractal = new PinkNoise(Settings.GenerationSeed);
		fractal.ScaleShift(0f, 1f);
		fractal.Frequency = .1f;
		fractal.Persistence = .4f;

		//Multiply combined = new Multiply(ridgeNoise, fractal);
		Add combined = new Add(ridgeNoise, fractal);

		PinkNoise pinkNoise = new PinkNoise(Settings.GenerationSeed);
		pinkNoise.ScaleShift(0f, 1f);
		pinkNoise.Frequency = .1f;
		pinkNoise.OctaveCount = 8;
		pinkNoise.Persistence = .4f;

		//GradientNoise gradient = new GradientNoise(Settings.GenerationSeed);
		PinkNoise gradient = new PinkNoise(Settings.GenerationSeed);
		gradient.Lacunarity = 1.2f;
		gradient.Frequency = 0.04f;
		gradient.ScaleShift(0f, 1f);
		gradient.Gain(0.6f);

		return GetNoiseHeights(combined.Blend(pinkNoise, gradient));
		//return GetNoiseHeights(pinkNoise);
	}

	/// <summary>
	/// Applies the passed FastNoise object to the created Terrain. 
	/// This will not work without CreateTerrain being called first.
	/// 
	/// This method can be called asynchronously off of the main Thread.
	/// </summary>
	/// <param name="noise">Noise to apply</param>
	public void ApplyNoise(float[,] heights) {
		TerrainData data = Terrain.terrainData;
		data.SetHeights(0, 0, heights);
		Terrain.Flush();
	}

	/// <summary>
	/// Destroys the underlying terrain in preparation for 
	/// garbage collection. This can be undone by creating 
	/// the terrain again.
	/// </summary>
	public void DestroyTerrain() {
		Object.Destroy(TerrainObject);
	}

	/// <summary>
	/// Applies the texture settings found in the TerrainSettings instance 
	/// to the Terrain gameobject.
	/// </summary>
	public void ApplyTextures() {
		// Get a reference to the terrain data
		TerrainData terrainData = Terrain.terrainData;

		//Apply textures to terrain
		TerrainSettings.MaterialInfo[] materials = Settings.Materials;
		SplatPrototype[] tex = new SplatPrototype[Settings.Materials.Length];

		for (int i = 0; i < Settings.Materials.Length; i++) {
			Material mat = materials[i].Material;
			tex[i] = new SplatPrototype();

			tex[i].normalMap = mat.GetTexture("_BumpMap") as Texture2D;
			tex[i].texture = mat.mainTexture as Texture2D;
			tex[i].smoothness = mat.GetFloat("_Glossiness");
			tex[i].metallic = mat.GetFloat("_Metallic");
			tex[i].tileOffset = mat.GetTextureOffset("_MainTex");
			tex[i].tileSize = mat.GetTextureScale("_MainTex");
		}

		terrainData.splatPrototypes = tex;

		// Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
		float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

		for (int y = 0; y < terrainData.alphamapHeight; y++) {
			for (int x = 0; x < terrainData.alphamapWidth; x++) {
				// Normalise x/y coordinates to range 0-1 
				float y_01 = y / (float)terrainData.alphamapHeight;
				float x_01 = x / (float)terrainData.alphamapWidth;

				// Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
				float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

				// Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
				Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

				// Calculate the steepness of the terrain
				float steepness = terrainData.GetSteepness(y_01, x_01);

				// Setup an array to record the mix of texture weights at this point
				float[] splatWeights = new float[terrainData.alphamapLayers];

				// CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

				// Texture[0] has constant influence
				splatWeights[0] = 0.5f;

				// Texture[1] is stronger at lower altitudes
				splatWeights[1] = Mathf.Clamp01((terrainData.heightmapHeight - height));

				// Texture[2] stronger on flatter terrain
				// Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
				// Subtract result from 1.0 to give greater weighting to flat surfaces
				splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 5.0f));

				// Texture[3] increases with height but only on surfaces facing positive Z axis 
				splatWeights[3] = height * Mathf.Clamp01(normal.z);

				// Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
				float z = splatWeights.Sum();

				// Loop through each terrain texture
				for (int i = 0; i < terrainData.alphamapLayers; i++) {

					// Normalize so that sum of all texture weights = 1
					splatWeights[i] /= z;

					// Assign this point to the splatmap array
					splatmapData[x, y, i] = splatWeights[i];
				}
			}
		}

		// Finally assign the new splatmap to the terrainData:
		terrainData.SetAlphamaps(0, 0, splatmapData);
	}
}
