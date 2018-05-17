using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Patterns
{
	///<summary>
	/// This generator does the opposite of texture generation. It takes a texture and returns its red channel as a settings value.
	/// Use it to incorporate hand-created patterns in your generation.
	///</summary>
	public class TexturePattern : Generator
	{
		private readonly Color[] m_Colors;
		private readonly int m_Width;
		private readonly int m_Height;
		private readonly TextureWrapMode m_WrapMode;

		///<summary>
		/// Create new texture generator
		///</summary>
		///<param name="texture">Texture to use. It must be readable. The texture is read in constructor, so any later changes to it will not affect this generator</param>
		///<param name="wrapMode">Wrapping mode</param>
		public TexturePattern(Texture2D texture, TextureWrapMode wrapMode)
		{
			m_Colors = texture.GetPixels();
			m_Width = texture.width;
			m_Height = texture.height;

			m_WrapMode = wrapMode;
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
			int ix = Mathf.FloorToInt(x * m_Width);
			int iy = Mathf.FloorToInt(y * m_Height);
			ix = Wrap(ix, m_Width);
			iy = Wrap(iy, m_Height);
			var c = m_Colors[iy*m_Width + ix];
			return c.r*2 - 1;
		}

		private int Wrap(int i, int size)
		{
			switch (m_WrapMode)
			{
				case TextureWrapMode.Repeat:
					return i >= 0 ? i%size : (i%size+size);
				case TextureWrapMode.Clamp:
					return i < 0 ? 0 : i > size ? size - 1 : i;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion
	}
}