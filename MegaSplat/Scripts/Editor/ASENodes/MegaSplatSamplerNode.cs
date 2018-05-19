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
	[NodeAttributes( "MegaSplat Sampler", "MegaSplat", "Sample and blend Texture Array" )]
	public sealed class MegaSplatSamplerNode : PropertyNode
	{
		public enum TexturePropertyValues
		{
			white,
			black,
			gray,
			bump
		}

		[SerializeField]
		private Texture2DArray texArray;

		[SerializeField]
		private TexturePropertyValues defaultValue = TexturePropertyValues.black;

		[SerializeField]
		private float defaultContrast = 0.5f;

		private string m_functionBody = string.Empty;
		private const string FunctionHeader = "ComputeMegaSplatWeights( {0},{1},{2},{3},{4})";


		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_currentParameterType = PropertyType.Property;
			AddInputPort( WirePortDataType.FLOAT2, true, "UVs" );
			AddInputPort( WirePortDataType.FLOAT, true, "Texture Choice" );
			AddInputPort( WirePortDataType.FLOAT, true, "Contrast" );
			AddInputPort( WirePortDataType.FLOAT4x4, true, "Resample Data" );

			AddOutputColorPorts( "Color" );
			AddOutputPort( WirePortDataType.FLOAT4x4, "Resample Data" );

			IOUtils.AddFunctionHeader( ref m_functionBody, "half3 ComputeMegaSplatWeights(half3 iWeights, half4 tex0, half4 tex1, half4 tex2, half contrast)" );
			IOUtils.AddFunctionLine( ref m_functionBody, "const half epsilon = 1.0f / 1024.0f;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "half3 weights = half3(iWeights.x * (tex0.a + epsilon), iWeights.y * (tex1.a + epsilon),iWeights.z * (tex2.a + epsilon));" );
			IOUtils.AddFunctionLine( ref m_functionBody, "half maxWeight = max(weights.x, max(weights.y, weights.z));" );
			IOUtils.AddFunctionLine( ref m_functionBody, "half transition = contrast * maxWeight;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "half threshold = maxWeight - transition;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "half scale = 1.0f / transition;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "weights = saturate((weights - threshold) * scale);" );
			IOUtils.AddFunctionLine( ref m_functionBody, "half weightScale = 1.0f / (weights.x + weights.y + weights.z);" );
			IOUtils.AddFunctionLine( ref m_functionBody, "weights *= weightScale;" );
			IOUtils.AddFunctionLine( ref m_functionBody, "return weights;" );
			IOUtils.CloseFunctionBody( ref m_functionBody );

		}

		// declare sampler
		public override string GetUniformValue()
		{
			return "UNITY_DECLARE_TEX2DARRAY(" + m_propertyName + ");";
		}
		// make a property
		public override string GetPropertyValue()
		{
			return m_propertyName + "(\"" + m_propertyInspectorName + "\", 2DArray) = \"" + defaultValue.ToString() + "\"";
		}


		public override void DrawProperties()
		{
			EditorGUI.BeginChangeCheck();
			base.DrawProperties();
			if ( EditorGUI.EndChangeCheck() )
			{
				OnPropertyNameChanged();
			}
			texArray = ( Texture2DArray ) EditorGUILayout.ObjectField( "Tex Array", this.texArray, typeof( Texture2DArray ), false );
			defaultValue = ( TexturePropertyValues ) EditorGUILayout.EnumPopup( "Default", defaultValue );
			defaultContrast = EditorGUILayout.Slider( "Contrast", defaultContrast, 0, 0.999f );
		}

		// should make ASE vertex shader node public const for this
		private const string _inputColorStr = "float4 vertexColor : COLOR";
		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
         dataCollector.AddToProperties( UniqueId, GetPropertyValue(), 0 );
         dataCollector.AddToUniforms( UniqueId, GetUniformValue() );


			bool hasVertexColors = dataCollector.ContainsInput( _inputColorStr );

			if ( !hasVertexColors )
			{
            dataCollector.AddToInput( UniqueId, _inputColorStr, true );
			}

			var inputUV = InputPorts[ 0 ];
			var inputChoice = InputPorts[ 1 ];
			var inputContrast = InputPorts[ 2 ];
			var inputData = InputPorts[ 3 ];

			if ( inputData.IsConnected )
			{
				inputData.GeneratePortInstructions( ref dataCollector );
			}

			string layerName = "mb_layer";
			if ( !inputData.IsConnected )
			{
            string decl = "float3 " + layerName + UniqueId;
				// add texcoord for 
            dataCollector.AddToInput( UniqueId, decl, true );

				MasterNodePortCategory currCategory = dataCollector.PortCategory;
				dataCollector.PortCategory = MasterNodePortCategory.Vertex;
            dataCollector.AddVertexInstruction( "half choice" + UniqueId + " = " + inputChoice.GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, ignoreLocalvar, true ), UniqueId );
				dataCollector.PortCategory = currCategory;

            dataCollector.AddVertexInstruction( Constants.VertexShaderOutputStr + "." + layerName + UniqueId + " = " +
               Constants.VertexShaderInputStr + ".color.xyz * choice" + UniqueId + " * 255",
               UniqueId );

			}

			// varnames
         string im0 = "im0_" + UniqueId;
         string im1 = "im1_" + UniqueId;
         string im2 = "im2_" + UniqueId;
         string weights = layerName + UniqueId;
         string localUV = "localUV" + UniqueId;
         string uv0 = "uv0_" + UniqueId;
         string uv1 = "uv1_" + UniqueId;
         string uv2 = "uv2_" + UniqueId;
			string tex0 = "tex0_" + UniqueId;
			string tex1 = "tex1_" + UniqueId;
			string tex2 = "tex2_" + UniqueId;
			string contrast = "contrast_" + UniqueId;
			string oWeights = "weights_" + UniqueId;
			string resTex = "resTex_" + UniqueId;
			// declare the result variables
			if ( inputData.IsConnected )
			{
				string varName = "portData" + UniqueId;
				string op = inputData.GetConnection( 0 ).NodeId.ToString();
				dataCollector.AddToLocalVariables( UniqueId, "float4x4 " + varName + " = megadata_" + op + ";" );
				dataCollector.AddToLocalVariables( UniqueId, "float4x4 " + varName + " = megadata_" + op + ";" );
				dataCollector.AddToLocalVariables( UniqueId, "float3 " + uv0 + " = " + varName + "[0].xyz;" );
				dataCollector.AddToLocalVariables( UniqueId, "float3 " + uv1 + " = " + varName + "[1].xyz;" );
				dataCollector.AddToLocalVariables( UniqueId, "float3 " + uv2 + " = " + varName + "[2].xyz;" );
				dataCollector.AddToLocalVariables( UniqueId, "float3 " + oWeights + " = " + varName + "[3].xyz;" );
				dataCollector.AddToLocalVariables( UniqueId, "float " + contrast + " = " + varName + "[0].w;" );
				// allow them to override the UV on a per node basis 
				if ( inputUV.ConnectionCount > 0 )
				{
					dataCollector.AddToLocalVariables( UniqueId, localUV + " = " + inputUV.GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT2, true ) + ";" );
					dataCollector.AddToLocalVariables( UniqueId, uv0 + " = float3(" + localUV + ", " + im0 + ");" );
					dataCollector.AddToLocalVariables( UniqueId, uv1 + " = float3(" + localUV + ", " + im1 + ");" );
					dataCollector.AddToLocalVariables( UniqueId, uv2 + " = float3(" + localUV + ", " + im2 + ");" );
				}

				dataCollector.AddToLocalVariables( UniqueId, "half4 " + tex0 + " = UNITY_SAMPLE_TEX2DARRAY(" + m_propertyName + ", " + uv0 + ");" );
				dataCollector.AddToLocalVariables( UniqueId, "half4 " + tex1 + " = UNITY_SAMPLE_TEX2DARRAY(" + m_propertyName + ", " + uv1 + ");" );
				dataCollector.AddToLocalVariables( UniqueId, "half4 " + tex2 + " = UNITY_SAMPLE_TEX2DARRAY(" + m_propertyName + ", " + uv2 + ");" );

			}
			else
			{
				dataCollector.AddToLocalVariables( UniqueId, "int " + im0 + " = round(" + Constants.InputVarStr + "." + weights + ".x / max(" + Constants.InputVarStr + ".vertexColor.x, 0.00001));" );
				dataCollector.AddToLocalVariables( UniqueId, "int " + im1 + " = round(" + Constants.InputVarStr + "." + weights + ".y / max(" + Constants.InputVarStr + ".vertexColor.y, 0.00001));" );
				dataCollector.AddToLocalVariables( UniqueId, "int " + im2 + " = round(" + Constants.InputVarStr + "." + weights + ".z / max(" + Constants.InputVarStr + ".vertexColor.z, 0.00001));" );
				dataCollector.AddToLocalVariables( UniqueId, "float2 " + localUV + " = " + inputUV.GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT2, true ) + ";" );
				dataCollector.AddToLocalVariables( UniqueId, "float3 " + uv0 + " = float3(" + localUV + ", " + im0 + ");" );
				dataCollector.AddToLocalVariables( UniqueId, "float3 " + uv1 + " = float3(" + localUV + ", " + im1 + ");" );
				dataCollector.AddToLocalVariables( UniqueId, "float3 " + uv2 + " = float3(" + localUV + ", " + im2 + ");" );
				if ( inputContrast.ConnectionCount > 0 )
				{
					dataCollector.AddToLocalVariables( UniqueId, "float " + contrast + " = " + inputContrast.GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, true ) + ";" );
				}
				else
				{
					dataCollector.AddToLocalVariables( UniqueId, "float " + contrast + " = " + defaultContrast + ";" );
				}

				dataCollector.AddToLocalVariables( UniqueId, "half4 " + tex0 + " = UNITY_SAMPLE_TEX2DARRAY(" + m_propertyName + ", " + uv0 + ");" );
				dataCollector.AddToLocalVariables( UniqueId, "half4 " + tex1 + " = UNITY_SAMPLE_TEX2DARRAY(" + m_propertyName + ", " + uv1 + ");" );
				dataCollector.AddToLocalVariables( UniqueId, "half4 " + tex2 + " = UNITY_SAMPLE_TEX2DARRAY(" + m_propertyName + ", " + uv2 + ");" );

				string result = dataCollector.AddFunctions( FunctionHeader, m_functionBody, Constants.InputVarStr + "." + weights, tex0, tex1, tex2, contrast );
				string wResult = CreateOutputLocalVariable( 1, result, ref dataCollector );
				dataCollector.AddToLocalVariables( UniqueId, "half3 " + oWeights + " = " + wResult + ";" );
			}

			dataCollector.AddToLocalVariables( UniqueId, "half4 " + resTex + " = " +
			   tex0 + " * " + oWeights + ".x + " +
			   tex1 + " * " + oWeights + ".y + " +
			   tex2 + " * " + oWeights + ".z;" );

			string dataPort = "megadata_" + UniqueId;
			dataCollector.AddToLocalVariables( UniqueId, "float4x4 " + dataPort + " = float4x4(" + uv0 + ", " + contrast + ", " + uv1 + ", 1, " + uv2 + ", 1, " + oWeights + ", 1);" );

			switch ( outputId )
			{
				case 0:
				{
					return resTex;
				}
				case 1:
				{
					return resTex + ".r";
				}
				case 2:
				{
					return resTex + ".g";
				}
				case 3:
				{
					return resTex + ".b";
				}
				case 4:
				{
					return resTex + ".a";
				}
				case 5:
				{
					return dataPort;
				}
			}
			Debug.LogError( "Unhandled output port" );
			return "error";
		}
	}

}

#endif

