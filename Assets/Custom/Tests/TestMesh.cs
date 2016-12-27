using UnityEngine;
using System.Collections;

public class TestMesh : MonoBehaviour {
	public bool testSquare = true;
	public bool testPlane = true;
	public bool testPerlinPlane = true;
	public bool testTiling = true;
	
	void Start () {
		if (testSquare) Square();
		if (testPlane) Plane();
		if (testPerlinPlane) PerlinPlane();
		if (testTiling) Tiling();
	}

	private void Square() {
		//Mesh m = TerrainMesh.CreateSquare(20, 20);

		//GameObject obj = new GameObject();
		//obj.AddComponent<MeshFilter>().mesh = m;
		//obj.AddComponent<MeshRenderer>();
	}

	private void Plane() {
		TerrainMesh tm = new TerrainMesh(10, 0.5f, 0.5f);
		tm.CreateTerrainMesh(0f, 0f);
	}

	private void PerlinPlane() {
		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		Tile t = new Tile(25);
		//t.CreateTerrainTile(10, .5f, .5f, 0, 0);
		t.CreateTerrainTile(20, .5f, .5f, 0, 0);
		t.ApplyNoise();

		sw.Stop();
		Debug.Log("Plane generation time in milliseconds: " + sw.ElapsedMilliseconds);
		Debug.Log("In seconds: " + sw.Elapsed.Seconds);
	}

	private void Tiling() {

	}
}
