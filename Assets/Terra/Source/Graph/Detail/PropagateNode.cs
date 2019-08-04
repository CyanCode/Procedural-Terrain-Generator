using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
    [CreateNodeMenu("Biomes/Modifiers/Propagate")]
    public class PropagateNode : DetailModifierNode {
        public float DistanceMin = 50f;
        public float DistanceMax = 30f;

        public int ObjectCountMin = 3;
        public int ObjectCountMax = 7;

        public override object GetValue(NodePort port) {
            return this;
        }

        public override Vector2[] SamplePositions(Vector2[] samples, System.Random random) {
            DetailNode obj = GetOutputValue();
            if (obj == null) {
                return new Vector2[0];
            }

            List<Vector2> positions = new List<Vector2>(samples.Length);
            foreach (Vector2 pos in samples) {
                for (int i = 0; i < random.Next(ObjectCountMin, ObjectCountMax + 1); i++) {
                    if (positions.Count >= obj.MaxObjects) {
                        return positions.ToArray();
                    }

                    //Calculate random rotation & offset
                    float min = DistanceMin / GridSize;
                    float max = DistanceMax / GridSize;

                    Vector2 offset = GetRandomInCircle(pos, min, max, random);
                    float x = offset.x;
                    float y = offset.y;

                    if (x > 0f && x < 1f && y > 0f && y < 1f) {
                        positions.Add(new Vector2(x, y));
                    }
                }
            }

            return positions.ToArray();
        }

        private Vector2 GetRandomInCircle(Vector2 center, float min, float max, System.Random random) {
            bool isPosX = random.Next(0, 2) == 0;
            bool isPosY = random.Next(0, 2) == 0;

            float x = isPosX ? random.NextFloat(center.x + min, center.x + max) :
                random.NextFloat(center.x - min, center.x - max);
            float y = isPosY ? random.NextFloat(center.y + min, center.y + max) :
                random.NextFloat(center.y - min, center.y - max);

            return new Vector2(x, y);
        }
    }
}
