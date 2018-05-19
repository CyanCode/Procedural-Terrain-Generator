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
   public class TerrainToSplatConverter : ITerrainPainterUtility
   {
      TerrainToMegaSplatConfig config;

      public string GetName()
      {
         return "MegaSplat : Terrain Converter";
      }

      string lastDir = "";
      public void OnGUI(TerrainJob[] jobs)
      {
         if (string.IsNullOrEmpty(lastDir))
         {
            lastDir = EditorPrefs.GetString("TerrainToMegaSplatLastPath");  
         }
         // restore last saved/loaded
         if ((config == null || config.clusterMap.Count == 0) && !string.IsNullOrEmpty(lastDir))
         {
            if (!string.IsNullOrEmpty(lastDir))
            {
               var c = AssetDatabase.LoadAssetAtPath<TerrainToMegaSplatConfig>(lastDir);
               if (c != null)
               {
                  config = c;
               }
               else
               {
                  EditorPrefs.SetString("TerrainToMegaSplatLastPath", ""); 
               }
            }
         }

         EditorGUILayout.Space();
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Load"))
         {
            var path = EditorUtility.OpenFilePanel("Load Config", lastDir, "");
            path = path.Replace("\\", "/");
            path = path.Replace(Application.dataPath, "");
            if (!path.StartsWith("Assets/"))
            {
               path = "Assets/" + path;
            }
            if (!string.IsNullOrEmpty(path))
            {
               lastDir = path;
               var c = AssetDatabase.LoadAssetAtPath<TerrainToMegaSplatConfig>(path);
               if (c != null)
               {
                  config = c;
                  EditorPrefs.SetString("TerrainToMegaSplatLastPath", lastDir);
               }
            }
         }

         if (config == null || GUILayout.Button("New"))
         {
            config = ScriptableObject.CreateInstance<TerrainToMegaSplatConfig>();
            EditorPrefs.SetString("TerrainToMegaSplatLastPath", "");
         }

         if (config != null && GUILayout.Button("Save"))
         {
            if (string.IsNullOrEmpty(lastDir))
            {
               lastDir = "Assets/";
            }
            var path = EditorUtility.SaveFilePanel("Save Config", lastDir, "terrain2megasplat", "");
            path = path.Replace("\\", "/");
            if (path.StartsWith(Application.dataPath))
            {
               path = path.Replace(Application.dataPath, "");
            }
            if (!path.EndsWith(".asset"))
            {
               path += ".asset";
            }
            if (!path.StartsWith("Assets/"))
            {
               path = "Assets/" + path;
            }

            if (!string.IsNullOrEmpty(path))
            {
               lastDir = path;
               EditorPrefs.SetString("TerrainToMegaSplatLastPath", lastDir);
               EditorUtility.SetDirty(config);
               AssetDatabase.DeleteAsset(path);
               AssetDatabase.CreateAsset(config, path);
               AssetDatabase.SaveAssets();

            }
         }
         EditorGUILayout.EndHorizontal();
         EditorGUILayout.Space();
         config.DrawGUI(jobs);
      }
   }
}
