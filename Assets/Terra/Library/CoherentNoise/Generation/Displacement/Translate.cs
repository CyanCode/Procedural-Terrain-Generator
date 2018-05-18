using UnityEngine;

namespace Terra.CoherentNoise.Generation.Displacement
{
	///<summary>
	/// This generator translates its source by given vector.
	///</summary>
	public class Translate:Generator
	{
		private readonly Generator m_Source;
		private readonly float m_X;
		private readonly float m_Y;
		private readonly float m_Z;

		///<summary>
		/// Create new translation
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="v">Translate value</param>
		public Translate(Generator source, Vector3 v):this(source,v.x,v.y,v.z)
		{
			
		}

		///<summary>
		/// Create new translation
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="x">Translate amount along X axis</param>
		///<param name="y">Translate amount along Y axis</param>
		///<param name="z">Translate amount along Z axis</param>
		public Translate(Generator source, float x, float y, float z)
		{
			m_Source = source;
			m_Z = z;
			m_Y = y;
			m_X = x;
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
			return m_Source.GetValue(x + m_X, y + m_Y, z + m_Z);
		}

		#endregion
	}
}