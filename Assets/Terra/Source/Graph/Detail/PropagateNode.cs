using System.Collections.Generic;
using Terra.Structures;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
    [CreateNodeMenu("Biomes/Propagate")]
    public class PropagateNode : DetailNode {
        [Output]
        public NodePort Output;

        [Input(ShowBackingValue.Never, ConnectionType.Override)]
        public DetailNode PlacementNode;

        public float DistanceMin = 50f;
        public float DistanceMax = 30f;

        public int ObjectCountMin = 3;
        public int ObjectCountMax = 7;

        private DetailNode _placement {
            get {
                return GetInputValue<DetailNode>("PlacementNode");
            }
        }

        public override object GetValue(NodePort port) {
            var placement = GetInputValue<DetailNode>("PlacementNode");
            return placement == null ? null : placement.GetValue(port);
        }

        public override Texture2D DidRequestTextureUpdate() {
            Texture2D tex = new Texture2D(PreviewTextureSize, PreviewTextureSize);

            //Fill texture with black
            for (int x = 0; x < PreviewTextureSize; x++) {
                for (int y = 0; y < PreviewTextureSize; y++) {
                    tex.SetPixel(x, y, Color.black);
                }
            }

            Vector2[] samples = SamplePositions();
            for (var i = 0; i < samples.Length; i++) {
                Vector2 sample = samples[i];
                int x = Mathf.Clamp((int)(sample.x * PreviewTextureSize), 0, PreviewTextureSize);
                int y = Mathf.Clamp((int)(sample.y * PreviewTextureSize), 0, PreviewTextureSize);

                tex.SetPixel(x, y, Color.white);

                if (i >= _placement.GetPlaceableObject().MaxObjects) {
                    break;
                }
            }

            tex.Apply();
            return tex;
        }

        public override Vector2[] SamplePositions() {
            Vector2[] startPositions = _placement.SamplePositions();

            int totalMaxObjs = _placement.GetPlaceableObject().MaxObjects;
            if (startPositions.Length >= totalMaxObjs) {
                return startPositions;
            }

            List<Vector2> positions = new List<Vector2>(startPositions.Length);
            foreach (Vector2 pos in startPositions) {
                for (int i = 0; i < Random.Range(ObjectCountMin, ObjectCountMax + 1); i++) {
                    if (positions.Count >= totalMaxObjs) {
                        return positions.ToArray();
                    }

                    //Calculate random rotation & offset
                    float min = DistanceMin / GRID_SIZE;
                    float max = DistanceMax / GRID_SIZE;

                    Vector2 offset = GetRandomInCircle(pos, min, max);
                    float x = offset.x;
                    float y = offset.y;

                    if (x > 0f && x < 1f && y > 0f && y < 1f) {
                        positions.Add(new Vector2(x, y));
                    }
                }
            }

            return positions.ToArray();
        }

        public override DetailData GetPlaceableObject() {
            return _placement.GetPlaceableObject();
        }

        private Vector2 GetRandomInCircle(Vector2 center, float min, float max) {
            bool isPosX = Random.Range(0, 2) == 0;
            bool isPosY = Random.Range(0, 2) == 0;

            float x = isPosX ? Random.Range(center.x + min, center.x + max) : 
                Random.Range(center.x - min, center.x - max);
            float y = isPosY ? Random.Range(center.y + min, center.y + max) :
                Random.Range(center.y - min, center.y - max);

            return new Vector2(x, y);
        }
    }
}
