using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Voronoi
{
	/// <summary>
	/// Voronoi cell diagram uses a set of control points to partition space into cells. Each point in space belongs to a cell that corresponds to closest control point.
	/// This generator distributes control points using a vector settings source, that displaces points with integer coordinates. Thus, every unit-sized cube will have a single control point in it,
	/// randomly placed. A user-supplied function is then used to obtain cell value for a given point.
	/// </summary>
	public class VoronoiCells : Generator
	{
		private readonly Func<int, int, int, float> m_CellValueSource;
		private readonly LatticeNoise[] m_ControlPointSource;

		/// <summary>
		/// Create new Voronoi diagram using seed. Control points will be obtained using random displacment seeded by supplied value
		/// </summary>
		/// <param name="seed">Seed value</param>
		/// <param name="cellValueSource">Function that returns cell's value</param>
		public VoronoiCells(int seed, Func<int, int, int, float> cellValueSource)
		{
			Frequency = 1;
			m_ControlPointSource = new[]
			                       	{
			                       		new LatticeNoise(seed), new LatticeNoise(seed + 1), new LatticeNoise(seed + 2),
			                       	};
			m_CellValueSource = cellValueSource;
		}

		/// <summary>
		/// Noise period. Used for repeating (seamless) settings.
		/// When Period &gt;0 resulting settings pattern repeats exactly every Period, for all coordinates.
		/// </summary>
		public int Period
		{
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
		public override float GetValue(float x, float y, float z)
		{
			if (Period > 0)
			{
				// make periodic lattice. Repeat every Period cells
				x = x % Period; if (x < 0) x += Period;
				y = y % Period; if (y < 0) y += Period;
				z = z % Period; if (z < 0) z += Period;
			}

			x *= Frequency;
			y *= Frequency;
			z *= Frequency;
			float min = float.MaxValue;
			int ix = 0, iy = 0, iz = 0;

			int xc = Mathf.FloorToInt(x);
			int yc = Mathf.FloorToInt(y);
			int zc = Mathf.FloorToInt(z);

			var v = new Vector3(x, y, z);

			for (int ii = xc - 1; ii < xc + 2; ii++)
			{
				for (int jj = yc - 1; jj < yc + 2; jj++)
				{
					for (int kk = zc - 1; kk < zc + 2; kk++)
					{
						Vector3 displacement = new Vector3(
							m_ControlPointSource[0].GetValue(ii, jj, kk) * 0.5f + 0.5f,
							m_ControlPointSource[1].GetValue(ii, jj, kk) * 0.5f + 0.5f,
							m_ControlPointSource[2].GetValue(ii, jj, kk) * 0.5f + 0.5f);

						Vector3 cp = new Vector3(ii, jj, kk) + displacement;
						float distance = Vector3.SqrMagnitude(cp - v);

						if (distance < min)
						{
							min = distance;
							ix = ii;
							iy = jj;
							iz = kk;
						}
					}
				}
			}

			return m_CellValueSource(ix, iy, iz);
		}

		#endregion;

		/// <summary>
		/// Frequency of control points. This has the same effect as applying <see cref="Scale"/> transform to the generator, or placing control points closer together (for high frequency) or further apart (for low frequency)
		/// </summary>
		public float Frequency { get; set; }
	}
}