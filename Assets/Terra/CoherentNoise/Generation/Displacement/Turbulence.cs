using Terra.CoherentNoise.Generation.Fractal;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Displacement
{
	///<summary>
	/// Turbulence is a case of Perturb generator, that uses 3 Perlin settings generators as displacement source.
	///</summary>
	public class Turbulence : Generator
	{
		private readonly int m_Seed;
		private readonly Generator m_Source;
        private Generator m_DisplacementX;
        private Generator m_DisplacementY;
        private Generator m_DisplacementZ;
        private float m_Frequency;
		private int m_OctaveCount;

		///<summary>
		/// Create new perturb generator
		///</summary>
		///<param name="source">Source generator</param>
		///<param name="seed">Seed value for perturbation settings</param>
		public Turbulence(Generator source, int seed)
		{
			m_Source = source;
			m_Seed = seed;
			Power = 1;
			Frequency = 1;
			OctaveCount = 6;
		}

		///<summary>
		/// Turbulence power, in other words, amount by which source will be perturbed.
		/// 
		/// Default value is 1.
		///</summary>
		public float Power { get; set; }

		///<summary>
		/// Frequency of perturbation settings. 
		/// 
		/// Default value is 1.
		///</summary>
		public float Frequency
		{
			get { return m_Frequency; }
			set
			{
				m_Frequency = value;
				CreateDisplacementSource();
			}
		}

		/// <summary>
		/// Octave count of perturbation settings
		/// 
		/// Default value is 6
		/// </summary>
		public int OctaveCount
		{
			get { return m_OctaveCount; }
			set
			{
				m_OctaveCount = value;
				CreateDisplacementSource();
			}
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
			Vector3 displacement = new Vector3(m_DisplacementX.GetValue(x, y, z),m_DisplacementY.GetValue(x,y,z),m_DisplacementZ.GetValue(x,y,z))*Power;
			return m_Source.GetValue(x + displacement.x, y + displacement.y, z + displacement.z);
		}

		#endregion

		private void CreateDisplacementSource()
		{
		    m_DisplacementX = new PinkNoise(m_Seed) {Frequency = Frequency, OctaveCount = OctaveCount};
		    m_DisplacementY = new PinkNoise(m_Seed + 1) {Frequency = Frequency, OctaveCount = OctaveCount};
		    m_DisplacementZ = new PinkNoise(m_Seed + 2) {Frequency = Frequency, OctaveCount = OctaveCount};
		}
	}
}