//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;


namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "Filter/Remap Curve")]
   public class NodeRemapCurve : Node 
   {
      public const string ID = "nodeRemapCurve";
      public override string GetID { get { return ID; } }

      public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
      public int index;

      public override Node Create (Vector2 pos) 
      {
         NodeRemapCurve node = CreateInstance <NodeRemapCurve> ();

         node.name = "Curve";
         node.rect = new Rect (pos.x, pos.y, 200, 148);

         NodeInput.Create(node, "In", "Float");
         NodeOutput.Create (node, "Out", "Float");

         return node;
      }

      protected internal override void NodeGUI () 
      {
         rect = new Rect (rect.x, rect.y, 200, 148);
         curve = RTEditorGUI.CurveField(curve, GUILayout.Width(186), GUILayout.Height(120));
      }


      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         index = data.GetNextCurveIndex();

         data.Indent();

         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.AppendLine(" = UNITY_SAMPLE_TEX2DARRAY(_CurveArray, float3(" + GetOrDefault(0, "0") + ", 0.5, " + index + ")).a;" );
         Outputs[0].varName = name;
      }

      static Color[] colors = new Color[256];
      public override void SetProperties(Material mat)
      {
         for (int i = 0; i < 256; ++i)
         {
            float v = curve.Evaluate((float)i / 255.0f);
            colors[i] = new Color(v, v, v, v);
         }
         EvalData.curves.SetPixels(colors, index);
      }
   }
}