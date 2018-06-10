using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Terra.Terrain;
using Terra.Terrain.Util;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grass: MonoBehaviour {
	private struct PositionData {
		public float height;
		public Vector3 normal;

		public PositionData(float height, Vector3 normal) {
			this.height = height;
			this.normal = normal;
		}

		public override int GetHashCode() {
			unchecked { //Ignore overflow, wrap instead
				return (int)height - height.GetHashCode() +
					normal.GetHashCode();
			}
		}
	}

	public GameObject Track;
	public float GridSize = 0.5f;
	public float Radius = 15f;
	public float Variation = 0.3f;
	public bool UseCache = true;

	public MeshFilter Filter;

	private LRUCacheDictionary<Vector2, PositionData> PositionCache = new LRUCacheDictionary<Vector2, PositionData>(CACHE_CAPACITY);
	private float MaxHeight = 5000; //Default to high value
	private Vector3 lastPosition;

	const int CACHE_CAPACITY = 1000;

	void OnEnable() {
		//Keep track of TerrainTiles that have been added
		//in order to know the maximum height

		TerraEvent.OnMeshDidForm += ((go, m) => {
			float height = go.transform.position.y + m.bounds.max.y;
			MaxHeight = height > MaxHeight ? height + 10 : MaxHeight;
		});
	}

	/// <summary>
	/// Gets a grid of circular positions underneath this 
	/// gameobject's transform component.
	/// </summary>
	public List<Vector2> GetPositions() {
		Vector3 cPos = transform.position;
		Vector2 gridPos = new Vector2(Mathf.Round(cPos.x / GridSize),
									  Mathf.Round(cPos.z / GridSize));

		List<Vector2> pos = CircleUtil.GetPointsFromGrid(gridPos, Radius, GridSize);

		return pos;
	}

	/// <summary>
	/// Places grass on the terrain via cached Y values 
	/// and raycasting.
	/// </summary>
	public void UpdateGrass() {
		List<Vector2> positions = GetPositions();
		List<Vector3> finalPositions = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		Stopwatch sw = new Stopwatch();
		sw.Start();

		for (int i = 0; i < positions.Count; i++) {
			if (UseCache) {
				PositionData data = PositionCache.Get(positions[i]);

				if (data.height == 0.0f) { //Height not found, calculate
					Ray r = new Ray(new Vector3(positions[i].x, MaxHeight, positions[i].y), Vector3.down);
					RaycastHit hit;

					if (Physics.Raycast(r, out hit)) {
						data = new PositionData(hit.point.y, hit.normal);
						PositionCache.Add(positions[i], data);
					} else {
						continue; //Didn't hit anything, ignore this position
					}
				}

				finalPositions.Add(new Vector3(positions[i].x, data.height, positions[i].y));
				normals.Add(data.normal);
			} else {
				PositionData data;

				Ray r = new Ray(new Vector3(positions[i].x, MaxHeight, positions[i].y), Vector3.down);
				RaycastHit hit;

				if (Physics.Raycast(r, out hit)) {
					data = new PositionData(hit.point.y, hit.normal);
					//PositionCache.Add(positions[i], data);
				} else {
					continue; //Didn't hit anything, ignore this position
				}
				
				finalPositions.Add(new Vector3(positions[i].x, data.height, positions[i].y));
				normals.Add(data.normal);
			}
		}
		
		UnityEngine.Debug.Log("Lookups saved by cache: " + sw.ElapsedMilliseconds);

		ConstructMesh(finalPositions, normals);
	}

	/// <summary>
	/// Constructs a mesh of vertices and send it to the 
	/// GPU for grass rendering
	/// </summary>
	/// <param name="positions">Positions of grass quads</param>
	void ConstructMesh(List<Vector3> positions, List<Vector3> normals) {
		int count = positions.Count;
		Vector3[] verts = new Vector3[count];
		int[] indicies = new int[count];
		
		for (int i = 0; i < count; i++) {
			float x = positions[i].x - transform.position.x;
			float z = positions[i].z - transform.position.z;

			verts[i] = new Vector3(x, positions[i].y, z);
			indicies[i] = i;
		}

		Mesh m = new Mesh();
		m.vertices = verts;
		m.SetNormals(normals);
		m.SetIndices(indicies, MeshTopology.Points, 0);
		Filter.mesh = m;
	}

	
	void Update() {
		if (Track != null) {
			var tPos = Track.transform.position;
			transform.position = new Vector3(tPos.x, 0, tPos.z);
		}
		
		if (lastPosition == null || lastPosition != transform.position) {
			UpdateGrass();
			lastPosition = transform.position;
		}
	}

	//TODO Remove
	void OnDrawGizmosSelected() {
		if (Track != null) {
			var tPos = Track.transform.position;
			transform.position = new Vector3(tPos.x, 0, tPos.z);
		}

		if (lastPosition == null || lastPosition != transform.position) {
			UpdateGrass();
			lastPosition = transform.position;
		}
	}
}
