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
   [Node (false, "Filter/Multi-Band")]
   public class NodeMultiBand2 : Node
   {
      public const string ID = "nodeMultiBand2";
      public override string GetID { get { return ID; } }

      public int tab = 0;
      string[] tabLabels = new string[] {"0", "1", "2", "3" };

      public override Node Create (Vector2 pos) 
      {
         NodeMultiBand2 node = CreateInstance <NodeMultiBand2> ();

         node.name = "MultiBand";
         node.rect = new Rect (pos.x, pos.y, 180, 300);

         NodeInput.Create(node, "val", "Float");
         NodeInput.Create(node, "Tex1", "Int");
         NodeInput.Create(node, "Tex2", "Int");
         NodeInput.Create(node, "Tex3", "Int");
         NodeInput.Create(node, "Tex4", "Int");
         NodeOutput.Create(node, "Blend", "Float");
         NodeOutput.Create(node, "Tex", "Int");

         return node;
      }

      [System.Serializable]
      public class Band
      {
         public bool active = false;
         public float center = 0.1f;
         public float width = 0.05f;
         public float blend = 0.05f;
         public Vector2 range = new Vector2(0, 1);
         public float noiseAmp = 0;
         public float noiseFreq = 0.05f;

         public int index = 0;

         public void DrawGUI()
         {
            active = RTEditorGUI.Toggle(active, "active");
            GUI.enabled = active;
            center = RTEditorGUI.Slider("center", center, 0, 1);
            width = RTEditorGUI.Slider("width", width, 0, 1);
            blend = RTEditorGUI.Slider("blend", blend, 0, 1);
            range = RTEditorGUI.Vector2Field("range", range);
            noiseAmp = RTEditorGUI.Slider("noise", noiseAmp, 0, 2);
            noiseFreq = RTEditorGUI.FloatField("freq", noiseFreq);
            RTEditorGUI.Space();
            GUI.enabled = true;
         }
      }

      public Band[] bands = new Band[4];



      protected internal override void NodeGUI () 
      {
         if (bands[0] == null)
         {
            for (int i = 0; i < bands.Length; ++i)
            {
               bands[i] = new Band();
               bands[i].center = ((float)i)/(bands.Length+1) + 0.1f;
               bands[i].index = i;
            }
            bands[0].active = true;
         }

         Inputs [0].DisplayLayout (new GUIContent ("val", "float value, height, angle, etc"));
         OutputKnob(0);
         Inputs [1].DisplayLayout (new GUIContent ("tex0", "first texture"));
         OutputKnob(1);
         Inputs [2].DisplayLayout (new GUIContent ("tex1", "second texture"));
         Inputs [3].DisplayLayout (new GUIContent ("tex2", "third texture"));
         Inputs [4].DisplayLayout (new GUIContent ("tex3", "fourth texture"));

         RTEditorGUI.Space();


         UnityEditor.EditorGUIUtility.labelWidth = 38;
         tab = GUILayout.Toolbar(tab, tabLabels);
         bands[tab].DrawGUI();
         UnityEditor.EditorGUIUtility.labelWidth = 0;
      }

      public string[] propBandRange = new string[4];
      public string[] propBandKeys = new string[4];

      List<Band> sortBands;
      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();

         // sort bands..
         sortBands = new List<Band>(bands);
         sortBands.Sort(delegate(Band x, Band y)
         {
            return x.center.CompareTo(y.center);
         });
            
         // remove inactive
         for (int i = 0; i < sortBands.Count; ++i)
         {
            Band b = sortBands[i];
            if (b.active == false)
            {
               sortBands.RemoveAt(i);
               --i;
            }
         }

         if (sortBands.Count == 0)
         {
            Outputs[0].varName = "0";
            Outputs[1].varName = "0";
            return;
         }

         // make props
         for (int i = 0; i < sortBands.Count; ++i)
         {
            propBandRange[i] = data.WritePropertyEntry("Vector", "(0,1,0.05,1)", "half4");
            propBandKeys[i] = data.WritePropertyEntry("Vector", "(0.5, 0.03, 0.03, 0)", "half3");
         }


         //float2 BandPass(float x, float t0, float t1, float2 range, float3 cwb, range2, cwb2, etc..)

         data.Indent();
         data.sb.Append("float2 ");
         data.sb.Append(name);
         data.sb.Append(" = BandPass(worldPos, ");
         data.sb.Append(GetOrDefault(0, "0"));
         data.sb.Append(", ");

         for (int i = 0; i < sortBands.Count; ++i)
         {
            data.sb.Append(GetOrDefault(sortBands[i].index + 1, "0"));
            data.sb.Append(", ");
         }
         data.sb.Append("0");

         for (int i = 0; i < sortBands.Count; ++i)
         {
            data.sb.Append(", ");
            data.sb.Append(propBandRange[i]);
            data.sb.Append(", ");
            data.sb.Append(propBandKeys[i]);
         }
         data.sb.AppendLine(");");

         var intname = data.GetNextName();
         data.Indent();
         data.sb.AppendLine("int " + intname + " = (int)" + name + ".y;");


         Outputs[0].varName = name + ".x";
         Outputs[1].varName = intname;
      }

      public override void SetProperties(Material mat)
      {
         for (int i = 0; i < sortBands.Count; ++i)
         {
            var band = sortBands[i];
            mat.SetVector(propBandKeys[i], new Vector3(band.center, band.width, band.blend));
            mat.SetVector(propBandRange[i], new Vector4(band.range.x, band.range.y, band.noiseFreq, band.noiseAmp));
         }
      }
   }
}
