//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace JBooth.MegaSplat
{
   [System.Serializable]
   public class TextureCluster
   {
      public enum Mode
      {
         Noise = 0,
         Angle,
         Height,
      }
      public string name;
      public List<int> indexes = new List<int>();
      public Mode mode;
      public NoiseParams noise;
      public AnimationCurve angleCurve = AnimationCurve.Linear(0, 0, 1.0f, 1.0f);
      public AnimationCurve heightCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
      #if UNITY_EDITOR
      [HideInInspector]
      public byte[] previewData;
      #endif
      [System.NonSerialized]
      public Texture2D previewTex;

      public int RemapIndex(float n)
      {
         int i = (int)(n * indexes.Count);
         return Mathf.Clamp(i, 0, indexes.Count - 1);
      }
         
      public int GetIndexForAngle(Vector3 normal)
      {
         float dt = Vector3.Dot(normal, Vector3.up);
         float angle = 1.0f - (dt * 0.5f + 0.5f);
         angle = angleCurve.Evaluate(angle);
         int chosen = (int)((float)indexes.Count * angle);
         return Mathf.Clamp(chosen, 0, indexes.Count - 1);
      }
         


      public int GetIndex(Vector3 pos, Vector3 normal, float heightRatio)
      {
         if (mode == TextureCluster.Mode.Noise)
         {
            int range = indexes.Count;
            float n = noise.GetNoise(pos);
            int texIdx = (int)(n * range);
            if (texIdx >= range - 1)
               texIdx = range - 1;

            return indexes[texIdx];
         }
         else if (mode == Mode.Angle)
         {
            return GetIndexForAngle(normal);
         }
         else if (mode == Mode.Height)
         {
            int range = indexes.Count;
            int index = Mathf.RoundToInt(Mathf.Lerp(0, range, heightRatio));
            return indexes[Mathf.Clamp(index, 0, range - 1)];
         }
         return 0;
      }

      /// <summary>
      /// Automatically paints the layer based on existing data. The input/output is in
      /// Terrain shading format (color packed with indexes in r/g and blend in b)
      /// </summary>
      /// <param name="existing">existing color data in terrain shader format</param>
      /// <param name="curVal">Current value.</param>
      /// <param name="index">Index of texture to apply.</param>
      /// <param name="r">The brush weight.</param>
      /// <param name="cluster">Optional texture cluster when using clusters. Used for search.</param>
      public static Color AutoColor(Color existing, int index, float r, TextureCluster cluster = null)
      {
         const float thresh = 1.0f / 255.0f;
         const float oneMinusThresh = 1.0f - thresh;
         float f = (float)index / 255.0f;

         bool rfound = false;
         bool gfound = false;
         if (cluster == null)
         {
            rfound = existing.r == f;
            gfound = existing.g == f;
         }
         else
         {
            for (int i = 0; i < cluster.indexes.Count; ++i)
            {
               float fidx = (float)cluster.indexes[i] / 255.0f;
               if (fidx == existing.r)
                  rfound = true;
               if (fidx == existing.g)
                  gfound = true;
            }
         }
         // we want to replace the layer which is least dominant, 
         // but if the other layer already has that texture, we just want to blend that layer in more..

         // if no layer or both layers are set, pick the lowest..
         if (rfound == gfound)
         {
            // both found
            if (rfound == true)
            {
               if (existing.b >= 0.5f)
               {
                  existing.b = Mathf.Clamp01(existing.b + r); 
               }
               else
               {
                  existing.b = Mathf.Clamp01(existing.b - r); 
               }
            }
            else
            {
               if (existing.b >= 0.5f)
               {
                  existing.r = f;
                  existing.b = Mathf.Clamp01(existing.b - r);
               }
               else
               {
                  existing.g = f;
                  existing.b = Mathf.Clamp01(existing.b + r);
               } 
            }

         }
         else if (rfound)
         {
            existing.r = f;
            existing.b = Mathf.Clamp01(existing.b - r);
            if (existing.b < thresh)
               existing.g = f;
         }
         else if (gfound)
         {
            existing.b = Mathf.Clamp01(existing.b + r);
            existing.g = f;
            if (existing.b > oneMinusThresh)
               existing.r = f;
         }
         return existing;

      }

   }
}
