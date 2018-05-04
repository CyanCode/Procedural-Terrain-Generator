// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Nature/Terrain/Standard-Flat" {
    Properties {
        // set by terrain engine
        [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
        [HideInInspector] [Gamma] _Metallic0 ("Metallic 0", Range(0.0, 1.0)) = 0.0
        [HideInInspector] [Gamma] _Metallic1 ("Metallic 1", Range(0.0, 1.0)) = 0.0
        [HideInInspector] [Gamma] _Metallic2 ("Metallic 2", Range(0.0, 1.0)) = 0.0
        [HideInInspector] [Gamma] _Metallic3 ("Metallic 3", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Smoothness0 ("Smoothness 0", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness1 ("Smoothness 1", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness2 ("Smoothness 2", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness3 ("Smoothness 3", Range(0.0, 1.0)) = 1.0

        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader {
        Tags {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer fullforwardshadows noinstancing
        #pragma multi_compile_fog
        #pragma target 3.0
        // needs more than 8 texcoords
        #pragma exclude_renderers gles psp2
        #include "UnityPBSLighting.cginc"

        #pragma multi_compile __ _TERRAIN_NORMAL_MAP

        #define TERRAIN_STANDARD_SHADER
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
        #include "TerrainSplatmapCommonFlat.cginc"

        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;

		struct appdata {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 vertex : TEXCOORD1;
		};

        void surf (Input IN, inout SurfaceOutputStandard o) {
            half4 defaultAlpha = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
			half4 splat_control = tex2D(_Control, IN.tc_Control);

			#ifdef _TERRAIN_NORMAL_MAP
				fixed4 nrm = 0.0f;
				nrm += splat_control.r * tex2D(_Normal0, IN.uv_Splat0);
				nrm += splat_control.g * tex2D(_Normal1, IN.uv_Splat1);
				nrm += splat_control.b * tex2D(_Normal2, IN.uv_Splat2);
				nrm += splat_control.a * tex2D(_Normal3, IN.uv_Splat3);
			o.Normal = UnpackNormal(nrm);
			#endif

			// Normalize weights before lighting and restore weights in final modifier functions so that the overal
			// lighting result can be correctly weighted.
			half weight = dot(splat_control, half4(1, 1, 1, 1));
			splat_control /= (weight + 1e-3f);

			o.Albedo = IN.color.rgb;
            o.Alpha = weight;
            o.Smoothness = IN.color.a;
            o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
        }

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.tc_Control = TRANSFORM_TEX(v.texcoord, _Control);  // Need to manually transform uv here, as we choose not to use 'uv' prefix for this texcoord.
			float4 pos = UnityObjectToClipPos(v.vertex);
			UNITY_TRANSFER_FOG(data, pos);

			#ifdef _TERRAIN_NORMAL_MAP
				v.tangent.xyz = cross(v.normal, float3(0, 0, 1));
				v.tangent.w = -1;
			#endif

			//Copied SplatmapMix function
			half4 splat_control = tex2D(_Control, data.tc_Control);
			half weight = dot(splat_control, half4(1, 1, 1, 1));
			fixed4 mixedDiffuse;
			half4 defaultAlpha = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);

			#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
				clip(weight == 0.0f ? -1 : 1);
			#endif

			mixedDiffuse = 0.0f;
			splat_control /= (weight + 1e-3f);

			#ifdef TERRAIN_STANDARD_SHADER
				mixedDiffuse += splat_control.r * tex2D(_Splat0, data.uv_Splat0) * half4(1.0, 1.0, 1.0, defaultAlpha.r);
				mixedDiffuse += splat_control.g * tex2D(_Splat1, data.uv_Splat1) * half4(1.0, 1.0, 1.0, defaultAlpha.g);
				mixedDiffuse += splat_control.b * tex2D(_Splat2, data.uv_Splat2) * half4(1.0, 1.0, 1.0, defaultAlpha.b);
				mixedDiffuse += splat_control.a * tex2D(_Splat3, data.uv_Splat3) * half4(1.0, 1.0, 1.0, defaultAlpha.a);
			#endif

			//Send mixedDiffuse to data
			data.color = mixedDiffuse;

			//end copied function

			#ifdef _TERRAIN_NORMAL_MAP
				v.tangent.xyz = cross(v.normal, float3(0, 0, 1));
				v.tangent.w = -1;
			#endif
		}

        ENDCG
    }

    Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Standard-AddPass"
    Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/Standard-Base"

    Fallback "Nature/Terrain/Diffuse"
}
