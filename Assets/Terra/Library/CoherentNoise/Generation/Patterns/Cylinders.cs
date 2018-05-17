using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Patterns
{
	///<summary>
	/// Generates concentric cylinders centered in (0,0,0) and parallel to Z axis. Resulting "settings" has value -1 in the center, 1 at radius, -1 at 2*radius etc. 
	///</summary>
	public class Cylinders : Function
	{
		///<summary>
		/// Create new cylinders pattern
		///</summary>
		///<param name="radius">radius</param>
		///<exception cref="ArgumentException">When radius &lt;=0 </exception>
		public Cylinders(float radius)
			: base((x, y, z) =>
			{
				var d = new Vector2(x, y).magnitude;
				return Helpers.Saw(d / radius);
			})
		{
			if (radius <= 0)
				throw new ArgumentException("Radius must be > 0");
		}
	}
}