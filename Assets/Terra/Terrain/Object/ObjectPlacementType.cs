using UnityEngine;

[System.Serializable]
public class ObjectPlacementType {
	private System.Random Rand;

	#region GeneralParams
	/// <summary>
	/// Extents for a random transformation.
	/// </summary>
	[System.Serializable]
	public struct RandomTransformExtent {
		/// <summary>
		/// The defined maximum transformation distance.
		/// </summary>
		public Vector3 Max;

		/// <summary>
		/// The defined minimum transformation distance.
		/// </summary>
		public Vector3 Min;
	}

	/// <summary>
	/// Prefab to procedurally place in the scene
	/// </summary>
	public GameObject Prefab;

	/// <summary>
	/// Max amount of objects of this type to place 
	/// in the scene
	/// </summary>
	public int MaxObjects = 500;

	/// <summary>
	/// Are other objects allowed to intersect with this?
	/// </summary>
	public bool AllowsIntersection = false;

	/// <summary>
	/// How dense should the objects be placed next to 
	/// each other.
	/// </summary>
	public float Density = 1f;

	/// <summary>
	/// How big should the "grid" be that is used to poll 
	/// the Poisson disc sampler?
	/// <note type="note">Setting this value too high can have
	/// unintended consequences. It is suggested that this value 
	/// does not exceed 15 unless you are sure you want more samples.</note>
	/// </summary>
	public int GridSize = 10;
	#endregion

	#region PlacementParams
	[System.Serializable]
	public enum EvaluateOptions {
		/// <summary>
		/// If the height evaluation passes, the object can 
		/// be placed.
		/// </summary>
		HeightFirst,

		/// <summary>
		/// If the angle evaluation passes, the object can
		/// be placed.
		/// </summary>
		AngleFirst,

		/// <summary>
		/// If both the angle evaluation and height evaluation 
		/// pass, the object can be placed
		/// </summary>
		Both
	}

	/// <summary>
	/// Maximum height to evaluate to when placing objects.
	/// </summary>
	public float MaxHeight;

	/// <summary>
	/// Minimum height to evaluate when placing objects.
	/// </summary>
	public float MinHeight;

	/// <summary>
	/// Optionally constrain at which height the objects will 
	/// show up
	/// </summary>
	public bool ConstrainHeight;

	/// <summary>
	/// Height curve to evaluate when procedurally placing 
	/// objects. Top of curve = 100% chance of placing the object,
	/// bottom of curve = 0% chance of placing the object.
	/// </summary>
	public AnimationCurve HeightProbCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	/// <summary>
	/// Maximum angle to evaluate to when placing objects. 
	/// Cannot exceed at 180 degrees.
	/// </summary>
	public float MaxAngle;

	/// <summary>
	/// Minimum angle to evaluate when placing objects.
	/// Cannot drop below 0 degrees
	/// </summary>
	public float MinAngle;

	/// <summary>
	/// Optionally constrain at which angle the objects will 
	/// show up.
	/// </summary>
	public bool ConstrainAngle;

	/// <summary>
	/// Angle curve to evaluate when procedurally placing 
	/// objects. Top of curve = 100% chance of placing the object,
	/// bottom of curve = 0% chance of placing the object.
	/// Evaluates from 0 - 180 degrees.
	/// </summary>
	public AnimationCurve AngleProbCurve = AnimationCurve.Linear(0f, 1f, 180f, 1f);

	/// <summary>
	/// Specifies how the object will be evaluated.
	/// </summary>
	public EvaluateOptions EvaluateOption = EvaluateOptions.Both;
	#endregion

	#region RotationParams
	/// <summary>
	/// Is the object randomly rotated?
	/// </summary>
	public bool IsRandomRotation;

	/// <summary>
	/// If the object is not randomly rotated, should it
	/// be rotated by a specified amount?
	/// If it is randomly rotated, what is the maximum amount 
	/// that it should be allowed to rotate?
	/// </summary>
	public Vector3 RotationAmount = Vector3.zero;

	/// <summary>
	/// The maximum and minimum amounts that this object 
	/// can randomly rotate in euler values.
	/// </summary>
	public RandomTransformExtent RandomRotationExtents;
	#endregion

	#region TranslateParams
	/// <summary>
	/// Is the object randomly translated?
	/// </summary>
	public bool IsRandomTranslate;

	/// <summary>
	/// If the object is not randomly translated, 
	/// should it be translated by a specified amount?
	/// If it is randomly translated, what is the maximum amount 
	/// that it should be allowed to transate?
	/// </summary>
	public Vector3 TranslationAmount = Vector3.zero;

	/// <summary>
	/// The maximum and minimum amounts that this object 
	/// can randomly translate.
	/// </summary>
	public RandomTransformExtent RandomTranslateExtents;
	#endregion

	#region ScaleParams
	/// <summary>
	/// Is the object randomly scaled?
	/// </summary>
	public bool IsRandomScale;

	/// <summary>
	/// If the object is not randomly scaled, 
	/// should it be scaled by a specified amount?
	/// If it is randomly translated, what is the maximum amount 
	/// that it should be allowed to transate?
	/// </summary>
	public Vector3 ScaleAmount = Vector3.zero;

	/// <summary>
	/// The maximum and minimum amounts that this object 
	/// can randomly scale.
	/// </summary>
	public RandomTransformExtent RandomScaleExtents;
	#endregion

	/// <param name="seed">Optional seed value to use in the random number generator</param>
	public ObjectPlacementType(int seed = -1) {
		if (seed == -1)
			Rand = new System.Random();
		else
			Rand = new System.Random(seed);
	}

	/// <summary>
	/// Decides whether or not this object should be placed at
	/// the specified height and angle. Does not check for intersections.
	/// </summary>
	/// <param name="height">Height to evaluate at</param>
	/// <param name="angle">Angle to evaluate at (0 to 180 degrees)</param>
	/// <returns></returns>
	public bool ShouldPlaceAt(float height, float angle) {


		return false; //TODO implement
	}

	/// <summary>
	/// Whether or not the passed height fits within the 
	/// specified max and min.
	/// </summary>
	/// <param name="height">height to check</param>
	public bool IsInHeightExtents(float height) {
		return height < MaxHeight && height > MinHeight;
	}

	/// <summary>
	/// Whether or not the passed angle fits within the 
	/// specified max and min.
	/// </summary>
	/// <param name="angle">angle to check</param>
	public bool IsInAngleExtents(float angle) {
		return angle < MaxAngle && angle > MinAngle;
	}

	/// <summary>
	/// Decides whether the passed height passes evaluation. 
	/// The HeightProbCurve is sampled at the passed height and its result 
	/// is used in a random number generator as a probability.
	/// For example, if the HeightCurve sample was 0.5, there would be a 
	/// 50% chance of being placed.
	/// 
	/// If the HeightProbCurve is not being used, the PlacementProbability
	/// is used instead.
	/// </summary>
	/// <param name="height">Height to sample</param>
	/// <returns></returns>
	public bool EvaluateHeight(float height) {
		float probability = 1f;
		//TODO reimplement
		if (ConstrainHeight) {
			float totalHeight = MaxHeight - MinHeight;
			float sampleAt = height / totalHeight;

			probability = HeightProbCurve.Evaluate(sampleAt);
		} else {
			//probability = PlacementProbability;
		}

		float chance = ((float)Rand.Next(0, 100)) / 100;
		return chance > (1 - probability);
	}
}
