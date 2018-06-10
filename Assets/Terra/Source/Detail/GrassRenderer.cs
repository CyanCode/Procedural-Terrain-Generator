using System.Collections.Generic;
using Terra.CoherentNoise;
using Terra.Terrain;
using UnityEngine;

public class GrassRenderer {
	#region Instance Vars

	/// <summary>
	/// Stores information used in the creation of 
	/// a point cloud mesh
	/// </summary>
	private class PointCloudMesh {
		public Vector3[] points;
		public Vector3[] normals;
		public int[] indices;
	}

	/// <summary>
	/// Stores the height and mask maps used in computing a 
	/// fast point cloud
	/// </summary>
	private class PointCloudData {
		public int Resolution;

		/// <summary>
		/// Contains matrix of Y positions representing 
		/// the heights of the mesh (in world space)
		/// </summary>
		public float[,] HeightMap;

		/// <summary>
		/// Contains matrix of mask values pulled from the 
		/// supplied Generator
		/// </summary>
		public float[,] MaskMap;
	}

	/// <summary>
	/// Stores vertex / index data for the point clouds
	/// </summary>
	private List<PointCloudMesh> CloudMesh;

	private List<GrassDataTest> Data;

	private PointCloudData CloudData = new PointCloudData();

	/// <summary>
	/// TerrainTile component attached to this gameobject
	/// </summary>
	public TerrainTile Tile;

	/// <summary>
	/// GameObject attached to the TerrainTile component
	/// </summary>
	private GameObject gameObject {
		get {
			return Tile == null ? null : Tile.gameObject;
		}
	}

	/// <summary>
	/// If this GrassRenderer is attached to a gameobject that 
	/// has a TerrainTile
	/// </summary>
	private bool HasTile {
		get {
			return Tile != null;
		}
	}
	
	/// <summary>
	/// Returns true if this GrassRenderer has the required 
	/// information to create a point cloud
	/// </summary>
	public bool HasPointCloudData {
		get {
			return CloudMesh != null && CloudMesh.Count == Data.Count;
		}
	}

	#endregion

	/// <summary>
	/// Creates a new GrassRenderer and places grass according 
	/// to the settings specified in each GrassData instance
	/// </summary>
	public GrassRenderer(TerrainTile tt, List<GrassDataTest> data, int mapRes) {
		Tile = tt;
		Data = data;
		CloudData.Resolution = mapRes;
	}


