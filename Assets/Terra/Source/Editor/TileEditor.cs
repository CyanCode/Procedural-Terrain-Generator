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
        
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            
            if (GUILayout.Button("Preview biome")) {
                SetPreviewTexture();
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

        private void SetPreviewTexture() {
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
            int[,] map = GetBiomeMap();
            // float[,] blurred = BlurUtils.BoxBlur(map, 20);
            float[,] blurred = BlurUtils.GaussianConvolution(map, 4);
         
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

        private void WriteMap(int[,] map, int res, string fileName) {
            WriteMapGeneric((x,y) => map[x,y].ToString(), res, fileName);
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