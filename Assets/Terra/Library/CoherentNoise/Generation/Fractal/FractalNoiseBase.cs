using UnityEngine;

namespace Terra.CoherentNoise.Generation.Fractal
{
	///<summary>
	/// base class for fractal settings generators. Fractal generators use a source settings, that is sampled at several frequencies. 
	/// These sampled values are then combined into a result using some algorithm. 
	///</summary>
	public abstract class FractalNoiseBase : Generator
	{
		private static readonly Quaternion s_Rotation = Quaternion.Euler(30, 30, 30);

		private readonly Generator m_Noise;
		private float m_Frequency;
		private float m_Lacunarity;
		private int m_OctaveCount;

		/// <summary>
		/// Creates a new fractal settings using default source: gradient settings seeded by supplied seed value
		/// </summary>
		/// <param name="seed">seed value</param>
		protected FractalNoiseBase(int seed)
		{
			m_Noise = new GradientNoise(seed);
			Lacunarity = 2.17f;
			OctaveCount = 6;
			Frequency = 1;
		}

		/// <summary>
		/// Creates a new fractal settings, supplying your own source generator
		/// </summary>
		/// <param name="source">source settings</param>
		protected FractalNoiseBase(Generator source)
		{
			m_Noise = source;
			Lacunarity = 2.17f;
			OctaveCount = 6;
			Frequency = 1;
		}

		///<summary>
		/// Frequency coefficient. Sampling frequency is multiplied by lacunarity value with each octave.
		/// Default value is 2, so that every octave doubles sampling frequency
		///</summary>
		public float Lacunarity
		{
			get { return m_Lacunarity; }
			set
			{
				m_Lacunarity = value;
				OnParamsChanged();
			}
		}

		/// <summary>
		/// Number of octaves to sample. Default is 6.
		/// </summary>
		public int OctaveCount
		{
			get { return m_OctaveCount; }
			set
			{
				m_OctaveCount = value;
				OnParamsChanged();
			}
		}

		/// <summary>
		/// Initial frequency.
		/// </summary>
		public float Frequency
		{
			get { return m_Frequency; }
			set
			{
				m_Frequency = value;
				OnParamsChanged();
			}
		}

		/// <summary>
		///  Returns settings value at given point. 
		///  </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param><returns>Noise value</returns>
		public override float GetValue(float x, float y, float z)
		{
			float value = 0;
			float signal = 0;

			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			for (int curOctave = 0; curOctave < OctaveCount; curOctave++)
			{
				// Get the coherent-settings value from the input value and add it to the
				// final result.
				signal = m_Noise.GetValue(x, y, z);
				// дефолтный перлин - складывает все значения с уменьшающимся весом
				value = CombineOctave(curOctave, signal, value);

				// Prepare the next octave.
				// scale coords to increase frequency, then rotate to break up lattice pattern
				var rotated = s_Rotation*(new Vector3(x, y, z) * Lacunarity);
				x = rotated.x;
				y = rotated.y;
				z = rotated.z;
			}

			return value;
		}

		/// <summary>
		/// Returns new resulting settings value after source settings is sampled. 
		/// </summary>
		/// <param name="curOctave">Octave at which source is sampled (this always starts with 0</param>
		/// <param name="signal">Sampled value</param>
		/// <param name="value">Resulting value from previous step</param>
		/// <returns>Resulting value adjusted for this sample</returns>
		protected abstract float CombineOctave(int curOctave, float signal, float value);

		/// <summary>
		/// This method is called whenever any generator's parameter is changed (i.e. Lacunarity, Frequency or OctaveCount). Override it to precalculate any values used in generation.
		/// </summary>
		protected virtual void OnParamsChanged()
		{
		}
	}
}