//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using JBooth.MegaSplat.NodeEditorFramework;
using System.Text;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "Float")]
   public class NodeValue : Node 
   {
      public const string ID = "nodeValue";
      public override string GetID { get { return ID; } }

      public float value = 0.0f;
      public string propName = "";

      public override Node Create (Vector2 pos) 
      {
         NodeValue node = CreateInstance <NodeValue> ();

         node.name = "Value";
         node.rect = new Rect (pos.x, pos.y, 120, 50);

         NodeOutput.Create (node, "NodeValue", "Float");

         return node;
      }

      protected internal override void NodeGUI () 
      {
         value = RTEditorGUI.FloatField (new GUIContent ("Value", "The input value of type float"), value);

         OutputKnob (0);
      }


      public override void WriteVariables()
      {
         var data = EvalData.data;

         propName = data.WritePropertyEntry("Float", "0", "float");


         var name = data.GetNextName();
         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = ");
         data.sb.Append(propName);
         data.sb.AppendLine(";");
         Outputs[0].varName = name;

      } 

      public override void SetProperties(Material mat)
      {
         mat.SetFloat(propName, value);
      }


   }
}