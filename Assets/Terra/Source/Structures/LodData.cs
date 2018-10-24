using System;
using Terra.Terrain;
using UnityEngine;

namespace Terra.Structure {
	[Serializable]
	public class LodData {
		public enum LodLevelType {
			High, Medium, Low
		}

		/// <summary>
		/// Represents a level of detail for each <see cref="TileMesh"/> to 
		/// adhere to. This specifies the resolution of various maps across 
		/// <see cref="Tile"/>s.
		/// 
		/// The resolution of <see cref="SplatmapResolution"/> and 
		/// <see cref="MeshResolution"/> cannot be greater than 
		/// <see cref="MapResolution"/>.
		/// </summary>
		[Serializable]
		public class LodLevel {
			[SerializeField]
			private int _mapRes;
			[SerializeField]
			private int _splatmapRes;
			[SerializeField]
			private int _meshRes;

			/// <summary>
			/// Where on the circular grid does this LOD level start to appear?
			/// </summary>
			public int StartRadius;

			public static bool operator <(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes < rhs._mapRes;
			}

			public static bool operator >(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes > rhs._mapRes;
			}

			public static bool operator <=(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes <= rhs._mapRes;
			}

			public static bool operator >=(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes >= rhs._mapRes;
			}

			/// <summary>
			/// Resolution of the height, moisture, and temperature maps for 
			/// each <see cref="Tile"/>.
			/// </summary>
			public int MapResolution {
				get { return _mapRes; }
				set {
					_mapRes = value;
					VerifyResolutions();
				}
			}

			/// <summary>
			/// Resolution of to-be created splatmap for tiles of this <see cref="LodLevel"/>
			/// </summary>
			public int SplatmapResolution {
				get { return _splatmapRes; }
				set {
					_splatmapRes = value;
					VerifyResolutions(); 
				}
			}

			/// <summary>
			/// Resolution of to-be created mesh for tiles of this <see cref="LodLevel"/>
			/// </summary>
			public int MeshResolution {
				get { return _meshRes; }
				set {
					_meshRes = value;
					VerifyResolutions();
				}
			}

			public LodLevel(int startRadius, int mapRes, int splatmapRes, int meshRes) {
				StartRadius = startRadius;
				_mapRes = mapRes;
				_splatmapRes = splatmapRes;
				_meshRes = meshRes;

				VerifyResolutions();
			}

			private void VerifyResolutions() {
				_mapRes = Mathf.ClosestPowerOfTwo(_mapRes);
				_splatmapRes = Mathf.ClosestPowerOfTwo(_splatmapRes);
				_meshRes = Mathf.ClosestPowerOfTwo(_meshRes);
			
				if (SplatmapResolution > MapResolution)
					_splatmapRes = MapResolution;
				if (MeshResolution > MapResolution)
					_meshRes = MapResolution;
			}
		}

		[SerializeField]
		private bool _useLowLod;
		[SerializeField]
		private bool _useMediumLod;
		[SerializeField]
		private bool _useHighLod = true;

		public bool UseLowLodLevel {
			get { return _useLowLod; }
			set { _useLowLod = value; VerifyLodLevelEnabled(); }
		}
		public bool UseMediumLodLevel {
			get { return _useMediumLod; }
			set { _useMediumLod = value; VerifyLodLevelEnabled(); }
		}
		public bool UseHighLodLevel {
			get { return _useHighLod; }
			set { _useHighLod = value; VerifyLodLevelEnabled(); }
		}

		public LodLevel Low = new LodLevel(2, 32, 32, 32);
		public LodLevel Medium = new LodLevel(1, 64, 64, 64);
		public LodLevel High = new LodLevel(0, 512, 128, 512);

		/// <summary>
		/// Get the LodLevel associated with the passed radius. If 
		/// no level matches, <see cref="High"/> is returned instead.
		/// </summary>
		/// <param name="radius">Radius to look for</param>
		public LodLevel GetLevelForRadius(int radius) {
			foreach (var lvl in new[]{ Low, Medium, High }) {
				if (lvl.StartRadius < radius) {
					return lvl;
				}
			}

			return High;
		}

		/// <summary>
		/// Works exactly the same as <see cref="GetLevelForRadius"/> 
		/// but instead of returning the <see cref="LodLevel"/> itself, 
		/// the <see cref="LodLevelType"/> is returned.
		/// </summary>
		/// <param name="radius">Radius to look for</param>
		public LodLevelType GetLevelTypeForRadius(int radius) {
			if (UseLowLodLevel && Low.StartRadius < radius)
				return LodLevelType.Low;
			if (UseMediumLodLevel && Medium.StartRadius < radius)
				return LodLevelType.Medium;

			return LodLevelType.High;
		}
		
		/// <summary>
		/// Ensures that at least one LOD level is enabled at a time. 
		/// If all levels are false, <see cref="UseHighLodLevel"/> is 
		/// made true.
		/// </summary>
		private void VerifyLodLevelEnabled() {
			if (!UseLowLodLevel && !UseMediumLodLevel && !UseHighLodLevel) {
				UseHighLodLevel = true;
			}
		}
	}
}
