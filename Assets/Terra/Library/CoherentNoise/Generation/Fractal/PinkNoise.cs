using System;

namespace Terra.CoherentNoise.Generation.Fractal
{
	/// <summary>
	/// Pink settings is a fractal settings that adds together weighted signals sampled at different frequencies, with weight inversely proportional to frequency. .
	/// When source settings is <see cref="GradientNoise"/>, this becomes Perlin settings.
	/// </summary>
	public class PinkNoise:FractalNoiseBase
	{
		private float m_CurPersistence;

		///<summary>
		/// Create new pink settings generator using seed. Seed is used to create a <see cref="GradientNoise"/> source. 
		///</summary>
		///<param name="seed">seed value</param>
		public PinkNoise(int seed) : base(seed)
		{
			Persistence = 0.5f;
		}

		///<summary>
		/// Create new pink settings generator with user-supplied source. Usually one would use this with <see cref="ValueNoise"/> or gradient settings with less dimensions, but 
		/// some weird effects may be achieved with other generators.
		///</summary>
		///<param name="source">settings source</param>
		public PinkNoise(Generator source) : base(source)
		{
			Persistence = 0.5f;
		}

		#region Overrides of FractalNoiseBase

		/// <summary>
		/// Returns new resulting settings value after source settings is sampled. Perlin generator adds signal, multiplied by current persistence value. Persistence value
		/// is then decreased, so that higher frequencies will have less impact on resulting value.
		/// </summary>
		/// <param name="curOctave">Octave at which source is sampled (this always starts with 0)</param>
		/// <param name="signal">Sampled value</param>
		/// <param name="value">Resulting value from previous step</param>
		/// <returns>Resulting value adjusted for this sample</returns>
		protected override float CombineOctave(int curOctave, float signal, float value)
		{
			if (curOctave == 0)
				m_CurPersistence = 1;
			value = value + signal*m_CurPersistence;
			m_CurPersistence *= Persistence;
			return value;
		}

		#endregion

		/// <summary>
		/// Persistence value determines how fast signal diminishes with frequency. i-th octave sugnal will be multiplied by presistence to the i-th power.
		/// Note that persistence values >1 are possible, but will not produce interesting settings (lower frequencies will just drown out)
		/// 
		/// Default value is 0.5
		/// </summary>
		public float Persistence { get; set; }
	}
}