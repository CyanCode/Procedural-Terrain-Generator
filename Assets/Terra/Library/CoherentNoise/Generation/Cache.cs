
namespace Terra.CoherentNoise.Generation
{
	/// <summary>
	/// This generator returns its source unchanged. However, it caches last returned value, and does not recalculate it if called several times for the same point.
	/// This is handy if you use same settings generator in different places.
	/// 
	/// Note that displacement, fractal and Voronoi generators call GetValue at different points for their respective source generators.  
	/// This wil trash the Cache and negate any performance benefit, so there's no point in using Cache with these generators.
	/// </summary>
	public class Cache: Generator
	{
		private float m_X;
		private float m_Y;
		private float m_Z;
		private float m_Cached;
		private readonly Generator m_Source;

		///<summary>
		///Create new caching generator
		///</summary>
		///<param name="source">Source generator</param>
		public Cache(Generator source)
		{
			m_Source = source;
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
			if (x == m_X && y == m_Y && z == m_Z)
			{
				return m_Cached;
			}
			else
			{
				m_X = x;
				m_Y = y;
				m_Z = z;
				return m_Cached = m_Source.GetValue(x, y, z);
			}
		}

		#endregion
	}
}