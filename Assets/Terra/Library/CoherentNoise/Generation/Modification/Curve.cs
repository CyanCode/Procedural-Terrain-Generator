using UnityEngine;

namespace Terra.CoherentNoise.Generation.Modification
{
	///<summary>
	/// This generator modifies source settings by applying a curve transorm to it. Curves can be edited using Unity editor's CurveFields, or created procedurally.
	///</summary>
	public class Curve : Generator
	{
		private Generator m_Source;
		private AnimationCurve m_Curve;

		///<summary>
		/// Create a new curve generator
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="curve">Curve to use</param>
		public Curve(Generator source, AnimationCurve curve)
		{
			m_Source = source;
			m_Curve = curve;
		}

		#region Overrides of NoiseGen

		/// <summary>
		///  Returns settings value at given point. 
		///  </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param><returns>Noise value</returns>
		public override float GetValue(float x, float y, float z)
		{
			float v = m_Source.GetValue(x, y, z);
			return m_Curve.Evaluate(v);
		}

		#endregion
	}
}