// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Terra/TerrainFirstPass"
{
	Properties
	{
		[Header(Four Splats First Pass Terrain)]
		[HideInInspector]_Control("Control", 2D) = "white" {}
		[HideInInspector]_Smoothness3("Smoothness3", Range( 0 , 1)) = 1
		[HideInInspector]_Smoothness1("Smoothness1", Range( 0 , 1)) = 1
		[HideInInspector]_Smoothness0("Smoothness0", Range( 0 , 1)) = 1
		[HideInInspector]_Smoothness2("Smoothness2", Range( 0 , 1)) = 1
		_Splat3("Splat3", 2D) = "white" {}
		_Splat1("Splat1", 2D) = "white" {}
		_Splat2("Splat2", 2D) = "white" {}
		_Splat0("Splat0", 2D) = "white" {}
		_Normal1("Normal1", 2D) = "white" {}
		_Normal2("Normal2", 2D) = "white" {}
		_Normal0("Normal0", 2D) = "white" {}
		_Normal3("Normal3", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry-100" "SplatCount"="4" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.5
		#pragma multi_compile_fog
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Control;
		uniform float4 _Control_ST;
		uniform sampler2D _Normal0;
		uniform sampler2D _Normal1;
		uniform sampler2D _Normal2;
		uniform sampler2D _Normal3;
		uniform float _Smoothness0;
		uniform sampler2D _Splat0;
		uniform float _Smoothness1;
		uniform sampler2D _Splat1;
		uniform float _Smoothness2;
		uniform sampler2D _Splat2;
		uniform float _Smoothness3;
		uniform sampler2D _Splat3;
		uniform float _Metallic;


		inline float4 TriplanarSamplingSF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( topTexMap, tilling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( topTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( topTexMap, tilling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float localCalculateTangents16_g1716_g17 = ( 0.0 );
			v.tangent.xyz = cross ( v.normal, float3( 0, 0, 1 ) );
			v.tangent.w = -1;
			float3 temp_cast_0 = (localCalculateTangents16_g1716_g17).xxx;
			v.vertex.xyz += temp_cast_0;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Control = i.uv_texcoord * _Control_ST.xy + _Control_ST.zw;
			float4 tex2DNode5_g17 = tex2D( _Control, uv_Control );
			float dotResult20_g17 = dot( tex2DNode5_g17 , float4(1,1,1,1) );
			float SplatWeight22_g17 = dotResult20_g17;
			float4 SplatControl26_g17 = ( tex2DNode5_g17 / ( SplatWeight22_g17 + 0.001 ) );
			float4 temp_output_59_0_g17 = SplatControl26_g17;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar78_g17 = TriplanarSamplingSF( _Normal0, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 triplanar87_g17 = TriplanarSamplingSF( _Normal1, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 triplanar89_g17 = TriplanarSamplingSF( _Normal2, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 triplanar91_g17 = TriplanarSamplingSF( _Normal3, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 weightedBlendVar8_g17 = temp_output_59_0_g17;
			float4 weightedBlend8_g17 = ( weightedBlendVar8_g17.x*triplanar78_g17 + weightedBlendVar8_g17.y*triplanar87_g17 + weightedBlendVar8_g17.z*triplanar89_g17 + weightedBlendVar8_g17.w*triplanar91_g17 );
			o.Normal = UnpackNormal( weightedBlend8_g17 );
			float4 appendResult33_g17 = (float4(1.0 , 1.0 , 1.0 , _Smoothness0));
			float4 triplanar75_g17 = TriplanarSamplingSF( _Splat0, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 appendResult36_g17 = (float4(1.0 , 1.0 , 1.0 , _Smoothness1));
			float4 triplanar81_g17 = TriplanarSamplingSF( _Splat1, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 appendResult39_g17 = (float4(1.0 , 1.0 , 1.0 , _Smoothness2));
			float4 triplanar83_g17 = TriplanarSamplingSF( _Splat2, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 appendResult42_g17 = (float4(1.0 , 1.0 , 1.0 , _Smoothness3));
			float4 triplanar85_g17 = TriplanarSamplingSF( _Splat3, ase_worldPos, ase_worldNormal, 1.0, 1.0, 0 );
			float4 weightedBlendVar9_g17 = temp_output_59_0_g17;
			float4 weightedBlend9_g17 = ( weightedBlendVar9_g17.x*( appendResult33_g17 * triplanar75_g17 ) + weightedBlendVar9_g17.y*( appendResult36_g17 * triplanar81_g17 ) + weightedBlendVar9_g17.z*( appendResult39_g17 * triplanar83_g17 ) + weightedBlendVar9_g17.w*( appendResult42_g17 * triplanar85_g17 ) );
			float4 MixDiffuse28_g17 = weightedBlend9_g17;
			o.Albedo = MixDiffuse28_g17.xyz;
			o.Metallic = _Metallic;
			o.Smoothness = (MixDiffuse28_g17).w;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma exclude_renderers gles 
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}

	Dependency "BaseMapShader"="ASESampleShaders/SimpleTerrainBase"
	Fallback "Nature/Terrain/Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14401
281;92;1255;650;1197.439;509.8323;1.745812;False;False
Node;AmplifyShaderEditor.FunctionNode;49;-333.7468,44.2099;Float;False;Four Splats First Pass Terrain;0;;17;37452fdfb732e1443b7e39720d05b708;0;6;59;FLOAT4;0,0,0,0;False;60;FLOAT4;0,0,0,0;False;61;FLOAT3;0,0,0;False;57;FLOAT;0.0;False;58;FLOAT;0.0;False;62;FLOAT;0.0;False;6;FLOAT4;0;FLOAT3;14;FLOAT;56;FLOAT;45;FLOAT;19;FLOAT;17
Node;AmplifyShaderEditor.RangedFloatNode;21;134.9534,153.83;Float;False;Property;_Metallic;Metallic;18;0;Create;True;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;29;92.84039,315.6637;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;657.0565,40.11808;Float;False;True;3;Float;ASEMaterialInspector;0;0;Standard;Terra/TerrainFirstPass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;False;0;Opaque;0.5;True;True;-100;False;Opaque;;Geometry;All;True;True;True;False;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4.5;10;40;True;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;Nature/Terrain/Diffuse;-1;-1;-1;-1;0;1;SplatCount=4;1;multi_compile_fog;False;1;BaseMapShader=ASESampleShaders/SimpleTerrainBase;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;0;49;17
WireConnection;0;0;49;0
WireConnection;0;1;49;14
WireConnection;0;3;21;0
WireConnection;0;4;49;45
WireConnection;0;11;29;0
ASEEND*/
//CHKSM=8222FDC61B996364BB554B776EFEFA51D0C8B18C