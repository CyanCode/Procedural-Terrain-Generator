//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JBooth.VertexPainterPro;
using UnityEditor;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   public class EvalData
   {
      public Bounds bounds;

      int varCount = 0;
      int propCount = 0;
      int indent = 0;
      int curveCount = 0;

      public System.Text.StringBuilder sb = new System.Text.StringBuilder(16384);
      public System.Text.StringBuilder props = new System.Text.StringBuilder(4096);
      public System.Text.StringBuilder defn = new System.Text.StringBuilder(4096);
      public static Texture2DArray curves;

      public static EvalData data = null;

      public void Indent() { sb.Append(' ', indent); }
      public void IncreaseIndent() { indent++; }
      public void DecreaseIndent() { indent--; }

      public string GetNextName()
      {
         varCount++;
         return "var_" + varCount;
      }

      public string GetNextProp()
      {
         propCount++;
         return "_Prop_" + propCount;
      }

      public int GetNextCurveIndex()
      {
         curveCount++;
         return curveCount-1;
      }

      public string WritePropertyEntry(string propType, string val, string codeType)
      {
         string propName = GetNextProp();
         props.Append(propName);
         props.Append("(\"");
         props.Append(propName);
         props.Append("\", ");
         props.Append(propType);
         props.Append(") = ");
         props.AppendLine(val);
         defn.Append(codeType);
         defn.Append(" ");
         defn.Append(propName);
         defn.AppendLine(";");
         return propName;
      }
   };



   [System.Serializable]
   [Node (false, "MegaSplat/Output")]
   public class NodeMegaSplatOutput : Node 
   {
      public const string ID = "megaSplatOutput";
      public override string GetID { get { return ID; } }

      public TextureArrayConfig config;
      public static TextureArrayConfig sConfig;

      public bool generateCavity;
      public int cavitySamples = 16;
      public float cavityDistance = 1;

      public override bool CanDelete()
      {
         return false;
      }

      public override bool CanCreate()
      {
         return false;
      }

      [System.Serializable]
      public class Job
      {
         public VertexPainterPro.VertexInstanceStream stream;
         public Terrain terrain;
         public Texture2D worldPosTex;
         public Texture2D localPosTex;
         public Texture2D worldNormalTex;
         public Texture2D localNormalTex;
         public Texture2D UVCavity;
         public RenderTexture buffer;
         public RenderTexture fxBuffer;
         public Texture2D result;
         public Texture2D fxResult;
         public bool needFx = false;
         public Bounds bounds;

         public bool needsApply = false;
         public bool needsRender = false;

         public void Render(Material mat)
         {
            needsRender = false;
            mat.SetTexture("_LocalPositions", localPosTex);
            mat.SetTexture("_WorldPositions", worldPosTex);
            mat.SetTexture("_LocalNormals", localNormalTex);
            mat.SetTexture("_WorldNormals", worldNormalTex);
            mat.SetTexture("_UVCavity", UVCavity);
            mat.SetVector("_HeightBounds", new Vector4(bounds.min.y, bounds.max.y, 0, 0));


            mat.SetFloat("_Pass", 0);
            Graphics.Blit(Texture2D.blackTexture, buffer, mat);

            RenderTexture.active = buffer;

            result.ReadPixels(new Rect(0, 0, buffer.width, buffer.height), 0, 0);
            result.Apply();
            if (needFx)
            {
               mat.SetFloat("_Pass", 1);
               Graphics.Blit(Texture2D.blackTexture, fxBuffer, mat);
               RenderTexture.active = fxBuffer;

               fxResult.ReadPixels(new Rect(0, 0, buffer.width, buffer.height), 0, 0);
               fxResult.Apply();
            }

            RenderTexture.active = null;
            needsApply = true;
         }

         public void Release()
         {
            stream = null;
            if (worldPosTex != null)
            {
               DestroyImmediate(worldPosTex);
            }
            if (localPosTex != null)
            {
               DestroyImmediate(localPosTex);
            }
            if (worldNormalTex != null)
            {
               DestroyImmediate(worldNormalTex);
            }
            if (localNormalTex != null)
            {
               DestroyImmediate(localNormalTex);
            }
            if (UVCavity != null)
            {
               DestroyImmediate(UVCavity);
            }
            if (buffer != null)
            {
               buffer.Release();
               DestroyImmediate(buffer);
            }
            if (fxBuffer != null)
            {
               fxBuffer.Release();
               DestroyImmediate(fxBuffer);
            }
            if (result != null)
            {
               DestroyImmediate(result);
            }
            if (fxResult != null)
            {
               DestroyImmediate(fxResult);
            }

         }

         Vector3 PixelToLocal(Terrain t, int x, int y)
         {
            Vector3 pos = new Vector3(x, 0, y);
            pos.x *= t.terrainData.heightmapScale.x;
            pos.z *= t.terrainData.heightmapScale.z;
            return pos;
         }


         public Bounds Init(Terrain t, NodeMegaSplatOutput masterNode)
         {
            terrain = t;
            var mat = t.materialTemplate;
            var control = mat.GetTexture("_SplatControl");
            int width = control.width;
            int height = control.height;
            int awidth = t.terrainData.heightmapResolution;
            int aheight = t.terrainData.heightmapResolution;
            localPosTex = new Texture2D(awidth, aheight, TextureFormat.RGBAFloat, false);
            worldPosTex = new Texture2D(awidth, aheight, TextureFormat.RGBAFloat, false);
            localNormalTex = new Texture2D(awidth, aheight, TextureFormat.RGBAFloat, false);
            worldNormalTex = new Texture2D(awidth, aheight, TextureFormat.RGBAFloat, false);
            UVCavity = new Texture2D(awidth, aheight, TextureFormat.RGBAFloat, false);

            buffer = new RenderTexture(width, height, 0);
            result = new Texture2D(width, height, TextureFormat.RGB24, false);
            fxBuffer = new RenderTexture(width, height, 0);
            fxResult = new Texture2D(width, height, TextureFormat.RGB24, false);

            Vector2 aoRange = new Vector2(0.001f, masterNode.cavityDistance);
            aoData.Clear();
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            Matrix4x4 mtx = terrain.transform.localToWorldMatrix;
            for (int x = 0; x < awidth; ++x)
            {
               for (int y = 0; y < aheight; ++y)
               {
                  float h = t.terrainData.GetHeight(x, y);
                  Vector3 localPos = PixelToLocal(t, x, y);
                  localPos.y = h;
                  Vector3 worldPos = mtx.MultiplyPoint(localPos);
                  float fx = (float)x / awidth;
                  float fy = (float)y / aheight;
                  Vector3 normal = t.terrainData.GetInterpolatedNormal(fx, fy);
                  float cav = 0;
                  if (masterNode.generateCavity)
                  {
                     cav = ComputeAO(worldPos, normal, masterNode.cavitySamples, aoRange); 
                  }
                  localPosTex.SetPixel(x, y, new Color(localPos.x, localPos.y, localPos.z));
                  worldPosTex.SetPixel(x, y, new Color(worldPos.x, worldPos.y, worldPos.z));
                  localNormalTex.SetPixel(x, y, new Color(normal.x, normal.y, normal.z));
                  worldNormalTex.SetPixel(x, y, new Color(normal.x, normal.y, normal.z));
                  UVCavity.SetPixel(x, y, new Color(fx, fy, cav));

                  if (worldPos.y < minY)
                  {
                     minY = worldPos.y;
                  }
                  if (worldPos.y > maxY)
                  {
                     maxY = worldPos.y;
                  }
               }
            }

            UVCavity.Apply();
            localPosTex.Apply();
            worldPosTex.Apply();
            localNormalTex.Apply();
            worldNormalTex.Apply();
            Bounds b = new Bounds(t.transform.position, t.terrainData.size);
            b.min = new Vector3(b.min.x, minY, b.min.z);
            b.max = new Vector3(b.max.x, maxY, b.max.z);
            return b;

         }

         public Bounds Init(VertexPainterPro.VertexInstanceStream vis, NodeMegaSplatOutput masterNode)
         {
            stream = vis;
            var mtx = vis.transform.localToWorldMatrix;
            var count = vis.GetComponent<MeshFilter>().sharedMesh.vertexCount;
            int width = 1024;
            int height = count / width + 1;

            Collider tempCol = null;
            if (vis.GetComponent<Collider>() == null)
            {
               tempCol = vis.gameObject.AddComponent<MeshCollider>();
            }

            Vector2 aoRange = new Vector2(0.001f, masterNode.cavityDistance);
            localPosTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            worldPosTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            localNormalTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            worldNormalTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            UVCavity = new Texture2D(width, height, TextureFormat.RGBAFloat, false);

            buffer = new RenderTexture(width, height, 0);
            result = new Texture2D(width, height, TextureFormat.RGB24, false);
            fxBuffer = new RenderTexture(width, height, 0);
            fxResult = new Texture2D(width, height, TextureFormat.RGB24, false);

            aoData.Clear();
            for (int idx = 0; idx < count; ++idx)
            {
               var localPos = vis.GetSafePosition(idx);
               var localNormal = vis.GetSafeNormal(idx).normalized;
               var worldPos = mtx.MultiplyPoint(localPos);
               var worldNormal = mtx.MultiplyVector(localNormal).normalized;
               var uv = vis.GetSafeUV0(idx);

               float cav = 0;
               if (masterNode.generateCavity)
               {
                  cav = ComputeAO(worldPos, worldNormal, masterNode.cavitySamples, aoRange);
               }
               var y = idx / width;
               var x = ((float)idx / (float)width) - y;
               x *= width;

               int xx = (int)x;
               int yy = (int)y;
               localPosTex.SetPixel(xx,  yy, new Color(localPos.x, localPos.y, localPos.z));
               worldPosTex.SetPixel(xx,  yy, new Color(worldPos.x, worldPos.y, worldPos.z));
               localNormalTex.SetPixel(xx, yy, new Color(localNormal.x, localNormal.y, localNormal.z));
               worldNormalTex.SetPixel(xx, yy, new Color(worldNormal.x, worldNormal.y, worldNormal.z));
               UVCavity.SetPixel(xx, yy, new Color(uv.x, uv.y, cav));
            }

            localPosTex.Apply();
            worldPosTex.Apply();
            localNormalTex.Apply();
            worldNormalTex.Apply();
            UVCavity.Apply();
            if (tempCol != null)
            {
               DestroyImmediate(tempCol);
            }

            return vis.GetComponent<Renderer>().bounds;

         }

         RaycastHit hit = new RaycastHit();
         Dictionary<Vector3, float> aoData = new Dictionary<Vector3, float>();
         public float ComputeAO(Vector3 worldPosition, Vector3 worldNormal, int aoSamples, Vector2 aoRange)
         {
            float totalOcclusion = 0;
            if (aoData.ContainsKey(worldPosition))
               return aoData[worldPosition];
            
            // the slow part..
            for (int j = 0; j < aoSamples; j++)
            {
               // random rotate around hemisphere
               float rot = 180.0f;
               float rot2 = rot / 2.0f;
               float rotx = ((rot * Random.value) - rot2);
               float roty = ((rot * Random.value) - rot2);
               float rotz = ((rot * Random.value) - rot2);

               Vector3 dir = Quaternion.Euler(rotx, roty, rotz) * Vector3.up;
               Quaternion dirq = Quaternion.FromToRotation(Vector3.up, worldNormal);
               Vector3 ray = dirq * dir;
               Vector3 offset = Vector3.Reflect(ray, worldNormal);

               // raycast
               ray = ray * (aoRange.y / ray.magnitude);
               if (Physics.Linecast(worldPosition - (offset * 0.1f), worldPosition + ray, out hit))
               {
                  if (hit.distance > aoRange.x)
                  {
                     totalOcclusion += Mathf.Clamp01(1 - (hit.distance / aoRange.y));
                  }
               }
            }

            totalOcclusion = Mathf.Clamp01(1 - (totalOcclusion / aoSamples));
            aoData[worldPosition] = totalOcclusion;
            return totalOcclusion;
         }

         public void Apply()
         {
            needsApply = false;
            if (stream != null)
            {
               var count = stream.GetComponent<MeshFilter>().sharedMesh.vertexCount;
               var mf = stream.GetComponent<MeshFilter>();
               var colors = stream.colors;
               if (colors.Length != count)
               {
                  colors = mf.sharedMesh.colors;
               }
               var uv3 = stream.uv3;
               var uv1 = stream.uv1;
            
               int width = 1024;

               if (needFx && (uv1 == null || uv1.Count != count))
               {
                  uv1 = new List<Vector4>(count);
                  mf.sharedMesh.GetUVs(1, uv1);
                  if (uv1 == null || uv1.Count != count)
                  {
                     uv1 = new List<Vector4>(count);
                  }
               }
               if (uv3 == null || uv3.Count != count)
               {
                  uv3 = new List<Vector4>(count);
               }


               //var resColors = result.GetPixels();
               for (int i = 0; i < count; ++i)
               {
                  var y = i / width;
                  var x = ((float)i / (float)width) - y;
                  x *= width;

                  int xx = (int)x;
                  int yy = (int)y;

                  // we can speed this up by prefetching all the pixels into an array,
                  // but unity doesn't support this until 5.5 or so, so we go the slower way for now
                  // so we don't alloc 2mb of memory in the test scene.
                  //var c = resColors[yy * 1024 + xx];
                  var c = result.GetPixel(xx, yy);
                  Color fx = Color.black;
                  Vector4 u1 = Vector4.zero;
                  if (needFx)
                  {
                     fx = fxResult.GetPixel(xx, yy);
                     u1 = uv1[i];
                  }
                  Color clr = colors[i];
                  Vector4 u = uv3[i];

                  clr.a = c.r;
                  u.w = c.g;
                  u.x = c.b;


                  if (needFx)
                  {
                     u.y = fx.g;
                     u1.w = fx.r;
                     uv1[i] = u1;
                  }
                  colors[i] = clr;
                  uv3[i] = u;

               }

               stream.colors = colors;
               stream.uv3 = uv3;
               if (needFx)
               {
                  stream.uv1 = uv1;
               }
               stream.Apply();
            }
            if (terrain != null)
            {
               var mat = terrain.materialTemplate;
               var control = mat.GetTexture("_SplatControl") as Texture2D;
               var parms = mat.GetTexture("_SplatParams") as Texture2D;
               RenderTexture.active = buffer;

               control.ReadPixels(new Rect(0, 0, buffer.width, buffer.height), 0, 0);
               control.Apply();
               if (parms != null && needFx)
               {
                  // unfortunately, we need to swizzle these out into the right place
                  for (int x = 0; x < fxResult.width; ++x)
                  {
                     for (int y = 0; y < fxResult.height; ++y)
                     {
                        Color c = fxResult.GetPixel(x, y);
                        parms.SetPixel(x, y, new Color(0.5f, 0.5f, c.r, c.g));
                     }
                  }
          
                  parms.Apply();
               }
            
               RenderTexture.active = null;
               // must save via terrain painter?
            }
         }


      }


      void CreateInputs(NodeMegaSplatOutput node)
      {
         NodeInput.Create(node, "Top Layer", "Int");
         NodeInput.Create(node, "Bottom Layer", "Int");
         NodeInput.Create(node, "Blend", "Float");
         NodeInput.Create(node, "Wetness", "Float");
         NodeInput.Create(node, "Puddles", "Float");
      }

      public override Node Create (Vector2 pos) 
      {
         NodeMegaSplatOutput node = CreateInstance <NodeMegaSplatOutput>();

         node.name = "MegaSplatOutput";
         node.rect = new Rect (pos.x, pos.y, 250, 200);

         CreateInputs(node);


         return node;
      }


      protected internal override void NodeGUI () 
      {
         //RTEditorGUI.labelWidth = 100;
         //this.rect = new Rect(this.rect.x, this.rect.y, 250, 200);
         if (Inputs != null && Inputs.Count == 5)
         {
            Inputs[0].DisplayLayout(new GUIContent("Top Layer", "Top Layer Texture Index"));
            Inputs[1].DisplayLayout(new GUIContent("Bottom Layer", "Bottom Layer Texture Index"));
            Inputs[2].DisplayLayout(new GUIContent("Blend", "Blend between layers"));
            Inputs[3].DisplayLayout(new GUIContent("Wetness", "Amount of Wetness (wetness must be enabled in the material)"));
            Inputs[4].DisplayLayout(new GUIContent("Puddles", "Amount of Puddles (puddles must be enabled in the material)"));
         }
         else
         {
            CreateInputs(this);
         }
      }

      public void MainGUI()
      {
         config = EditorGUILayout.ObjectField("Config", config, typeof(TextureArrayConfig), false) as TextureArrayConfig;
         generateCavity = EditorGUILayout.Toggle("Generate Cavity", generateCavity);
         if (generateCavity)
         {
            cavitySamples = EditorGUILayout.IntSlider("Samples", cavitySamples, 16, 512);
            cavityDistance = EditorGUILayout.FloatField("Max Distance", cavityDistance);
         }

      }


      static TextAsset header;
      static TextAsset body;
      static TextAsset body2;
      static TextAsset footer;

      string lastCompile;
      public void WriteShader(EvalData ed)
      {
         string[] paths = AssetDatabase.FindAssets("texturegraph_ t:TextAsset");
         for (int i = 0; i < paths.Length; ++i)
         {
            paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
         }
         for (int i = 0; i < paths.Length; ++i)
         {
            var p = paths[i];
            if (p.EndsWith("texturegraph_header.txt"))
            {
               header = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            else if (p.EndsWith("texturegraph_body.txt"))
            {
               body = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            else if (p.EndsWith("texturegraph_body2.txt"))
            {
               body2 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            else if (p.EndsWith("texturegraph_footer.txt"))
            {
               footer = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }

         ed.IncreaseIndent();
         ed.IncreaseIndent();
         ed.IncreaseIndent();

         // top, bottom, blend
         string i0 = "0";
         string i1 = "0";
         string b = "0";
         string wet = "0";
         string pud = "0";
         if (Inputs[0].connection != null)
         {
            Inputs[0].connection.body.WriteInputs();
            i0 = Inputs[0].connection.varName;
         }
         if (Inputs[1].connection != null)
         {
            Inputs[1].connection.body.WriteInputs();
            i1 = Inputs[1].connection.varName;
         }
         if (Inputs[2].connection != null)
         {
            Inputs[2].connection.body.WriteInputs();
            b = Inputs[2].connection.varName;
         }
         if (Inputs[3].connection != null)
         {
            Inputs[3].connection.body.WriteInputs();
            wet = Inputs[3].connection.varName;
         }
         if (Inputs[4].connection != null)
         {
            Inputs[4].connection.body.WriteInputs();
            pud = Inputs[4].connection.varName;
         }

         ed.Indent();
         ed.sb.AppendLine("if (_Pass < 1)");
         ed.Indent();
         ed.sb.AppendLine("   return float4((float)" + i0 + " / 255.0, (float)" + i1 + " / 255.0, 1.0 - saturate(" + b + "), 1);");
         ed.Indent();
         ed.sb.AppendLine("else");
         ed.Indent();
         ed.sb.AppendLine("   return float4(" + wet + ", " + pud + ", 0, 1);");
         // write shader
         System.Text.StringBuilder shader = new System.Text.StringBuilder();

         shader.Append(header.text);
         shader.Append(ed.props.ToString());
         shader.Append(body.text);
         shader.Append(ed.defn.ToString());
         shader.Append(body2.text);
         shader.Append(ed.sb.ToString());
         shader.Append(footer.text);


         // saves about 80ms on subsequent changes..
         string shaderStr = shader.ToString();
         if (lastCompile != shaderStr)
         {
            System.IO.File.WriteAllText(Application.dataPath + "/temp.shader", shaderStr);
            AssetDatabase.Refresh();
            lastCompile = shaderStr;
         }

      }

      public static Material sRenderMat = null;
      public void Process(List<Job> jobs, Bounds bounds, EvalData ed)
      {
         bool useFx = false;
         if (sRenderMat == null)
         {
            sRenderMat = new Material(Shader.Find("Hidden/MegaOutputShader"));
         }
         if (Inputs[0].connection != null)
         {
            Inputs[0].connection.body.ActivateProperties(sRenderMat);
         }
         if (Inputs[1].connection != null)
         {
            Inputs[1].connection.body.ActivateProperties(sRenderMat);
         }
         if (Inputs[2].connection != null)
         {
            Inputs[2].connection.body.ActivateProperties(sRenderMat);
         }
         if (Inputs[3].connection != null)
         {
            useFx = true;
            Inputs[3].connection.body.ActivateProperties(sRenderMat);
         }
         if (Inputs[4].connection != null)
         {
            useFx = true;
            Inputs[4].connection.body.ActivateProperties(sRenderMat);
         }
         // apply curve data..
         EvalData.curves.Apply();
         sRenderMat.SetTexture("_CurveArray", EvalData.curves);
         for (int i = 0; i < jobs.Count; ++i)
         {
            jobs[i].needsRender = true;
            jobs[i].bounds = bounds;
            jobs[i].needFx = useFx;
         }
            
         //System.IO.File.Delete(Application.dataPath + "/temp.shader");

      }
         
   }
}