using System;
using Terra.Structures;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome { 
	[Serializable, CreateNodeMenu("Biomes/Objects/Tree")]
	public class TreeDetailNode: DetailNode {
        public GameObject Prefab;
        public bool RandomRotation;

        public TreeInstance GetTreeInstance(Vector3 position, int prototypeIndex, System.Random random) {
            return new TreeInstance {
                position = position,
                heightScale = GetRandomHeight(random),
                widthScale = GetRandomWidthScale(random),
                rotation = UnityEngine.Random.Range(0, Mathf.PI * 2),
                color = Color.white,
                lightmapColor = Color.white,
                prototypeIndex = prototypeIndex
            };
        }

        public override object GetValue(NodePort port) {
            return this;
        }

	    public virtual object GetDetailPrototype() {
	        return new TreePrototype {
	            bendFactor = BendFactor,
	            prefab = Prefab
	        };
        }
	}
}