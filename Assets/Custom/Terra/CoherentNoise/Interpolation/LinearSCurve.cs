using System;

namespace Terra.CoherentNoise.Interpolation
{
	///<summary>
	/// Linear interpolator is the fastest and has the lowest quality, only ensuring continuity of settings values, not their derivatives.
	///</summary>
	internal class LinearSCurve : SCurve
	{
		#region Overrides of Interpolator

		public override float Interpolate(float t)
		{
			return t;
		}

		#endregion
	}
}