//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;


namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "Data/Height")]
   public class NodeHeight : Node 
   {
      public const string ID = "nodeHeight";
      public override string GetID { get { return ID; } }


      public override Node Create (Vector2 pos) 
      {
         NodeHeight node = CreateInstance <NodeHeight> ();

         node.name = "Height";
         node.rect = new Rect (pos.x, pos.y, 72, 90);

         NodeOutput.Create (node, "NodeHeight", "Float");

         return node;
      }

      static Texture2D sIcon;
      protected internal override void NodeGUI () 
      {
         rect = new Rect(rect.x, rect.y, 72, 90);
         if (sIcon == null)
         {
            sIcon = ResourceManager.LoadTexture("Resources/Textures/icon_height");
         }
         RTEditorGUI.DrawTexture(sIcon, 64);
      }


      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         data.Indent();

         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = ");
         data.sb.AppendLine("((worldPos.y - heights.x) / (heights.y - heights.x));");
         Outputs[0].varName = name;
      }
         
   }
}