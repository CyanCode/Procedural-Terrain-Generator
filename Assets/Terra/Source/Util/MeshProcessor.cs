using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Terra.Terrain.Util {
	class MeshProcessor {
		public static Mesh Process(Mesh mesh) {
			// cache data for speed
			List<Vector3> positions = new List<Vector3>(mesh.vertices);
			List<Vector2> uv0 = new List<Vector2>(positions.Count);
			List<Vector4> uv1 = new List<Vector4>(positions.Count);
			List<Vector4> uv2 = new List<Vector4>(positions.Count);
			List<Vector4> uv3 = new List<Vector4>(positions.Count);

			mesh.GetUVs(0, uv0);
			mesh.GetUVs(1, uv1);
			mesh.GetUVs(2, uv2);
			mesh.GetUVs(3, uv3);

			var normals = new List<Vector3>(mesh.normals);
			var tangents = new List<Vector4>(mesh.tangents);
			var faces = new List<int>(mesh.triangles);
			var colors = new List<Color>(positions.Count);
			for (int i = 0; i < positions.Count; ++i)
				colors.Add(Color.black);

			Color[] markings = new Color[3] { Color.red, Color.green, Color.blue };
			bool[] state = new bool[3];
			// go through data structure and mark colors, adding new splits when necissary
			int tcount = faces.Count;
			for (int i = 0; i < tcount; i = i + 3) {
				state[0] = false;
				state[1] = false;
				state[2] = false;

				int[] fIdx = new int[] { faces[i], faces[i + 1], faces[i + 2] };

				// mark currently used colors in face
				for (int x = 0; x < 3; ++x) {
					int index = fIdx[x];
					Color c = colors[index];
					if (c == Color.red) {
						state[0] = true;
					} else if (c == Color.green) {
						state[1] = true;
					} else if (c == Color.blue) {
						state[2] = true;
					}
				}
				for (int x = 0; x < 3; ++x) {
					int index = fIdx[x];
					Color c = colors[index];
					if (c == Color.black) {
						for (int y = 0; y < 3; ++y) {
							if (state[y] == false) {
								state[y] = true;
								colors[index] = markings[y];
								break;
							}
						}
					}
				}
				for (int x = 0; x < 3; ++x) {
					int index = fIdx[x];
					Color c = colors[index];
					Color c0 = colors[fIdx[0]];
					Color c1 = colors[fIdx[1]];
					Color c2 = colors[fIdx[2]];

					// out of colors? make a new index and map this triangle to use it
					if (c == Color.black ||
						  ((x == 0 && (c == c1 || c == c2)) ||
						  (x == 1 && (c == c0 || c == c2)) ||
						  (x == 2 && (c == c0 || c == c1)))) {
						int origLen = positions.Count;
						int newIdx = positions.Count;
						positions.Add(positions[index]);
						faces[i + x] = newIdx;

						if (normals != null && normals.Count == origLen) {
							normals.Add(normals[index]);
						}
						if (tangents != null && tangents.Count == origLen) {
							tangents.Add(tangents[index]);
						}
						if (uv0 != null && uv0.Count == origLen) {
							uv0.Add(uv0[index]);
						}
						if (uv1 != null && uv1.Count == origLen) {
							uv1.Add(uv1[index]);
						}
						if (uv2 != null && uv2.Count == origLen) {
							uv2.Add(uv2[index]);
						}
						if (uv3 != null && uv3.Count == origLen) {
							uv3.Add(uv3[index]);
						}

						// figure out which color we can use
						// add so we get something like 1, 1, 0

						Color cc = Color.red;
						if (c0 == cc || c1 == cc || c2 == cc) {
							cc = Color.green;
							if (c0 == cc || c1 == cc || c2 == cc)
								cc = Color.blue;
						}

						fIdx[x] = newIdx;
						colors.Add(cc);
					}


				}
			}
#if UNITY_2017_3_OR_GREATER

#else
			if (positions.Count == 65533) {
				Debug.LogError("Resulting Mesh " + mesh.name + " is over vertex limit, please use a mesh with less vertices\n"
				   + mesh.vertexCount + "->" + positions.Count);
				return null;
			}

#endif

			Mesh m = new Mesh();
			m.Clear();
#if UNITY_2017_3_OR_GREATER
         m.indexFormat = mesh.indexFormat;
#endif
			m.vertices = positions.ToArray();
			m.SetUVs(0, uv0);
			if (uv1 != null && uv1.Count > 0) {
				m.SetUVs(1, uv1);
			}
			if (uv2 != null && uv2.Count > 0) {
				m.SetUVs(2, uv1);
			}
			if (uv3 != null && uv3.Count > 0) {
				m.SetUVs(3, uv1);
			}
			m.triangles = faces.ToArray();
			m.colors = colors.ToArray();
			m.normals = normals.ToArray();
			m.tangents = tangents.ToArray();

			m.name = mesh.name;
			m.RecalculateBounds();
			m.UploadMeshData(false);
			return m;

		}
	}
}
