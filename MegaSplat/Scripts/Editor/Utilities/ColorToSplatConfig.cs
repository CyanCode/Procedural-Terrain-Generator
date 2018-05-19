//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.MegaSplat
{
   [CreateAssetMenu(menuName = "MegaSplat/ColorToSplatConfig", fileName = "ColorToSplatConfig")]
   public class ColorToSplatConfig : ScriptableObject 
   {
      public TextureArrayConfig textureConfig;

      public enum MatchMode
      {
         Color,
         Index,
         SplatTexture
      }

      public MatchMode mode;

      [System.Serializable]
      public class Mapping
      {
         public Color color;
         public TextureArrayConfig.BrushData brushData = new TextureArrayConfig.BrushData();


         [System.NonSerialized]
         public Texture2D previewTex;
      }

      public List<Mapping> mappings = new List<Mapping>();


      Vector3 toLab(Color rgb)
      {
         Vector3 xyz = new Vector3( 1.5f  * rgb.r + rgb.g + 0.75f * rgb.b,
            0.5f  * rgb.r + 2.5f * rgb.g + 0.25f * rgb.b,
            3.25f * rgb.b)/3.25f;
         return new Vector3(xyz.y, xyz.x-xyz.y, xyz.y-xyz.z);
      }

      float GetColorDifference(Vector3 labA, Vector3 labB) 
      {
         return Mathf.Sqrt(Mathf.Pow(labB.x - labA.x, 2) + Mathf.Pow(labB.y - labA.y, 2) + Mathf.Pow(labB.z - labA.z, 2));
      }

      float[] fts = new float[4];

      public void GetSplatBrushes(Color c,
         out TextureArrayConfig.BrushData primary, 
         out TextureArrayConfig.BrushData secondary,
         out float blend)
      {
         int pIdx = 0;
         int sIdx = 0;
         float pWeight = 0;
         float sWeight = 1;
         fts[0] = c.r;
         fts[1] = c.g;
         fts[2] = c.b;
         fts[3] = c.a;

         for (int i = 0; i < 4; ++i)
         {
            if (pWeight < fts[i])
            {
               pWeight = fts[i];
               pIdx = i;
            }
         }
         for (int i = 0; i < 4; ++i)
         {
            if (sWeight < fts[i] && i != pIdx)
            {
               sWeight = fts[i];
               sIdx = i;
            }
         }

         float totalWeight = Mathf.Max(pWeight + sWeight, 0.01f);
         float r = 2.0f / totalWeight;
         pWeight *= r;
         pWeight *= 0.5f;

         blend = pWeight;
         primary = mappings[pIdx].brushData;
         secondary = mappings[sIdx].brushData;
      }

      public TextureArrayConfig.BrushData GetBrush(Color c)
      {
         
         if (mode == MatchMode.Color)
         {
            TextureArrayConfig.BrushData brushData = null;
            float chosenDiff = float.MaxValue;
            for (int x = 0; x < mappings.Count; ++x)
            {
               var m = mappings[x];

               if (mode == MatchMode.Color)
               {
                  float diff = GetColorDifference(toLab(c), toLab(m.color));
                  if (diff < chosenDiff)
                  {
                     brushData = m.brushData;
                     chosenDiff = diff;
                  }
               }
            }
            return brushData;
         }
         else if (mode == MatchMode.Index)
         {
            int index = (int)(c.r * 255);
            if (index < mappings.Count)
            {
               return mappings[index].brushData;
            }
            return null;
         }
         return null;
      }
   }
}
