//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using JBooth.MegaSplat;

[CustomEditor(typeof(RuntimePainting))]
public class RuntimePaintingEditor : Editor 
{
   public override void OnInspectorGUI()
   {
      EditorGUI.BeginChangeCheck();
      RuntimePainting rp = target as RuntimePainting;

      rp.textureList = (MegaSplatTextureList)EditorGUILayout.ObjectField("Texture List", rp.textureList, typeof(MegaSplatTextureList), false);
      rp.mode = (RuntimePainting.Mode)EditorGUILayout.EnumPopup("Mode", rp.mode);
      if (rp.mode == RuntimePainting.Mode.Texture)
      {
         if (rp.textureList != null)
         {
            rp.textureIndex = EditorGUILayout.Popup("Texture", rp.textureIndex, rp.textureList.textureNames);
         }
         else
         {
            rp.textureIndex = EditorGUILayout.IntField("Texture index", rp.textureIndex); 
         }
      }
      else if (rp.mode == RuntimePainting.Mode.TextureCluster)
      {
         rp.clusterNoiseScale = EditorGUILayout.FloatField("Cluster Noise Scale", rp.clusterNoiseScale);
         if (rp.textureList == null)
         {
            EditorGUILayout.HelpBox("Need to assign a texture list to use cluster mode", MessageType.Warning);
         }
         else
         {
            GUIContent[] names = new GUIContent[rp.textureList.clusters.Length];
            for (int i = 0; i < names.Length; ++i)
            {
               names[i] = new GUIContent();
               names[i].text = rp.textureList.clusters[i].name;
               if (rp.textureList.clusters[i].previewTex != null)
               {
                  names[i].image = rp.textureList.clusters[i].previewTex;
               }
            }
            rp.textureIndex = EditorGUILayout.Popup(new GUIContent("Cluster"), rp.textureIndex, names);
         }
      }
      EditorGUILayout.Space();
      rp.layerMode = (RuntimePainting.LayerMode)EditorGUILayout.EnumPopup("Layer Mode", rp.layerMode);
      rp.brushSize = EditorGUILayout.FloatField("Brush Size", rp.brushSize);
      rp.brushFlow = EditorGUILayout.FloatField("Brush Flow", rp.brushFlow);
      rp.brushFalloff = EditorGUILayout.FloatField("Brush Falloff", rp.brushFalloff);
      rp.targetWeight = EditorGUILayout.Slider("Target Weight", rp.targetWeight, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
         EditorUtility.SetDirty(rp);
      }
   }
}
