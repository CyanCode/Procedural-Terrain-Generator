using System;

namespace Terra.CoherentNoise.Generation
{
	/// <summary>
	/// This generator creates "settings" that is actually a function of coordinates. Use it to create regular patterns that are then perturbed by settings
	/// </summary>
	public class Function: Generator
	{
		private readonly Func<float, float, float, float> m_Func;

		/// <summary>
		/// Create new function generator
		/// </summary>
		/// <param name="func">Value function</param>
		public Function(Func<float, float, float, float> func)
		{
			m_Func = func;
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
			return m_Func(x, y, z);
		}

		#endregion
	}
}