using System;
using Terra.CoherentNoise.Generation.Displacement;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Voronoi {
	/// <summary>
	/// Voronoi cell diagram uses a set of control points to partition space into cells. Each point in space belongs to a cell that corresponds to closest control point.
	/// This generator distributes control pointsby randomly displacing points with integer coordinates. Thus, every unit-sized cube will have a single control point in it,
	/// randomly placed. A user-supplied function is then used to obtain cell value for a given point.
	/// 
	/// 2D version is faster, but ignores Z coordinate.
	/// </summary>
	public class VoronoiCellsBasic: Generator {
		private readonly Func<int, int, float> m_CellValueSource;
		private readonly LatticeNoise[] m_ControlPointSource;
		private readonly int m_Seed;
		private readonly float m_Displacement;

		/// <summary>
		/// Create new Voronoi diagram using seed. Control points determined by random displacement.
		/// </summary>
		/// <param name="seed">Seed value</param>
		public VoronoiCellsBasic(int seed, float displacement) {
			Frequency = 1;
			m_Seed = seed;
			m_Displacement = displacement;
		}

		/// <summary>
		/// Noise period. Used for repeating (seamless) settings.
		/// When Period &gt;0 resulting settings pattern repeats exactly every Period, for all coordinates.
		/// </summary>
		public int Period {
			get;
			set;
		}

		#region Overrides of Noise

		/// <summary>
		///  Returns settings value at given point. 
		///  </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param><returns>Noise value</returns>
		public override float GetValue(float x, float y, float z) {
			// This method could be more efficient by caching the seed values.  Fix
			// later.

			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			int xInt = (x > 0.0 ? (int)x : (int)x - 1);
			int yInt = (y > 0.0 ? (int)y : (int)y - 1);
			int zInt = (z > 0.0 ? (int)z : (int)z - 1);

			double minDist = 2147483647.0;
			float xCandidate = 0;
			float yCandidate = 0;
			float zCandidate = 0;

			ValueNoise vn = new ValueNoise(m_Seed);

			// Inside each unit cube, there is a seed point at a random position.  Go
			// through each of the nearby cubes until we find a cube with a seed point
			// that is closest to the specified position.
			for (int zCur = zInt - 2; zCur <= zInt + 2; zCur++) {
				for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++) {
					for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++) {

						// Calculate the position and distance to the seed point inside of
						// this unit cube.
						float xPos = xCur + vn.GetValue(xCur, yCur, zCur);
						float yPos = yCur + vn.GetValue(xCur, yCur, zCur);
						float zPos = zCur + vn.GetValue(xCur, yCur, zCur);
						double xDist = xPos - x;
						double yDist = yPos - y;
						double zDist = zPos - z;
						double dist = xDist * xDist + yDist * yDist + zDist * zDist;

						if (dist < minDist) {
							// This seed point is closer to any others found so far, so record
							// this seed point.
							minDist = dist;
							xCandidate = xPos;
							yCandidate = yPos;
							zCandidate = zPos;
						}
					}
				}
			}

			float value = 0.0f;
			//if (m_enableDistance) {
			//	// Determine the distance to the nearest seed point.
			//	double xDist = xCandidate - x;
			//	double yDist = yCandidate - y;
			//	double zDist = zCandidate - z;
			//	value = (sqrt(xDist * xDist + yDist * yDist + zDist * zDist)
			//		  ) * SQRT_3 - 1.0;
			//} else {
			//	value = 0.0;
			//}

			// Return the calculated distance with the displacement value applied.
			return value + m_Displacement * new ValueNoise(m_Seed).GetValue(
				       Mathf.Floor(xCandidate),
				       Mathf.Floor(yCandidate),
				       Mathf.Floor(zCandidate));
		}

		#endregion;

		/// <summary>
		/// Frequency of control points. This has the same effect as applying <see cref="Scale"/> transform to the generator, or placing control points closer together (for high frequency) or further apart (for low frequency)
		/// </summary>
		public float Frequency { get; set; }
	}
}