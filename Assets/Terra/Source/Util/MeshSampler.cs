using UnityEngine;

namespace Terra.Util {
	class MeshSampler {
		public class MeshSample {
			public readonly float Height;
			public readonly float Angle;

			public MeshSample(float height = 0f, float angle = 0f) {
				Height = height;
				Angle = angle;
			}
		}

		private Vector3[] _normals;
		private Vector3[] _vertices;
		private int _meshResolution;

		public MeshSampler(Vector3[] normals, Vector3[] vertices, int resolution) {
			_normals = normals;
			_vertices = vertices;
			_meshResolution = resolution;
		}

		public MeshSampler(Mesh m, int resolution) {
			_normals = m.normals;
			_vertices = m.vertices;
			_meshResolution = resolution;
		}

		/// <summary>
		/// Finds the height and angle of the passed x and z values on the mesh.
		/// </summary>
		/// <param name="x">Normalized x position to sample</param>
		/// <param name="z">Normalized z position to sample</param>
		/// <returns>MeshSample instance with calculated height and angle (0 to 90)</returns>
		public MeshSample SampleAt(float x, float z) {
			float res = _meshResolution;
			int sampleLoc = Mathf.RoundToInt(Mathf.Clamp(x * res, 0f, res - 1)) +
				Mathf.RoundToInt(Mathf.Clamp(z * res, 0f, res - 1)) * _meshResolution;
			float height = _vertices[sampleLoc].y;
			float angle = Vector3.Angle(_normals[sampleLoc], Vector3.up);

			return new MeshSample(height, angle);
		}
	}
}
