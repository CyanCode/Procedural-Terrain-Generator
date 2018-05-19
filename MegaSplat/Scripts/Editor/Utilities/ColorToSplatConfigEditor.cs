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
   [CustomEditor(typeof(ColorToSplatConfig))]
   public class ColorToSplatConfigEditor : Editor 
   {
      int selectedIndex = 0;

      void DrawLeft(ColorToSplatConfig cfg)
      {
         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField("Key", GUILayout.Width(40));
         if (cfg.mode == ColorToSplatConfig.MatchMode.SplatTexture)
         {
            int count = cfg.mappings.Count;
            if (count > 4)
               count = 4;
            for (int i = 0; i < count; ++i)
            {
               if (GUILayout.Button(i.ToString(), GUILayout.Width(30)))
               {
                  selectedIndex = i;
               }
            }
         }
         else
         {
            for (int i = 0; i < cfg.mappings.Count; ++i)
            {
               var m = cfg.mappings[i];

               if (cfg.mode == ColorToSplatConfig.MatchMode.Color)
               {
                  if (m.previewTex == null)
                  {
                     m.previewTex = new Texture2D(18, 18, TextureFormat.RGB24, false);
                     Color c = m.color;
                     c.a = 1; 
                     for (int x = 0; x < 18; ++x)
                     {
                        for (int y = 0; y < 18; ++y)
                        { 
                           m.previewTex.SetPixel(x, y, c);
                        }
                     }
                     m.previewTex.Apply();

                  }
                  var old = GUI.backgroundColor;
                  if (i == selectedIndex)
                     GUI.backgroundColor = Color.blue;
                  if (GUILayout.Button(m.previewTex, GUILayout.Width(30), GUILayout.Height(30)))
                  {
                     selectedIndex = i;
                  }
                  GUI.backgroundColor = old;
               }
               else if (cfg.mode == ColorToSplatConfig.MatchMode.Index)
               {
                  if (GUILayout.Button(i.ToString(), GUILayout.Width(30)))
                  {
                     selectedIndex = i;
                  }
               }
            }

         }
         if (GUILayout.Button("+", GUILayout.Width(30)))
         {
            cfg.mappings.Add(new ColorToSplatConfig.Mapping());
         }
         EditorGUILayout.EndVertical();
      }

      void DrawRight(ColorToSplatConfig cfg)
      {
         
         if (selectedIndex >= cfg.mappings.Count)
         {
            selectedIndex = cfg.mappings.Count - 1;
         }
         EditorGUILayout.BeginVertical();
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.LabelField("Mapping", GUILayout.Width(80));
         if (GUILayout.Button("Delete Mapping"))
         {
            cfg.mappings.RemoveAt(selectedIndex);
         }
         EditorGUILayout.EndHorizontal();
         EditorGUI.indentLevel++;
         var m = cfg.mappings[selectedIndex];
         if (cfg.mode == ColorToSplatConfig.MatchMode.Color)
         {
            var oldCol = m.color;
            m.color = EditorGUILayout.ColorField("Color", m.color);
            if (oldCol != m.color)
            {
               if (m.previewTex != null)
                  DestroyImmediate(m.previewTex);
            }
         }
      
         m.brushData.DrawGUI(cfg.textureConfig);
         EditorGUI.indentLevel--;
         EditorGUILayout.EndVertical();
      }

      public override void OnInspectorGUI()
      {
         EditorGUI.BeginChangeCheck();
         ColorToSplatConfig cfg = target as ColorToSplatConfig;

         cfg.textureConfig = EditorGUILayout.ObjectField("Texture Config", cfg.textureConfig, typeof(TextureArrayConfig), false) as TextureArrayConfig;
         if (cfg.textureConfig == null)
         {
            return;
         }

         if (cfg.mappings == null)
         {
            cfg.mappings = new List<ColorToSplatConfig.Mapping>();
         }
         cfg.mode = (ColorToSplatConfig.MatchMode)EditorGUILayout.EnumPopup("Mapping Mode", cfg.mode);

         if (cfg.mode == ColorToSplatConfig.MatchMode.SplatTexture)
         {
            while (cfg.mappings.Count < 4)
            {
               cfg.mappings.Add(new ColorToSplatConfig.Mapping());
            }
         }
         if (cfg.mappings.Count == 0)
         {
            cfg.mappings.Add(new ColorToSplatConfig.Mapping());
         }

         EditorGUILayout.Space();
         GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
         if (cfg.mappings.Count > 0)
         {
            
            EditorGUILayout.BeginHorizontal();
            DrawLeft(cfg);
            GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));
            DrawRight(cfg);
            EditorGUILayout.EndHorizontal();
         }
         if (EditorGUI.EndChangeCheck())
         {
            EditorUtility.SetDirty(cfg);
         }
      }

   }
}
