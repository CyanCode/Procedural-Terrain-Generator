//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.MegaSplat
{
   public static class TextureClusterExtensions
   {
      
      public static void UpdatePreview(this TextureCluster cluster, TextureArrayConfig config)
      {
         TextureArrayConfig.BrushData bd = new TextureArrayConfig.BrushData();
         bd.clusterNoiseScale = 1;
         bd.layerNoise = new NoiseParams();
         bd.layerNoise.amplitude = cluster.noise.amplitude;
         bd.layerNoise.blend = cluster.noise.blend;
         bd.layerNoise.frequency = cluster.noise.frequency;
         bd.bottom = cluster.name;
         bd.brushMode = TextureArrayConfig.BrushData.BrushMode.Cluster;
         bd.layerMode = TextureArrayConfig.BrushData.LayerMode.Bottom;

         var image = MegaSplatUtilities.RenderBrushSpherePreview(config, bd);

         cluster.previewData = image.EncodeToJPG();
         GameObject.DestroyImmediate(image);
      }

      public static bool DrawLayer(this TextureCluster cluster, string label, TextureArrayConfig config, bool drawRollup = true)
      {
         if (cluster.indexes == null)
         {
            cluster.indexes = new List<int>();
         }
         if (cluster.noise == null)
         {
            cluster.noise = new NoiseParams();
         }

         if (cluster.indexes.Count < 1)
         {
            cluster.indexes.Add(0);
         }

         EditorGUI.BeginChangeCheck();
         if (drawRollup == false || MegaSplatUtilities.DrawRollup(label, false, true))
         {

            var oldName = cluster.name;
            cluster.name = EditorGUILayout.TextField("Name", cluster.name);
            if (oldName != cluster.name)
            {
               // draw it again to add the new name to the rollup table..
               MegaSplatUtilities.DrawRollup(cluster.name, true, true);
            }
            if (config != null && config.sourceTextures != null && config.sourceTextures.Count > 0)
            {
               if (GUILayout.Button("Add Texture"))
               {
                  cluster.indexes.Add(0);
               }

               for (int i = 0; i < cluster.indexes.Count; ++i)
               {
                  if (cluster.indexes[i] < 0)
                  {
                     cluster.indexes[i] = 0;
                  }
                  else if (cluster.indexes[i] >= config.sourceTextures.Count)
                  {
                     cluster.indexes[i] = cluster.indexes.Count - 1;
                  }
                  if (config.sourceTextures[cluster.indexes[i]].diffuse != null)
                  {
                     EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128)),
                        config.sourceTextures[cluster.indexes[i]].diffuse);
                  }
                  EditorGUILayout.BeginHorizontal();
                  if (GUILayout.Button("Remove", GUILayout.Width(30)))
                  {
                     cluster.indexes.RemoveAt(i);
                     i--;
                     continue;
                  }
                  cluster.indexes[i] = EditorGUILayout.IntSlider(cluster.indexes[i], 0, config.sourceTextures.Count - 1);
                  EditorGUILayout.EndHorizontal();
                  EditorGUILayout.Separator();
               }
            }
            cluster.mode = (TextureCluster.Mode)EditorGUILayout.EnumPopup("Mode", cluster.mode);
            if (cluster.mode == TextureCluster.Mode.Noise)
            {
               cluster.noise.DrawGUI();
            }
            if (cluster.mode == TextureCluster.Mode.Angle)
            {
               EditorGUILayout.HelpBox("Vertex Normal Angle Mapping\n  0 = down\n  0.5 == vertical\n  1 == up", MessageType.Info);
               cluster.angleCurve = EditorGUILayout.CurveField("Angle", cluster.angleCurve);
            }
            if (cluster.mode == TextureCluster.Mode.Height)
            {
               EditorGUILayout.HelpBox("Vertex Height Mapping\n  0 = bottom\n  1 = top", MessageType.Info);
               cluster.heightCurve = EditorGUILayout.CurveField("Height", cluster.heightCurve);
            }

            if (EditorGUI.EndChangeCheck() || cluster.previewTex == null)
            {
               if (cluster.previewTex != null)
               {
                  GameObject.DestroyImmediate(cluster.previewTex);
                  cluster.previewTex = null;
               }
               string path = AssetDatabase.GetAssetPath(config);
               path = path.Replace(".asset", "_tarray.asset");

               UpdatePreview(cluster, config);

            }

            if (cluster.previewTex == null)
            {
               cluster.previewTex = new Texture2D(128, 128);
               cluster.previewTex.LoadImage(cluster.previewData);
               cluster.previewTex.Apply();
            }
            if (cluster.previewTex != null)
            {
               EditorGUILayout.LabelField("Preview");
               EditorGUILayout.BeginHorizontal();
               EditorGUILayout.Space();
               EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128)), cluster.previewTex);
               EditorGUILayout.Space();
               EditorGUILayout.EndHorizontal();
            }
            return true;
         }
         EditorGUI.EndChangeCheck();
         return false;


      }

   }
}
