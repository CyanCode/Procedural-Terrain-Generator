using UnityEngine;

[System.Serializable]
public class ObjectPlacementType {
	private System.Random Rand;

	[SerializeField]
	private int Seed;

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

	public bool ShowTranslateFoldout = false;
	public bool ShowRotateFoldout = false;
	public bool ShowScaleFoldout = false;

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
	/// How spread apart should the objects be from each other?
	/// </summary>
	public float Spread = 10f;

	/// <summary>
	/// The probability that an object will be placed
	/// </summary>
	public int PlacementProbability = 100;

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
	/// Is this object scaled uniformly?
	/// </summary>
	public bool IsUniformScale = false;

	/// <summary>
	/// Min amount to scale IF this object is being scaled 
	/// uniformly.
	/// </summary>
	public float UniformScaleMin = 1f;

	/// <summary>
	/// Max amount to scale IF this object is being scaled 
	/// uniformly.
	/// </summary>
	public float UniformScaleMax = 1.5f;

	/// <summary>
	/// If the object is not randomly scaled, 
	/// should it be scaled by a specified amount?
	/// If it is randomly translated, what is the maximum amount 
	/// that it should be allowed to transate?
	/// </summary>
	public Vector3 ScaleAmount = new Vector3(1f, 1f, 1f);

	/// <summary>
	/// The maximum and minimum amounts that this object 
	/// can randomly scale.
	/// </summary>
	public RandomTransformExtent RandomScaleExtents;
	#endregion

	/// <param name="seed">Optional seed value to use in the random number generator</param>
	public ObjectPlacementType(int seed = -1) {
		Seed = seed;
		InitRNG();

		//Set transformation defaults
		RandomRotationExtents = new RandomTransformExtent();
		RandomRotationExtents.Max = new Vector3(0, 360, 0);

		RandomScaleExtents = new RandomTransformExtent();
		RandomScaleExtents.Max = new Vector3(1.5f, 1.5f, 1.5f);
		RandomScaleExtents.Min = new Vector3(1f, 1f, 1f);

		RandomTranslateExtents = new RandomTransformExtent();
		RandomTranslateExtents.Max = new Vector3(1f, 1f, 1f);
		RandomTranslateExtents.Min = new Vector3(-1f, -1f, -1f);
	}

