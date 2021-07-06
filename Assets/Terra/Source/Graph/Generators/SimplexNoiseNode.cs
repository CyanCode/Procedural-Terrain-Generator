using Terra.CoherentNoise;
using UnityEngine;

namespace Terra.Graph.Generators {
    [CreateNodeMenu(MENU_PARENT_NAME + "Simplex")]
    public class SimplexNoiseNode : AbsGeneratorNode {
        [Input] public Vector2 Offset = Vector2.zero;
        [Input] public float Frequency = 1f;

        public override Generator GetGenerator() {
            CoherentNoise.SimplexNoise generator = new CoherentNoise.SimplexNoise();
            generator.Offset = Offset;
            generator.Frequency = Frequency;

            return generator;
        }

        public override string GetTitle() {
            return "Simplex";
        }
    }
}