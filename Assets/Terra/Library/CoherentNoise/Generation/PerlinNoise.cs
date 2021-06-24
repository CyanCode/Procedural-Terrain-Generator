using Terra.SimplexNoise;

namespace Terra.CoherentNoise {
    public class PerlinNoise : Generator {
        new public bool RequiresRemap = false;

        public override float GetValue(float x, float y, float z) {
            return (float)new OpenSimplexNoise(TerraConfig.Instance.Seed).Evaluate(x, y);
        }
    }
}
