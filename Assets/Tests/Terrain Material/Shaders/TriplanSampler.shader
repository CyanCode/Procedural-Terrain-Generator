// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "New AmplifyShader"
{
	Properties
	{
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 4
		_TessMin( "Tess Min Distance", Float ) = 10
		_TessMax( "Tess Max Distance", Float ) = 40
		_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.5
		_Texture1("Texture 1", 2DArray) = "white" {}
		_Texture0("Texture 0", 2DArray) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "Tessellation.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
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
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform UNITY_DECLARE_TEX2DARRAY( _Texture1 );
		uniform UNITY_DECLARE_TEX2DARRAY( _Texture0 );
		uniform float _TessValue;
		uniform float _TessMin;
		uniform float _TessMax;
		uniform float _TessPhongStrength;


		inline float3 TriplanarSamplingCNFA( UNITY_ARGS_TEX2DARRAY( topTexMap ), UNITY_ARGS_TEX2DARRAY( midTexMap ), UNITY_ARGS_TEX2DARRAY( botTexMap ), float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm = ( UNITY_SAMPLE_TEX2DARRAY( midTexMap, float3( tilling * worldPos.zy * float2( nsign.x, 1.0 ), index.y ) ) );
			yNorm = ( UNITY_SAMPLE_TEX2DARRAY( topTexMap, float3( tilling * worldPos.xz * float2( nsign.y, 1.0 ), index.x ) ) );
			yNormN = ( UNITY_SAMPLE_TEX2DARRAY( botTexMap, float3( tilling * worldPos.xz * float2( nsign.y, 1.0 ), index.z ) ) );
			zNorm = ( UNITY_SAMPLE_TEX2DARRAY( midTexMap, float3( tilling * worldPos.xy * float2( -nsign.z, 1.0 ), index.y ) ) );
			xNorm.xyz = half3( UnpackNormal( xNorm ).xy * float2( nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz = half3( UnpackNormal( yNorm ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz = half3( UnpackNormal( zNorm ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			yNormN.xyz = half3( UnpackNormal( yNormN ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
		}


		inline float4 TriplanarSamplingCFA( UNITY_ARGS_TEX2DARRAY( topTexMap ), UNITY_ARGS_TEX2DARRAY( midTexMap ), UNITY_ARGS_TEX2DARRAY( botTexMap ), float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm = ( UNITY_SAMPLE_TEX2DARRAY( midTexMap, float3( tilling * worldPos.zy * float2( nsign.x, 1.0 ), index.y ) ) );
			yNorm = ( UNITY_SAMPLE_TEX2DARRAY( topTexMap, float3( tilling * worldPos.xz * float2( nsign.y, 1.0 ), index.x ) ) );
			yNormN = ( UNITY_SAMPLE_TEX2DARRAY( botTexMap, float3( tilling * worldPos.xz * float2( nsign.y, 1.0 ), index.z ) ) );
			zNorm = ( UNITY_SAMPLE_TEX2DARRAY( midTexMap, float3( tilling * worldPos.xy * float2( -nsign.z, 1.0 ), index.y ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _TessMin, _TessMax, _TessValue );
		}

		void vertexDataFunc( inout appdata_full v )
		{
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 triplanar8 = TriplanarSamplingCNFA( UNITY_PASS_TEX2DARRAY(_Texture1), UNITY_PASS_TEX2DARRAY(_Texture1), UNITY_PASS_TEX2DARRAY(_Texture1), ase_worldPos, ase_worldNormal, 5.0, 0.3, float3(0.0,1.0,2.0) );
			float3 tanTriplanarNormal8 = mul( ase_worldToTangent, triplanar8 );
			o.Normal = tanTriplanarNormal8;
			float4 triplanar1 = TriplanarSamplingCFA( UNITY_PASS_TEX2DARRAY(_Texture0), UNITY_PASS_TEX2DARRAY(_Texture0), UNITY_PASS_TEX2DARRAY(_Texture0), ase_worldPos, ase_worldNormal, 5.0, 0.3, float3(0.0,1.0,2.0) );
			o.Albedo = triplanar1.xyz;
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction tessphong:_TessPhongStrength 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
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
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
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
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14401
204;92;1437;655;1381.871;491.2074;1;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;6;-1027.931,-551.9642;Float;True;Property;_Texture0;Texture 0;6;0;Create;True;3a18215054b434c7bf83da3ae44c46dd;3a18215054b434c7bf83da3ae44c46dd;False;white;LockedToTexture2DArray;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-1028.447,-356.1705;Float;False;Constant;_Tiling;Tiling;3;0;Create;True;0.3;0;0;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-1026.738,-207.8955;Float;True;Property;_Texture1;Texture 1;5;0;Create;True;None;05a5a4c60440ccabdf5c3352ab7e2bd2;False;white;LockedToTexture2DArray;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-1029.447,-283.1705;Float;False;Constant;_Falloff;Falloff;3;0;Create;True;5;0;1;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;8;-567.05,-236.2103;Float;True;Cylindrical;World;True;Top Texture 1;_TopTexture1;white;-1;None;Mid Texture 1;_MidTexture1;white;-1;None;Bot Texture 1;_BotTexture1;white;-1;None;Triplanar Sampler;True;8;0;SAMPLER2D;;False;5;FLOAT;0.0;False;1;SAMPLER2D;;False;6;FLOAT;1.0;False;2;SAMPLER2D;;False;7;FLOAT;2.0;False;3;FLOAT;1.0;False;4;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;9;-360.5416,178.9117;Float;False;Constant;_Smoothness;Smoothness;4;0;Create;True;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;1;-566.2244,-478.4786;Float;True;Cylindrical;World;False;Top Texture 0;_TopTexture0;white;0;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;True;8;0;SAMPLER2D;;False;5;FLOAT;0.0;False;1;SAMPLER2D;;False;6;FLOAT;1.0;False;2;SAMPLER2D;;False;7;FLOAT;2.0;False;3;FLOAT;1.0;False;4;FLOAT;0.71;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-355.9028,95.41584;Float;False;Constant;_Metalic;Metalic;4;0;Create;True;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;80,25;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;New AmplifyShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;True;0;4;10;40;True;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;0;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;7;0
WireConnection;8;1;7;0
WireConnection;8;2;7;0
WireConnection;8;3;12;0
WireConnection;8;4;13;0
WireConnection;1;0;6;0
WireConnection;1;1;6;0
WireConnection;1;2;6;0
WireConnection;1;3;12;0
WireConnection;1;4;13;0
WireConnection;0;0;1;0
WireConnection;0;1;8;0
WireConnection;0;3;10;0
WireConnection;0;4;9;0
ASEEND*/
//CHKSM=E161FC46605CE36862EAF4D140DF6811585F80BB