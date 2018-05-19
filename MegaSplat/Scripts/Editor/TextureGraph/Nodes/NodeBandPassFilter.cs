//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace JBooth.MegaSplat.NodeEditorFramework
{

   [System.Serializable]
   [Node (false, "Filter/Band Pass")]
   public class NodeBandPassFilter : Node
   {
      public const string ID = "nodeBandPassFilter";
      public override string GetID { get { return ID; } }

      public Vector2 range = new Vector2(0, 1);
      public Vector3 keys = new Vector3(0.5f, 0.1f, 0.1f); // center, width, blend

      public override Node Create (Vector2 pos) 
      {
         NodeBandPassFilter node = CreateInstance <NodeBandPassFilter> ();

         node.name = "Band Filter";
         node.rect = new Rect (pos.x, pos.y, 180, 150);

         NodeOutput.Create (node, "Out", "Float");
         NodeInput.Create(node, "In", "Float");

         return node;
      }


      public AnimationCurve curve;
      protected internal override void NodeGUI () 
      {
         RTEditorGUI.BeginChangeCheck();
         UnityEditor.EditorGUIUtility.labelWidth = 35;

         keys.x = RTEditorGUI.Slider("center", keys.x, 0, 1);
         InputKnob(0);
         OutputKnob(0);
         keys.y = RTEditorGUI.Slider("blend", keys.y, 0, 1);
         keys.z = RTEditorGUI.Slider("width", keys.z, 0, 1);

         range = RTEditorGUI.Vector2Field("min/max", range);

         if (curve == null)
         {
            Keyframe[] k = new Keyframe[6];

            curve = new AnimationCurve(k);
         }
         var ks = curve.keys;
         ks[0].time = 0;
         ks[0].value = range.x;
         ks[1].time = Mathf.Clamp01(keys.x - keys.y - keys.z);
         ks[1].value = range.x;
         ks[2].time = Mathf.Clamp01(keys.x - keys.z);
         ks[2].value = range.y;
         ks[3].time = Mathf.Clamp01(keys.x + keys.z);
         ks[3].value = range.y;
         ks[4].time = Mathf.Clamp01(keys.x + keys.y + keys.z);
         ks[4].value = range.x;
         ks[5].time = 1;
         ks[5].value = range.x;
         curve.keys = ks;

         GUI.enabled = false;
         curve = RTEditorGUI.CurveField(curve, GUILayout.Height(40));
         GUI.enabled = true;
         UnityEditor.EditorGUIUtility.labelWidth = 0;
         if (RTEditorGUI.EndChangeCheck())
         {
            curve = new AnimationCurve(ks);
         }
      }
      public string propRange;
      public string propKeys;
      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();
         propRange = data.WritePropertyEntry("Vector", "(0,1,0,0)", "half2");
         propKeys = data.WritePropertyEntry("Vector", "(0.5, 0.1, 0.1, 0)", "half3");

         string format = "float {0} = lerp({1}.x, {1}.y, lerp(0.0, 1.0, smoothstep(saturate({2}.x - {2}.y - {2}.z), saturate({2}.x - {2}.y), {3})) * lerp(1.0, 0.0, smoothstep(saturate({2}.x + {2}.y), saturate({2}.x + {2}.y + {2}.z), {3})));";
         data.Indent();
         data.sb.AppendFormat(format, name, propRange, propKeys, GetOrDefault(0, "0"));


         Outputs[0].varName = name;
      }

      public override void SetProperties(Material mat)
      {
         mat.SetVector(propKeys, keys);
         mat.SetVector(propRange, range);
      }
   }
}
