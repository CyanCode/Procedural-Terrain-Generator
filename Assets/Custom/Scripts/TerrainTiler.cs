using UnityEngine;

public class TerrainTiler : MonoBehaviour {
	public bool generateOnStart = true;
	public bool testNoise = false;
	public int resolution = 20;
	public float tileSize = 0.5f;

	void Start () {
		//Used for testing noise settings
		if (testNoise) {
			GameObject toTrack = GameObject.CreatePrimitive(PrimitiveType.Cube);
			toTrack.transform.position = new Vector3(0, 0, 0);

			Tile t = new Tile(50);
			t.CreateTerrainTile(resolution, tileSize, tileSize, 0, 0);
			t.tilePosition = new Vector2(0, 0);
			t.ApplyNoise();

			TilePool pool = new TilePool(toTrack);
		}

		if (generateOnStart) {
			//TODO Start tiling process
		}
	}
}