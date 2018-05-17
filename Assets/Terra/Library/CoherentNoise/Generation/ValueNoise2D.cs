using Terra.CoherentNoise.Interpolation;
using UnityEngine;

namespace Terra.CoherentNoise.Generation
{
	/// <summary>
	/// This is the same settings as <see cref="ValueNoise"/>, but it does not change in Z direction. This is more efficient if you're only interested in 2D settings anyway.
	/// </summary>
	public class ValueNoise2D : Generator
	{
		private readonly CoherentNoise.LatticeNoise m_Source;
		private readonly SCurve m_SCurve;

		/// <summary>
		/// Create new generator with specified seed
		/// </summary>
		/// <param name="seed">settings seed</param>
		public ValueNoise2D(int seed)
			: this(seed, null)
		{

		}

		/// <summary>
		/// Create new generator with specified seed and interpolation algorithm. Different interpolation algorithms can make settings smoother at the expense of speed.
		/// </summary>
		/// <param name="seed">settings seed</param>
		/// <param name="sCurve">Interpolator to use. Can be null, in which case default will be used</param>
		public ValueNoise2D(int seed, SCurve sCurve)
		{
			m_Source = new CoherentNoise.LatticeNoise(seed);
			m_SCurve = sCurve;
		}

		private SCurve SCurve { get { return m_SCurve ?? SCurve.Default; } }

		/// <summary>
		/// Noise period. Used for repeating (seamless) settings.
		/// When Period &gt;0 resulting settings pattern repeats exactly every Period, for all coordinates.
		/// </summary>
		public int Period { get { return m_Source.Period; } set { m_Source.Period = value; } }

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

			// interpolate the coordinates instead of values - it's way faster
			float xs = SCurve.Interpolate(x - ix);
			float ys = SCurve.Interpolate(y - iy);

			// THEN we can use linear interp to find our value - biliear actually

			float n0 = m_Source.GetValue(ix, iy, 0);
			float n1 = m_Source.GetValue(ix + 1, iy, 0);
			float ix0 = Mathf.Lerp(n0, n1, xs);

			n0 = m_Source.GetValue(ix, iy + 1, 0);
			n1 = m_Source.GetValue(ix + 1, iy + 1, 0);
			float ix1 = Mathf.Lerp(n0, n1, xs);

			return Mathf.Lerp(ix0, ix1, ys);
		}


		#endregion
	}
}