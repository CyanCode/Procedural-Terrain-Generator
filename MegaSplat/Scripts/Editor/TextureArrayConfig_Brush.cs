//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor; 
using JBooth.VertexPainterPro;
using System.Collections.Generic;


namespace JBooth.MegaSplat
{
   public partial class TextureArrayConfig : VertexPainterCustomBrush 
   {
      
      // I like a little object to hold the brush settings..
      [System.Serializable]
      public class BrushData
      {

         public enum LayerMode
         {
            Auto,
            Top,
            Bottom
         }
         public enum BrushMode
         {
            Single,
            Cluster,
            Blended
         }

         public LayerMode layerMode = LayerMode.Bottom;
         public BrushMode brushMode = BrushMode.Cluster;
         static string[] modeNames = new string[] { "Single", "Cluster", "Blended" };
         [HideInInspector]
         public float clusterNoiseScale = 0.05f;
         public int singleTop;
         public int singleBottom;
         public string multiSelection;
         public string top;
         public string bottom;
         public NoiseParams layerNoise;
         public byte[] multiPreview;
         Texture2D previewTex;
         public static float brushWeightTarget = 1;

         // draw any custom GUI we want for this brush in the editor
         int DrawSingle(TextureArrayConfig config, BrushData bd, int index)
         {
            
            if (config.sourceTextures != null)
            {
               int numPerWidth = (int)EditorGUIUtility.currentViewWidth / 129;
               if (numPerWidth < 1)
                  numPerWidth = 1;
               
               index = MegaSplatUtilities.SelectionGrid(index,
                  config.sourceTextures, 
                  numPerWidth);

            }
            index = Mathf.Clamp(index, 0, config.sourceTextures.Count - 1);
            return index;
         }

         void UpdateMultiLayerPreview(TextureArrayConfig config, BrushData bd)
         {
            var image = MegaSplatUtilities.RenderBrushSpherePreview(config, bd, 256);
            this.multiPreview = image.EncodeToJPG();
            DestroyImmediate(image);
         }

