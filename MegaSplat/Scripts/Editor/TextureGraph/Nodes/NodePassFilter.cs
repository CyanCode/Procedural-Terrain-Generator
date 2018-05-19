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
   [Node (false, "Filter/Pass Filter")]
   public class NodePassFilter : Node
   {
      public const string ID = "nodePassFilter";
      public override string GetID { get { return ID; } }


      public Vector4 keys = new Vector4(0, 0.1f, 0.1f, 1);  // 0 val, start, width, endval

      public override Node Create (Vector2 pos) 
      {
         NodePassFilter node = CreateInstance <NodePassFilter> ();

         node.name = "Pass Filter";
         node.rect = new Rect (pos.x, pos.y, 170, 120);

         NodeOutput.Create (node, "Out", "Float");
         NodeInput.Create(node, "In", "Float");

         return node;
      }
      public AnimationCurve curve;
      protected internal override void NodeGUI () 
      {
         RTEditorGUI.BeginChangeCheck();
         UnityEditor.EditorGUIUtility.labelWidth = 30;
         float rangeX = keys.y;
         float rangeY = keys.z;
         RTEditorGUI.MinMaxSlider(ref rangeX, ref rangeY, 0.0f, 1.0f);
         keys.y = rangeX;
         keys.z = rangeY;

         InputKnob(0);
         OutputKnob(0);
         Vector2 v = new Vector2(keys.x, keys.w);
         GUILayout.BeginHorizontal();
         v = RTEditorGUI.Vector2Field("min/max", v);
         keys.x = Mathf.Clamp01(v.x);
         keys.w = Mathf.Clamp01(v.y);

         GUILayout.EndHorizontal();

         if (curve == null)
         {
            Keyframe[] k = new Keyframe[4];

            curve = new AnimationCurve(k);
         }
         var ks = curve.keys;
         ks[0].time = 0;
         ks[0].value = keys.x;
         ks[1].time = keys.y;
         ks[1].value = keys.x;
         ks[2].time = Mathf.Clamp01(keys.y + keys.z);
         ks[2].value = keys.w;
         ks[3].time = 1;
         ks[3].value = keys.w;
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
      public string propName;
      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         propName = data.WritePropertyEntry("Vector", "(0, 0.1, 0.1, 1)", "float4");

         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = lerp(");
         data.sb.Append(propName);
         data.sb.Append(".x, ");
         data.sb.Append(propName);
         data.sb.Append(".w, smoothstep(");
         data.sb.Append(propName);
         data.sb.Append(".y, ");
         data.sb.Append(propName);
         data.sb.Append(".y + ");
         data.sb.Append(propName);
         data.sb.Append(".z, ");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.AppendLine("));");
         Outputs[0].varName = name;
      }

      public override void SetProperties(Material mat)
      {
         mat.SetVector(propName, keys);
      }
   }
}
