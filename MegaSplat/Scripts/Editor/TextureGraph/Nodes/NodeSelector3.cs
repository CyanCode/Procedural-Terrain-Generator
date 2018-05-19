//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "MegaSplat/Selector3")]
   public class NodeSelector3 : Node 
   {
      public const string ID = "nodeSelector";
      public override string GetID { get { return ID; } }

      public Vector2 midWeight = new Vector2(0.33f, 0.66f);

      public string propName;

      public override Node Create (Vector2 pos) 
      {
         NodeSelector3 node = CreateInstance <NodeSelector3> ();

         node.name = "Texture Selector";
         node.rect = new Rect (pos.x, pos.y, 200, 140);;


         NodeInput.Create(node, "t0", "Int");
         NodeInput.Create(node, "t1", "Int");
         NodeInput.Create(node, "t2", "Int");
         NodeInput.Create(node, "r", "Float");
         NodeInput.Create(node, "SideMin", "Float");
         NodeInput.Create(node, "SideMax", "Float");
         NodeOutput.Create(node, "TextureIndex", "Int");
         return node;
      }

      protected internal override void NodeGUI () 
      {
         Inputs [0].DisplayLayout (new GUIContent ("Tex0", "Texture Index"));
         Inputs [1].DisplayLayout (new GUIContent ("Tex1", "Texture Index"));
         Inputs [2].DisplayLayout (new GUIContent ("Tex2", "Texture Index"));

         midWeight.x = Mathf.Clamp(RTEditorGUI.FloatField("Mid Min", midWeight.x), 0, midWeight.y);
         InputKnob(4);
         midWeight.y = Mathf.Clamp(RTEditorGUI.FloatField("Mid Max", midWeight.y), midWeight.x, 1.0f);
         InputKnob(5);
         Inputs [3].DisplayLayout (new GUIContent ("Selection", "Float representing selection weight"));



      }

      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();
         Outputs[0].varName = name;

         propName = data.WritePropertyEntry("Vector", "(0,0,0,0)", "float2");
         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = Selector3(");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(1, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(2, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(3, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(4, propName + ".x"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(5, propName + ".y"));
         data.sb.AppendLine(");");
      } 
     
      public override void SetProperties(Material mat)
      {
         mat.SetVector(propName, midWeight);
      }
   }
}
