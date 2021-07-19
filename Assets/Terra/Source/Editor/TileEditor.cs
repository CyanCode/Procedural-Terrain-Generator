using System.IO;
using System.Text;
using Terra.Graph.Biome;
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
            BiomeNode[] biomes = TerraConfig.Instance.Graph.GetEndNode().GetBiomes();
            BiomeSampler sampler = new BiomeSampler(biomes);

            int[,] map = sampler.GetBiomeMap(TerraConfig.Instance, tile.GridPosition, res);

            string tileName = tile.name.Replace(" ", "_");
            string path = $"Assets/Terra/{tileName}_biome_map.txt";

            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, true);

            StringBuilder sb = new StringBuilder("[");
            for (int x = 0; x < res; x++) {
                sb.Append("[");
                
                for (int y = 0; y < res; y++) {
                    sb.Append(map[x, y]);

                    if (y != res - 1) {
                        sb.Append(", ");
                    }
                }

                sb.Append("]");
                if (x != res - 1) {
                    sb.Append(",\n");
                }
            }

            sb.Append("]");
            
            writer.Write(sb.ToString());
            writer.Close();
        }
    }
}