         Vector2 scroll;
         public void DrawGUI(TextureArrayConfig config)
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (config.sourceTextures != null && config.sourceTextures.Count > 0)
            {
               EditorGUI.BeginChangeCheck();

               brushMode = (BrushMode)GUILayout.Toolbar((int)brushMode, modeNames);
               if (brushMode != BrushMode.Blended)
               {
                  layerMode = (LayerMode)EditorGUILayout.EnumPopup("layer", layerMode);
               }

               if (layerMode != LayerMode.Auto)
               {
                  BrushData.brushWeightTarget = EditorGUILayout.Slider("Blend Target", BrushData.brushWeightTarget, 0.0f, 1.0f);
               }
    

               Event e = Event.current; // Grab the current event
               if (e.isKey && e.type == EventType.keyDown)
               {         
                  if (e.keyCode == KeyCode.M)
                  {
                     if (brushMode != BrushMode.Blended)
                     {
                        brushMode = (BrushMode)((int)brushMode + 1);
                     }
                     else
                     {
                        brushMode = BrushMode.Single;
                     }
                     e.Use();
                  }
                  if (e.keyCode == KeyCode.L)
                  {
                     if (layerMode != LayerMode.Top)
                     {
                        layerMode = (LayerMode)((int)layerMode + 1);
                     }
                     else
                     {
                        layerMode = LayerMode.Auto;
                     }
                     e.Use();
                  }
               }
                  
               if (brushMode == BrushMode.Single)
               {
                  if (layerMode == LayerMode.Auto || layerMode == LayerMode.Bottom)
                  {
                     singleBottom = DrawSingle(config, this, singleBottom);
                  }
                  else
                  {
                     singleTop = DrawSingle(config, this, singleTop);
                  }
               }
               else if (brushMode == BrushMode.Cluster)
               {
                  clusterNoiseScale = EditorGUILayout.FloatField("Cluster Noise Scale", clusterNoiseScale);
                  if (layerMode == LayerMode.Bottom)
                  {
                     multiSelection = config.DrawTextureClusterSelection(multiSelection, "Bottom Layer");
                  }
                  else if (layerMode == LayerMode.Top)
                  {
                     multiSelection = config.DrawTextureClusterSelection(multiSelection, "Top Layer");
                  }
                  else
                  {
                     multiSelection = config.DrawTextureClusterSelection(multiSelection, "Auto Layer");
                  }
               }
               else if (brushMode == BrushMode.Blended)
               {
                  clusterNoiseScale = EditorGUILayout.FloatField("Cluster Noise Scale", clusterNoiseScale);
                  if (layerNoise == null)
                  {
                     layerNoise = new NoiseParams();
                  }

                  layerNoise.DrawGUI();


                  top = config.DrawTextureClusterSelection(top, "Top Layer", false);
                  bottom = config.DrawTextureClusterSelection(bottom, "Bottom Layer", false);
               }
               if (EditorGUI.EndChangeCheck() || multiPreview == null)
               {
                  UpdateMultiLayerPreview(config, this);
                  if (previewTex)
                  {
                     DestroyImmediate(previewTex);
                     previewTex = null;
                  }
               }

               if (multiPreview != null && brushMode == BrushMode.Blended)
               {
                  if (previewTex == null)
                  {
                     previewTex = new Texture2D(256, 256);
                     previewTex.LoadImage(multiPreview);
                     previewTex.Apply();
                  }
                  EditorGUILayout.LabelField("Preview Texture");
                  EditorGUILayout.BeginHorizontal();
                  EditorGUILayout.Space();
                  EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(256), GUILayout.Height(256)), previewTex);
                  EditorGUILayout.Space();
                  EditorGUILayout.EndHorizontal();
               }


            }
            EditorGUILayout.EndScrollView();
         }
      }

      [HideInInspector]
      public BrushData brushData = new BrushData();

      public override Channels GetChannels()
      {
         return Channels.Colors | Channels.UV3;
      }

      public override Color GetPreviewColor()
      {
         return Color.yellow;
      }

      public override object GetBrushObject()
      {
         return brushData;
      }


      public override void DrawGUI()
      {
         brushData.DrawGUI(this);
      }
         



      // TODO: Merge with lerper..
      public Color GetValues(Terrain t, Color curVal, BrushData bd, Vector3 worldPos, float height, Vector3 normal, float r)
      {
         Color o = curVal;

         int bottomIndex = bd.singleBottom;
         int topIndex = bd.singleTop;

         TextureCluster bottomCluster = null;
         TextureCluster topCluster = null;

         if (bd.brushMode == BrushData.BrushMode.Single)
         {
            if (bd.layerMode == BrushData.LayerMode.Bottom)
            {
               o.r = (float)bottomIndex / 255.0f;
               o.b = Mathf.Lerp(curVal.b, 1.0f - BrushData.brushWeightTarget, r);
            }
            else if (bd.layerMode == BrushData.LayerMode.Top)
            {
               o.g = (float)topIndex / 255.0f;
               o.b = Mathf.Lerp(curVal.b, BrushData.brushWeightTarget, r);  
            }
            else if (bd.layerMode == BrushData.LayerMode.Auto)
            {
               o = TextureCluster.AutoColor(o, bottomIndex, r, null);
            }
            return o;

         }
         else 
         {
            // chose a texture based on noise
            if (bd.brushMode == BrushData.BrushMode.Blended)
            {
               bottomCluster = FindInLibrary(bd.bottom);
               topCluster = FindInLibrary(bd.top);
            }
            else if (bd.layerMode == BrushData.LayerMode.Bottom || bd.layerMode == BrushData.LayerMode.Auto)
            {
               bottomCluster = FindInLibrary(bd.multiSelection);
            }
            else if (bd.layerMode == BrushData.LayerMode.Top)
            {
               topCluster = FindInLibrary(bd.multiSelection);
            }
            else
            {
               bottomCluster = FindInLibrary(bd.bottom);
               topCluster = FindInLibrary(bd.top);
            }

            int bottomRange = 0;
            int topRange = 0;
            int bottomII = 0;
            int topII = 0;
            if (bottomCluster != null && bottomCluster.mode == TextureCluster.Mode.Noise)
            {
               bottomRange = bottomCluster.indexes.Count;
               float n = bottomCluster.noise.GetNoise(worldPos * bd.clusterNoiseScale);
               bottomII = (int)(n * bottomRange);
               if (bottomII >= bottomRange-1)
                  bottomII = bottomRange-1;
               bottomIndex = bottomCluster.indexes[bottomII];

            }
            if (topCluster != null)
            {
               topRange = topCluster.indexes.Count;
               float n = topCluster.noise.GetNoise(worldPos * bd.clusterNoiseScale);
               topII = (int)(n * topRange);
               if (topII >= topRange-1)
                  topII = topRange-1;
               topIndex = topCluster.indexes[topII];
            }

            if (bottomCluster != null && bottomCluster.mode == TextureCluster.Mode.Angle)
            {
               int chosen = bottomCluster.GetIndexForAngle(normal);
               bottomIndex = bottomCluster.indexes[chosen];
            }

            if (topCluster != null && topCluster.mode == TextureCluster.Mode.Angle)
            {
               int chosen = topCluster.GetIndexForAngle(normal);
               topIndex = topCluster.indexes[chosen];
            }

            if (bottomCluster != null && bottomCluster.mode == TextureCluster.Mode.Height)
            {
               float curH = 1.0f - Mathf.Clamp01(height / (t.terrainData.heightmapHeight * t.terrainData.heightmapScale.y));
               int chosen = (int)(bottomCluster.heightCurve.Evaluate(curH) * bottomCluster.indexes.Count);
               if (chosen >= bottomCluster.indexes.Count)
                  chosen = bottomCluster.indexes.Count - 1;
               bottomIndex = bottomCluster.indexes[chosen];
            }

            if (topCluster != null && topCluster.mode == TextureCluster.Mode.Height)
            {
               float curH = 1.0f - Mathf.Clamp01(height / (t.terrainData.heightmapHeight * t.terrainData.heightmapScale.y));
               int chosen = (int)(topCluster.heightCurve.Evaluate(curH) * topCluster.indexes.Count);
               if (chosen >= topCluster.indexes.Count)
                  chosen = topCluster.indexes.Count - 1;
               topIndex = topCluster.indexes[chosen];
            }
         }

         if (bd.brushMode == BrushData.BrushMode.Blended)
         {
            float target = bd.layerNoise.GetNoise(worldPos);
            o.r = (float)bottomIndex / 255.0f;
            o.b = Mathf.Lerp(curVal.b, target, r);
            o.g = (float)topIndex / 255.0f;
         }
         else if (bd.layerMode == BrushData.LayerMode.Bottom)
         {
            o.r = (float)bottomIndex / 255.0f;
            o.b = Mathf.Lerp(curVal.b, 1.0f - BrushData.brushWeightTarget, r);
         }
         else if (bd.layerMode == BrushData.LayerMode.Top)
         {
            o.g = (float)topIndex / 255.0f;
            o.b = Mathf.Lerp(curVal.b, BrushData.brushWeightTarget, r); 
         }
         else if (bd.layerMode == BrushData.LayerMode.Auto)
         {
            o = TextureCluster.AutoColor(o, bottomIndex, r, bottomCluster);
         }
         return o;
      }


      void LerpFunc(PaintJob j, int idx, ref object val, float r)
      {
         TextureCluster bottomCluster = null;
         TextureCluster topCluster = null;
         // retrieve our brush data and get the stream we're painting into
         BrushData bd = val as BrushData;
         var s = j.stream;
         Color c = s.colors[idx];
         Vector4 uv3 = s.uv3[idx];

         int bottomIndex = bd.singleBottom;
         int topIndex = bd.singleTop;
         Vector3 pos = j.GetPosition(idx);

         if (bd.brushMode == BrushData.BrushMode.Single)
         {
            if (bd.layerMode == BrushData.LayerMode.Top)
            {
               uv3.w = (float)topIndex / 255.0f;
               uv3.x = Mathf.Lerp(uv3.x, BrushData.brushWeightTarget, r);
            }
            else if (bd.layerMode == BrushData.LayerMode.Bottom)
            {
               c.a = (float)bottomIndex / 255.0f;
               uv3.x = Mathf.Lerp(uv3.x, 1.0f - BrushData.brushWeightTarget, r);
            }
            else if (bd.layerMode == BrushData.LayerMode.Auto)
            {
               Color o = new Color(c.a, uv3.w, uv3.x);
               o = TextureCluster.AutoColor(o, bottomIndex, r, bottomCluster);
               c.a = o.r;
               uv3.w = o.g;
               uv3.x = o.b;
            }
            s.uv3[idx] = uv3;
            s.colors[idx] = c;
            return;
         }
         else
         {
            if (bd.brushMode == BrushData.BrushMode.Blended)
            {
               bottomCluster = FindInLibrary(bd.bottom);
               topCluster = FindInLibrary(bd.top);
            }
            else if (bd.layerMode == BrushData.LayerMode.Bottom || bd.layerMode == BrushData.LayerMode.Auto)
            {
               bottomCluster = FindInLibrary(bd.multiSelection);
            }
            else if (bd.layerMode == BrushData.LayerMode.Top)
            {
               topCluster = FindInLibrary(bd.multiSelection);
            }
            else
            {
               bottomCluster = FindInLibrary(bd.bottom);
               topCluster = FindInLibrary(bd.top);
            }

            int bottomRange = 0;
            int topRange = 0;
            int bottomII = 0;
            int topII = 0;
            if (bottomCluster != null && bottomCluster.mode == TextureCluster.Mode.Noise)
            {
               bottomRange = bottomCluster.indexes.Count;
               float n = bottomCluster.noise.GetNoise(pos * bd.clusterNoiseScale);
               bottomII = (int)(n * bottomRange);
               if (bottomII >= bottomRange-1)
                  bottomII = bottomRange-1;
               bottomIndex = bottomCluster.indexes[bottomII];

            }
            if (topCluster != null)
            {
               topRange = topCluster.indexes.Count;
               float n = topCluster.noise.GetNoise(pos * bd.clusterNoiseScale);
               topII = (int)(n * topRange);
               if (topII >= topRange-1)
                  topII = topRange-1;
               topIndex = topCluster.indexes[topII];
            }

            if (bottomCluster != null && bottomCluster.mode == TextureCluster.Mode.Angle)
            {
               int chosen = bottomCluster.GetIndexForAngle(j.GetNormal(idx).normalized);
               bottomIndex = bottomCluster.indexes[chosen];
            }

            if (topCluster != null && topCluster.mode == TextureCluster.Mode.Angle)
            {
               int chosen = topCluster.GetIndexForAngle(j.GetNormal(idx).normalized);
               topIndex = topCluster.indexes[chosen];
            }

            if (bottomCluster != null && bottomCluster.mode == TextureCluster.Mode.Height)
            {
               Vector3 scale = j.meshFilter.transform.lossyScale;
               pos.y *= scale.y; // scale so it matches the bounds
               Bounds b = j.meshFilter.GetComponent<Renderer>().bounds;

               pos.y += b.center.y*0.5f;
               float curH = 1.0f - Mathf.Clamp01(pos.y / b.size.y);
               int chosen = (int)(bottomCluster.heightCurve.Evaluate(curH) * bottomCluster.indexes.Count);
               if (chosen >= bottomCluster.indexes.Count)
                  chosen = bottomCluster.indexes.Count - 1;
               bottomIndex = bottomCluster.indexes[chosen];
            }

            if (topCluster != null && topCluster.mode == TextureCluster.Mode.Height)
            {
               Vector3 scale = j.meshFilter.transform.lossyScale;
               pos.y *= scale.y; // scale so it matches the bounds
               Bounds b = j.meshFilter.GetComponent<Renderer>().bounds;

               pos.y += b.center.y*0.5f;
               float curH = 1.0f - Mathf.Clamp01(pos.y / b.size.y);
               int chosen = (int)(topCluster.heightCurve.Evaluate(curH) * topCluster.indexes.Count);
               if (chosen >= topCluster.indexes.Count)
                  chosen = topCluster.indexes.Count - 1;
               topIndex = topCluster.indexes[chosen];
            }
         }

         if (bd.brushMode == BrushData.BrushMode.Blended)
         {
            c.a = (float)bottomIndex / 255.0f;
            uv3.w = (float)topIndex / 255.0f;
            float target = 1.0f - bd.layerNoise.GetNoise(pos);
            uv3.x = Mathf.Lerp(uv3.x, target, r);
         }
         else if (bd.layerMode == BrushData.LayerMode.Bottom)
         {
            c.a = (float)bottomIndex / 255.0f;
            uv3.x = Mathf.Lerp(uv3.x, 1.0f - BrushData.brushWeightTarget, r);
         }
         else if (bd.layerMode == BrushData.LayerMode.Top)
         {
            uv3.w = (float)topIndex / 255.0f;
            uv3.x = Mathf.Lerp(uv3.x, BrushData.brushWeightTarget, r);
         }
         else if (bd.layerMode == BrushData.LayerMode.Auto)
         {
            Color o = new Color(c.a, uv3.w, uv3.x);
            o = TextureCluster.AutoColor(o, bottomIndex, r, bottomCluster);
            c.a = o.r;
            uv3.w = o.g;
            uv3.x = o.b;
         }


         s.uv3[idx] = uv3;
         s.colors[idx] = c;
      }

      public override VertexPainterWindow.Lerper GetLerper()
      {
         return LerpFunc;
      }

   }

}