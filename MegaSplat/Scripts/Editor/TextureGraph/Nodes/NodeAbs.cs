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
   [Node (false, "Math/Abs")]
   public class NodeAbs : Node
   {
      public const string ID = "nodeAbs";
      public override string GetID { get { return ID; } }


      public override Node Create (Vector2 pos) 
      {
         NodeAbs node = CreateInstance <NodeAbs> ();

         node.name = "Abs";
         node.rect = new Rect (pos.x, pos.y, 100, 60);

         NodeOutput.Create (node, "Out", "Float");
         NodeInput.Create(node, "In", "Float");

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
         data.sb.Append(" = abs(");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.AppendLine(");");
         Outputs[0].varName = name;
      }

   }
}
