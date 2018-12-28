using System;
using UnityEngine;

namespace Terra.Structures {
	[Serializable]
	public enum ConstraintMixMethod {
		AND, OR
	}

	/// <summary>
	/// Represents a constraint between a minimum and 
	/// a maximum.
	/// </summary>
	[Serializable]
	public struct Constraint {
		public float Min;
		public float Max;

		public Constraint(float min, float max) {
			Min = min;
			Max = max;
		}

		/// <summary>
		/// Does the passed value fit within the min and max?
		/// </summary>
		/// <param name="val">Value to check</param>
		public bool Fits(float val) {
			return val >= Min && val <= Max;
		}

		/// <summary>
		/// Calculates the "weight" of the passed value by finding
		/// the whether it fits within the min & max and how much to 
		/// show if the value is within the blend distance
		/// </summary>
		/// <param name="value"></param>
		/// <param name="blend"></param>
		/// <returns>A weight in the range of 0 and 1</returns>
		public float Weight(float value, float blend) {
			if (value > Max - blend) {
				float nmin = Max - blend;
				float a = (value - nmin) / (Max - nmin);
				return Mathf.Lerp(value, 0, a);
			}
			if (value < Min + blend) {
				float nmax = Min + blend;
				float a = (value - Min) / (nmax - Min);
				return Mathf.Lerp(value, 0, 1 - a);
			}

			//return Fits(value) ? 1f : 0f;
			return value;
		}
	}
}
