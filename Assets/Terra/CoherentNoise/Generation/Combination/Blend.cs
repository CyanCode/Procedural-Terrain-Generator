using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Combination
{
	/// <summary>
	/// This generator blends two noises together, using third as a blend weight. Note that blend weight's value is clamped to [0,1] range
	/// </summary>
	public class Blend : Generator
	{
		private readonly Generator m_A;
		private readonly Generator m_B;
		private readonly Generator m_Weight;

		///<summary>
		/// Create new blend generator
		///</summary>
		///<param name="a">First generator to blend (this is returned if weight==0)</param>
		///<param name="b">Second generator to blend (this is returned if weight==1)</param>
		///<param name="weight">Blend weight source</param>
		public Blend(Generator a, Generator b, Generator weight)
		{
			m_A = a;
			m_Weight = weight;
			m_B = b;
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
			var w = Mathf.Clamp01(m_Weight.GetValue(x, y, z));
			return m_A.GetValue(x, y, z) * (1 - w) + m_B.GetValue(x, y, z) * w;
		}

		#endregion
	}
}