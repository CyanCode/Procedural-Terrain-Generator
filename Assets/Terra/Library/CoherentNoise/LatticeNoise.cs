namespace Terra.CoherentNoise
{
	internal class LatticeNoise
	{
		private int m_Seed;

		internal LatticeNoise(int seed)
		{
			m_Seed = seed;
		}

		/// <summary>
		/// Lattice period. Used for periodic (seamless) generators.
		/// Noise is non-periodic if Period &lt;= 0
		/// </summary>
		internal int Period { get; set; }

		/// <summary>
		/// Noise value at integer coordinates. Used as a source for interpolated coherent settings
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		internal float GetValue(int x, int y, int z)
		{
			if (Period > 0)
			{
				// make periodic lattice. Repeat every Period cells
				x = x % Period; if (x < 0) x += Period;
				y = y % Period; if (y < 0) y += Period;
				z = z % Period; if (z < 0) z += Period;
			}
			// All constants are primes and must remain prime in order for this settings
			// function to work correctly.
			// These constant values are lifted directly from libnoise
			int n = (
				Constants.MultiplierX * x
			  + Constants.MultiplierY * y
			  + Constants.MultiplierZ * z
			  + Constants.MultiplierSeed * m_Seed)
			  & 0x7fffffff;
			n = (n >> 13) ^ n;
			n = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
			return 1 - (n / 1073741824f); // normalize for [-1,1]
		}
	}
}