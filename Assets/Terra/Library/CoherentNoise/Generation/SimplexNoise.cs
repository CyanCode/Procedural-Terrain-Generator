using Terra.SimplexNoise;
using Terra.Source;
using UnityEngine;

namespace Terra.CoherentNoise {
    public class SimplexNoise : Generator {
        new public bool RequiresRemap = false;
        public Vector2 Offset = Vector2.zero;
        public float Frequency = 1f;

        public override float GetValue(float x, float y, float z) {
            return (float)new OpenSimplexNoise(TerraConfig.Instance.Seed).Evaluate((x + Offset.x) * Frequency,(y + Offset.y) * Frequency);
        }
    }
}