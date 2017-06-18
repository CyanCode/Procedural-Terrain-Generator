using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TerrainObject: ScriptableObject {
	public List<GameObject> Objects = new List<GameObject>();

	[Tooltip("Increase the object's Y position by the following value")]
	public float IncreaseY = 0f;
	[Tooltip("When enabled a random number is chosen between 0 and IncreaseY to increase by")]
	public bool RandomIncrease = false;
	[Tooltip("Min height object can be placed within. -1 to disable.")]
	public float MinHeight = 0f;
	[Tooltip("Max height object can be placed within. -1 to disable.")]
	public float MaxHeight = 1000f;
	[Tooltip("Chance that the object will be placed (0 being no chance, 1 being definite)")]
	[Range(0, 1)]
	public float PlacementPossibility = .5f;
	public string AssetPath {
		get {
			string path = AssetDatabase.GetAssetPath(this);
			path = path.Substring(path.IndexOf("Resources/") + "Resources/".Length);
			return path.Replace(".prefab", "");
		}
	}

	/// <summary>
	/// Checks whether the object should be placed at the passed 
	/// position based on the following input parameters:
	/// <list type="bullet">
	/// <item>Minimum height</item>
	/// <item>Maximum height</item>
	/// <item>Steepness</item>
	/// </list>
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	public bool ShouldPlaceAtPosition(Vector3 position) {
		if (position.y < MinHeight && MinHeight != -1) return false;
		if (position.y > MaxHeight && MaxHeight != -1) return false;

		return Random.Range(0, 1) <= PlacementPossibility;
	}

	/// <summary>
	/// Creates and places the object at the passed position if 
	/// the object <i>should</i> be placed. Object will not 
	/// be placed if it has already been created.
	/// </summary>
	/// <param name="position">Position to place object</param>
	/// <returns>Instance of created gameobject if one was created, null otherwise</returns>
	public GameObject PlaceAtPosition(Vector3 position) {
		if (ShouldPlaceAtPosition(position)) {
			return ForcePlaceAtPosition(position);
		}

		return null;
	}

	/// <summary>
	/// Creates and places the object at the passed position ignoring 
	/// whether or not the object <i>should</i> be placed. Object will not 
	/// be placed if it has already been created.
	/// </summary>
	/// <param name="position">Position to place object</param>
	/// <returns>Instance of created gameobject</returns>
	public GameObject ForcePlaceAtPosition(Vector3 position) {
		GameObject instance = Instantiate(Resources.Load<GameObject>(AssetPath));
		Objects.Add(instance);

		float increase = RandomIncrease ? Random.Range(0, IncreaseY) : IncreaseY;
		instance.transform.position = new Vector3(position.x, position.y + increase, position.z);

		return instance;
	}
}
