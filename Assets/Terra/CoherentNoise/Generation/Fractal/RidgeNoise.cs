using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Fractal
{
	/// <summary>
	/// This generator adds samples with weight decreasing with frequency, like Perlin settings; however, each signal is taken as absolute value, and weighted by previous (i.e. lower-frequency) signal,
	/// creating a sort of feedback loop. Resulting settings has sharp ridges, somewhat resembling cliffs. This is useful for terrain generation.
	/// </summary>
	public class RidgeNoise : FractalNoiseBase
	{
		private float m_Exponent;
		private float[] m_SpectralWeights;
		private float m_Weight;

		///<summary>
		/// Create new ridge generator using seed (seed is used to create a <see cref="GradientNoise"/> source)
		///</summary>
		///<param name="seed">seed value</param>
		public RidgeNoise(int seed)
			: base(seed)
		{
			Offset = 1;
			Gain = 2;
			Exponent = 1;
		}

		///<summary>
		/// Create new ridge generator with user-supplied source. Usually one would use this with <see cref="ValueNoise"/> or gradient settings with less dimensions, but 
		/// some weird effects may be achieved with other generators.
		///</summary>
		///<param name="source">settings source</param>
		public RidgeNoise(Generator source)
			: base(source)
		{
			Offset = 1;
			Gain = 2;
			Exponent = 1;
		}

		/// <summary>
		/// Exponent defines how fast weights decrease with frequency. The higher the exponent, the less weight is given to high frequencies. 
		/// Default value is 1
		/// </summary>
		public float Exponent
		{
			get { return m_Exponent; }
			set
			{
				m_Exponent = value;
				OnParamsChanged();
			}
		}

		/// <summary>
		/// Offset is applied to signal at every step. Default value is 1
		/// </summary>
		public float Offset { get; set; }

		/// <summary>
		/// Gain is the weight factor for previous-step signal. Higher gain means more feedback and noisier ridges. 
		/// Default value is 2.
		/// </summary>
		public float Gain { get; set; }

		#region Overrides of FractalNoiseBase

		/// <summary>
		/// Returns new resulting settings value after source settings is sampled. 
		/// </summary>
		/// <param name="curOctave">Octave at which source is sampled (this always starts with 0</param>
		/// <param name="signal">Sampled value</param>
		/// <param name="value">Resulting value from previous step</param>
		/// <returns>Resulting value adjusted for this sample</returns>
		protected override float CombineOctave(int curOctave, float signal, float value)
		{
			if (curOctave == 0)
				m_Weight = 1;
			// Make the ridges.
			signal = Offset - Mathf.Abs(signal);

			// Square the signal to increase the sharpness of the ridges.
			signal *= signal;

			// The weighting from the previous octave is applied to the signal.
			// Larger values have higher weights, producing sharp points along the
			// ridges.
			signal *= m_Weight;

			// Weight successive contributions by the previous signal.
			m_Weight = signal * Gain;
			if (m_Weight > 1)
			{
				m_Weight = 1;
			}
			if (m_Weight < 0)
			{
				m_Weight = 0;
			}

			// Add the signal to the output value.
			return value + (signal * m_SpectralWeights[curOctave]);
		}

		/// <summary>
		/// This method is called whenever any generator's parameter is changed (i.e. Lacunarity, Frequency or OctaveCount). Override it to precalculate any values used in generation.
		/// </summary>
		protected override void OnParamsChanged()
		{
			PrecalculateWeights();
		}

		#endregion

		private void PrecalculateWeights()
		{
			float frequency = 1;
			m_SpectralWeights = new float[OctaveCount];
			for (int ii = 0; ii < OctaveCount; ii++)
			{
				// Compute weight for each frequency.
				m_SpectralWeights[ii] = Mathf.Pow(frequency, -Exponent);
				frequency *= Lacunarity;
			}
		}

	}
}