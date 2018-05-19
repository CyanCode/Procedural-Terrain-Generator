//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace JBooth.MegaSplat
{
   class TextureArrayPostProcessor : AssetPostprocessor 
   {
      static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
      {
         var cfgs = Resources.FindObjectsOfTypeAll<TextureArrayConfig>();
         for (int i = 0; i < cfgs.Length; ++i)
         {
            var cfg = cfgs[i];
            int hash = cfg.GetNewHash();
            if (hash != cfg.hash)
            {
               cfg.hash = hash;
               Debug.Log("Rebuilding texture array");
               TextureArrayConfigEditor.CompileConfig(cfg);
               EditorUtility.SetDirty(cfg);
            }
         }
      }
   }
}
