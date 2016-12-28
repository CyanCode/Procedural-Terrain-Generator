using UnityEngine;

[RequireComponent(typeof(MaterialSetting))]
public class TerrainTiler : MonoBehaviour {
	public bool generateOnStart = true;
	public bool testNoise = false;

	public MaterialSetting MaterialSettings;
	public int Resolution = 20;
	public float TileSize = 0.5f;
	public int gain = 25;

	void Start () {
		//Used for testing noise settings
		if (testNoise) {
			GameObject toTrack = GameObject.CreatePrimitive(PrimitiveType.Cube);
			toTrack.transform.position = new Vector3(0, 0, 0);

			TilePool pool = new TilePool(toTrack, this);
			pool.UpdateTiles();
		}

		if (generateOnStart) {
			//TODO Start tiling process
		}
	}
}