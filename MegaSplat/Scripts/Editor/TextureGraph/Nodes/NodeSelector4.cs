//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "MegaSplat/Selector4")]
   public class NodeSelector4 : Node 
   {
      public const string ID = "nodeSelector4";
      public override string GetID { get { return ID; } }

      public Vector3 midWeight = new Vector3(0.25f, 0.5f, 0.75f);
      public string propName;

      public override Node Create (Vector2 pos) 
      {
         NodeSelector4 node = CreateInstance <NodeSelector4> ();

         node.name = "Texture Selector";
         node.rect = new Rect (pos.x, pos.y, 200, 180);;


         NodeInput.Create(node, "t0", "Int");
         NodeInput.Create(node, "t1", "Int");
         NodeInput.Create(node, "t2", "Int");
         NodeInput.Create(node, "t3", "Int");
         NodeInput.Create(node, "r", "Float");
         NodeInput.Create(node, "Min", "Float");
         NodeInput.Create(node, "Mid", "Float");
         NodeInput.Create(node, "Max", "Float");
         NodeOutput.Create(node, "TextureIndex", "Int");
         return node;
      }

      protected internal override void NodeGUI () 
      {
         Inputs [0].DisplayLayout (new GUIContent ("Tex0", "Texture Index"));
         Inputs [1].DisplayLayout (new GUIContent ("Tex1", "Texture Index"));
         Inputs [2].DisplayLayout (new GUIContent ("Tex2", "Texture Index"));
         Inputs [3].DisplayLayout (new GUIContent ("Tex3", "Texture Index"));

         midWeight.x = Mathf.Clamp(RTEditorGUI.FloatField("Min", midWeight.x), 0, midWeight.y);
         InputKnob(5);
         midWeight.y = Mathf.Clamp(RTEditorGUI.FloatField("Mid", midWeight.y), midWeight.x, 1.0f);
         InputKnob(6);
         midWeight.z = Mathf.Clamp(RTEditorGUI.FloatField("Max", midWeight.z), midWeight.z, 1.0f);
         InputKnob(7);
         Inputs [4].DisplayLayout (new GUIContent ("Selection", "Float representing selection weight"));



      }

      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();
         Outputs[0].varName = name;

         propName = data.WritePropertyEntry("Vector", "(0,0,0,0)", "float3");

         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = Selector4(");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(1, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(2, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(3, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(4, "0"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(5, propName + ".x"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(6, propName + ".y"));
         data.sb.Append(", ");
         data.sb.Append(GetOrDefault(7, propName + ".z"));
         data.sb.AppendLine(");");
      } 

      public override void SetProperties(Material mat)
      {
         mat.SetVector(propName, midWeight);
      }
   }
}
