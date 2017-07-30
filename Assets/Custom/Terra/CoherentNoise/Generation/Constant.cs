namespace Terra.CoherentNoise.Generation
{
	///<summary>
	/// Constant "settings". This generator returns constant value, ignoring input coordinates. Used for arithmetic operations on settings generators
	///</summary>
	public class Constant: Generator
	{
		private readonly float m_Value;

		///<summary>
		/// Create new constant generator
		///</summary>
		///<param name="value">Value returned by generator</param>
		public Constant(float value)
		{
			m_Value = value;
		}

		#region Implementation of Noise

		/// <summary>
		/// Returns settings value at given point. 
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <returns>Noise value</returns>
		public override float GetValue(float x, float y, float z)
		{
			return m_Value;
		}

		#endregion
	}
}