	/// <summary>
	/// Decides whether or not this object should be placed at
	/// the specified height and angle. Does not check for intersections.
	/// </summary>
	/// <param name="height">Height to evaluate at</param>
	/// <param name="angle">Angle to evaluate at (0 to 180 degrees)</param>
	/// <returns></returns>
	public bool ShouldPlaceAt(float height, float angle) {
		//Fails height or angle check
		if (ConstrainHeight && !IsInHeightExtents(height)) {
			return false;
		}
		if (ConstrainAngle && !IsInAngleExtents(angle)) {
			return false;
		}

		//Fails constrained height or constrained angle check
		if (!EvaluateHeight(height) || !EvaluateAngle(angle)) {
			return false;
		}

		//Place probability check
		if (PlacementProbability != 100f) {
			if (Rand == null) {
				InitRNG();
			}

			int result = Rand.Next(0, 100);
			return result >= (100 - PlacementProbability);
		}

		return true;
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
	/// </summary>
	/// <param name="height">Height to sample</param>
	/// <returns></returns>
	public bool EvaluateHeight(float height) {
		float probability = 1f;

		if (ConstrainHeight) {
			float totalHeight = MaxHeight - MinHeight;
			float sampleAt = (height + MaxHeight) / totalHeight;

			probability = HeightProbCurve.Evaluate(sampleAt);
		} else {
			return true;
		}

		if (Rand == null) {
			InitRNG();
		}

		float chance = ((float)Rand.Next(0, 100)) / 100;
		return chance > (1 - probability);
	}

	/// <summary>
	/// Decides whether the passed angle passes evaluation. 
	/// The AngleProbCurve is sampled at the passed angle and 
	/// its result is used in a random number generator as a 
	/// probability.
	/// For example, if the AngleCurve sample was 0.1, there would 
	/// be a 50% chance of being placed.
	/// </summary>
	/// <param name="angle">Angle to sample</param>
	/// <returns></returns>
	public bool EvaluateAngle(float angle) {
		float probability = 1f;

		if (ConstrainAngle) {
			float totalAngle = MaxAngle - MinAngle;
			float sampleAt = (angle + MaxAngle) / totalAngle;

			probability = AngleProbCurve.Evaluate(sampleAt);
		} else {
			return true;
		}

		if (Rand == null) {
			InitRNG();
		}

		float chance = ((float)Rand.Next(0, 100)) / 100;
		return chance > (1 - probability);
	}

	/// <summary>
	/// Samples a rotation for this object placement type. Can 
	/// return the rotation value that was set OR a random rotation 
	/// within the specified extends (if random rotation is enabled)
	/// </summary>
	/// <returns>A rotation in Euler angles</returns>
	public Vector3 GetRotation() {
		if (IsRandomRotation) {
			Vector3 max = RandomRotationExtents.Max;
			Vector3 min = RandomRotationExtents.Min;

			float x = Random.Range(min.x, max.x);
			float y = Random.Range(min.y, max.y);
			float z = Random.Range(min.z, max.z);

			return new Vector3(x, y, z);
		} else {
			return RotationAmount;
		}
	}

	/// <summary>
	/// Samples a scaling factor for this object placement type. 
	/// Can return the scale value that was set OR a random scale 
	/// within the specified extends (if random scaling is enabled)
	/// </summary>
	public Vector3 GetScale() {
		if (IsRandomScale) {
			if (IsUniformScale) {
				float max = UniformScaleMax;
				float min = UniformScaleMin;
				float amount = Random.Range(min, max);

				return new Vector3(amount, amount, amount);
			} else {
				Vector3 max = RandomScaleExtents.Max;
				Vector3 min = RandomScaleExtents.Min;

				float x = Random.Range(min.x, max.x);
				float y = Random.Range(min.y, max.y);
				float z = Random.Range(min.z, max.z);

				return new Vector3(x, y, z);
			}
		} else {
			return ScaleAmount;
		}
	}

	/// <summary>
	/// Applies the translations specified in this instance. 
	/// This includes translations OR a random translation between 
	/// the specified min and max.
	/// </summary>
	/// <param name="pos">Start position before additional translations</param>
	/// <returns>Translated Vector3</returns>
	public Vector3 GetTranslation(Vector3 pos) {
		if (IsRandomTranslate) {
			Vector3 max = RandomTranslateExtents.Max;
			Vector3 min = RandomTranslateExtents.Min;

			float x = Random.Range(min.x, max.x);
			float y = Random.Range(min.y, max.y);
			float z = Random.Range(min.z, max.z);

			return new Vector3(x, y, z) + pos;
		} else {
			return TranslationAmount + pos;
		}
	}

	/// <summary>
	/// Transforms the passed gameobject according to the 
	/// passed Vector3 position and the (optional) random 
	/// transformations specified in this placement type.
	/// This does not set the parent for this gameobject
	/// </summary>
	/// <param name="go">GameObject to transform</param>
	/// <param name="position">Calculated position</param>
	/// <param name="length">Length of the mesh</param>
	/// <param name="tileOffset">Location where the TerrainTile starts</param>
	public void TransformGameObject(GameObject go, Vector3 position, int length, Vector3 tileOffset) {
		float xPos = ((position.x * length) - length / 2) + tileOffset.x;
		float yPos = position.y;
		float zPos = ((position.z * length) - length / 2) + tileOffset.z;

		go.transform.position = GetTranslation(new Vector3(xPos, yPos, zPos));
		go.transform.eulerAngles = GetRotation();
		go.transform.localScale = GetScale();
	}

	/// <summary>
	/// Initializes the random number generator
	/// </summary>
	void InitRNG() {
		if (Seed == -1)
			Rand = new System.Random();
		else
			Rand = new System.Random(Seed);
	}
}
