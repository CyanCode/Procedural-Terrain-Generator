using UnityEngine;

public class TestMesh : MonoBehaviour {
	public bool testSquare = true;
	public bool testPlane = true;
	public bool testPerlinPlane = true;
	public bool testTerrain = true;
	public bool testTiling = true;

	public Texture2D[] Textures;

	void Start () {
		if (testSquare) Square();
		if (testPlane) Plane();
		if (testPerlinPlane) PerlinPlane();
		if (testTiling) Tiling();
		if (testTerrain) Terrain();
	}

	private void Square() {
		//Mesh m = TerrainMesh.CreateSquare(20, 20);

		//GameObject obj = new GameObject();
		//obj.AddComponent<MeshFilter>().mesh = m;
		//obj.AddComponent<MeshRenderer>();
	}

	private void Plane() {
		Tile t = new Tile(25, 20, .5f, .5f);
		t.CreateTerrainTile(0, 0);
		t.Render();
	}

	private void PerlinPlane() {
		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		Tile t = new Tile(25, 20, .5f, .5f);
		t.CreateTerrainTile(0, 0);
		t.ApplyNoise();

		sw.Stop();
		Debug.Log("Plane generation time in milliseconds: " + sw.ElapsedMilliseconds);
		Debug.Log("In seconds: " + sw.Elapsed.Seconds);
	}

	private void Terrain() {
		GameObject toTrack = GameObject.CreatePrimitive(PrimitiveType.Cube);
		toTrack.transform.position = new Vector3(0, 0, 0);


		//new TilePool(toTrack, null).UpdateTiles2(Textures);
	}

	private void Tiling() {

	}
}
