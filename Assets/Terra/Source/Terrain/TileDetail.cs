using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terra.Graph.Biome;
using Terra.Source;
using Terra.Structures;
using UnityEngine;

namespace Terra.Terrain {
    [Serializable]
    public class TileDetail {
        private const float DETAIL_SHOW_THRESHHOLD = 0.5f;

        [SerializeField]
        private Tile _tile;

        private UnityEngine.Terrain _terrain => _tile.GetComponent<UnityEngine.Terrain>();

        private int[,] _biomeMap;
        private TilePaint _painter;
        private BiomeSampler _sampler;

        public TileDetail(Tile tile, TilePaint painter, int[,] biomeMap) {
            _tile = tile;
            _biomeMap = biomeMap;
            _painter = painter;
            _sampler = new BiomeSampler(_painter.Biomes);
        }

        /// <summary>
        /// Adds trees according to the <see cref="_biomeMap"/> in 
        /// this <see cref="Tile"/>
        /// </summary>
        public IEnumerator AddTrees() {
            if (_biomeMap == null) {
                yield break;
            }

            //Collect prototypes from tree nodes
            TreeDetailNode[] allTreeNodes = _painter.Biomes
                .SelectMany(biome => biome.GetTreeInputs())
                .ToArray();
            List<TreePrototype> prototypes = new List<TreePrototype>(allTreeNodes.Length);

            foreach (TreeDetailNode treeNode in allTreeNodes) {
                prototypes.Add((TreePrototype)treeNode.GetDetailPrototype());
            }

            int coroutineRes = TerraConfig.Instance.Generator.CoroutineRes;
            int iterations = 0;
            GenerationData conf = TerraConfig.Instance.Generator;
            _terrain.terrainData.treePrototypes = prototypes.ToArray();
            _terrain.terrainData.RefreshPrototypes();
            _terrain.treeDistance = conf.TreeDistance;
            _terrain.treeBillboardDistance = conf.BillboardStart;
            _terrain.treeMaximumFullLODCount = conf.MaxMeshTrees;

            BiomeNode[] biomeNodes = _painter.Biomes;
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
                    Vector2[] samples = null;
                    if (!TerraConfig.Instance.IsEditor) {
                        bool isDone = false;
                        TreeDetailNode node = treeNode;
                        TerraConfig.Instance.Worker.Enqueue(() => samples = node.SamplePositions(_tile.Random), () => isDone = true);

                        while (!isDone)
                            yield return null;
                    } else {
                        samples = treeNode.SamplePositions(_tile.Random);
                    }

                    foreach (Vector2 sample in samples) {
                        int selectedBiome = _sampler.GetBiomeAtInterpolatedCoords(_biomeMap, sample.x, sample.y);

                        if (iterations > coroutineRes) {
                            iterations = 0;
                            yield return null;
                        }
                        if (selectedBiome != i) {
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
                            TreeInstance tree = treeNode.GetTreeInstance(treeLoc, prototypeIndex, _tile.Random);
                            _terrain.AddTreeInstance(tree);
                        }
                    }

                    prototypeIndex++;
                }
            }
        }

        /// <summary>
        /// Adds all non-tree details to the Terrain according to the 
        /// <see cref="_biomeMap"/> in this <see cref="Tile"/>. This adds 
        /// grass and detail meshes.
        /// </summary>
        public IEnumerator AddDetailLayers() {
            BiomeNode[] biomeNodes = _painter.Biomes;
            int res = TerraConfig.Instance.Generator.DetailmapResolution;

            //Collect prototypes
            GrassDetailNode[] allDetailNodes = biomeNodes
                .SelectMany(bn => bn.GetGrassInputs())
                .ToArray();
            List<DetailPrototype> prototypes = new List<DetailPrototype>(allDetailNodes.Length);

            foreach (GrassDetailNode detailNode in allDetailNodes) {
                prototypes.Add((DetailPrototype)detailNode.GetDetailPrototype());
            }

            int coroutineRes = TerraConfig.Instance.Generator.CoroutineRes;
            int iterations = 0;
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
                    Vector2[] samples = null;
                    if (!TerraConfig.Instance.IsEditor) {
                        bool isDone = false;
                        GrassDetailNode node = grassNode;
                        TerraConfig.Instance.Worker.Enqueue(() => samples = node.SamplePositions(_tile.Random), () => isDone = true);

                        while (!isDone)
                            yield return null;
                    } else {
                        samples = grassNode.SamplePositions(_tile.Random);
                    }

                    foreach (Vector2 sample in samples) {
                        iterations++;

                        if (iterations > coroutineRes) {
                            iterations = 0;
                            yield return null;
                        }

                        int selectedBiome = _sampler.GetBiomeAtInterpolatedCoords(_biomeMap, sample.x, sample.y);
                        if (selectedBiome != i) {
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