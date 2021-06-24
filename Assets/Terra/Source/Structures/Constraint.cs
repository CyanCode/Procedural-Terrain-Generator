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

		private float? _cachedBlendValue;
		private AnimationCurve _animationCurve;

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
			return GetAnimationCurve(blend).Evaluate(value);
		}


		//public float Weight(float value, float blend) {
		//	if (Fits(value)) {
		//		float midpoint = (Max - Min) / 2f;

		//		if (value < midpoint) {
		//			return value * (Min == 0f ? 1f : 1-Mathf.Clamp01(((1 - Min) - value) / blend));
		//              } else {
		//			return value * (Max == 1f ? 1f : Mathf.Clamp01((Max - value) / blend));
		//              }
		//          }

		//	return 0f;
		//}

		//public float Weight(float value, float blend) {
		//	if (value > Max - blend) {
		//		//Value within max transition zone
		//		if (IsMaxConstraint) {
		//			return value;
		//		}

		//		float nmin = Max - blend;
		//		float a = (value - nmin) / (Max - nmin);
		//		return Mathf.Lerp(value, 0, a);
		//	}
		//	if (value < Min + blend) {
		//		//Value within min transition zone
		//		if (IsMinConstraint) {
		//			return value;
		//		}

		//		float nmax = Min + blend;
		//		float a = (value - Min) / (nmax - Min);
		//		return Mathf.Lerp(value, 0, 1 - a);
		//	}

		//	return value;
		//}

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

		private AnimationCurve GetAnimationCurve(float blend) {
			if (_animationCurve == null || _cachedBlendValue == null || _cachedBlendValue.Value != blend || _animationCurve == null) {
				_cachedBlendValue = blend;
				_animationCurve = new AnimationCurve();
				
				if (IsMinConstraint) {
					_animationCurve.AddKey(new Keyframe(0, 1));
				} else {
					_animationCurve.AddKey(new Keyframe(0, 0));
					_animationCurve.AddKey(new Keyframe(Min, 0));
					_animationCurve.AddKey(new Keyframe(Min + blend, 1f));
                }

				if (IsMaxConstraint) {
					_animationCurve.AddKey(new Keyframe(1f, 1f));
                } else {
					_animationCurve.AddKey(new Keyframe(Max - blend, 1f));
					_animationCurve.AddKey(new Keyframe(Max, 0));
					_animationCurve.AddKey(1, 0);
                }
			}

			return _animationCurve;
        }
    }
}
