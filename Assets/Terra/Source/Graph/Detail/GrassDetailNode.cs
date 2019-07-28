using System;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome { 
	[Serializable, CreateNodeMenu("Biomes/Objects/Grass")]
	public class GrassDetailNode : DetailNode {
        public DetailRenderMode RenderMode;

        public Texture2D Texture;
	    public GameObject Prefab;

	    public float NoiseSpread;
        public Color HealthyColor = Color.white;
        public Color DryColor = Color.white;

	    public virtual object GetDetailPrototype() {
            return new DetailPrototype {
	            prototypeTexture = Texture,
                healthyColor = HealthyColor,
                dryColor = DryColor,
	            noiseSpread = NoiseSpread,
	            minHeight = HeightScale.x,
	            maxHeight = HeightScale.y,
	            minWidth = WidthScale.x,
	            maxWidth = WidthScale.y,
	            bendFactor = BendFactor,

	            renderMode = RenderMode,
	            prototype = Prefab,
	            usePrototypeMesh = RenderMode == DetailRenderMode.VertexLit
	        };
        }

        public override object GetValue(NodePort port) {
            return this;
        }
	}
}