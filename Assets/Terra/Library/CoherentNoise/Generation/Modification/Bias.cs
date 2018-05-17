using System;

namespace Terra.CoherentNoise.Generation.Modification
{
	/// <summary>
	/// Bias generator is used to "shift" mean value of source settings. Source is assumed to have values between -1 and 1; after Bias is applied,
	/// the result is still between -1 and 1, but the points that were equal to 0 are shifted by <i>bias value</i>.
	/// </summary>
	public class Bias: Generator
	{
		private readonly float m_Bias;
		private readonly Generator m_Source;

		///<summary>
		/// Create new generator
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="bias">Bias value</param>
		public Bias(Generator source, float bias)
		{
			if (m_Bias <= -1 || m_Bias >= 1)
				throw new ArgumentException("Bias must be between -1 and 1");

			m_Source = source;
			m_Bias = bias/(1f+bias);
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
			var f = m_Source.GetValue(x, y, z);
            // clamp f to [-1,1] so that we don't ever get a division by 0 error
            if (f < -1)
                f = -1;
            if (f > 1)
                f = 1;
			return (f + 1.0f)/(1.0f - m_Bias*(1.0f - f)*0.5f) - 1.0f;
		}

		#endregion
	}
}