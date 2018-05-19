//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "MegaSplat/Selector2")]
   public class NodeSelector2 : Node 
   {
      public const string ID = "nodeSelector2";
      public override string GetID { get { return ID; } }

      public float midWeight = 0.5f;
      public string propName = "";

      public override Node Create (Vector2 pos) 
      {
         NodeSelector2 node = CreateInstance <NodeSelector2> ();

         node.name = "Texture Selector 2";
         node.rect = new Rect (pos.x, pos.y, 200, 110);;


         NodeInput.Create(node, "t0", "Int");
         NodeInput.Create(node, "t1", "Int");
         NodeInput.Create(node, "r", "Float");
         NodeInput.Create(node, "Mid", "Float");
         NodeOutput.Create(node, "TextureIndex", "Int");
         return node;
      }

      protected internal override void NodeGUI () 
      {
         Inputs [0].DisplayLayout (new GUIContent ("Tex0", "Texture Index"));
         Inputs [1].DisplayLayout (new GUIContent ("Tex1", "Texture Index"));

         midWeight = Mathf.Clamp(RTEditorGUI.FloatField("Mid Point", midWeight), 0, 1);
         InputKnob(3);
         Inputs [2].DisplayLayout (new GUIContent ("Selection", "Float representing selection weight"));



      }

      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();
         Outputs[0].varName = name;

         propName = data.WritePropertyEntry("Float", "0", "float");

         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = Selector2(");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(1, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(2, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(3, propName));
         data.sb.AppendLine(");");
      } 

      public override void SetProperties(Material mat)
      {
         mat.SetFloat(propName, midWeight);
      }
   }
}
