//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////
#if AMPLIFY_SHADER_EDITOR
using UnityEngine;
using System.Collections;
using AmplifyShaderEditor;
using System;
using UnityEditor;


namespace JBooth.MegaSplat
{
   [Serializable]
   [NodeAttributes( "MegaSplat Blend Layer", "MegaSplat", "Compute blend factor for two height layers" )]
   public sealed class MegaSplatBlendLayer : ParentNode
   {
      private string m_functionBody = string.Empty;
      private const string FunctionHeader = "BlendHeightLayer( {0},{1},{2},{3})";
      [SerializeField]
      private float defaultContrast = 0.5f;

      protected override void CommonInit( int uniqueId )
      {
         base.CommonInit( uniqueId );
         AddInputPort(WirePortDataType.FLOAT, true, "Height A");
         AddInputPort(WirePortDataType.FLOAT, true, "Height B");
         AddInputPort(WirePortDataType.FLOAT, true, "Slope");
         AddInputPort(WirePortDataType.FLOAT, true, "Contrast");
         AddOutputPort(WirePortDataType.FLOAT, "Blend");



         IOUtils.AddFunctionHeader(ref m_functionBody, "half BlendHeightLayer(half h1, half h2, half slope, half contrast)");
         IOUtils.AddFunctionLine(ref m_functionBody, "half width = contrast;");
         IOUtils.AddFunctionLine(ref m_functionBody, "contrast = 1 - contrast;");
         IOUtils.AddFunctionLine(ref m_functionBody, "h2 = 1 - h2;");
         IOUtils.AddFunctionLine(ref m_functionBody, "half tween = saturate((slope - min(h1, h2)) / max(abs(h1 - h2), 0.001));");
         IOUtils.AddFunctionLine(ref m_functionBody, "return saturate((tween - contrast) / max(width, 0.001));");
         IOUtils.CloseFunctionBody( ref m_functionBody );
      }


      public override void DrawProperties()
      {
         EditorGUI.BeginChangeCheck();
         base.DrawProperties();
         defaultContrast = EditorGUILayout.Slider("Contrast", defaultContrast, 0.0f, 0.999f);
      }

      // should make ASE vertex shader node public const for this
      public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
      {
         string h1 = InputPorts[0].GenerateShaderForOutput(ref dataCollector, WirePortDataType.FLOAT, true);
         string h2 = InputPorts[1].GenerateShaderForOutput(ref dataCollector, WirePortDataType.FLOAT, true);
         string slope = InputPorts[2].GenerateShaderForOutput(ref dataCollector, WirePortDataType.FLOAT, true);
         string c = InputPorts[3].GenerateShaderForOutput(ref dataCollector, WirePortDataType.FLOAT, true);

         dataCollector.AddToLocalVariables(UniqueId, "half h1_" + UniqueId + " = " + h1 + ";");
         dataCollector.AddToLocalVariables(UniqueId, "half h2_" + UniqueId + " = " + h2 + ";");
         dataCollector.AddToLocalVariables(UniqueId, "half slope_" + UniqueId + " = " + slope + ";");
         dataCollector.AddToLocalVariables(UniqueId, "half contrast_" + UniqueId + " = " + c + ";");

         string result = dataCollector.AddFunctions( FunctionHeader, m_functionBody, "h1_" + UniqueId, "h2_" + UniqueId, "slope_" + UniqueId, "contrast_" + UniqueId);
         return CreateOutputLocalVariable(0, result, ref dataCollector );


      }
   }
}

#endif
