using System;
using UnityEngine;
using XNode;
using Random = UnityEngine.Random;

namespace Terra.Graph.Biome {
    [Serializable, CreateNodeMenuAttribute("Biomes/Objects/Object")]
    public class ObjectDetailNode : DetailNode {
        /// <summary>
        /// Extents for a random transformation.
        /// </summary>
        [Serializable]
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

        public GameObject Prefab;

        public bool ShowTranslateFoldout = false;
        public bool ShowRotateFoldout = false;
        public bool ShowScaleFoldout = false;

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

        public override object GetValue(NodePort port) {
            return this;
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
            }

            return RotationAmount;
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
            }

            return ScaleAmount;
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
            }

            return TranslationAmount + pos;
        }

        /// <summary>
        /// Transforms the passed gameobject according to the 
        /// passed Vector3 position and the (optional) random 
        /// transformations specified in this placement type.
        /// This does not set the parent for this gameobject
        /// </summary>
        /// <param name="go">GameObject to transform</param>
        /// <param name="position">Calculated position</param>
        public void TransformGameObject(GameObject go, Vector3 position) {
            go.transform.position = GetTranslation(position);
            go.transform.eulerAngles = GetRotation();
            go.transform.localScale = GetScale();
        }
    }
}