namespace Terra.CoherentNoise.Generation.Voronoi
{
	/// <summary>
	/// This generator creates a "valleys" Voronoi diargam, that returns difference between two closest distances. Resulting settings has highest value at control points and 0 away from control points.
	/// </summary>
	public class VoronoiValleys:VoronoiDiagramBase
	{
		/// <summary>
		/// Create new Voronoi diagram using seed. Control points will be obtained using random <see cref="GradientNoise"/> displacment seeded by supplied value
		/// </summary>
		/// <param name="seed">Seed value</param>
		public VoronoiValleys(int seed) : base(seed)
		{
		}

		#region Overrides of VoronoiDiagramBase

		/// <summary>
		/// Override this method to calculate final value using distances to closest control points.
		/// Note that distances that get passed to this function are adjusted for frequency (i.e. distances are scaled so that 
		/// control points are in unit sized cubes)
		/// </summary>
		/// <param name="min1">Distance to closest point</param>
		/// <param name="min2">Distance to second-closest point</param>
		/// <param name="min3">Distance to third-closest point</param>
		/// <returns></returns>
		protected override float GetResult(float min1, float min2, float min3)
		{
			return min2-min1;
		}

		#endregion
	}
}