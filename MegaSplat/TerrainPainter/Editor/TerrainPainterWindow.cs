//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

// Paints megasplat data onto Unity Terrains

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.TerrainPainter
{
   public partial class TerrainPainterWindow : EditorWindow 
   {
      [MenuItem("Window/Terrain Painter")]
      public static void ShowWindow()
      {
         var window = GetWindow<JBooth.TerrainPainter.TerrainPainterWindow>();
         window.InitTerrains();
         window.Show();
      }

      bool enabled = true;


      TerrainJob[] terrains;
      bool[] jobEdits;

      TerrainJob FindJob(Terrain t)
      {
         if (terrains == null)
            return null;
         for (int i = 0; i < terrains.Length; ++i)
         {
            if (terrains[i].terrain == t)
               return terrains[i];
         }
         return null;
      }

      List<Terrain> rawTerrains = new List<Terrain>();

      void InitTerrains()
      {
         Object[] objs = Selection.GetFiltered(typeof(Terrain), SelectionMode.Editable | SelectionMode.OnlyUserModifiable | SelectionMode.Deep);
         List<TerrainJob> ts = new List<TerrainJob>();
         rawTerrains.Clear();
         for (int i = 0; i < objs.Length; ++i)
         {
            Terrain t = objs[i] as Terrain;
            rawTerrains.Add(t);
            if (t.materialType == Terrain.MaterialType.Custom && t.materialTemplate != null)
            {
               if (!t.materialTemplate.HasProperty("_SplatControl"))
                  continue;
               if (t.materialTemplate.GetTexture("_SplatControl") == null)
               {
                  CreateTexture(t);
               }

               var tj = FindJob(t);
               if (tj != null)
               {
                  tj.collider = t.GetComponent<Collider>();
                  tj.terrainTex = t.materialTemplate.GetTexture("_SplatControl") as Texture2D;
                  if (t.materialTemplate.HasProperty("_SplatParams"))
                  {
                     tj.terrainParams = t.materialTemplate.GetTexture("_SplatParams") as Texture2D;
                  }
                  ts.Add(tj);
               }
               else
               {
                  tj = TerrainJob.CreateInstance<TerrainJob>();
                  tj.terrain = t;
                  tj.collider = t.GetComponent<Collider>();
                  tj.terrainTex = t.materialTemplate.GetTexture("_SplatControl") as Texture2D;
                  if (t.materialTemplate.HasProperty("_SplatParams"))
                  {
                     tj.terrainParams = t.materialTemplate.GetTexture("_SplatParams") as Texture2D;
                  }
                  ts.Add(tj);
               }
            }
         }
         if (terrains != null)
         {
            // clear out old terrains
            for (int i = 0; i < terrains.Length; ++i)
            {
               if (!ts.Contains(terrains[i]))
               {
                  DestroyImmediate(terrains[i]);
               }
            }
         }

         terrains = ts.ToArray();
         jobEdits = new bool[ts.Count];
      }

      void OnSelectionChange()
      {
         InitTerrains();
         this.Repaint();
      }

      void OnFocus() 
      {
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         SceneView.onSceneGUIDelegate += this.OnSceneGUI;

         Undo.undoRedoPerformed -= this.OnUndo;
         Undo.undoRedoPerformed += this.OnUndo;

         this.titleContent = new GUIContent("Terrain Paint");
         InitTerrains();
         Repaint();
      }

      void OnUndo()
      {
         if (terrains == null)
            return;
         for (int i = 0; i < terrains.Length; ++i)
         {
            if (terrains[i] != null)
            {
               terrains[i].RestoreUndo();
            }
         }
         Repaint();
      }

      void OnInspectorUpdate()
      {
         // unfortunate...
         Repaint ();
      }

      void OnDestroy() 
      {
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         terrains = null;
      }


   }
}

