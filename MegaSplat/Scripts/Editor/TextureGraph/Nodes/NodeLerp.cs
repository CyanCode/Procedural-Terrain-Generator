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
   [Node (false, "Math/Lerp")]
   public class NodeLerp : Node
   {
      public const string ID = "nodeLerp";
      public override string GetID { get { return ID; } }


      public override Node Create (Vector2 pos) 
      {
         NodeLerp node = CreateInstance <NodeLerp> ();

         node.name = "Lerp";
         node.rect = new Rect (pos.x, pos.y, 100, 60);

         NodeOutput.Create (node, "Out", "Float");
         NodeInput.Create(node, "a", "Float");
         NodeInput.Create(node, "b", "Float");
         NodeInput.Create(node, "t", "Float");

         return node;
      }

      protected internal override void NodeGUI () 
      {
      }

      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = lerp(");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(1, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(2, "0"));
         data.sb.AppendLine(");");
         Outputs[0].varName = name;
      }
   }
}
