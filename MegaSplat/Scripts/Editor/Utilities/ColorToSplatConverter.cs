//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using JBooth.VertexPainterPro;
using UnityEditor;
using System.Collections.Generic;
using JBooth.TerrainPainter;

namespace JBooth.MegaSplat
{
   public class ColorToSplatConverter : IVertexPainterUtility, ITerrainPainterUtility
   {
      Texture2D bakingTex = null;
      BakeSourceUV bakeSourceUV = BakeSourceUV.UV0;
      Vector2 worldSpaceLower = new Vector2(0, 0);
      Vector2 worldSpaceUpper = new Vector2(1, 1);

      ColorToSplatConfig colorToSplatConfig;


      public string GetName()
      {
         return "MegaSplat : Splat from Color Map";
      }

      public void OnGUI(TerrainJob[] jobs)
      {
         bakingTex = EditorGUILayout.ObjectField("Macro Texture", bakingTex, typeof(Texture2D), false) as Texture2D;
         colorToSplatConfig = EditorGUILayout.ObjectField("Color To Splat", colorToSplatConfig, typeof(ColorToSplatConfig), false) as ColorToSplatConfig;
         EditorGUILayout.Separator();


         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Bake from Image"))
         {
            if (bakingTex != null && colorToSplatConfig != null)
            {
               BakeFromTexture(jobs);
            }
            else
            {
               EditorUtility.DisplayDialog("Error", "Baking texture or Color->Splat config is not set", "ok");
            }
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();

         EditorGUILayout.Separator();

      }

      public void OnGUI(PaintJob[] jobs)
      {
         bakingTex = EditorGUILayout.ObjectField("Macro Texture", bakingTex, typeof(Texture2D), false) as Texture2D;
         colorToSplatConfig = EditorGUILayout.ObjectField("Color To Splat", colorToSplatConfig, typeof(ColorToSplatConfig), false) as ColorToSplatConfig;
         bakeSourceUV = (BakeSourceUV)EditorGUILayout.EnumPopup("Source UVs", bakeSourceUV);
         if (bakeSourceUV == BakeSourceUV.WorldSpaceXY || bakeSourceUV == BakeSourceUV.WorldSpaceXZ || bakeSourceUV == BakeSourceUV.WorldSpaceYZ)
         {
            worldSpaceLower = EditorGUILayout.Vector2Field("Lower world position", worldSpaceLower);
            worldSpaceUpper = EditorGUILayout.Vector2Field("Upper world position", worldSpaceUpper);
         }

         EditorGUILayout.Separator();


         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Bake"))
         {
            if (bakingTex != null && colorToSplatConfig != null)
            {
               BakeFromTexture(jobs);
            }
            else
            {
               EditorUtility.DisplayDialog("Error", "Baking texture or Color->Splat config is not set", "ok");
            }
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();

         EditorGUILayout.Separator();

         // draw mappings ui
      }

      public enum BakeSourceUV
      {
         UV0,
         UV1,
         UV2,
         UV3,
         WorldSpaceXY,
         WorldSpaceXZ,
         WorldSpaceYZ
      }
         
      void BakeFromTexture(PaintJob[] jobs)
      {
         VertexPainterWindow vpw = VertexPainterWindow.GetWindow<VertexPainterWindow>();

         // lets avoid the whole read/write texture thing, because it's lame to require that..
         var tex = new Texture2D(bakingTex.width, bakingTex.height, bakingTex.format, bakingTex.mipmapCount > 1);
         Graphics.CopyTexture(bakingTex, tex);

         int jobIdx = 0;
         foreach (PaintJob job in jobs)
         {
            vpw.InitColors(job);
            vpw.InitUV3(job);
            EditorUtility.DisplayProgressBar("Baking meshes", "", (float)jobIdx / (float)jobs.Length);
            List<Vector4> srcUV0 = new List<Vector4>();
            List<Vector4> srcUV1 = new List<Vector4>();
            List<Vector4> srcUV2 = new List<Vector4>();
            List<Vector4> srcUV3 = new List<Vector4>();
            job.meshFilter.sharedMesh.GetUVs(0, srcUV0);
            job.meshFilter.sharedMesh.GetUVs(1, srcUV1);
            job.meshFilter.sharedMesh.GetUVs(2, srcUV2);
            job.meshFilter.sharedMesh.GetUVs(3, srcUV3);

            var lerp = colorToSplatConfig.textureConfig.GetLerper();

            for (int i = 0; i < job.verts.Length; ++i)
            {
               Vector4 uv = Vector4.zero;

               switch (bakeSourceUV)
               {
                  case BakeSourceUV.UV0:
                     {
                        if (job.stream.uv0 != null && job.stream.uv0.Count == job.verts.Length)
                           uv = job.stream.uv0[i];
                        else if (srcUV0 != null && srcUV0.Count == job.verts.Length)
                           uv = srcUV0[i];
                        break;
                     }
                  case BakeSourceUV.UV1:
                     {
                        if (job.stream.uv1 != null && job.stream.uv1.Count == job.verts.Length)
                           uv = job.stream.uv1[i];
                        else if (srcUV1 != null && srcUV1.Count == job.verts.Length)
                           uv = srcUV1[i];
                        break;
                     }
                  case BakeSourceUV.UV2:
                     {
                        if (job.stream.uv2 != null && job.stream.uv2.Count == job.verts.Length)
                           uv = job.stream.uv2[i];
                        else if (srcUV2 != null && srcUV2.Count == job.verts.Length)
                           uv = srcUV2[i];
                        break;
                     }
                  case BakeSourceUV.UV3:
                     {
                        if (job.stream.uv3 != null && job.stream.uv3.Count == job.verts.Length)
                           uv = job.stream.uv3[i];
                        else if (srcUV3 != null && srcUV3.Count == job.verts.Length)
                           uv = srcUV3[i];
                        break;
                     }
                  case BakeSourceUV.WorldSpaceXY:
                     {
                        Vector3 pos = job.stream.transform.localToWorldMatrix.MultiplyPoint(job.GetPosition(i));
                        Vector2 p = new Vector2(pos.x, pos.y) - worldSpaceLower;
                        Vector2 scale = worldSpaceUpper - worldSpaceLower;
                        scale.x = Mathf.Max(0.000001f, scale.x);
                        scale.y = Mathf.Max(0.000001f, scale.y);
                        uv = p;
                        uv.x /= scale.x;
                        uv.y /= scale.y;
                        break;
                     }
                  case BakeSourceUV.WorldSpaceXZ:
                     {
                        Vector3 pos = job.stream.transform.localToWorldMatrix.MultiplyPoint(job.GetPosition(i));
                        Vector2 p = new Vector2(pos.x, pos.z) - worldSpaceLower;
                        Vector2 scale = worldSpaceUpper - worldSpaceLower;
                        scale.x = Mathf.Max(0.000001f, scale.x);
                        scale.y = Mathf.Max(0.000001f, scale.y);
                        uv = p;
                        uv.x /= scale.x;
                        uv.y /= scale.y;
                        break;
                     }
                  case BakeSourceUV.WorldSpaceYZ:
                     {
                        Vector3 pos = job.stream.transform.localToWorldMatrix.MultiplyPoint(job.GetPosition(i));
                        Vector2 p = new Vector2(pos.y, pos.z) - worldSpaceLower;
                        Vector2 scale = worldSpaceUpper - worldSpaceLower;
                        scale.x = Mathf.Max(0.000001f, scale.x);
                        scale.y = Mathf.Max(0.000001f, scale.y);
                        uv = p;
                        uv.x /= scale.x;
                        uv.y /= scale.y;
                        break;
                     }
               }
               // find best choice in mapping

               Color c = tex.GetPixel((int)(uv.x*bakingTex.width), (int)(uv.y*bakingTex.height));

               if (colorToSplatConfig.mode == ColorToSplatConfig.MatchMode.SplatTexture)
               {
                  TextureArrayConfig.BrushData primary;
                  TextureArrayConfig.BrushData secondary;
                  float blend;
                  colorToSplatConfig.GetSplatBrushes(c, out primary, out secondary, out blend);

                  object o = primary as object;
                  lerp.Invoke(job, i, ref o, blend);
                  o = secondary as object;
                  lerp.Invoke(job, i, ref o, 1 - blend);
               }
               else
               {
                  TextureArrayConfig.BrushData brushData = colorToSplatConfig.GetBrush(c);
                  if (brushData != null)
                  {
                     object o = brushData as object;
                     lerp.Invoke(job, i, ref o, 1);
                  }
               }
            }
            job.stream.Apply();
            EditorUtility.ClearProgressBar();
         }
      }

      void BakeFromTexture(TerrainJob[] jobs)
      {
         // lets avoid the whole read/write texture thing, because it's lame to require that..
         var tex = new Texture2D(bakingTex.width, bakingTex.height, bakingTex.format, true);
         Graphics.CopyTexture(bakingTex, tex);

         int jobIdx = 0;
         foreach (TerrainJob job in jobs)
         {
            EditorUtility.DisplayProgressBar("Baking terrains", "", (float)jobIdx / (float)jobs.Length);
            int width = job.terrainTex.width;
            int height = job.terrainTex.height;
            for (int x = 0; x < width; ++x)
            {
               for (int y = 0; y < height; ++y)
               {
                  Color c = tex.GetPixel(width-x-1, height-y-1);
                  float h = job.terrain.terrainData.GetHeight(x, y);
                  Vector3 n = job.terrain.terrainData.GetInterpolatedNormal(x, y);
                  Vector3 worldPos = MegaSplatUtilities.TerrainToWorld(job.terrain, x, y, tex);

                  if (colorToSplatConfig.mode == ColorToSplatConfig.MatchMode.SplatTexture)
                  {
                     TextureArrayConfig.BrushData primary;
                     TextureArrayConfig.BrushData secondary;
                     float blend;
                     colorToSplatConfig.GetSplatBrushes(c, out primary, out secondary, out blend);

                     primary.brushMode = TextureArrayConfig.BrushData.BrushMode.Cluster;
                     if (primary.layerMode == TextureArrayConfig.BrushData.LayerMode.Top)
                     {
                        primary.layerMode = TextureArrayConfig.BrushData.LayerMode.Bottom;
                     }
                     Color val = colorToSplatConfig.textureConfig.GetValues(job.terrain, 
                        job.terrainTex.GetPixel(x, y), primary, worldPos, h, n, 1.0f); 
                     
                     if (secondary.layerMode == TextureArrayConfig.BrushData.LayerMode.Bottom)
                     {
                        secondary.layerMode = TextureArrayConfig.BrushData.LayerMode.Top;
                     }

                     Color val2 = colorToSplatConfig.textureConfig.GetValues(job.terrain, 
                        job.terrainTex.GetPixel(x, y), secondary, worldPos, h, n, 1.0f);

                     // take most weighted

                     Color final = new Color(val.r, val2.r, blend);

                     job.terrainTex.SetPixel(x, y, final);

                  }
                  else
                  {
                     TextureArrayConfig.BrushData brushData = colorToSplatConfig.GetBrush(c);
                     if (brushData != null)
                     {
                        Color val = colorToSplatConfig.textureConfig.GetValues(job.terrain, 
                           job.terrainTex.GetPixel(x, y), brushData, worldPos, h, n, 1.0f); 

                        job.terrainTex.SetPixel(x, y, val);
                     }
                  }

               }
            }
            job.terrainTex.Apply();

            EditorUtility.ClearProgressBar();
         }
      }
   }
}
