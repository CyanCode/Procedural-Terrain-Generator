namespace Terra.CoherentNoise.Generation.Combination
{
	/// <summary>
	/// Generator that adds two settings values together
	/// </summary>
	public class Add: Generator
	{
		private readonly Generator m_A;
		private readonly Generator m_B;

		///<summary>
		/// Create new generator
		///</summary>
		///<param name="a">First generator to add</param>
		///<param name="b">Second generator to add</param>
		public Add(Generator a, Generator b)
		{
			m_A = a;
			m_B = b;
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
			return m_A.GetValue(x, y, z) + m_B.GetValue(x, y, z);
		}

		#endregion
	}
}