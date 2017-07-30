using System;

namespace Terra.CoherentNoise.Generation.Modification
{
	/// <summary>
	/// This generator binarizes its source settings, returning only value 0 and 1. A constant treshold value is user for binarization. I.e. result will be 0 where source value is less than treshold,
	/// and 1 elsewhere.
	/// </summary>
	public class Binarize:Generator
	{
		private readonly Generator m_Source;
		private readonly float m_Treshold;

		///<summary>
		/// Create new binarize generator
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="treshold">Treshold value</param>
		public Binarize(Generator source, float treshold)
		{
			m_Source = source;
			m_Treshold = treshold;
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
			return m_Source.GetValue(x, y, z) > m_Treshold ? 1 : 0;
		}

		#endregion
	}
}