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
   [Node (false, "Data/Noise3D")]
   public class NodeNoise3D : Node
   {
      public const string ID = "nodeNoise3D";
      public override string GetID { get { return ID; } }

      public enum Space
      {
         Local,
         World
      }

      public Space space = Space.World;
      public float frequency = 0.05f;
      public float amplitude = 1;
      public float freqSlider = 1;
      public float offsetSlider = 0;

      public string propName;

      public override Node Create (Vector2 pos) 
      {
         NodeNoise3D node = CreateInstance <NodeNoise3D> ();

         node.name = "Noise3D";
         node.rect = new Rect (pos.x, pos.y, 200, 120);

         NodeOutput.Create (node, "NodeNoise3D", "Float");
         NodeInput.Create(node, "frequency", "Float");
         NodeInput.Create(node, "amplitude", "Float");

         return node;
      }

      protected internal override void NodeGUI () 
      {
         rect = new Rect (rect.x, rect.y, 200, 120);

         space = (Space)RTEditorGUI.EnumPopup("Space", space);
         frequency = RTEditorGUI.FloatField (new GUIContent ("Frequency", "frequency of noise"), frequency);
         InputKnob (0);
         amplitude = RTEditorGUI.FloatField (new GUIContent ("Amplitude", "frequency of noise"), amplitude);
         InputKnob (1);
         freqSlider = RTEditorGUI.Slider("freq", freqSlider, 0.5f, 2.0f);
         offsetSlider = RTEditorGUI.Slider("offset", offsetSlider, 0, 100);

      }

      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();
         Outputs[0].varName = name;

         propName = data.WritePropertyEntry("Vector", "(0.05, 1, 1, 0)", "float4");

         data.Indent();
         data.sb.Append("float ");
         data.sb.Append(name);
         data.sb.Append(" = FBM3D(");

         data.sb.Append(space == Space.Local ? "(localPos + float3(0, 0, " + propName + ".w)) * " : "(worldPos + float3(0, 0, " + propName + ".w)) * ");  
         data.sb.Append((Inputs[0].connection != null) ? Inputs[0].connection.varName : propName + ".x");
         data.sb.Append(" * ");
         data.sb.Append(propName);
         data.sb.Append(".z) * ");
         data.sb.Append((Inputs[1].connection != null) ? Inputs[1].connection.varName : propName + ".y");
         data.sb.AppendLine(";");
      
      } 

      public override void SetProperties(Material mat)
      {
         mat.SetVector(propName, new Vector4(frequency, amplitude, freqSlider, offsetSlider));
      }

   }
}
