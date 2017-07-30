using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Displacement
{
	/// <summary>
	/// This generator perturbs its source, using a user-supplied function to obtain displacement values. In other words, <see cref="Perturb"/> nonuniformly displaces each value of
	/// its source.
	/// </summary>
	public class Perturb: Generator
	{
		private readonly Generator m_Source;
        private readonly Func<Vector3, Vector3> m_DisplacementSource;

		///<summary>
		/// Create new perturb generator
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="displacementSource">Displacement generator</param>
        public Perturb(Generator source, Func<Vector3, Vector3> displacementSource)
		{
			m_Source = source;
			m_DisplacementSource = displacementSource;
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
			Vector3 displacement = m_DisplacementSource(new Vector3(x, y, z));
			return m_Source.GetValue(x + displacement.x, y + displacement.y, z + displacement.z);
		}

		#endregion
	}
}