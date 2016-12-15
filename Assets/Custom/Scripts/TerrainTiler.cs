using UnityEngine;

public class TerrainTiler : MonoBehaviour {
	public bool generateOnStart = true;
	public bool testNoise = false;
	public int resolution = 100;
	public int tileSize = 100;

	void Start () {
		//Used for testing noise settings
		if (testNoise) {
			Tile t = new Tile(50);
			t.CreateTerrainTile(resolution, tileSize, tileSize);
			t.PlaceTerrainTile(0, 0, 0);
			t.ApplyNoise();
		}
	}
}
