using Terra.CoherentNoise;

namespace Terra.Graph.Generators {
    [CreateNodeMenu(MENU_PARENT_NAME + "Perlin")]
    public class PerlinNoiseNode : AbsGeneratorNode {
        public override Generator GetGenerator() {
            return new PerlinNoise();
        }

        public override string GetTitle() {
            return "Perlin";
        }
    }
}