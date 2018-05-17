using System;

namespace Terra.CoherentNoise.Generation.Modification
{
    /// <summary>
    /// This generator is used to "sharpen" settings, shifting extreme values closer to -1 and 1, while leaving 0 in place. Source settings is
    /// clamped to [-1,1], as values outside of this range may result in division by 0. Resulting settings is between -1 and 1, with values that
    /// were equal to 0.5 shifted to 0.5+gain/2, and those that were equal to -0.5 shifted to -0.5-gain/2.
    /// </summary>
    public class Gain: Generator
	{
		private readonly float m_Gain;
		private readonly Generator m_Source;

		///<summary>
		/// Create new generator
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="gain">Gain value</param>
		public Gain(Generator source, float gain)
		{
			if (m_Gain <= -1 || m_Gain >= 1)
				throw new ArgumentException("Gain must be between -1 and 1");

			m_Source = source;
			m_Gain = gain;
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
            if (f >= 0)
                return BiasFunc(f);
            else
                return -BiasFunc(-f);
		}

		#endregion

        private float BiasFunc(float f)
        {
            // clamp f to [0,1] so that we don't ever get a division by 0 error
            if (f < 0)
                f = 0;
            if (f > 1)
                f = 1;
            // Bias curve that makes a "half" of gain
            return f * (1.0f + m_Gain) / (1.0f + m_Gain - (1.0f - f) * 2.0f * m_Gain);
        }
	}
}