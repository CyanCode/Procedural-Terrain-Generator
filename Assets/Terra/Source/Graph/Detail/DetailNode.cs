using Terra.Structures;
using UnityEngine;

namespace Terra.Graph.Biome {
    [CreateNodeMenu("Biomes/Tree")]
    public abstract class DetailNode : PreviewableNode {
        internal const int GRID_SIZE = 100;

        public abstract Vector2[] SamplePositions();

        public abstract DetailData GetPlaceableObject();
    }
}
