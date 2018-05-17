using Terra.CoherentNoise.Generation.Displacement;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Voronoi
{
	/// <summary>
	/// Base class for 2D Voronoi diagrams generators. Voronoi diagrams use a set of control points, that are somehow distributed, and for every point calculate distances to the closest control points.
	/// These distances are then combined to obtain final settings value.
	/// This generator distributes control points by randomly displacing points with integer coordinates. Thus, every unit-sized cube will have a single control point in it,
	/// randomly placed.
	/// 2D version is faster, but ignores Z coordinate.
	/// </summary>
	public abstract class VoronoiDiagramBase2D : Generator
	{
		private readonly LatticeNoise[] m_ControlPointSource;

		/// <summary>
		/// Create new Voronoi diagram using seed. Control points will be obtained using random displacment seeded by supplied value
		/// </summary>
		/// <param name="seed">Seed value</param>
		protected VoronoiDiagramBase2D(int seed)
		{
			Frequency = 1;
			m_ControlPointSource = new[]
			                       	{
			                       		new LatticeNoise(seed), new LatticeNoise(seed + 1),
			                       	};
		}

		/// <summary>
		/// Noise period. Used for repeating (seamless) settings.
		/// When Period &gt;0 resulting settings pattern repeats exactly every Period, for all coordinates.
		/// </summary>
		public int Period
		{
			get; set;
		}


		#region Overrides of Noise

		/// <summary>
		///  Returns settings value at given point. 
		///  </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <returns>Noise value</returns>
		public override float GetValue(float x, float y, float z)
		{
			if (Period > 0)
			{
				// make periodic lattice. Repeat every Period cells
				x = x % Period; if (x < 0) x += Period;
				y = y % Period; if (y < 0) y += Period;
			}

			// stretch values to match desired frequency
			x *= Frequency;
			y *= Frequency;

			float min1 = float.MaxValue, min2 = float.MaxValue, min3 = float.MaxValue;

			int xc = Mathf.FloorToInt(x);
			int yc = Mathf.FloorToInt(y);

			var v = new Vector2(x, y);

			for (int ii = xc - 1; ii < xc + 2; ii++)
			{
				for (int jj = yc - 1; jj < yc + 2; jj++)
				{
					Vector2 displacement = new Vector2(
						m_ControlPointSource[0].GetValue(ii, jj, 0)*0.5f + 0.5f,
						m_ControlPointSource[1].GetValue(ii, jj, 0)*0.5f + 0.5f);

					Vector2 cp = new Vector2(ii, jj) + displacement;
					float distance = Vector2.SqrMagnitude(cp - v);

					if (distance < min1)
					{
						min3 = min2;
						min2 = min1;
						min1 = distance;
					}
					else if (distance < min2)
					{
						min3 = min2;
						min2 = distance;
					}
					else if (distance < min3)
					{
						min3 = distance;
					}
				}
			}

			return GetResult(Mathf.Sqrt(min1), Mathf.Sqrt(min2), Mathf.Sqrt(min3));
		}

		/// <summary>
		/// Override this method to calculate final value using distances to closest control points.
		/// Note that distances that get passed to this function are adjusted for frequency (i.e. distances are scaled so that 
		/// control points are in unit sized cubes)
		/// </summary>
		/// <param name="min1">Distance to closest point</param>
		/// <param name="min2">Distance to second-closest point</param>
		/// <param name="min3">Distance to third-closest point</param>
		/// <returns></returns>
		protected abstract float GetResult(float min1, float min2, float min3);

		#endregion

		/// <summary>
		/// Frequency of control points. This has the same effect as applying <see cref="Scale"/> transform to the generator, or placing control points closer together (for high frequency) or further apart (for low frequency)
		/// </summary>
		public float Frequency { get; set; }
	}
}