using System;

namespace CoherentNoise.Interpolation
{
	internal class QuinticSCurve: SCurve
	{
		#region Overrides of Interpolator

		public override float Interpolate(float t)
		{
			var t3 = t * t * t;
			var t4 = t3 * t;
			var t5 = t4 * t;
			return 6 * t5 - 15 * t4 + 10 * t3;
		}

		#endregion
	}
}