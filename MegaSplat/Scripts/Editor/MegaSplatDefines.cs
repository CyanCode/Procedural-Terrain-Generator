//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;

namespace JBooth.MegaSplat
{
   [InitializeOnLoad]
   public class MegaSplatDefines
   {
      const string sMegaSplatDefine = "__MEGASPLAT__";
      static MegaSplatDefines()
      {
         InitDefines();
      }

      static void InitDefines()
      {
         var target = EditorUserBuildSettings.selectedBuildTargetGroup;
         string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
         if ( !defines.Contains( sMegaSplatDefine ) )
         {
            if ( string.IsNullOrEmpty( defines ) )
            {
               PlayerSettings.SetScriptingDefineSymbolsForGroup( target, sMegaSplatDefine );
            }
            else
            {
               if (!defines[ defines.Length - 1 ].Equals(';'))
               {
                  defines += ';'; 
               }
               defines += sMegaSplatDefine;
               PlayerSettings.SetScriptingDefineSymbolsForGroup( target, defines );
            }
         }
      }

      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         InitDefines();  
      }

   }                             
}