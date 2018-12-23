using System;
using UnityEngine;

namespace Terra.Structure {
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
		/// Checks whether:
		/// val >= max 
		/// OR 
		/// val <= min
		/// </summary>
		/// <param name="val">Value to check</param>
		public bool FitsMinMax(float val) {
			return val >= Max || val <= Min;
		}

		/// <summary>
		/// Calculates the "weight" of the passed value by finding
		/// the passed value's smaller distance between the min & max 
		/// and dividing the value by <see cref="blend"/>. The result is 
		/// then raised to the power of <see cref="falloff"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="blend"></param>
		/// <param name="falloff"></param>
		/// <returns>A weight in the range of 0 and 1</returns>
		public float Weight(float value, float blend, float falloff = 1f) {

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

			return value;
		}
	}
}
