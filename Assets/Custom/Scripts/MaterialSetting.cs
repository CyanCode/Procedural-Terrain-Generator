using UnityEngine;
using System.Collections;

public class MaterialSetting : MonoBehaviour {
	[System.Serializable]
	public class MaterialSettingType {
		public Material Mat;

		[Tooltip("The minimum height to show the material (-1 for default)")]
		public float MinHeight;

		[Tooltip("The maximum height to show the material (-1 for default)")]
		public float MaxHeight;

		[Tooltip("What angle to start showing the material at (-1 for never)")]
		public float Angle;
	}

	public MaterialSettingType[] Settings;

	/// <summary>
	/// Get the material associated with the passed height and angle. 
	/// Angle overrides height so if both height and angle match, the material
	/// associated with the greater andgle will be returned over the matched
	/// height.
	/// </summary>
	/// <param name="height">Height to query</param>
	/// <param name="angle">Angle to query</param>
	/// <returns>
	/// Material associated with height and angle if found, null if no
	/// Material matched the query.
	/// </returns>
	public Material GetMaterial(float height, float angle) {
		Material def = null;

		foreach (MaterialSettingType setting in Settings) {
			if (angle > setting.Angle && setting.Angle > 0) {
				return setting.Mat;
			} else if (height > setting.MinHeight && height < setting.MaxHeight) {
				return setting.Mat;
			} else if (setting.MinHeight == -1f || setting.MaxHeight == -1f) {
				def = setting.Mat;
			}
		}

		return def;
	}
}
