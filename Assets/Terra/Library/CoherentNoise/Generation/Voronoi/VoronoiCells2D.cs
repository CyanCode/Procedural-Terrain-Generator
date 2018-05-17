using System;
using Terra.CoherentNoise.Generation.Displacement;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Voronoi
{
	/// <summary>
	/// Voronoi cell diagram uses a set of control points to partition space into cells. Each point in space belongs to a cell that corresponds to closest control point.
	/// This generator distributes control pointsby randomly displacing points with integer coordinates. Thus, every unit-sized cube will have a single control point in it,
	/// randomly placed. A user-supplied function is then used to obtain cell value for a given point.
	/// 
	/// 2D version is faster, but ignores Z coordinate.
	/// </summary>
	public class VoronoiCells2D : Generator
	{
		private readonly Func<int, int, float> m_CellValueSource;
		private readonly LatticeNoise[] m_ControlPointSource;

		/// <summary>
		/// Create new Voronoi diagram using seed. Control points will be obtained using random displacment seeded by supplied value
		/// </summary>
		/// <param name="seed">Seed value</param>
		/// <param name="cellValueSource">Function that returns cell's value</param>
		public VoronoiCells2D(int seed, Func<int, int, float> cellValueSource)
		{
			Frequency = 1;
			m_ControlPointSource = new[]
			                       	{
			                       		new LatticeNoise(seed), new LatticeNoise(seed + 1),
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
			}

			x *= Frequency;
			y *= Frequency;
			float min = float.MaxValue;
			int ix = 0, iy = 0;

			int xc = Mathf.FloorToInt(x);
			int yc = Mathf.FloorToInt(y);

			var v = new Vector2(x, y);

			for (int ii = xc - 1; ii < xc + 2; ii++)
			{
				for (int jj = yc - 1; jj < yc + 2; jj++)
				{
						Vector2 displacement = new Vector2(
							m_ControlPointSource[0].GetValue(ii, jj, 0) * 0.5f + 0.5f,
							m_ControlPointSource[1].GetValue(ii, jj, 0) * 0.5f + 0.5f);

						Vector2 cp = new Vector2(ii, jj) + displacement;
						float distance = Vector2.SqrMagnitude(cp - v);

						if (distance < min)
						{
							min = distance;
							ix = ii;
							iy = jj;
						}
				}
			}

			return m_CellValueSource(ix, iy);
		}

		#endregion;

		/// <summary>
		/// Frequency of control points. This has the same effect as applying <see cref="Scale"/> transform to the generator, or placing control points closer together (for high frequency) or further apart (for low frequency)
		/// </summary>
		public float Frequency { get; set; }
	}
}