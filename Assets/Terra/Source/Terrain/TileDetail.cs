using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Adds trees according to the <see cref="BiomeMap"/> in 
        /// this <see cref="Tile"/>
        /// </summary>
        public void AddTrees() {
            if (BiomeMap == null) {
                return;
            }

            //Collect prototypes from tree nodes
            TreeDetailNode[] allTreeNodes = _tile.Painter.Combiner
                .GetConnectedBiomeNodes()
                .SelectMany(biome => biome.GetTreeInputs())
                .ToArray();
            List<TreePrototype> prototypes = new List<TreePrototype>(allTreeNodes.Length);

            foreach (TreeDetailNode treeNode in allTreeNodes) {
                prototypes.Add((TreePrototype)treeNode.GetDetailPrototype());
            }

            GenerationData conf = TerraConfig.Instance.Generator;
            _terrain.terrainData.treePrototypes = prototypes.ToArray();
            _terrain.terrainData.RefreshPrototypes();
            _terrain.treeDistance = conf.TreeDistance;
            _terrain.treeBillboardDistance = conf.BillboardStart;
            _terrain.treeMaximumFullLODCount = conf.MaxMeshTrees;

            BiomeCombinerNode combiner = _tile.Painter.Combiner;
            BiomeNode[] biomeNodes = combiner.GetConnectedBiomeNodes();
            int prototypeIndex = 0;

            for (int i = 0; i < biomeNodes.Length; i++) {
                //Collect all trees for this biome
                BiomeNode biome = biomeNodes[i];
                TreeDetailNode[] treeNodes = biome.GetTreeInputs();

                if (treeNodes.Length == 0) { //A biome may contain no trees
                    continue;
                }

                foreach (TreeDetailNode treeNode in treeNodes) {
                    //Get map of normalized "tree positions"
                    Vector2[] samples = treeNode.SamplePositions();

                    foreach (Vector2 sample in samples) {
                        float[] biomeWeights = combiner.Sampler.GetBiomeWeightsInterpolated(BiomeMap, sample.x, sample.y);

                        if (biomeWeights[i] < DETAIL_SHOW_THRESHHOLD) {
                            continue; //Not in this biome, skip
                        }

                        //Check whether a tree can be placed here
                        float amp = TerraConfig.Instance.Generator.Amplitude;
                        float height = _terrain.terrainData.GetInterpolatedHeight(sample.x, sample.y) / amp;
                        float angle = Vector3.Angle(Vector3.up, _terrain.terrainData.GetInterpolatedNormal(sample.x, sample.y)) / 90;
                        Vector2 world = MathUtil.NormalToWorld(_tile.GridPosition, sample);

                        if (treeNode.ShouldPlaceAt(world.x, world.y, height, angle)) {
                            //Add tree to terrain
                            Vector3 treeLoc = new Vector3(sample.x, height, sample.y);

                            //Tree sample set index matches the tree prototype index (j)
                            TreeInstance tree = treeNode.GetTreeInstance(treeLoc, prototypeIndex);
                            _terrain.AddTreeInstance(tree);
                        }
                    }

                    prototypeIndex++;
                }
            }
        }

        /// <summary>
        /// Adds all non-tree details to the Terrain according to the 
        /// <see cref="BiomeMap"/> in this <see cref="Tile"/>. This adds 
        /// grass and detail meshes.
        /// </summary>
        public void AddDetailLayers() {
            BiomeCombinerNode combiner = _tile.Painter.Combiner;
            BiomeNode[] biomeNodes = combiner.GetConnectedBiomeNodes();
            int res = TerraConfig.Instance.Generator.DetailmapResolution;

            //Collect prototypes
            GrassDetailNode[] allDetailNodes = biomeNodes
                .SelectMany(bn => bn.GetGrassInputs())
                .ToArray();
            List<DetailPrototype> prototypes = new List<DetailPrototype>(allDetailNodes.Length);

            foreach (GrassDetailNode detailNode in allDetailNodes) {
                prototypes.Add((DetailPrototype)detailNode.GetDetailPrototype());
            }

            int prototypeIndex = 0;
            GenerationData conf = TerraConfig.Instance.Generator;
            _terrain.terrainData.SetDetailResolution(res, 16);
            _terrain.terrainData.detailPrototypes = prototypes.ToArray();
            _terrain.detailObjectDistance = conf.DetailDistance;
            _terrain.detailObjectDensity = conf.DetailDensity;

            for (int i = 0; i < biomeNodes.Length; i++) {
                //Collect all details for this biome
                BiomeNode biome = biomeNodes[i];
                GrassDetailNode[] grassNodes = biome.GetGrassInputs();

                if (grassNodes.Length == 0) { //A biome may contain no grass nodes
                    continue;
                }

                foreach (GrassDetailNode grassNode in grassNodes) {
                    int[,] layer = new int[res, res];

                    //Get map of normalized placement positions
                    Vector2[] samples = grassNode.SamplePositions();
                    foreach (Vector2 sample in samples) {
                        float[] biomeWeights = combiner.Sampler.GetBiomeWeightsInterpolated(BiomeMap, sample.x, sample.y);

                        if (biomeWeights[i] < DETAIL_SHOW_THRESHHOLD) {
                            continue; //Not in this biome, skip
                        }

                        //Check whether an object can be placed here
                        float amp = TerraConfig.Instance.Generator.Amplitude;
                        float height = _terrain.terrainData.GetInterpolatedHeight(sample.x, sample.y) / amp;
                        float angle = Vector3.Angle(Vector3.up, _terrain.terrainData.GetInterpolatedNormal(sample.x, sample.y)) / 90;
                        Vector2 world = MathUtil.NormalToWorld(_tile.GridPosition, sample);

                        if (grassNode.ShouldPlaceAt(world.x, world.y, height, angle)) {
                            //Convert normalized x,y coordinates to positions in layer map
                            Vector2 sampleWorld = sample * res;
                            int x = Mathf.Clamp(Mathf.RoundToInt(sampleWorld.x), 0, res - 1);
                            int y = Mathf.Clamp(Mathf.RoundToInt(sampleWorld.y), 0, res - 1);

                            layer[y, x] = 1; //Display object here
                        }
                    }

                    _terrain.terrainData.SetDetailLayer(0, 0, prototypeIndex, layer);
                    prototypeIndex++;
                }
            }
        }
    }
}