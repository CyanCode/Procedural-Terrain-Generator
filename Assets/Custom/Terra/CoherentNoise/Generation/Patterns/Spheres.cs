using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Patterns
{
	///<summary>
	/// Generates concentric spheres centered in (0,0,0). Resulting "settings" has value -1 in the center, 1 at radius, -1 at 2*radius etc. 
	///</summary>
	public class Spheres: Function
	{
		///<summary>
		/// Create new spheres pattern
		///</summary>
		///<param name="radius">radius</param>
		///<exception cref="ArgumentException">When radius &lt;=0 </exception>
		public Spheres(float radius) 
			: base((x,y,z)=>
			       	{
			       		var d = new Vector3(x, z, y).magnitude;
			       		return Helpers.Saw(d/radius);
			       	})
		{
			if (radius <= 0)
				throw new ArgumentException("Radius must be > 0");
		}
	}
}