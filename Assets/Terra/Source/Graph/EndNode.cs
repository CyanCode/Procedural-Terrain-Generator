using System.Collections.Generic;
using Terra.CoherentNoise;
using Terra.Graph.Generators;
using Terra.Graph.Biome;
using Terra.Terrain;
using UnityEngine;
using Terra.Structures;
using UnityEditorInternal;

namespace Terra.Graph {
    public enum BlendStrategy {
        ORDERED,
        RANDOM
    }

    [CreateNodeMenu("End Generator")]
    public class EndNode : PreviewableNode {
        [Input(ShowBackingValue.Never)] public BiomeNode Biomes;

        public BlendStrategy BlendStrategy;

        public int[] BiomeOrder = new int[0];

        [Input(ShowBackingValue.Never, ConnectionType.Override)]
        public AbsGeneratorNode HeightMap;

        [Input(ShowBackingValue.Never, ConnectionType.Override)]
        public AbsGeneratorNode TemperatureMap;

        [Input(ShowBackingValue.Never, ConnectionType.Override)]
        public AbsGeneratorNode MoistureMap;

        private GeneratorSampler _heightMapSampler;
        private GeneratorSampler _temperatureMapSampler;
        private GeneratorSampler _moistureMapSampler;

        public Generator GetHeightMap() {
            return GetGeneratorFromNode("HeightMap");
        }

        public Generator GetTemperatureMap() {
            return GetGeneratorFromNode("TemperatureMap");
        }

        public Generator GetMoistureMap() {
            return GetGeneratorFromNode("MoistureMap");
        }

        public BiomeNode[] GetBiomes() {
            BiomeNode[] biomes = GetInputValues<BiomeNode>("Biomes");
            if (BlendStrategy != BlendStrategy.ORDERED || BiomeOrder.Length != biomes.Length) 
                return biomes;

            BiomeNode[] sorted = new BiomeNode[biomes.Length];
            foreach (int i in BiomeOrder) {
                sorted[i] = biomes[BiomeOrder[i]];
            }

            return sorted;
        }

        public GeneratorSampler GetHeightmapSampler() {
            return _heightMapSampler ??= new GeneratorSampler(GetHeightMap());
        }

        public GeneratorSampler GetTemperatureSampler() {
            return _temperatureMapSampler ??= new GeneratorSampler(GetTemperatureMap());
        }

        public GeneratorSampler GetMoistureMapSampler() {
            return _moistureMapSampler ??= new GeneratorSampler(GetMoistureMap());
        }

        private Generator GetGeneratorFromNode(string name) {
            AbsGeneratorNode iv = GetInputValue<AbsGeneratorNode>(name);
            return iv == null ? null : iv.GetGenerator();
        }

        public override Texture2D DidRequestTextureUpdate(int size, float spread) {
            Texture2D tex = new Texture2D(size, size);
            BiomeNode[] biomes = GetBiomes();
            BiomeSampler sampler = new BiomeSampler(biomes);

            int[,] map = sampler.GetBiomeMap(GridPosition.Zero, 1, spread, size);

            //Set texture
            MathUtil.LoopXy(size, (x, y) => { tex.SetPixel(x, y, biomes[map[x, y]].PreviewColor); });

            tex.Apply();
            return tex;
        }
    }
}