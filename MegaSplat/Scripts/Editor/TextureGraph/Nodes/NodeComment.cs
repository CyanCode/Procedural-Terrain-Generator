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
   [Node (false, "Comment")]
   public class NodeComment : Node
   {
      public const string ID = "nodeComment";
      public override string GetID { get { return ID; } }

      public string text = "";

      public override Node Create (Vector2 pos) 
      {
         NodeComment node = CreateInstance <NodeComment> ();

         node.name = "Comment";
         node.rect = new Rect (pos.x, pos.y, 240, 120);

         return node;
      }

      protected internal override void NodeGUI () 
      {
         text = RTEditorGUI.TextArea(text, 95);
      }



   }
}
