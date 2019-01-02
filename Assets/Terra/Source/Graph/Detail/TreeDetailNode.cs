using System;
using Terra.Structures;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome { 
	[Serializable, CreateNodeMenu("Biomes/Details/Tree")]
	public class TreeDetailNode: DetailNode {
        [Output] public NodePort Output;

		public DetailData Placeable;
        public float BendFactor;
        
		protected override void Init() {
			base.Init();

			if (Placeable == null) {
                Placeable = CreateInstance<DetailData>().Init(TerraConfig.Instance.Seed);
			}
		}

        public TreeInstance GetTreeInstance(Vector3 position, int prototypeIndex) {
            return new TreeInstance {
                position = position,
                heightScale = Placeable.GetScale().y,
                rotation = Placeable.GetRotation().y,
                widthScale = Placeable.GetScale().x,
                prototypeIndex = prototypeIndex
            };
        }

        public TreePrototype GetTreePrototype() {
            return new TreePrototype {
                bendFactor = BendFactor,
                prefab = Placeable.Prefab
            };
        }

        public override object GetValue(NodePort port) {
            return this;
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

                if (i >= Placeable.MaxObjects) {
                    break;
                }
            }

            tex.Apply();
            return tex;
		}

	    public override Vector2[] SamplePositions() {
	        //Fill in sampled spots
	        DetailSampler sampler = new DetailSampler(Placeable);
	        Vector2[] samples = sampler.GetPoissonGridSamples(GRID_SIZE);

	        return samples;
        }

	    public override DetailData GetPlaceableObject() {
            return Placeable;
	    }
	}
}