using Terra.CoherentNoise.Interpolation;
using UnityEngine;

namespace Terra.CoherentNoise.Generation
{
	/// <summary>
	/// Most basic coherent settings: value settings. This algorithm generates random values in integer coordinates and smoothly interpolates between them.
	/// Generated settings has no special characteristics except that it's noisy. 
	/// 
	/// Values returned range from -1 to 1.
	/// </summary>
	public class ValueNoise : Generator
	{
		private readonly CoherentNoise.LatticeNoise m_Source;
		private readonly SCurve m_SCurve;

		/// <summary>
		/// Create new generator with specified seed
		/// </summary>
		/// <param name="seed">settings seed</param>
		public ValueNoise(int seed)
			: this(seed, null)
		{

		}

		/// <summary>
		/// Noise period. Used for repeating (seamless) settings.
		/// When Period &gt;0 resulting settings pattern repeats exactly every Period, for all coordinates.
		/// </summary>
		public int Period { get { return m_Source.Period; } set { m_Source.Period = value; } }

		/// <summary>
		/// Create new generator with specified seed and interpolation algorithm. Different interpolation algorithms can make settings smoother at the expense of speed.
		/// </summary>
		/// <param name="seed">settings seed</param>
		/// <param name="sCurve">Interpolator to use. Can be null, in which case default will be used</param>
		public ValueNoise(int seed, SCurve sCurve)
		{
			m_Source = new CoherentNoise.LatticeNoise(seed);
			m_SCurve = sCurve;
		}

		private SCurve SCurve { get { return m_SCurve ?? SCurve.Default; } }

		#region Implementation of Noise

		/// <summary>
		/// Returns settings value at given point. 
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <returns>Noise value</returns>
		public override float GetValue(float x, float y, float z)
		{
			int ix = Mathf.FloorToInt(x);
			int iy = Mathf.FloorToInt(y);
			int iz = Mathf.FloorToInt(z);

			// interpolate the coordinates instead of values - it's way faster
			float xs = SCurve.Interpolate(x - ix);
			float ys = SCurve.Interpolate(y - iy);
			float zs = SCurve.Interpolate(z - iz);

			// THEN we can use linear interp to find our value - triliear actually

			float n0 = m_Source.GetValue(ix, iy, iz);
			float n1 = m_Source.GetValue(ix + 1, iy, iz);
			float ix0 = Mathf.Lerp(n0, n1, xs);

			n0 = m_Source.GetValue(ix, iy + 1, iz);
			n1 = m_Source.GetValue(ix + 1, iy + 1, iz);
			float ix1 = Mathf.Lerp(n0, n1, xs);

			float iy0 = Mathf.Lerp(ix0, ix1, ys);

			n0 = m_Source.GetValue(ix, iy, iz + 1);
			n1 = m_Source.GetValue(ix + 1, iy, iz + 1);
			ix0 = Mathf.Lerp(n0, n1, xs); // on y=0, z=1 edge

			n0 = m_Source.GetValue(ix, iy + 1, iz + 1);
			n1 = m_Source.GetValue(ix + 1, iy + 1, iz + 1);
			ix1 = Mathf.Lerp(n0, n1, xs); // on y=z=1 edge

			float iy1 = Mathf.Lerp(ix0, ix1, ys);

			return Mathf.Lerp(iy0, iy1, zs); // inside cube
		}


		#endregion
	}
}