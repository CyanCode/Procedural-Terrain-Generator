using System.Collections;
using System.Collections.Generic;
using Terra.Terrain;
using UnityEngine;

public class GrassPlacer : MonoBehaviour {
	public float StepLength = 0.5f;
	public GameObject Parent;
	public Material Material;

	private List<GrassTile.MeshData> md;

	public void CalculateVerts() {
		TerrainTile tt = GetComponent<TerrainTile>();
		GrassTile gt = new GrassTile(tt, StepLength);

		//gt.CalculateCells(1, (data) => {
		//	md = data;
		//});
		md = gt.CalculateCells(1, null);

		//Apply material to each Mesh
		foreach (GrassTile.MeshData data in md) {
			GameObject go = new GameObject();
			go.transform.parent = Parent.transform;

			var mf = go.AddComponent<MeshFilter>();
			var mr = go.AddComponent<MeshRenderer>();
			mr.material = Material;

			Mesh m = new Mesh();
			m.SetVertices(data.vertices);
			m.SetNormals(data.normals);
			m.SetIndices(data.indicies.ToArray(), MeshTopology.Points, 0);

			mf.mesh = m;
		}
	}

	void OnDrawGizmosSelected() {
		if (md != null) {
			bool meshPreview = false;

			if (!meshPreview) {
				foreach (GrassTile.MeshData data in md) {
					//Form square from first and last verts
					Vector3 first = data.vertices[0];
					Vector3 last = data.vertices[data.vertices.Count - 1];
					Vector3 center = new Vector3((first.x + last.x) / 2, 0, (first.z + last.z) / 2);
					Vector3 size = new Vector3(first.x - last.x, 1, first.z - last.z);

					Gizmos.DrawCube(center, size);
				}
			} else {
				var data = md[0];
				Mesh m = new Mesh();
				m.SetVertices(data.vertices);
				m.SetNormals(data.normals);
				m.SetIndices(data.indicies.ToArray(), MeshTopology.Points, 0);

				Gizmos.DrawWireMesh(m);
			}
		}
	}

	private class GrassTile {
		public delegate void CalcFinished(List<MeshData> data);

		public struct MeshData {
			public List<Vector3> vertices;
			public List<Vector3> normals;
			public List<int> indicies;
		}

		private MeshCollider _mc;
		private MeshCollider MeshCollider {
			get {
				if (_mc == null) {
					_mc = Tile.GetComponent<MeshCollider>();
				}

				return _mc;
			}
		}

		private TerrainTile Tile;
		private float StepLength;
		private int NumDivisions;

		public GrassTile(TerrainTile tile, float stepLength, int numDivisions = 5) {
			Tile = tile;
			StepLength = stepLength;
			NumDivisions = numDivisions;
		}

		/// <summary>
		/// Calculates MeshData incrementally using a coroutine. 
		/// TerrainTile instance must have a MeshCollider attached to 
		/// the same gameobject
		/// </summary>
		/// <param name="meshMaxVerts">Maximum amount of vertices each mesh should contain</param>
		/// <param name="onCalculated">Callback delegate when operations have finished</param>
		public List<MeshData> CalculateCells(int meshMaxVerts, CalcFinished onCalculated) {
			List<MeshData> data = new List<MeshData>(NumDivisions * NumDivisions);

			if (MeshCollider == null) {
				Debug.LogError("GrassPlacer cannot be used on a TerrainTile without a collider.");
				//yield break;
			}

			float cellXStart = MeshCollider.bounds.min.x;
			float cellZStart = MeshCollider.bounds.min.z;
			float cellXEnd = cellXStart + (MeshCollider.bounds.size.x);
			float cellZEnd = cellZStart + (MeshCollider.bounds.size.z);
			float cellLength = (MeshCollider.bounds.size.x / NumDivisions);
			for (float x = cellXStart; x < cellXEnd; x += cellLength) {
				for (float z = cellZStart; z < cellZEnd; z += cellLength) {
					data.Add(CalculateCell(x, z, meshMaxVerts));
					//yield return null; //Wait for next frame
				}
			}

			//onCalculated(data);
			return data;
		}

		private MeshData CalculateCell(float cellX, float cellZ, int meshMaxVerts) {
			MeshData md = new MeshData();
			List<Vector3> verts = new List<Vector3>(meshMaxVerts);
			List<Vector3> norms = new List<Vector3>(meshMaxVerts);
			List<int> indicies = new List<int>(meshMaxVerts);

			float tileLen = MeshCollider.bounds.size.x;
			float cellLength = tileLen / NumDivisions;
			float height = MeshCollider.bounds.max.y + 5;

			int index = 0;
			for (float x = 0; x < cellLength; x += StepLength) {
				for (float z = 0; z < cellLength; z += StepLength) {
					float worldX = x + cellX;
					float worldZ = z + cellZ;

					Vector3 origin = new Vector3(worldX, height, worldZ);
					Ray r = new Ray(origin, Vector3.down);
					RaycastHit hit;

					//If raycast hit a collider that contains a TerrainTile
					if (Physics.Raycast(r, out hit) && hit.collider.GetComponent<TerrainTile>() != null) {
						Vector3 pos = new Vector3(worldX, hit.point.y, worldZ);
						Vector3 normal = hit.normal;
						verts.Add(pos);
						norms.Add(normal);
						indicies.Add(index);

						index++;
					}
				}
			}

			md.vertices = verts;
			md.normals = norms;
			md.indicies = indicies;
			return md;
		}
	}
}
