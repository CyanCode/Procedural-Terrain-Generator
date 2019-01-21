using System;
using Terra.CoherentNoise;
using Terra.Graph.Generators;
using Terra.Structures;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
    [Serializable, CreateNodeMenu("Biomes/Modifiers/Constraint")]
    public class ConstraintNode : Node {
        [Output]
        public NodePort Output;

        [Input(ShowBackingValue.Never, ConnectionType.Override)]
        public NodePort Mask;

        /// <summary>
        /// [0, 1] range of when to cutoff referencing mask
        /// </summary>
        public float Tolerance;

        /// <summary>
        /// The probability that an object will be placed when 
        /// evaluating <see cref="ShouldPlaceAt"/>
        /// </summary>
        public int PlacementProbability = 100;

        public Constraint HeightConstraint;
        public Constraint AngleConstraint;

        /// <summary>
        /// Optionally constrain at which height the objects will 
        /// show up
        /// </summary>
        public bool ConstrainHeight = false;

        /// <summary>
        /// Optionally constrain at which angle the objects will 
        /// show up.
        /// </summary>
        public bool ConstrainAngle = false;

        /// <summary>
        /// Height curve to evaluate when procedurally placing 
        /// objects. Top of curve = 100% chance of placing the object,
        /// bottom of curve = 0% chance of placing the object.
        /// </summary>
        public AnimationCurve HeightProbCurve = AnimationCurve.Linear(0, 1, 1, 1);

        /// <summary>
        /// Angle curve to evaluate when procedurally placing 
        /// objects. Top of curve = 100% chance of placing the object,
        /// bottom of curve = 0% chance of placing the object.
        /// Evaluates from 0 - 1 where 1 = 90 degrees.
        /// </summary>
        public AnimationCurve AngleProbCurve = AnimationCurve.Linear(0, 1, 1, 1);

        public AbsGeneratorNode GetMaskValue() {
            return GetInputValue<AbsGeneratorNode>("Mask");
        }

        private AbsGeneratorNode _cachedGeneratorNode;
        private Generator _cachedGenerator;
        private System.Random _random = new System.Random();

        /// <summary>
        /// Decides whether or not this object should be placed at
        /// the specified height and angle. Does not check for intersections.
        /// </summary>
        /// <param name="height">Height to evaluate at</param>
        /// <param name="angle">Angle to evaluate at (0 to 180 degrees)</param>
        /// <returns></returns>
        public bool ShouldPlaceAt(float x, float y, float height, float angle) {
            if (ConstrainHeight && !HeightConstraint.Fits(height)) {
                return false;
            }
            if (ConstrainAngle && !AngleConstraint.Fits(angle)) {
                return false;
            }

            //Fails constrained height or constrained angle check
            if (!EvaluateHeight(height) || !EvaluateAngle(angle)) {
                return false;
            }
            
            //Place probability check
            if (Math.Abs(PlacementProbability - 100f) > 0.01f) {
                int result = UnityEngine.Random.Range(0, 101);
                if (result < 100 - PlacementProbability) {
                    return false;
                }
            }

            //Mask check
            if (_cachedGeneratorNode == null) {
                _cachedGeneratorNode = GetMaskValue();
            }
            
            if (_cachedGeneratorNode != null) {
                if (_cachedGenerator == null) {
                    _cachedGenerator = _cachedGeneratorNode.GetGenerator();
                }

                if (_cachedGenerator != null) {
                    return _cachedGenerator.GetValue(x, y, 0) < Tolerance;
                }
            }

            return true;
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
                float totalHeight = HeightConstraint.Max - HeightConstraint.Min;
                float sampleAt = (height + HeightConstraint.Max) / totalHeight;

                probability = HeightProbCurve.Evaluate(sampleAt);
            } else {
                return true;
            }

            float chance = _random.Next(0, 100) / (float)100;
            return chance > 1 - probability;
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
            if (ConstrainAngle) {
                float totalAngle = AngleConstraint.Max - AngleConstraint.Min;
                float sampleAt = (angle + AngleConstraint.Max) / totalAngle;

                float probability = AngleProbCurve.Evaluate(sampleAt);
                float chance = _random.Next(0, 100) / (float)100;
                return chance > 1 - probability;
            }

            return true;
        }

        public override object GetValue(NodePort port) {
            return this;
        }
    }
}