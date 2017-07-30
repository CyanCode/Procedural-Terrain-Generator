using UnityEngine;

namespace CoherentNoise.Generation.Patterns
{
	internal class Helpers
	{
		/// <summary>
		/// Saw function that is equal to 1 in odd points and -1 at even points
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static float Saw(float x)
		{
			var i = Mathf.FloorToInt(x);
			if (i % 2 == 0)
			{
				return 2 * (x - i) - 1;
			}
			else
			{
				return 2 * (1 - (x - i)) - 1;
			}
		}
	}
}