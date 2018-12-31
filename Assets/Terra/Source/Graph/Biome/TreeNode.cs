using System;
using Terra.Structures;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome { 
	[Serializable, CreateNodeMenu("Biomes/Tree")]
	public class TreeNode: PreviewableNode {
        [Output] public NodePort Output;
		public PlaceableObject Placeable;

		protected override void Init() {
			base.Init();

			if (Placeable == null) {
                Placeable = CreateInstance<PlaceableObject>().Init(TerraConfig.GenerationSeed);
			}
		}

        public override object GetValue(NodePort port) {
            return this;
        }

        public override Texture2D DidRequestTextureUpdate() {
			Texture2D tex = new Texture2D(PreviewTextureSize, PreviewTextureSize);
            ObjectSampler sampler = new ObjectSampler(Placeable);

            //Fill texture with black
            for (int x = 0; x < PreviewTextureSize; x++) {
                for (int y = 0; y < PreviewTextureSize; y++) {
                    tex.SetPixel(x, y, Color.black);
                }
            }

            //Fill in sampled spots
            const int gridSize = 100;
            Vector2[] samples = sampler.GetPoissonGridSamples(gridSize);
            for (var i = 0; i < samples.Length; i++) {
                Vector2 sample = samples[i];
                int x = Mathf.Clamp((int) (sample.x / gridSize * PreviewTextureSize), 0, PreviewTextureSize);
                int y = Mathf.Clamp((int) (sample.y / gridSize * PreviewTextureSize), 0, PreviewTextureSize);

                tex.SetPixel(x, y, Color.white);

                if (i >= Placeable.MaxObjects) {
                    break;
                }
            }

            tex.Apply();
            return tex;
		}
	}
}