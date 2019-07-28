using System;
using Terra.CoherentNoise.Generation.Combination;
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

		/// <summary>
		/// If <see cref="Max"/> equals 1
		/// </summary>
		public bool IsMaxConstraint {
			get {
				return Max == 1f;
			}
		}

		/// <summary>
		/// If <see cref="Min"/> equals 0
		/// </summary>
		public bool IsMinConstraint {
			get {
				return Min == 0f;
			}
		}

		public Constraint(float min, float max) : this() {
			Min = min;
			Max = max;
		}

        /// <summary>
        /// Returns a random number in the range of min and max
        /// </summary>
        public float Random() {
            return UnityEngine.Random.Range(Min, Max);
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
		/// <param name="value">Value within the range of 0 and 1 to weight</param>
		/// <param name="blend">Blend distance</param>
		/// <returns>A weight in the range of 0 and 1</returns>
		public float Weight(float value, float blend) {
			if (value > Max - blend) {
				//Value within max transition zone
				if (IsMaxConstraint) {
					return value;
				}

				float nmin = Max - blend;
				float a = (value - nmin) / (Max - nmin);
				return Mathf.Lerp(value, 0, a);
			}
			if (value < Min + blend) {
				//Value within min transition zone
				if (IsMinConstraint) {
					return value;
				}

				float nmax = Min + blend;
				float a = (value - Min) / (nmax - Min);
				return Mathf.Lerp(value, 0, 1 - a);
			}

			return value;
		}

	    public Constraint Clamp(Constraint to) {
	        Constraint from = new Constraint(Min, Max);

	        if (from.Min < to.Min) {
	            from.Min = to.Min;
	        }
	        if (from.Max > to.Max) {
	            from.Max = to.Max;
	        }

	        return from;
	    }
    }
}
