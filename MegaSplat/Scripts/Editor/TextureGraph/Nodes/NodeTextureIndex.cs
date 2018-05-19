//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "MegaSplat/TextureIndex")]
   public class NodeTextureIndex : Node 
   {
      public const string ID = "nodeTextureIndex";
      public override string GetID { get { return ID; } }

      public int value = 0;

      public override Node Create (Vector2 pos) 
      {
         NodeTextureIndex node = CreateInstance <NodeTextureIndex> ();

         node.name = "Texture Index";
         node.rect = new Rect (pos.x, pos.y, 150, 140);

         NodeOutput.Create (node, "TextureIndex", "Int");

         return node;
      }


      public static int DrawTextureSelector(int value)
      {
         Texture2D tex = Texture2D.blackTexture;
         if (NodeMegaSplatOutput.sConfig != null && value < NodeMegaSplatOutput.sConfig.sourceTextures.Count)
         {
            tex = NodeMegaSplatOutput.sConfig.sourceTextures[value].diffuse;

         }
         RTEditorGUI.DrawTexture(tex, 64);
         if (NodeMegaSplatOutput.sConfig != null)
         {
            if (NodeMegaSplatOutput.sConfig.sourceTextures.Count == 0)
            {
               value = RTEditorGUI.IntField(new GUIContent("Index", "The index of the texture"), value);
            }
            else
            {
               value = RTEditorGUI.IntSlider(value, 0, NodeMegaSplatOutput.sConfig.sourceTextures.Count-1);
               string nm = NodeMegaSplatOutput.sConfig.sourceTextures[value].diffuse == null ? "null" : NodeMegaSplatOutput.sConfig.sourceTextures[value].diffuse.name;
               RTEditorGUI.Label(nm);
            }

         }
         return value;
      }

      protected internal override void NodeGUI () 
      {
         value = DrawTextureSelector(value);
         OutputKnob (0);

      }
      public string propName;
      public override void WriteVariables()
      {
         var data = EvalData.data;
         propName = data.WritePropertyEntry("Int", "0", "int");
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
         mat.SetInt(propName, value);
      }
   }
}