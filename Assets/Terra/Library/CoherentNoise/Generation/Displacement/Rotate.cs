using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Displacement
{
	/// <summary>
	/// This generator rotates its source around origin.
	/// </summary>
	public class Rotate: Generator
	{
		private Generator m_Source;
		private Quaternion m_Rotation;

		///<summary>
		/// Create new rotation using a quaternion
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="rotation">Rotation</param>
		public Rotate(Generator source, Quaternion rotation)
		{
			m_Source = source;
			m_Rotation = rotation;
		}

		///<summary>
		/// Create new rotation using Euler angles
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="angleX">Rotation around X axis</param>
		///<param name="angleY">Rotation around Y axis</param>
		///<param name="angleZ">Rotation around Z axis</param>
		public Rotate(Generator source, float angleX, float angleY, float angleZ):this(source, Quaternion.Euler(angleX,angleY,angleZ))
		{
			
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
			Vector3 v = m_Rotation*new Vector3(x,y,z);
			return m_Source.GetValue(v);
		}

		#endregion
	}
}