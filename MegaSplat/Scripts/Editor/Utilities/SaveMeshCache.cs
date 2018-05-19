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
   [System.Serializable]
   public class SaveMeshCache : IVertexPainterUtility
   {
      public string GetName() 
      {
         return "Bake Scene to Mesh Cache";
      }

      bool enabledStaticBatching = false;
      bool keepMeshReadWrite = true;
      bool useCurrentJobs = true;

      public void OnGUI(PaintJob[] jobs)
      {
         EditorGUILayout.HelpBox("Bakes all VertexInstanceStreams down to new meshes, and replaces the current meshes with the baked ones", MessageType.Info);

         enabledStaticBatching = EditorGUILayout.Toggle("Enable Static Batching", enabledStaticBatching);
         keepMeshReadWrite = EditorGUILayout.Toggle("Keep Mesh Editable", keepMeshReadWrite);
         useCurrentJobs = EditorGUILayout.Toggle("Process only current selection", useCurrentJobs);

         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Save and Replace"))
         {
            Bake(enabledStaticBatching, keepMeshReadWrite, useCurrentJobs ? jobs : null);
         }

         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
      }

      static bool MeshInUse(MeshFilter[] filters, Mesh m)
      {
         for (int i = 0; i < filters.Length; ++i)
         {
            if (filters[i].sharedMesh == m)
            {
               return true;
            }
         }
         return false;
      }

      public static void Bake(bool enableStaticBatching, bool keepMeshesReadWrite, PaintJob[] jobs = null)
      {
         List<VertexInstanceStream> streams = new List<VertexInstanceStream>();
         if (jobs == null)
         {
            streams = new List<VertexInstanceStream>(GameObject.FindObjectsOfType<VertexInstanceStream>());
         }
         else
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               if (jobs[i]._stream != null)
               {
                  streams.Add(jobs[i]._stream);
               }
            }
         }

         //if (streams.Count == 0)
         //{
         //   Debug.Log("No streams to save");
         //   return;
         //}
         
         var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
         string path = scene.path;
         path = path.Replace("\\", "/");
         if (path.Contains(".unity"))
         {
            path = path.Replace(".unity", "");
         }

         path += "_meshcache";
         string fullFolderPath = Application.dataPath + path.Substring(6);

         System.IO.Directory.CreateDirectory(fullFolderPath);
         path += "/";
         AssetDatabase.Refresh();

         for (int i = 0; i < streams.Count; ++i)
         {
            var stream = streams[i];
            var mf = stream.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
               continue;

            var msci = mf.GetComponent<MegaSplatCollisionInfo>();
            if (msci)
            {
               msci.BakeForSerialize();
               EditorUtility.SetDirty(msci);
            }

            Mesh m = VertexPainterUtilities.BakeDownMesh(mf.sharedMesh, stream);
            string name = mf.gameObject.name;
            m.name = name;

            mf.sharedMesh = m;
            mf.gameObject.isStatic = enableStaticBatching;
            m.UploadMeshData(!keepMeshesReadWrite);

            GameObject.DestroyImmediate(mf.gameObject.GetComponent<VertexInstanceStream>());

            EditorUtility.SetDirty(mf.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mf.gameObject.scene);
            string newpath = AssetDatabase.GenerateUniqueAssetPath(path + name + ".asset");
            AssetDatabase.CreateAsset(m, newpath);

         }

         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
         Selection.activeObject = null;
        
         MeshFilter[] existing = GameObject.FindObjectsOfType<MeshFilter>();
         // now go cleanup anything in that directory which we are not using..
         string[] files = System.IO.Directory.GetFiles(fullFolderPath);
         for (int i = 0; i < files.Length; ++i)
         {
            string f = files[i];

            f = f.Substring(Application.dataPath.Length-6);
            Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(f);
            bool found = false;
            for (int j = 0; j < existing.Length; ++j)
            {
               if (existing[j].sharedMesh == m)
               {
                  found = true;
                  break;
               }
            }
            if (!found)
            {
               AssetDatabase.DeleteAsset(f);
            }

         }
      }

   }
}