	#region OLD
	/// <summary>
	/// Updates the height and mask maps by Raycasting 
	/// the terrain
	/// </summary>
	public void UpdateMaps() {
		if (HasTile && gameObject.GetComponent<MeshCollider>() != null &&
				gameObject.GetComponent<MeshRenderer>() != null) {

			Random.InitState(TerraSettings.GenerationSeed);
			MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

			if (CloudData == null)
				CloudData = new PointCloudData();

			//Initialize maps
			int res = CloudData.Resolution;
			CloudData.HeightMap = new float[res, res];
			CloudData.MaskMap = new float[res, res];

			float length = mr.bounds.size.x; //X & Z lengths are equal
			float xStart = mr.bounds.extents.x;
			float zStart = mr.bounds.extents.z;
			float maxHeight = mr.bounds.extents.y + 10;

			for (int i = 0; i < res; i++) {
				for (int j = 0; j < res; j++) {
					float x = (i / (float)res) * length;
					float z = (j / (float)res) * length;
					Vector3 rayPos = new Vector3(xStart + x, maxHeight, zStart + z);

					Ray r = new Ray(rayPos, Vector3.down);
					RaycastHit hit;

					if (Physics.Raycast(r, out hit) && hit.collider.GetComponent<TerrainTile>() != null) {
						//Raycast hit a terrain tile
						CloudData.HeightMap[i, j] = hit.point.y;

						foreach (GrassDataTest gd in Data) {
							if (gd.MaskMap == null)
								gd.MaskMap = new float[res, res];

							gd.MaskMap[i, j] = gd.Mask.GetValue(x, z, 0);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Calculates the necessary data needed for the creation of 
	/// a point cloud. Requires that this gameobject has an 
	/// attached MeshCollider & MeshRenderer
	/// </summary>
	public void CalculatePointCloudData() {
		//Cannot calculate point cloud data without 
		//a TerrainTile, MeshCollider, & MeshRenderer
		if (HasTile && gameObject.GetComponent<MeshCollider>() != null &&
			gameObject.GetComponent<MeshRenderer>() != null) {

			//Clear existing point cloud data if it exists
			if (CloudMesh != null)
				CloudMesh.Clear();

			Random.InitState(TerraSettings.GenerationSeed);
			MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
			MeshCollider mc = gameObject.GetComponent<MeshCollider>();

			for (int i = 0; i < Data.Count; i++) {
				PointCloudMesh pcd = new PointCloudMesh();
				GrassDataTest gd = Data[i];

				const int ESTIMATED_AMT = 1000;
				List<Vector3> verts = new List<Vector3>(ESTIMATED_AMT);
				List<Vector3> norms = new List<Vector3>(ESTIMATED_AMT);
				List<int> indices = new List<int>(ESTIMATED_AMT);

				float length = mr.bounds.size.x; //X & Z lengths are equal
				float xStart = mr.bounds.extents.x;
				float zStart = mr.bounds.extents.z;
				float maxHeight = mr.bounds.extents.y + 1000;

				for (float x = xStart; x > xStart - mr.bounds.size.x; x -= gd.Density) {
					for (float z = zStart; z < zStart + mr.bounds.size.z; z += gd.Density) {
						//Add some random variation to the grass positions
						float modX = x + Random.Range(-0.5f, 0.5f);
						float modZ = z + Random.Range(-0.5f, 0.5f);

						Vector3 pos = new Vector3(modX, maxHeight, modZ);
						Ray r = new Ray(pos, Vector3.down);
						RaycastHit hit;

						if (Physics.Raycast(r, out hit)) {
							bool hitTT = hit.collider.gameObject.GetComponent<TerrainTile>() != null;
							bool canPlace = gd.PlaceAt(hit.point.x, hit.point.z);

							if (hitTT && canPlace) {
								verts.Add(hit.point);
								norms.Add(hit.normal);
								indices.Add(i);
							}
						}
					}
				}

				if (CloudMesh == null)
					CloudMesh = new List<PointCloudMesh>();

				//Unfortunately we don't know the size of 
				//these data structures at the start
				pcd.points = verts.ToArray();
				pcd.normals = norms.ToArray();
				pcd.indices = indices.ToArray();
			}
		}
	}

	#endregion
}

/// <summary>
/// A data handler class that is responsible for 
/// keeping track of settings applied to a GrassRenderer
/// </summary>
public class GrassDataTest {
	/// <summary>
	/// Texture to be displayed on quad(s)
	/// </summary>
	public Texture2D Texture { get; private set; }

	/// <summary>
	/// Mask used for finding where to place 
	/// grass points
	/// </summary>
	public Generator Mask { get; private set; }

	/// <summary>
	/// How big the polled generator point must be 
	/// before deciding whether to show grass.
	/// 
	/// Ex. Influence = 0.75, Generator Point = 0.8
	/// Grass shows up
	/// </summary>
	public float Influence { get; private set; }

	/// <summary>
	/// How spread out the grass pieces are from each other
	/// </summary>
	public float Density { get; private set; }

	/// <summary>
	/// Map used for placing grass using a Generator
	/// </summary>
	public float[,] MaskMap;
	
	public GrassDataTest(Texture2D texture, Generator mask, float influence, float density) {
		Texture = texture;
		Mask = mask;
		Influence = influence;
		Density = density;
	}

	/// <summary>
	/// Should a grass quad be placed at the passed x & z 
	/// coordinates. Polls the Mask Generator and returns true 
	/// if it passes the influence
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="z">Z coordinate</param>
	/// <returns></returns>
	public bool PlaceAt(float x, float z) {
		float res = Mask.GetValue(x, z, 0);
		return res < Influence;
	}
}