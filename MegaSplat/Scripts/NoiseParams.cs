//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JBooth.MegaSplat
{
   [System.Serializable]
   public class NoiseParams
   {
      public float frequency = 1.0f;
      public float amplitude = 1.0f;
      public float blend = 0.5f;       // center point of value

      Texture2D preview;

      #if UNITY_EDITOR
      public void DrawGUI()
      {

         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.BeginVertical();
         EditorGUI.BeginChangeCheck();
         frequency = EditorGUILayout.FloatField("frequency", frequency);
         amplitude = EditorGUILayout.FloatField("amplitude", amplitude);
         blend = EditorGUILayout.Slider("blend", blend, 0.0f, 1.0f);
         bool render = false;
         if (EditorGUI.EndChangeCheck())
            render = true;
         EditorGUILayout.EndVertical();

         int texSize = 64;
         if (preview == null || preview.width != texSize)
         {
            preview = new Texture2D(64, 64, TextureFormat.Alpha8, false);
            render = true;
         }
         if (render)
         {
            // don't show frequency, otherwise we get lost in scaling
            float scale = 1.0f/Mathf.Min(Mathf.Abs(frequency)) * 2;
            for (int x = 0; x < texSize; ++x)
            {
               for (int y = 0; y < texSize; ++y)
               {
                  float px = (float)x / texSize * scale;
                  float py = (float)y / texSize * scale;

                  float n = GetNoise(new Vector3(px, py, 0));
                  preview.SetPixel(x, y, new Color(n,n,n,n));
               }
            }
            preview.Apply();
         }
         EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(texSize), GUILayout.Height(texSize)), preview);
         EditorGUILayout.EndHorizontal();
      }
      #endif

      /*
      Vector3 hash( Vector3 p )
      {
         p = new Vector3( Vector3.Dot(p,new Vector3(127.1f,311.7f, 74.7f)),
            Vector3.Dot(p,new Vector3(269.5f,183.3f,246.1f)),
            Vector3.Dot(p,new Vector3(113.5f,271.9f,124.6f)));

         p.x = Mathf.Sin(p.x);
         p.y = Mathf.Sin(p.y);
         p.z = Mathf.Sin(p.z);

         p *= 43758.5453123f;
         p.x -= (int)p.x;
         p.y -= (int)p.y;
         p.z -= (int)p.z;
         p *= 2;
         p -= Vector3.one;
         return p;
      }
         

      float noise(float x, float y, float z )
      {
         Vector3 p = new Vector3(x, y, z);
         Vector3 i = new Vector3((int)p.x, (int)p.y, (int)p.z);
         Vector3 f = p - i;
         Vector3 inr = f * 2;
         inr.x = 3.0f - inr.x;
         inr.y = 3.0f - inr.y;
         inr.z = 3.0f - inr.z;
         Vector3 u = f;
         u.x = u.x * f.x * inr.x;
         u.y = u.y * f.y * inr.y;
         u.z = u.z * f.z * inr.z;


         return Mathf.Lerp( Mathf.Lerp( Mathf.Lerp( Vector3.Dot( hash( i ), f ), 
            Vector3.Dot( hash( i + new Vector3(1.0f,0.0f,0.0f) ), f - new Vector3(1.0f,0.0f,0.0f) ), u.x),
            Mathf.Lerp( Vector3.Dot( hash( i + new Vector3(0.0f,1.0f,0.0f) ), f - new Vector3(0.0f,1.0f,0.0f) ), 
               Vector3.Dot( hash( i + new Vector3(1.0f,1.0f,0.0f) ), f - new Vector3(1.0f,1.0f,0.0f) ), u.x), u.y),
            Mathf.Lerp( Mathf.Lerp( Vector3.Dot( hash( i + new Vector3(0.0f,0.0f,1.0f) ), f - new Vector3(0.0f,0.0f,1.0f) ), 
               Vector3.Dot( hash( i + new Vector3(1.0f,0.0f,1.0f) ), f - new Vector3(1.0f,0.0f,1.0f) ), u.x),
               Mathf.Lerp( Vector3.Dot( hash( i + new Vector3(0.0f,1.0f,1.0f) ), f - new Vector3(0.0f,1.0f,1.0f) ), 
                  Vector3.Dot( hash( i + new Vector3(1.0f,1.0f,1.0f) ), f - new Vector3(1.0f,1.0f,1.0f) ), u.x), u.y), u.z );
      }
*/

      public float GetNoise(Vector3 pos)
      {
         pos.x *= frequency;
         pos.y *= frequency;
         pos.z *= frequency;

         /*
         float n = noise(pos.x, pos.y, pos.z) * amplitude * 0.5f;
         n += noise(pos.y * 2.01f, pos.z * 2.02f, pos.x * 2.03f) * amplitude * 0.3125f;
         n += noise(pos.z * 4.01f, pos.x * 4.02f, pos.y * 4.03f) * amplitude * 0.1875f;
         return Mathf.Clamp01(blend + n);
*/

         float n = Noise.Generate(pos.x, pos.y, pos.z) * amplitude * 0.5f;
         n += Noise.Generate(pos.y * 2.01f, pos.z * 2.02f, pos.x * 2.03f) * amplitude * 0.3125f;
         n += Noise.Generate(pos.z * 4.01f, pos.x * 4.02f, pos.y * 4.03f) * amplitude * 0.1875f;
         return Mathf.Clamp01(blend + n);

      }
   }
}