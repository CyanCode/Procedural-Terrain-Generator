using System;
using System.Globalization;
using System.IO;
using System.Text;
using Terra.Graph.Biome;
using Terra.Source.Util;
using Terra.Structures;
using Terra.Terrain;
using UnityEditor;
using UnityEngine;

namespace Terra.Source.Editor {
    [CustomEditor(typeof(Tile))]
    public class TileEditor : UnityEditor.Editor {
        private Texture2D _previewTexture;
        private Tile _tile => (Tile) target;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (!TerraConfig.Instance.EditorState.ShowDebugMessages) {
                return;
            }

            _tile.PreviewDeviation = EditorGUILayout.IntField("Deviation", _tile.PreviewDeviation);

            if (_tile.PreviewGradient != null) {
                EditorGUILayout.GradientField("Biome Gradient", _tile.PreviewGradient);
            }

            if (GUILayout.Button("Preview biome")) {
                SetBiomePreviewTexture();
            }

            if (GUILayout.Button("Preview blurred biome map")) {
                SetBlurredBiomePreviewTexture();
            }

            if (GUILayout.Button("Export biome map")) {
                ExportBiomeMap();
            }

            if (GUILayout.Button("Export blurred biome map")) {
                ExportBlurredBiomeMap();
            }

            if (_previewTexture != null) {
                int padding = 4;
                int width = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth) - padding * 2;
                Rect ctrl = EditorGUILayout.GetControlRect(false, width);
                EditorGUI.DrawPreviewTexture(ctrl, _previewTexture);
            }
        }

        private void SetBiomePreviewTexture() {
            Tile tile = (Tile) target;
            int res = tile.MeshManager.HeightmapResolution;
            BiomeNode[] biomes = TerraConfig.Instance.Graph.GetEndNode().GetBiomes();
            BiomeSampler sampler = new BiomeSampler(biomes);

            int[,] map = sampler.GetBiomeMap(TerraConfig.Instance, tile.GridPosition, res);

            //Set texture
            Texture2D tex = new Texture2D(res, res);
            MathUtil.LoopXY(res, (x, y) => { tex.SetPixel(x, y, biomes[map[x, y]].PreviewColor); });

            tex.Apply();
            _previewTexture = tex;
        }

        private void SetBlurredBiomePreviewTexture() {
            Tile tile = (Tile) target;
            int res = tile.MeshManager.HeightmapResolution;
            BiomeNode[] biomes = TerraConfig.Instance.Graph.GetEndNode().GetBiomes();
            BiomeSampler sampler = new BiomeSampler(biomes);

            Texture2D tex;
            if (biomes.Length == 1) {
                tex = new Texture2D(res, res);
                MathUtil.LoopXY(res, (x, y) => {
                    // Set colors based on how much biome is showing
                    tex.SetPixel(x, y, biomes[0].PreviewColor);
                });
                tex.Apply();
                _previewTexture = tex;
                return;
            }

            float[,] blurredMap = GetBlurredBiomeMap(tile.PreviewDeviation);

            _tile.PreviewGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[biomes.Length];
            float stepSize = 1f / (biomes.Length - 1);
            float offset = 0f;
            for (int i = 0; i < biomes.Length; i++) {
                GradientColorKey colorKey = new GradientColorKey {color = biomes[i].PreviewColor, time = offset};
                colorKeys[i] = colorKey;

                offset += stepSize;
            }

            _tile.PreviewGradient.colorKeys = colorKeys;

            //Set texture
            tex = new Texture2D(res, res);
            MathUtil.LoopXY(res, (x, y) => {
                // Set colors based on how much biome is showing
                float normalized = blurredMap[x, y] / biomes.Length;
                tex.SetPixel(x, y, _tile.PreviewGradient.Evaluate(normalized));
            });

            tex.Apply();
            _previewTexture = tex;
        }

        private void ExportBiomeMap() {
            Tile tile = (Tile) target;
            int res = tile.MeshManager.HeightmapResolution;
            int[,] map = GetBiomeMap();

            string tileName = tile.name.Replace(" ", "_");
            WriteMap(map, res, $"{tileName}_biome_map.txt");
        }

        private void ExportBlurredBiomeMap() {
            Tile tile = (Tile) target;
            int res = tile.MeshManager.HeightmapResolution;
            // int[,] map = GetBiomeMap();
            // // float[,] blurred = BlurUtils.BoxBlur(map, 20);
            // float[,] blurred = BlurUtils.GaussianConvolution(map, 4);
            float[,] blurred = GetBlurredBiomeMap(tile.PreviewDeviation);
            
            string tileName = tile.name.Replace(" ", "_");
            WriteMap(blurred, res, $"{tileName}_blurred_biome_map.txt");
        }

        private int[,] GetBiomeMap() {
            Tile tile = (Tile) target;
            int res = tile.MeshManager.HeightmapResolution;
            BiomeNode[] biomes = TerraConfig.Instance.Graph.GetEndNode().GetBiomes();
            BiomeSampler sampler = new BiomeSampler(biomes);

            return sampler.GetBiomeMap(TerraConfig.Instance, tile.GridPosition, res);
        }

        private float[,] GetBlurredBiomeMap(int deviation) {
            Tile tile = (Tile) target;
            int res = tile.MeshManager.HeightmapResolution;
            BiomeNode[] biomes = TerraConfig.Instance.Graph.GetEndNode().GetBiomes();
            BiomeSampler sampler = new BiomeSampler(biomes);

            return sampler.GetGaussianBlurredBiomeMap(TerraConfig.Instance, tile.GridPosition, res, deviation);
        }

        private void WriteMap(int[,] map, int res, string fileName) {
            WriteMapGeneric((x, y) => map[x, y].ToString(), res, fileName);
        }

        private void WriteMap(float[,] map, int res, string fileName) {
            WriteMapGeneric((x, y) => map[x, y].ToString("0.00000"), res, fileName);
        }

        private void WriteMapGeneric(Func<int, int, String> sampler, int res, string fileName) {
            string path = $"Assets/Terra/{fileName}";

            StreamWriter writer = new StreamWriter(path, false);
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < res; x++) {
                for (int y = 0; y < res; y++) {
                    sb.Append(sampler.Invoke(x, y));

                    if (y != res - 1) {
                        sb.Append(" ");
                    }
                }

                if (x != res - 1) {
                    sb.Append("\n");
                }
            }

            writer.Write(sb.ToString());
            writer.Close();
        }
    }
}