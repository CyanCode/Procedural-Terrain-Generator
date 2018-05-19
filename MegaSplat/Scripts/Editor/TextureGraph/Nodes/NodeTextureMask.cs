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
   [Node (false, "Data/Texture Mask")]
   public class NodeTextureMask : Node
   {
      public const string ID = "nodeTextureMask";
      public override string GetID { get { return ID; } }

      public Texture2D texture;

      public override Node Create (Vector2 pos) 
      {
         NodeTextureMask node = CreateInstance <NodeTextureMask> ();

         node.name = "Texture Mask";
         node.rect = new Rect (pos.x, pos.y, 120, 160);

         NodeOutput.Create (node, "R", "Float");
         NodeOutput.Create (node, "G", "Float");
         NodeOutput.Create (node, "B", "Float");
         NodeOutput.Create (node, "A", "Float");

         return node;
      }

      protected internal override void NodeGUI () 
      {
         Outputs [0].DisplayLayout (new GUIContent ("R", "R"));
         Outputs [1].DisplayLayout (new GUIContent ("G", "G"));
         Outputs [2].DisplayLayout (new GUIContent ("B", "B"));
         Outputs [3].DisplayLayout (new GUIContent ("A", "A"));

         texture = RTEditorGUI.ObjectField<Texture2D>(texture, false);
      }
      public string propName;
      public override void WriteVariables()
      {
         var data = EvalData.data;
         propName = data.WritePropertyEntry("2D", "\"black\"", "sampler2D");
         var name = data.GetNextName();

         data.Indent();
         data.sb.Append("float4 ");
         data.sb.Append(name);
         data.sb.Append(" = tex2D(");
         data.sb.Append(propName);
         data.sb.AppendLine(", UV.xy);");
         Outputs[0].varName = name + ".r";
         Outputs[1].varName = name + ".g";
         Outputs[2].varName = name + ".b";
         Outputs[3].varName = name + ".a";
      }

      public override void SetProperties(Material mat)
      {
         mat.SetTexture(propName, texture);
      }
   }
}
