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
   [Node (false, "Math/Pow")]
   public class NodePow : Node
   {
      public const string ID = "nodePow";
      public override string GetID { get { return ID; } }


      public override Node Create (Vector2 pos) 
      {
         NodePow node = CreateInstance <NodePow> ();

         node.name = "Pow";
         node.rect = new Rect (pos.x, pos.y, 100, 60);

         NodeOutput.Create (node, "Out", "Float");
         NodeInput.Create(node, "Val", "Float");
         NodeInput.Create(node, "Pow", "Float");

         return node;
      }

      protected internal override void NodeGUI () 
      {
         Inputs [0].DisplayLayout (new GUIContent ("Val", "Value"));
         Inputs [1].DisplayLayout (new GUIContent ("Pow", "Raise to the power of"));
      }

      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = pow(");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(1, "0"));
         data.sb.AppendLine(");");
         Outputs[0].varName = name;
      }

   }
}
