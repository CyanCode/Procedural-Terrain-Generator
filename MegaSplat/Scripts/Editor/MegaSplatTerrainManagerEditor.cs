using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MegaSplatTerrainManager))]
public class MegaSplatTerrainManagerEditor : Editor 
{
   public override void OnInspectorGUI()
   {
      DrawDefaultInspector();
      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Sync"))
      {
         var mgr = target as MegaSplatTerrainManager;
         mgr.Sync();
      }
      if (GUILayout.Button("Sync All"))
      {
         MegaSplatTerrainManager.SyncAll();
      }
      EditorGUILayout.EndHorizontal();
   }
}
