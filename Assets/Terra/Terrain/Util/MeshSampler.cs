using UnityEngine;

namespace Terra.Terrain.Util {
	class MeshSampler {
		public class MeshSample {
			readonly public float Height;
			readonly public float Angle;

			public MeshSample(float height = 0f, float angle = 0f) {
				Height = height;
				Angle = angle;
			}
		}

		private Vector3[] Normals;
		private Vector3[] Vertices;
		private int MeshResolution;

		public MeshSampler(Vector3[] normals, Vector3[] vertices, int resolution) {
			Normals = normals;
			Vertices = vertices;
			MeshResolution = resolution;
		}

		public MeshSampler(Mesh m, int resolution) {
			Normals = m.normals;
			Vertices = m.vertices;
			MeshResolution = resolution;
		}

		/// <summary>
		/// Finds the height and angle of the passed x and z values on the mesh.
		/// </summary>
		/// <param name="x">Normalized x position to sample</param>
		/// <param name="z">Normalized z position to sample</param>
		/// <returns>MeshSample instance with calculated height and angle (0 to 90)</returns>
		public MeshSample SampleAt(float x, float z) {
			float res = MeshResolution;
			int sampleLoc = Mathf.RoundToInt(Mathf.Clamp(x * res, 0f, res - 1)) +
				Mathf.RoundToInt(Mathf.Clamp(z * res, 0f, res - 1)) * MeshResolution;
			float height = Vertices[sampleLoc].y;
			float angle = Vector3.Angle(Normals[sampleLoc], Vector3.up);

			return new MeshSample(height, angle);
		}
	}
}
