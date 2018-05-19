//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "Data/Normal Angle")]
   public class NodeNormalAngle : Node 
   {
      public const string ID = "nodeNormalAngle";
      public override string GetID { get { return ID; } }

      public enum Space
      {
         Local,
         World
      }

      public Vector3 upVec = Vector3.up;
      public Space space = Space.World;

      public float value = 0.0f;

      public override Node Create (Vector2 pos) 
      {
         NodeNormalAngle node = CreateInstance <NodeNormalAngle> ();

         node.name = "Normal Angle";
         node.rect = new Rect (pos.x, pos.y, 180, 132);

         NodeOutput.Create (node, "NodeNormalAngle", "Float");

         return node;
      }
      static Texture2D sIcon;
      protected internal override void NodeGUI () 
      {
         rect = new Rect(rect.x, rect.y, 180, 132);
         if (sIcon == null)
         {
            sIcon = ResourceManager.LoadTexture("Resources/Textures/icon_angle");
         }
         RTEditorGUI.DrawTexture(sIcon, 64);
         space = (Space)RTEditorGUI.EnumPopup(space);
         upVec = RTEditorGUI.Vector3Field("", upVec).normalized; 
      }

      public string propName;
      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         propName = data.WritePropertyEntry("Vector", "(0,1,0,0)", "float3");
         data.Indent();

         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = dot(");
         data.sb.Append(space == Space.Local ? "localNormal" : "worldNormal");
         data.sb.Append(", " + propName + ") * 0.5f + 0.5f;");
         Outputs[0].varName = name;
      }

      public override void SetProperties(Material mat)
      {
         mat.SetVector(propName, upVec);
      }

   }
}