using System;
using UnityEngine;

namespace Terra.Structure {
	/// <summary>
	/// An implementation of <see cref="Mesh"/> that does not return 
	/// copies of its' parameters.
	/// </summary>
	[Serializable]
	public struct MeshData {
		public Vector3[] Vertices { 
			get { return _vertices; }
			set {
				_vertices = value;
				_meshDirty = true;
			}
		}
		public Vector3[] Normals {
			get { return _normals; }
			set {
				_normals = value;
				_meshDirty = true;
			}
		}
		public Vector2[] Uvs {
			get { return _uvs; }
			set {
				_uvs = value;
				_meshDirty = true;
			}
		}
		public int[] Triangles {
			get { return _triangles; }
			set {
				_triangles = value;
				_meshDirty = true;
			}
		}

		//Private instances of public variables for 
		//allowing mesh reconstruction on value change
		[SerializeField]
		private Vector3[] _vertices;
		[SerializeField]
		private Vector3[] _normals;
		[SerializeField]
		private Vector2[] _uvs;
		[SerializeField]
		private int[] _triangles;

		/// <summary>
		/// The <see cref="Mesh"/> class representation of this MeshData. 
		/// Internally, the construction of this Mesh instance is done upon 
		/// access if any of the instance variables have changed.
		/// </summary>
		public Mesh Mesh {
			get {
				if (_mesh == null || _meshDirty) {
					_mesh = new Mesh {
						vertices = Vertices,
						normals = Normals,
						uv = Uvs,
						triangles = Triangles
					};
					_meshDirty = false;
				}

				return _mesh;
			}
		}

		public static bool operator ==(MeshData lhs, MeshData rhs) {
			return lhs._vertices == rhs._vertices && lhs._normals == rhs._normals &&
				lhs._uvs == rhs._uvs && lhs._triangles == rhs._triangles;
		}

		public static bool operator !=(MeshData lhs, MeshData rhs) {
			return !(lhs == rhs);
		}

		private Mesh _mesh;
		private bool _meshDirty;
	}
}