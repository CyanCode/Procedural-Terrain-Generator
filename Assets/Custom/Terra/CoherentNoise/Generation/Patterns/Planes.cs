using System;
using UnityEngine;

namespace Terra.CoherentNoise.Generation.Patterns
{
	///<summary>
	/// Generates planes parallel to YZ plane. Resulting "settings" has value -1 on YZ plane, 1 at step distance, -1 at 2*step etc. 
	///</summary>
	public class Planes : Function
	{
		///<summary>
		/// Create new planes pattern
		///</summary>
		///<param name="step">step</param>
		///<exception cref="ArgumentException">When step &lt;=0 </exception>
		public Planes(float step)
			: base((x, y, z) => Helpers.Saw(x / step))
		{
			if (step <= 0)
				throw new ArgumentException("Step must be > 0");
		}
	}
}