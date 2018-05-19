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
   [Node (false, "Math/Add")]
   public class NodeAdd : Node
   {
      public const string ID = "nodeAdd";
      public override string GetID { get { return ID; } }


      public override Node Create (Vector2 pos) 
      {
         NodeAdd node = CreateInstance <NodeAdd> ();

         node.name = "Add";
         node.rect = new Rect (pos.x, pos.y, 100, 60);

         NodeOutput.Create (node, "Out", "Float");
         NodeInput.Create(node, "A", "Float");
         NodeInput.Create(node, "B", "Float");

         return node;
      }

      protected internal override void NodeGUI () 
      {
         Inputs [0].DisplayLayout (new GUIContent ("A", "A"));
         Inputs [1].DisplayLayout (new GUIContent ("B", "B"));
      }

      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = ");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.Append(" + ");
         data.sb.Append(GetOrDefault(1, "0"));
         data.sb.AppendLine(";");
         Outputs[0].varName = name;
      }

   }
}
