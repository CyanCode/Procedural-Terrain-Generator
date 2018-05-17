namespace Terra.CoherentNoise.Interpolation
{
	/// <summary>
	/// Base class for all S-curves. S-curves determine the interpolation algorithm. Using different curves, quality-speed balance may be tweaked,
	/// as better algorithms tend to be slower.
	/// </summary>
	public abstract class SCurve
	{
		/// <summary>
		/// Maps a value between 0 and 1 to some S-shaped curve.
		/// Interpolated value equals to 0 when <paramref name="t"/>==0 and to 1 when <paramref name="t"/>==1
		/// Values outside of [0,1] range are illegal
		/// 
		/// Good interpolation also has derivatives of result equal to 0 when <paramref name="t"/> is 0 or 1 (the higher order derivatives are zeroed, the better).
		/// </summary>
		/// <param name="t">Interpolation value (0 to 1)</param>
		/// <returns>Mapped value</returns>
		public abstract float Interpolate(float t);

		///<summary>
		/// Linear interpolator is the fastest and has the lowest quality, only ensuring continuity of settings values, not their derivatives.
		///</summary>
		public static readonly SCurve Linear = new LinearSCurve();
		///<summary>
		/// Cubic interpolation is a good compromise between speed and quality. It's slower than linear, but ensures continuity of 1-st order derivatives, making settings smooth.
		///</summary>
		public static readonly SCurve Cubic = new CubicSCurve();
		///<summary>
		/// Quintic interpolation is the most smooth, guarateeing continuinty of second-order derivatives. it is slow, however.
		///</summary>
		public static readonly SCurve Quintic = new QuinticSCurve();
		///<summary>
		/// Cosine interpolation uses cosine function instead of power curve, resulting in somewhat smoother settings than cubic interpolation, but still only achieving first-order continuity.
		/// Depending on target machine, it may be faster than quintic interpolation.
		///</summary>
		public static readonly SCurve Cosine = new CosineSCurve();

		///<summary>
		/// Default interpolator. Noise generators will use this one if you don't supply concrete interlpolator in the constructor. 
		///</summary>
		public static SCurve Default = Cubic;


	}
}