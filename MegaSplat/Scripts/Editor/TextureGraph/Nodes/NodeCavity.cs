//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;


namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "Data/Cavity")]
   public class NodeCavity : Node 
   {
      public const string ID = "nodeCavity";
      public override string GetID { get { return ID; } }


      public override Node Create (Vector2 pos) 
      {
         NodeCavity node = CreateInstance <NodeCavity> ();

         node.name = "Cavity";
         node.rect = new Rect (pos.x, pos.y, 72, 90);

         NodeOutput.Create (node, "NodeCavity", "Float");

         return node;
      }

      static Texture2D sIcon;
      protected internal override void NodeGUI () 
      {
         rect = new Rect(rect.x, rect.y, 72, 90);
         if (sIcon == null)
         {
            sIcon = ResourceManager.LoadTexture("Resources/Textures/icon_cavity");
         }
         RTEditorGUI.DrawTexture(sIcon, 64);
      }


      public override void WriteVariables()
      {
         Outputs[0].varName = "cavity";
      }

   }
}