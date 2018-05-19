//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   public class BakeSplatDataToVertices : IVertexPainterUtility 
   {
      public string GetName()
      {
         return "MegaSplat : SplatData to Vertex Data";
      }

      public void OnGUI(PaintJob[] jobs)
      {
         EditorGUILayout.HelpBox("Can be used to transfer splat data from terrains, or baked out from meshes using the Render baker", MessageType.Info);
         bakingTex = EditorGUILayout.ObjectField("Texture", bakingTex, typeof(Texture2D), false) as Texture2D;

         bakeSourceUV = (BakeSourceUV)EditorGUILayout.EnumPopup("Source UVs", bakeSourceUV);

         if (bakeSourceUV == BakeSourceUV.WorldSpaceXY || bakeSourceUV == BakeSourceUV.WorldSpaceXZ || bakeSourceUV == BakeSourceUV.WorldSpaceYZ)
         {
            worldSpaceLower = EditorGUILayout.Vector2Field("Lower world position", worldSpaceLower);
            worldSpaceUpper = EditorGUILayout.Vector2Field("Upper world position", worldSpaceUpper);
         }
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Bake"))
         {
            if (bakingTex != null)
            {
               BakeFromTexture(jobs);
            }
            else
            {
               EditorUtility.DisplayDialog("Error", "Baking texture is not set", "ok");
            }
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
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

      Texture2D bakingTex = null;
      BakeSourceUV bakeSourceUV = BakeSourceUV.UV0;

      Vector2 worldSpaceLower = new Vector2(0, 0);
      Vector2 worldSpaceUpper = new Vector2(1, 1);



      void InitBakeChannel(PaintJob[] jobs)
      {
         foreach (PaintJob job in jobs)
         {
            if (job.stream.colors == null || job.stream.colors.Length != job.verts.Length)
            {
               job.stream.colors = job.meshFilter.sharedMesh.colors;
            }
            if (job.stream.uv3 == null || job.stream.uv3.Count != job.verts.Length)
            {
               job.stream.SetUV3(Vector3.zero, job.verts.Length);
            }

            if (job.stream.colors == null || job.stream.colors.Length != job.verts.Length)
            {
               Debug.LogError("Mesh has no color data, and therefor has not been run through the MegaSplat preprocessor. Please preprocess the mesh before attempting the transfer");
            }

            EditorUtility.SetDirty(job.stream);
            EditorUtility.SetDirty(job.stream.gameObject);
         }
      }

      void BakeColor(PaintJob job, Color val, int i)
      {
         Color c = job.stream.colors[i];
         Vector4 uv3 = job.stream.uv3[i];
         c.a = val.r;
         uv3.w = val.g;
         uv3.x = val.b;
         job.stream.colors[i] = c;
         job.stream.uv3[i] = uv3;
      }

      void BakeFromTexture(PaintJob[] jobs)
      {
         // make sure we have the channels we're baking to..
         InitBakeChannel(jobs);
         // lets avoid the whole read/write texture thing, because it's lame to require that..
         int w = bakingTex.width;
         int h = bakingTex.height;
         RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
         Graphics.Blit(bakingTex, rt);
         Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
         RenderTexture.active = rt;
         tex.ReadPixels(new Rect(0,0,w,h), 0, 0);
         foreach (PaintJob job in jobs)
         {
            List<Vector4> srcUV0 = new List<Vector4>();
            List<Vector4> srcUV1 = new List<Vector4>();
            List<Vector4> srcUV2 = new List<Vector4>();
            List<Vector4> srcUV3 = new List<Vector4>();
            job.meshFilter.sharedMesh.GetUVs(0, srcUV0);
            job.meshFilter.sharedMesh.GetUVs(1, srcUV1);
            job.meshFilter.sharedMesh.GetUVs(2, srcUV2);
            job.meshFilter.sharedMesh.GetUVs(3, srcUV3);
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
               Color c = tex.GetPixel((int)(uv.x*w), (int)(uv.y*w));

               BakeColor(job, c, i);

            }
            job.stream.Apply();
         }
      }
   }
}
