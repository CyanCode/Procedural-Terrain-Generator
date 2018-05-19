//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using JBooth.VertexPainterPro;
using JBooth.TerrainPainter;
using System.Collections.Generic;
using JBooth.MegaSplat;

public partial class SplatArrayShaderGUI : ShaderGUI 
{
   bool HasProcedurals(Material mat)
   {
      return  (mat.IsKeywordEnabled("_PROJECTTEXTURE_WORLD") || mat.IsKeywordEnabled("_PROJECTTEXTURE_LOCAL") ||
         mat.IsKeywordEnabled("_PROJECTTEXTURE2_WORLD") || mat.IsKeywordEnabled("_PROJECTTEXTURE2_LOCAL"));
   }


   void Draw3WayTextureSelection(Material targetMat, string prop, string label, Texture2DArray ta)
   {
      if (ta != null)
      {
         if (MegaSplatUtilities.DrawRollup(label, true, true))
         {
            EditorGUI.BeginChangeCheck();
            Vector4 v = targetMat.GetVector(prop);
            EditorGUILayout.BeginHorizontal();
            v.x = MegaSplatUtilities.DrawTextureSelector((int)v.x, ta, true);
            v.y = MegaSplatUtilities.DrawTextureSelector((int)v.y, ta, true);
            v.z = MegaSplatUtilities.DrawTextureSelector((int)v.z, ta, true);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
               targetMat.SetVector(prop, v);
               EditorUtility.SetDirty(targetMat);
            }
         }
      }
      else
      {
         EditorGUILayout.HelpBox("Material needs a diffuse texture array assigned", MessageType.Info);
      }
   }


   void Draw3WayParams(Material targetMat, string prop)
   {
      EditorGUI.BeginChangeCheck();
      Vector4 v = targetMat.GetVector(prop);
      Vector2 thresh = new Vector2(v.x, v.y);
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PrefixLabel("Side Threshold");
      EditorGUILayout.MinMaxSlider(ref thresh.x, ref thresh.y, -1, 1);
      EditorGUILayout.EndHorizontal();
      v.x = thresh.x;
      v.y = thresh.y;
      v.z = EditorGUILayout.FloatField("Cluster Scale", v.z);
      if (EditorGUI.EndChangeCheck())
      {
         targetMat.SetVector(prop, v);
         EditorUtility.SetDirty(targetMat);
      }
   }

   void DrawLayerBlendNoise(Material targetMat, string prop)
   {
      EditorGUI.BeginChangeCheck();
      Vector4 v = targetMat.GetVector(prop);
      v.x = EditorGUILayout.FloatField("Noise Frequency", v.x);
      v.y = EditorGUILayout.FloatField("Noise Amplitude", v.y);

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PrefixLabel(new GUIContent("Min/Max", "Minimum and maximum values of noise; can be used to keep the blend within a specific range"));
      EditorGUILayout.MinMaxSlider(ref v.z, ref v.w, 0, 1);
      EditorGUILayout.EndHorizontal();

      if (EditorGUI.EndChangeCheck())
      {
         targetMat.SetVector(prop, v);
         EditorUtility.SetDirty(targetMat);
      }
   }

   void Draw3WayEditor(MaterialEditor materialEditor, MaterialProperty[] props, Material targetMat, FeatureData fData)
   {
      var albedoMap = FindProperty("_Diffuse", props);
      Texture2DArray ta = albedoMap.textureValue as Texture2DArray;

      if (!MegaSplatUtilities.DrawRollup("Texture Projection Editor"))
      {
         return;
      }

      if (fData.projectTextureMode == TextureChoiceMode.Project3Way && targetMat.HasProperty("_ProjectTexTop"))
      {
         Draw3WayTextureSelection(targetMat, "_ProjectTexTop", "Bottom Layer, Top", ta);
         Draw3WayTextureSelection(targetMat, "_ProjectTexSide", "Bottom Layer, Side", ta);
         Draw3WayTextureSelection(targetMat, "_ProjectTexBottom", "Bottom Layer, Bottom", ta);
         Draw3WayParams(targetMat, "_ProjectTexThresholdFreq");

      }

      if (fData.projectTextureMode2 == TextureChoiceMode.Project3Way && targetMat.HasProperty("_ProjectTexTop2"))
      {
         Draw3WayTextureSelection(targetMat, "_ProjectTexTop2", "Top Layer, Top", ta);
         Draw3WayTextureSelection(targetMat, "_ProjectTexSide2", "Top Layer, Side", ta);
         Draw3WayTextureSelection(targetMat, "_ProjectTexBottom2", "Top Layer, Bottom", ta);
         Draw3WayParams(targetMat, "_ProjectTexThresholdFreq2");
         EditorGUILayout.Space();
         DrawLayerBlendNoise(targetMat, "_ProjectTexBlendParams");
      }

      if (GUILayout.Button("Bake to Vertex Stream on selected Meshes"))
      {
         if (!BakeToSelection(targetMat))
         {
            Debug.LogError("No game objects found with this material and VertexInstanceStreams added");
         }
      }
   }

   bool BakeToSelection(Material targetMat)
   {
      bool ret = false;
      Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.OnlyUserModifiable | SelectionMode.Deep);
      for (int i = 0; i < objs.Length; ++i)
      {
         GameObject go = objs[i] as GameObject;
         if (go != null)
         {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            Renderer r = go.GetComponent<Renderer>();
            VertexInstanceStream stream = go.GetComponent<VertexInstanceStream>();
            if (targetMat == r.sharedMaterial)
            {
               if (mf != null && r != null && mf.sharedMesh != null && mf.sharedMesh.isReadable && stream != null)
               {
                  if (HasProcedurals(r.sharedMaterial))
                  {
                     ProceduralTexture(stream, mf.sharedMesh, r.sharedMaterial);
                     ret = true;
                  }
               }
            }
         }
      }
      return ret;
   }
      

   Vector3 Frac(Vector3 v)
   {
      Vector3 floor = Floor(v);
      v.x = v.x - floor.x;
      v.y = v.y - floor.y;
      v.z = v.z - floor.z;
      return v;
   }

   Vector3 Floor(Vector3 v)
   {
      v.x = (float)System.Math.Floor((double)v.x);
      v.y = (float)System.Math.Floor((double)v.y);
      v.z = (float)System.Math.Floor((double)v.z);
      return v;
   }

   Vector3 Frac(Vector3 v, Vector3 floor)
   {
      v.x = v.x - floor.x;
      v.y = v.y - floor.y;
      v.z = v.z - floor.z;
      return v;
   }
      
   // no trig based hash, not a great hash, but fast..
   float Hash(Vector3 p)
   {
      p *= 0.3183099f;
      p.x += 0.1f; p.y += 0.1f; p.z += 0.1f;
      p = Frac(p);

      p *= 17.0f;
      float h = ( p.x*p.y*p.z*(p.x+p.y+p.z) );
      return h - (float)System.Math.Floor((decimal)h);
   }



   float Noise( Vector3 x )
   {
      Vector3 p = Floor(x);
      Vector3 f = Frac(x, p);
      f.x = f.x*f.x*(3.0f-2.0f*f.x);
      f.y = f.y*f.y*(3.0f-2.0f*f.y);
      f.z = f.z*f.z*(3.0f-2.0f*f.z);

      return Mathf.Lerp(Mathf.Lerp(Mathf.Lerp( Hash(p), Hash(p + new Vector3(1,0,0)),f.x), 
         Mathf.Lerp( Hash(p + new Vector3(0,1,0)), Hash(p + new Vector3(1,1,0)),f.x),f.y),
         Mathf.Lerp(Mathf.Lerp(Hash(p + new Vector3(0,0,1)), Hash(p + new Vector3(1,0,1)),f.x), 
         Mathf.Lerp(Hash(p + new Vector3(0,1,1)), Hash(p + new Vector3(1,1,1)),f.x),f.y),f.z);
   }

   // given 4 texture choices for each projection, return texture index based on normal
   // seems like we could remove some branching here.. hmm..
   float ProjectTexture(Vector3 worldPos, Vector3 normal, Vector3 threshFreq, Vector3 top, Vector3 side, Vector3 bottom)
   {
      float d = Vector3.Dot(normal, new Vector3(0, 1, 0));
      Vector3 cvec = side;
      if (d < threshFreq.x)
      {
         cvec = bottom;
      }
      else if (d > threshFreq.y)
      {
         cvec = top;
      }

      float n = Noise(worldPos * threshFreq.z);
      if (n < 0.333f)
         return cvec.x/255.0f;
      else if (n < 0.666f)
         return cvec.y/255.0f;
      else
         return cvec.z/255.0f;
   }

   // Must remain in sync with GPU based procedural texturing..
   void ProceduralTexture(VertexInstanceStream stream, Mesh origMesh, Material mat)
   {
      bool worldSpace = mat.IsKeywordEnabled("_PROJECTTEXTURE_WORLD") || mat.IsKeywordEnabled("_PROJECTTEXTURE2_WORLD");
      bool layerOne = mat.IsKeywordEnabled("_PROJECTTEXTURE_LOCAL") || mat.IsKeywordEnabled("_PROJECTTEXTURE_WORLD");
      bool layerTwo = mat.IsKeywordEnabled("_PROJECTTEXTURE2_LOCAL") || mat.IsKeywordEnabled("_PROJECTTEXTURE2_WORLD");

      Vector4 top = Vector4.zero;
      Vector4 side = Vector4.zero;
      Vector4 bottom = Vector4.zero;
      Vector4 top2 = Vector4.zero;
      Vector4 side2 = Vector4.zero;
      Vector4 bottom2 = Vector4.zero;

      Vector4 threshFreq = Vector4.zero;
      Vector4 threshFreq2 = Vector4.zero;
      Vector4 blendParams = Vector4.zero;

      if (layerOne)
      {
         top = mat.GetVector("_ProjectTexTop");
         side = mat.GetVector("_ProjectTexSide");
         bottom = mat.GetVector("_ProjectTexBottom");
         threshFreq = mat.GetVector("_ProjectTexThresholdFreq");
      }
      if (layerTwo)
      {
         top2 = mat.GetVector("_ProjectTexTop2");
         side2 = mat.GetVector("_ProjectTexSide2");
         bottom2 = mat.GetVector("_ProjectTexBottom2");
         threshFreq2 = mat.GetVector("_ProjectTexThresholdFreq2");
         blendParams = mat.GetVector("_ProjectTexBlendParams");
      }

      int count = origMesh.vertexCount;
      var colors = stream.colors;
      var uv3 = stream.uv3;

      if (colors == null || colors.Length != count)
      {
         colors = origMesh.colors;
      }
      if (uv3 == null || uv3.Count != count)
      {
         uv3 = new System.Collections.Generic.List<Vector4>(new Vector4[count]);
      }

      var mtx = stream.transform.localToWorldMatrix;

      if (layerOne)
      {
         if (worldSpace)
         {
            for (int i = 0; i < count; ++i)
            {
               Color c = colors[i];
               c.a = ProjectTexture(mtx.MultiplyPoint(stream.GetSafePosition(i)), mtx.MultiplyVector(stream.GetSafeNormal(i)), threshFreq, top, side, bottom);
               colors[i] = c;
            }
         }
         else
         {
            for (int i = 0; i < count; ++i)
            {
               Color c = colors[i];
               c.a = ProjectTexture(stream.GetSafePosition(i), stream.GetSafeNormal(i), threshFreq, top, side, bottom);
               colors[i] = c;
            }
         }
         stream.colors = colors;
      }
      if (layerTwo)
      {
         if (worldSpace)
         {
            for (int i = 0; i < count; ++i)
            {
               var uv = uv3[i];
               uv.w = ProjectTexture(mtx.MultiplyPoint(stream.GetSafePosition(i)), mtx.MultiplyVector(stream.GetSafeNormal(i)), threshFreq2, top2, side2, bottom2);
               uv3[i] = uv;
            }
         }
         else
         {
            for (int i = 0; i < count; ++i)
            {
               var uv = uv3[i];
               uv.w = ProjectTexture(stream.GetSafePosition(i), stream.GetSafeNormal(i), threshFreq2, top2, side2, bottom2);
               uv3[i] = uv;
            }
         }
         for (int i = 0; i < count; ++i)
         {
            Vector3 wp = mtx.MultiplyPoint(stream.GetSafePosition(i));
            wp.x *= blendParams.x;
            wp.y *= blendParams.x;
            wp.z *= blendParams.x;


            float blendNoise = Noise(wp);
            blendNoise -= 0.5f;
            blendNoise *= blendParams.y;
            blendNoise += 0.5f;
            blendNoise = Mathf.Min(Mathf.Max(blendNoise, blendParams.z), blendParams.w);


            var uv = uv3[i];
            uv.x = Mathf.Clamp01(blendNoise);
            uv3[i] = uv;
         }
         stream.uv3 = uv3;
      }
      stream.Apply(false);

   }

}

