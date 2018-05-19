// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Ase_SingleLayer"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Contrast("_Contrast", Range( 0 , 1)) = 0.5
		_Normal("Normal", 2DArray) = "black"
		_Diffuse("Diffuse", 2DArray) = "black"
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.5
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float4 vertexColor : COLOR;
			float3 mb_layer1;
			float2 uv_texcoord;
		};

		UNITY_DECLARE_TEX2DARRAY(_Normal);
		UNITY_DECLARE_TEX2DARRAY(_Diffuse);
		uniform float _Contrast;


		half3 ComputeMegaSplatWeights(half3 iWeights, half4 tex0, half4 tex1, half4 tex2, half contrast)
		{
			const half epsilon = 1.0f / 1024.0f;
			half3 weights = half3(iWeights.x * (tex0.a + epsilon), iWeights.y * (tex1.a + epsilon),iWeights.z * (tex2.a + epsilon));
			half maxWeight = max(weights.x, max(weights.y, weights.z));
			half transition = contrast * maxWeight;
			half threshold = maxWeight - transition;
			half scale = 1.0f / transition;
			weights = saturate((weights - threshold) * scale);
			half weightScale = 1.0f / (weights.x + weights.y + weights.z);
			weights *= weightScale;
			return weights;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half choice1 = v.color.a;
			o.mb_layer1 = v.color.xyz * choice1 * 255;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			int im0_1 = round(i.mb_layer1.x / max(i.vertexColor.x, 0.00001));
			int im1_1 = round(i.mb_layer1.y / max(i.vertexColor.y, 0.00001));
			int im2_1 = round(i.mb_layer1.z / max(i.vertexColor.z, 0.00001));
			float2 localUV1 = i.uv_texcoord;
			float3 uv0_1 = float3(localUV1, im0_1);
			float3 uv1_1 = float3(localUV1, im1_1);
			float3 uv2_1 = float3(localUV1, im2_1);
			float contrast_1 = _Contrast;
			half4 tex0_1 = UNITY_SAMPLE_TEX2DARRAY(_Diffuse, uv0_1);
			half4 tex1_1 = UNITY_SAMPLE_TEX2DARRAY(_Diffuse, uv1_1);
			half4 tex2_1 = UNITY_SAMPLE_TEX2DARRAY(_Diffuse, uv2_1);
			half3 weights_1 = ComputeMegaSplatWeights( i.mb_layer1,tex0_1,tex1_1,tex2_1,contrast_1);
			half4 resTex_1 = tex0_1 * weights_1.x + tex1_1 * weights_1.y + tex2_1 * weights_1.z;
			float4x4 megadata_1 = float4x4(uv0_1, contrast_1, uv1_1, 1, uv2_1, 1, weights_1, 1);
			float4x4 portData5 = megadata_1;
			float3 uv0_5 = portData5[0].xyz;
			float3 uv1_5 = portData5[1].xyz;
			float3 uv2_5 = portData5[2].xyz;
			float3 weights_5 = portData5[3].xyz;
			float contrast_5 = portData5[0].w;
			half4 tex0_5 = UNITY_SAMPLE_TEX2DARRAY(_Normal, uv0_5);
			half4 tex1_5 = UNITY_SAMPLE_TEX2DARRAY(_Normal, uv1_5);
			half4 tex2_5 = UNITY_SAMPLE_TEX2DARRAY(_Normal, uv2_5);
			half4 resTex_5 = tex0_5 * weights_5.x + tex1_5 * weights_5.y + tex2_5 * weights_5.z;
			float4x4 megadata_5 = float4x4(uv0_5, contrast_5, uv1_5, 1, uv2_5, 1, weights_5, 1);
			o.Normal = UnpackNormal( resTex_5 );
			o.Albedo = resTex_1.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=3203
119;82;1321;729;1337.552;364.906;1.475955;True;True
Node;AmplifyShaderEditor.TexCoordVertexDataNode;2;-1031.5,-164.5;Float;0
Node;AmplifyShaderEditor.VertexColorNode;3;-1051.5,-19.5;Float
Node;AmplifyShaderEditor.RangedFloatNode;4;-1117.5,152.5;Float;Property;_Contrast;_Contrast;0;0.5;0;1
Node;JBooth.MegaSplat.MegaSplatSamplerNode;1;-818.5,-8.5;Float;Property;_Diffuse;Diffuse;0;FLOAT2;0,0;FLOAT;0.0;FLOAT;0.0;FLOAT4x4;0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
Node;JBooth.MegaSplat.MegaSplatSamplerNode;5;-486.5,62.5;Float;Property;_Normal;Normal;1;FLOAT2;0,0;FLOAT;0.0;FLOAT;0.0;FLOAT4x4;0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;174,-2;Float;True;3;Float;ASEMaterialInspector;Standard;Ase_SingleLayer;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;False;All;0;Zero;Zero;0;Zero;Zero;False;All;LogicalAnd;LogicalAnd;FLOAT3;0,0,0;FLOAT3;0,0,0;FLOAT3;0,0,0;FLOAT;0.0;FLOAT;0.0;FLOAT;0.0;FLOAT3;0,0,0;FLOAT3;0,0,0;FLOAT;0.0;OBJECT;0.0;FLOAT3;0,0,0;OBJECT;0.0;OBJECT;0.0;FLOAT4;0,0,0,0;FLOAT3;0,0,0
Node;AmplifyShaderEditor.UnpackScaleNormalNode;6;-110.5,103.5;Float;FLOAT4;0,0,0,0;FLOAT;1.0
WireConnection;1;0;2;0
WireConnection;1;1;3;4
WireConnection;1;2;4;0
WireConnection;5;3;1;5
WireConnection;0;0;1;0
WireConnection;0;1;6;0
WireConnection;6;0;5;0
ASEEND*/
//CHKSM=81EC3F7640E7390BF0D8FFB0169A6867111BC759