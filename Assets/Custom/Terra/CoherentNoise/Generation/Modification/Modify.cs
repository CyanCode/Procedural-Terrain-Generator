using System;

namespace Terra.CoherentNoise.Generation.Modification
{
	/// <summary>
	/// This generator takes a source generator and applies a function to its output.
	/// </summary>
	public class Modify: Generator
	{
		private Func<float, float> m_Modifier;
		private Generator m_Source;

		///<summary>
		/// Create new generator
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="modifier">Modifier function to apply</param>
		public Modify(Generator source, Func<float, float> modifier)
		{
			m_Source = source;
			m_Modifier = modifier;
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
			return m_Modifier(m_Source.GetValue(x, y, z));
		}

		#endregion
	}
}