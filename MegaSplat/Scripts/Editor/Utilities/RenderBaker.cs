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
   public class RenderBaker : IVertexPainterUtility, ITerrainPainterUtility
   {
      public string GetName()
      {
         return "MegaSplat : Render Baking";
      }

      public enum Resolutions
      {
         k256 = 256,
         k512 = 512,
         k1024 = 1024,
         k2048 = 2048, 
         k4096 = 4096, 
         k8192 = 8192
      };

      public enum Passes
      {
         Albedo = 1,
         Height = 2,
         Normal = 4,
         Metallic = 8,
         Smoothness = 16,
         AO = 32,
         Emissive = 64,
         SplatData = 128,
      };

      public Passes passes = 0;
      public Resolutions res = Resolutions.k1024;

      public string baseDir;

      void SharedGUI()
      {
         EditorGUILayout.BeginHorizontal();
         baseDir = EditorGUILayout.TextField("Output Path", baseDir);
         if (GUILayout.Button("!", GUILayout.Width(35)))
         {
            baseDir = EditorUtility.OpenFolderPanel("Folder to export files to", baseDir, baseDir);
         }
         EditorGUILayout.EndHorizontal();
         res = (Resolutions)EditorGUILayout.EnumPopup(new GUIContent("Resolution"), res);
         #if UNITY_2017_3_OR_NEWER
         passes = (Passes)EditorGUILayout.EnumFlagsField(new GUIContent("Features"), passes);
         #else
         passes = (Passes)EditorGUILayout.EnumMaskPopup(new GUIContent("Features"), passes);
         #endif


      }

      public void OnGUI(TerrainJob[] jobs)
      {
         if (needsBake && Event.current.type == EventType.Repaint)
         {
            needsBake = false;
            Bake(jobs);
         }
         SharedGUI();
         if (GUILayout.Button("Export Selected"))
         {
            needsBake = true;
         }
      }
      bool needsBake = false;
      public void OnGUI(PaintJob[] jobs)
      {
         if (needsBake && Event.current.type == EventType.Repaint)
         {
            needsBake = false;
            Bake(jobs);
         }
         SharedGUI();
         if (GUILayout.Button("Export Selected"))
         {
            needsBake = true;
         }
      }

      bool IsEnabled(Passes p)
      {
         return ((int)passes & (int)p) == (int)p;
      }


      class MeshDef
      {
         public Vector3[] verts;
         public int[] faces;
         public Color[] color;
         public Vector4[] uv0;
         public Vector4[] uv2;
         public Vector4[] uv3;

         public static int kMaxVert = 60000;

         public MeshDef(int triCount)
         {
            verts = new Vector3[triCount];
            faces = new int[triCount];
            color = new Color[triCount];
            uv0 = new Vector4[triCount];
            uv2 = new Vector4[triCount];
            uv3 = new Vector4[triCount];
         }
      }

      Material SetupMaterial(Material mat, SplatArrayShaderGUI.DebugOutput debugOutput)
      {
         SplatArrayShaderGUI.FeatureData fData = new SplatArrayShaderGUI.FeatureData();
         fData.Unpack(mat.shaderKeywords);
         fData.useMacroTexture = false;
         fData.parallax = false;
         fData.debugOutput = debugOutput;
         fData.tesselationMode = SplatArrayShaderGUI.TessellationMode.None;
         fData.generateFallback = false;
         fData.uvMode = SplatArrayShaderGUI.UVMode.UV;

         string[] features = fData.Pack();

         string shader = SplatArrayShaderGUI.Compile(features, "RenderBake_" + debugOutput.ToString());

         Shader s = ShaderUtil.CreateShaderAsset(shader);
         Material renderMat = new Material(mat);
         renderMat.shader = s;
         return renderMat;
      }

      List<GameObject> BuildRenderObjects(PaintJob[] jobs, SplatArrayShaderGUI.DebugOutput debugOutput)
      {
         List<GameObject> goes = new List<GameObject>();
         foreach (var job in jobs)
         {
            Material renderMat = SetupMaterial(job.renderer.sharedMaterial, debugOutput);


            var srcTri = job.meshFilter.sharedMesh.triangles;
            var srcUV0 = job.stream.uv0;
            var srcUV2 = job.stream.uv2;
            var srcUV3 = job.stream.uv3;
            var srcColor = job.stream.colors;

            int srcVCount = job.meshFilter.sharedMesh.vertexCount;
            if (srcUV0 == null || srcUV0.Count != srcVCount)
            {
               job.meshFilter.sharedMesh.GetUVs(0, srcUV0);
            }
            if (srcUV2 == null || srcUV2.Count != srcVCount)
            {
               job.meshFilter.sharedMesh.GetUVs(2, srcUV2);
            }
            if (srcUV3 == null || srcUV3.Count != srcVCount)
            {
               job.meshFilter.sharedMesh.GetUVs(3, srcUV3);
            }
            if (srcColor == null || srcColor.Length != srcVCount)
            {
               srcColor = job.meshFilter.sharedMesh.colors;
            }
               
            List<MeshDef> defs = new List<MeshDef>();

            int triCount = srcTri.Length;
            int left = triCount;
            while (left > 0)
            {
               defs.Add(new MeshDef(Mathf.Clamp(left, 0, MeshDef.kMaxVert)));
               left -= MeshDef.kMaxVert;
            }

            for (int i = 0; i < triCount; i++)
            {
               int defIdx = (int)(i / MeshDef.kMaxVert);
               int idxOffset = i - (defIdx * MeshDef.kMaxVert);
               var d = defs[defIdx];
               d.faces[idxOffset] = idxOffset;
               int vIdx = srcTri[i];
               d.verts[idxOffset] = new Vector3(srcUV0[vIdx].x, srcUV0[vIdx].y, 0);
               d.uv0[idxOffset] = srcUV0[vIdx];
               if (idxOffset < d.uv2.Length && vIdx < srcUV2.Count)
               {
                  d.uv2[idxOffset] = srcUV2[vIdx];
               }
               if (idxOffset < d.uv3.Length && vIdx < srcUV3.Count)
               {
                  d.uv3[idxOffset] = srcUV3[vIdx];
               }
               d.color[idxOffset] = srcColor[vIdx];
            }
            for (int i = 0; i < defs.Count; ++i)
            {
               var d = defs[i];
               Mesh renderMesh = new Mesh();
               renderMesh.vertices = d.verts;
               renderMesh.triangles = d.faces;
               renderMesh.colors = d.color;
               renderMesh.SetUVs(0, new List<Vector4>(d.uv0));
               renderMesh.SetUVs(2, new List<Vector4>(d.uv2));
               renderMesh.SetUVs(3, new List<Vector4>(d.uv3));
               renderMesh.RecalculateBounds();
               renderMesh.UploadMeshData(false);


               GameObject go = new GameObject();
               go.AddComponent<MeshRenderer>().sharedMaterial = renderMat;
               go.AddComponent<MeshFilter>().sharedMesh = renderMesh;
               go.transform.position = new Vector3(0, 10000, 0);
               goes.Add(go);
            }
         }
         return goes;
      }

      SplatArrayShaderGUI.DebugOutput OutputFromPass(Passes p)
      {
         if (p == Passes.Albedo)
         {
            return SplatArrayShaderGUI.DebugOutput.Albedo;
         }
         else if (p == Passes.AO)
         {
            return SplatArrayShaderGUI.DebugOutput.AO;
         }
         else if (p == Passes.Emissive)
         {
            return SplatArrayShaderGUI.DebugOutput.Emission;
         }
         else if (p == Passes.Height)
         {
            return SplatArrayShaderGUI.DebugOutput.Height;
         }
         else if (p == Passes.Metallic)
         {
            return SplatArrayShaderGUI.DebugOutput.Metallic;
         }
         else if (p == Passes.Normal)
         {
            return SplatArrayShaderGUI.DebugOutput.Normal;
         }
         else if (p == Passes.Smoothness)
         {
            return SplatArrayShaderGUI.DebugOutput.Smoothness;
         }
         else if (p == Passes.SplatData)
         {
            return SplatArrayShaderGUI.DebugOutput.SplatData;
         }
         return SplatArrayShaderGUI.DebugOutput.Albedo;
      }

      Camera SetupCamera()
      {
         Camera cam = new GameObject("cam").AddComponent<Camera>();
         cam.orthographic = true;
         cam.orthographicSize = 0.5f;
         cam.transform.position = new Vector3(0.5f, 10000.5f, -1);
         cam.nearClipPlane = 0.1f;
         cam.farClipPlane = 2.0f;
         cam.enabled = false;
         cam.depthTextureMode = DepthTextureMode.None;
         cam.clearFlags = CameraClearFlags.Color;
         cam.backgroundColor = Color.grey;
         return cam;
      }

      void Bake(PaintJob[] jobs)
      {
         
         Camera cam = SetupCamera();

         // for each pass
         int pass = 1;
         while (pass <= (int)(Passes.SplatData))
         {
            Passes p = (Passes)pass;
            pass *= 2;
            if (!IsEnabled(p))
            {
               continue;
            }
            SplatArrayShaderGUI.DebugOutput debugOutput = OutputFromPass(p);

            var readWrite = (debugOutput == SplatArrayShaderGUI.DebugOutput.Albedo || debugOutput == SplatArrayShaderGUI.DebugOutput.Emission) ?
               RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

            RenderTexture rt = RenderTexture.GetTemporary((int)res, (int)res, 0, RenderTextureFormat.ARGB32, readWrite);
            RenderTexture.active = rt;
            cam.targetTexture = rt;

            var meshes = BuildRenderObjects(jobs, debugOutput);

            bool fog = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
            cam.Render();
            Unsupported.SetRenderSettingsUseFogNoDirty(fog);

            Texture2D tex = new Texture2D((int)res, (int)res, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, (int)res, (int)res), 0, 0);
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  Color c = tex.GetPixel(x, y);
                  c.a = 1;
                  tex.SetPixel(x, y, c);
               }
            }
            tex.Apply();
            var bytes = tex.EncodeToPNG();
            string texPath = baseDir + "/" + jobs[0].stream.gameObject.name + "_" + debugOutput.ToString();
            System.IO.File.WriteAllBytes(texPath + ".png", bytes);

            for (int i = 0; i < meshes.Count; ++i)
            {
               if (meshes[i] == null)
                  continue;
               MeshRenderer mr = meshes[i].GetComponent<MeshRenderer>();
               MeshFilter mf = meshes[i].GetComponent<MeshFilter>();
               if (mr != null && mr.sharedMaterial != null)
               {
                  if (mr.sharedMaterial.shader != null)
                  {
                     GameObject.DestroyImmediate(meshes[i].GetComponent<MeshRenderer>().sharedMaterial.shader);
                  }
                  GameObject.DestroyImmediate(meshes[i].GetComponent<MeshRenderer>().sharedMaterial);
               }

               if (mf != null && mf.sharedMesh != null)
               {
                  GameObject.DestroyImmediate(meshes[i].GetComponent<MeshFilter>().sharedMesh);
               }

               GameObject.DestroyImmediate(meshes[i]);
            }


         }
         GameObject.DestroyImmediate(cam.gameObject);
         AssetDatabase.Refresh();

      }

      void Bake(TerrainJob[] jobs)
      {
         Camera cam = SetupCamera();
         foreach(var job in jobs)
         {
            // for each pass
            int pass = 1;
            while (pass <= (int)(Passes.Emissive))
            {
               Passes p = (Passes)pass;
               pass *= 2;
               if (!IsEnabled(p))
               {
                  continue;
               }
               var debugOutput = OutputFromPass(p);
               var readWrite = (debugOutput == SplatArrayShaderGUI.DebugOutput.Albedo || debugOutput == SplatArrayShaderGUI.DebugOutput.Emission) ?
                  RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

               RenderTexture rt = RenderTexture.GetTemporary((int)res, (int)res, 0, RenderTextureFormat.ARGB32, readWrite);
               RenderTexture.active = rt;
               cam.targetTexture = rt;

               GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
               go.transform.position = new Vector3(0, 10000, 0);
               cam.transform.position = new Vector3(0, 10000, -1);
               Material renderMat = SetupMaterial(job.terrain.materialTemplate, debugOutput);
               go.GetComponent<MeshRenderer>().sharedMaterial = renderMat;
               bool fog = RenderSettings.fog;
               Unsupported.SetRenderSettingsUseFogNoDirty(false);
               cam.Render();
               Unsupported.SetRenderSettingsUseFogNoDirty(fog);
               Texture2D tex = new Texture2D((int)res, (int)res, TextureFormat.ARGB32, false);
               tex.ReadPixels(new Rect(0, 0, (int)res, (int)res), 0, 0);
               RenderTexture.active = null;
               RenderTexture.ReleaseTemporary(rt);

               for (int x = 0; x < tex.width; ++x)
               {
                  for (int y = 0; y < tex.height; ++y)
                  {
                     Color c = tex.GetPixel(x, y);
                     c.a = 1;
                     tex.SetPixel(x, y, c);
                  }
               }
               tex.Apply();
               var bytes = tex.EncodeToPNG();
               string texPath = baseDir + "/" + job.terrain.name + "_" + debugOutput.ToString();
               System.IO.File.WriteAllBytes(texPath + ".png", bytes);

               MeshRenderer mr = go.GetComponent<MeshRenderer>();
               if (mr != null)
               {
                  if (mr.sharedMaterial != null)
                  {
                     if (mr.sharedMaterial.shader != null)
                        GameObject.DestroyImmediate(mr.sharedMaterial.shader);
                     GameObject.DestroyImmediate(mr.sharedMaterial);
                  }
               }

               GameObject.DestroyImmediate(go);
            }
            GameObject.DestroyImmediate(cam.gameObject);
            AssetDatabase.Refresh();

         }
      }

   }
}
