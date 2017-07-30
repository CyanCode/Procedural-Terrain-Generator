using System;

namespace CoherentNoise.Interpolation
{
	internal class CosineSCurve: SCurve
	{
		#region Overrides of Interpolator

		public override float Interpolate(float t)
		{
			return (float)( (1 - Math.Cos(t * 3.1415927)) * .5);
		}

		#endregion
	}
}