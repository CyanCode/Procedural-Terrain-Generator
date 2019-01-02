using System;
using System.Collections.Generic;
using Terra.Graph.Biome;
using Terra.Structures;
using UnityEngine;

namespace Terra.Terrain {
    [Serializable]
    public class TileDetail {
        private const float DETAIL_SHOW_THRESHHOLD = 0.5f;

        [SerializeField]
        private Tile _tile;

        private UnityEngine.Terrain _terrain {
            get {
                return _tile.GetComponent<UnityEngine.Terrain>();
            }
        }

        private float[,,] BiomeMap {
            get {
                if (_tile.Painter == null) {
                    Debug.LogError("TileDetail requires a non-null TilePaint instance");
                    return null;
                }
                if (_tile.Painter.BiomeMap == null) {
                    Debug.LogError("TileDetail requires a non-null BiomeMap in TilePaint");
                    return null;
                }

                return _tile.Painter.BiomeMap;
            }
        }

        public TileDetail(Tile tile) {
            _tile = tile;
        }

        /// <summary>
        /// Adds trees according to the BiomeMap in this <see cref="Tile"/>
        /// </summary>
        public void AddTrees() {
            if (BiomeMap == null) {
                return;
            }

            DetailNode[] allTreeNodes = _tile.Painter.Combiner.Sampler.GetAllTreeNodes();
            List<TreePrototype> prototypes = new List<TreePrototype>(allTreeNodes.Length);

            //Collect all sampled locations
            foreach (DetailNode treeNode in allTreeNodes) {
                prototypes.Add((treeNode as TreeDetailNode).GetTreePrototype());
            }

            _terrain.terrainData.treePrototypes = prototypes.ToArray();
            _terrain.terrainData.RefreshPrototypes();

            BiomeCombinerNode combiner = _tile.Painter.Combiner;
            BiomeNode[] biomeNodes = combiner.GetConnectedBiomeNodes();
            int prototypeIndex = 0;

            for (int i = 0; i < biomeNodes.Length; i++) {
                //Collect all trees for this biome
                BiomeNode biome = biomeNodes[i];
                DetailNode[] treeNodes = biome.GetTreeNodes();

                if (treeNodes == null) { //A biome may contain no trees
                    continue;
                }
                
                foreach (DetailNode treeNode in biome.GetTreeNodes()) {
                    DetailData placeable = treeNode.GetPlaceableObject();

                    //Get map of normalized "tree positions"
                    Vector2[] samples = treeNode.SamplePositions();
                    foreach (Vector2 sample in samples) {
                        float[] biomeWeights = combiner.Sampler.SampleBiomeMapAt(BiomeMap, sample.x, sample.y);
                        float thisBiomeWeight = biomeWeights[i];

                        if (thisBiomeWeight < DETAIL_SHOW_THRESHHOLD) {
                            continue; //Not in this biome, skip
                        }

                        //Check whether a tree can be placed here
                        float height = _terrain.terrainData.GetInterpolatedHeight(sample.x, sample.y);
                        float angle = Vector3.Angle(Vector3.up,
                            _terrain.terrainData.GetInterpolatedNormal(sample.x, sample.y));

                        if (placeable.ShouldPlaceAt(height, angle)) {
                            //Add tree to terrain
                            Vector3 world = new Vector3(sample.x, height, sample.y);

                            //Tree sample set index matches the tree prototype index (j)
                            TreeInstance tree = (treeNode as TreeDetailNode).GetTreeInstance(world, prototypeIndex);
                            _terrain.AddTreeInstance(tree);
                        }
                    }

                    prototypeIndex++;
                }
            }
        }
    }
}