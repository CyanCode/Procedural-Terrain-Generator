//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//
// Auto-generated shader code, don't hand edit!
//   Compiled with MegaSplat 1.01
//   Unity : 5.4.0f3
//   Platform : OSXEditor
//////////////////////////////////////////////////////

Shader "MegaSplat/MegaSplat_TessMesh_fallback" {
   Properties {
      // Splats
      _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.0
      [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0

      [NoScaleOffset]_Diffuse ("Diffuse Array", 2DArray) = "white" {}
      [NoScaleOffset]_Normal ("Normal Array", 2DArray) = "bump" {}
      [NoScaleOffset]_Gloss ("Metallic(R)/Smoothness(a) Array", 2DArray) = "black" {}
      [NoScaleOffset]_AlphaArray("Alpha Array", 2DArray) = "white" {}

      _TexScales("Texure Scales",  Vector) = (10,10,10,10)

      _Contrast("Blend Contrast", Range(0.01, 0.99)) = 0.4
      _Parallax ("Parallax Scale", Range (0.00, 0.12)) = 0.00

      _DistanceFades("Detail Fade Start/End", Vector) = (500,2000, 500, 2000)
      _DistanceFadesCached("Detail Fade Start/End", Vector) = (500,2000, 500, 2000)

      _GlobalPorosityWetness("Default Porosity and Wetness", Vector) = (0.4, 0.0, 0.0, 0.0)
      _Cutoff("Alpha Clip", Float) = 0.5


      // flow
      _FlowSpeed("Flow Speed", Float) = 1
      _FlowIntensity("Flow Intensity", Float) = 1
      _FlowAlpha ("Flow Alpha", Range(0, 1)) = 1
      _FlowRefraction("Flow Refraction", Range(0, 0.3)) = 0.04
      // pertex
      _PropertyTex("Properties Map", 2D) = "black" {}
      _PerTexScaleRange("Per Tex Scale Modifier", Vector) = (1, 2, 0, 0)
      // puddles
      _PuddleBlend("Puddle Blend", Range(1, 60)) = 6
      _PuddleTint("Puddle Tint", Color) = (0.5,0.5,0.5,0)

      // puddle flow
      _PuddleFlowParams("Puddle Flow Params", Vector) = (1,1,0.1, 0.5)
      _PuddleNormal("Normal Map", 2D) = "bump" {}
      _PuddleUVScales("Puddle UV Scales", Vector) = (80, 80, 0, 0)

   }
   SubShader {
      Tags {"RenderType"="Opaque"}
      CGPROGRAM
      #pragma exclude_renderers d3d9
      #pragma multi_compile_fog
      #pragma surface surf Standard vertex:vert fullforwardshadows nofog finalcolor:fogcolor
      #pragma target 3.5

      #define _FLOW 1
      #define _NORMALMAP 1
      #define _NORMAL_SMOOTH_AO 1
      #define _PUDDLEFLOW 1
      #define _TESSFALLBACK 1
      #define _TESSSHADOWS 1
      #define _TWOLAYER 1

         #include "UnityCG.cginc"
         #include "AutoLight.cginc"
         #include "Lighting.cginc"
         #include "UnityPBSLighting.cginc"
         #include "UnityStandardBRDF.cginc"

         // splat
         UNITY_DECLARE_TEX2DARRAY(_Diffuse);
         UNITY_DECLARE_TEX2DARRAY(_Normal);
         UNITY_DECLARE_TEX2DARRAY(_Gloss);
         #if _EMISMAP
         UNITY_DECLARE_TEX2DARRAY(_Emissive);
         #endif
         #if _DETAILMAP
         UNITY_DECLARE_TEX2DARRAY(_DetailAlbedo);
         UNITY_DECLARE_TEX2DARRAY(_DetailNormal);
         half _DetailTextureStrength;
         #endif

         #if _ALPHA || _ALPHATEST
         UNITY_DECLARE_TEX2DARRAY(_AlphaArray);
         #endif

         #if _LOWPOLY
         half _EdgeHardness;
         #endif

         #if _TERRAIN
         sampler2D _SplatControl;
         sampler2D _SplatParams;
         #endif

         #if _RAMPLIGHTING
         sampler2D _Ramp;
         #endif

         #if _UVLOCALTOP || _UVWORLDTOP || _UVLOCALFRONT || _UVWORLDFRONT || _UVLOCALSIDE || _UVWORLDSIDE
         float4 _UVProjectOffsetScale;
         #endif
         #if _UVLOCALTOP2 || _UVWORLDTOP2 || _UVLOCALFRONT2 || _UVWORLDFRONT2 || _UVLOCALSIDE2 || _UVWORLDSIDE2
         float4 _UVProjectOffsetScale2;
         #endif

         half _Contrast;

         #if _ALPHALAYER || _USEMACROTEXTURE
         // macro texturing
         sampler2D _MacroDiff;
         sampler2D _MacroBump;
         sampler2D _MetallicGlossMap;
         sampler2D _MacroAlpha;
         half2 _MacroTexScale;
         half _MacroTextureStrength;
         half2 _MacroTexNormAOScales;
         #endif

         // default spec
         half _Glossiness;
         half _Metallic;

         #if _DISTANCERESAMPLE
         float3  _ResampleDistanceParams;
         #endif

         #if _FLOW || _FLOWREFRACTION
         // flow
         half _FlowSpeed;
         half _FlowAlpha;
         half _FlowIntensity;
         half _FlowRefraction;
         #endif

         half2 _PerTexScaleRange;
         sampler2D _PropertyTex;

         // etc
         half _Parallax;
         half4 _TexScales;

         float4 _DistanceFades;
         float4 _DistanceFadesCached;
         int _ControlSize;

         #if _TRIPLANAR
         half _TriplanarTexScale;
         half _TriplanarContrast;
         float3 _TriplanarOffset;
         #endif

         #if _TESSDISTANCE || _TESSEDGE
         float4 _TessData1; // distance tessellation, displacement, edgelength
         float4 _TessData2; // min, max
         #endif

         #if _DETAILNOISE
         sampler2D _DetailNoise;
         half3 _DetailNoiseScaleStrengthFade;
         #endif

         #if _PUDDLES || _PUDDLEFLOW
         half3 _PuddleTint;
         half _PuddleBlend;
         #endif
         #if _PUDDLEFLOW
         half4 _PuddleFlowParams;
         float2 _PuddleUVScales;
         sampler2D _PuddleNormal;
         #endif

         half2 _GlobalPorosityWetness;

         #if _SNOW
         sampler2D _SnowDiff;
         sampler2D _SnowNormal;
         half4 _SnowParams; // influence, erosion, crystal, melt
         half _SnowAmount;
         half2 _SnowUVScales;
         half2 _SnowHeightRange;
         half3 _SnowUpVector;
         #endif

         #if _SOFTPARTICLE
         sampler2D_float _CameraDepthTexture;
         float _InvFade;
         #endif

         struct VertexOutput 
         {
             float4 pos          : SV_POSITION;
             #if !_TERRAIN
             fixed3 weights      : TEXCOORD0;
             float4 valuesMain   : TEXCOORD1;      //index rgb, triplanar W
                #if _TWOLAYER || _ALPHALAYER
                float4 valuesSecond : TEXCOORD2;      //index rgb + alpha
                #endif
             #elif _TRIPLANAR
             float3 triplanarUVW : TEXCOORD3;
             #endif
             float2 coords       : TEXCOORD4;      // uv, or triplanar UV
             float3 posWorld     : TEXCOORD5;
             float3 normal       : TEXCOORD6;

             float4 camDist      : TEXCOORD7;      // distance from camera (for fades) and fog
             float4 extraData    : TEXCOORD8;      // flowdir + fades, or if triplanar triplanarView + detailFade
             float3 tangent      : TEXCOORD9;
             float3 bitangent    : TEXCOORD10;
             float4 _ShadowCoord : TEXCOORD14;


             #if !_PASSMETA && !_PASSSHADOWCASTER && !_PASSDEFERRED
             LIGHTING_COORDS(11,12)
             UNITY_FOG_COORDS(13)
             #endif

             #if _PASSSHADOWCASTER
             float3 vec : TEXCOORD11;  // nice naming, Unity...
             #endif

             #if _WETNESS
             half wetness : TEXCOORD15;   //wetness
             #endif

             #if _SECONDUV
             float3 macroUV : TEXCOORD16;
             #endif

            
         };


         struct MegaSplatLayer
         {
            half3 Albedo;
            half3 Normal;
            half3 Emission;
            half  Metallic;
            half  Smoothness;
            half  Occlusion;
            half  Height;
            half  Alpha;
         };

         struct SplatInput
         {
            float3 weights;
            float2 splatUV;
            float2 macroUV;
            float3 valuesMain;
            half3 viewDir;
            float4 camDist;

            #if _TWOLAYER || _ALPHALAYER
            float3 valuesSecond;
            half layerBlend;
            #endif

            #if _TRIPLANAR
            float3 triplanarUVW;
            #endif
            half3 triplanarBlend; // passed to func, so always present

            #if _FLOW || _FLOWREFRACTION || _PUDDLEFLOW || _PUDDLES
            half2 flowDir;
            #endif

            #if _PUDDLES || _PUDDLEFLOW
            half puddleHeight;
            #endif

            #if _WETNESS
            half wetness;
            #endif

            #if _TESSDAMPENING
            half displacementDampening;
            #endif

            #if _SNOW
            half snowHeightFade;
            #endif
         };


         struct LayerParams
         {
            float3 uv0, uv1, uv2;
            float2 dxuv, dyuv;
            #if _TRIPLANAR
            float3 tpuv0_x, tpuv0_y, tpuv0_z;
            float3 tpuv1_x, tpuv1_y, tpuv1_z;
            float3 tpuv2_x, tpuv2_y, tpuv2_z;
            float3 dxtpcoords, dytpcoords;
            #endif
            #if _FLOW || _FLOWREFRACTION
            float3 fuv0a, fuv0b;
            float3 fuv1a, fuv1b;
            float3 fuv2a, fuv2b;
            float2 dxfuva, dxfuvb;
            float2 dyfuva, dyfuvb;
            #endif

            #if _DISTANCERESAMPLE
               half distanceBlend;
               float3 db_uv0, db_uv1, db_uv2;
               float2 db_dxuv, db_dyuv;
               #if _TRIPLANAR
               float3 db_tpuv0_x, db_tpuv0_y, db_tpuv0_z;
               float3 db_tpuv1_x, db_tpuv1_y, db_tpuv1_z;
               float3 db_tpuv2_x, db_tpuv2_y, db_tpuv2_z;
               float3 db_dxtpcoords, db_dytpcoords;
               #endif
            #endif

            half layerBlend;
            half3 metallic;
            half3 smoothness;
            half3 porosity;
            #if _FLOW || _FLOWREFRACTION
            half3 flowIntensity;
            half flowOn;
            half3 flowAlphas;
            half3 flowRefracts;
            half3 flowInterps;
            #endif
            half3 weights;

            #if _TESSDISTANCE || _TESSEDGE
            half3 displacementScale;
            half3 upBias;
            #endif

            #if _PERTEXNOISESTRENGTH
            half3 detailNoiseStrength;
            #endif

            #if _PERTEXNORMALSTRENGTH
            half3 normalStrength;
            #endif

            #if _PERTEXPARALLAXSTRENGTH
            half3 parallaxStrength;
            #endif

            #if _PERTEXAOSTRENGTH
            half3 aoStrength;
            #endif

            half3 contrast;
         };

         struct VirtualMapping
         {
            float3 weights;
            fixed4 c0, c1, c2;
            fixed4 param;
         };

         void InitDistanceResample(inout LayerParams lp, float dist)
         {
            #if _DISTANCERESAMPLE
               lp.distanceBlend = saturate((dist - _ResampleDistanceParams.y) / (_ResampleDistanceParams.z - _ResampleDistanceParams.y));
               lp.db_uv0 = lp.uv0;
               lp.db_uv1 = lp.uv1;
               lp.db_uv2 = lp.uv2;

               lp.db_uv0.xy *= _ResampleDistanceParams.xx;
               lp.db_uv1.xy *= _ResampleDistanceParams.xx;
               lp.db_uv2.xy *= _ResampleDistanceParams.xx;

               lp.db_dxuv = ddx(lp.db_uv0);
               lp.db_dyuv = ddy(lp.db_uv0);

               #if _TRIPLANAR
               lp.db_tpuv0_x = lp.tpuv0_x;
               lp.db_tpuv1_x = lp.tpuv1_x;
               lp.db_tpuv2_x = lp.tpuv2_x;
               lp.db_tpuv0_y = lp.tpuv0_y;
               lp.db_tpuv1_y = lp.tpuv1_y;
               lp.db_tpuv2_y = lp.tpuv2_y;
               lp.db_tpuv0_z = lp.tpuv0_z;
               lp.db_tpuv1_z = lp.tpuv1_z;
               lp.db_tpuv2_z = lp.tpuv2_z;

               lp.db_tpuv0_x.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv1_x.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv2_x.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv0_y.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv1_y.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv2_y.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv0_z.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv1_z.xy *= _ResampleDistanceParams.xx;
               lp.db_tpuv2_z.xy *= _ResampleDistanceParams.xx;

               lp.db_dxtpcoords = ddx(lp.db_tpuv0_x);
               lp.db_dytpcoords = ddy(lp.db_tpuv0_x);
               #endif
            #endif
         }


         LayerParams NewLayerParams()
         {
            LayerParams l = (LayerParams)0;
            l.metallic = _Metallic.xxx;
            l.smoothness = _Glossiness.xxx;
            l.porosity = _GlobalPorosityWetness.xxx;

            l.layerBlend = 0;
            #if _FLOW || _FLOWREFRACTION
            l.flowIntensity = 0;
            l.flowOn = 0;
            l.flowAlphas = half3(1,1,1);
            l.flowRefracts = half3(1,1,1);
            #endif

            #if _TESSDISTANCE || _TESSEDGE
            l.displacementScale = half3(1,1,1);
            l.upBias = half3(0,0,0);
            #endif

            #if _PERTEXNOISESTRENGTH
            l.detailNoiseStrength = half3(1,1,1);
            #endif

            #if _PERTEXNORMALSTRENGTH
            l.normalStrength = half3(1,1,1);
            #endif

            #if _PERTEXAOSTRENGTH
            l.aoStrength = half3(1,1,1);
            #endif

            #if _PERTEXPARALLAXSTRENGTH
            l.parallaxStrength = half3(1,1,1);
            #endif

            l.contrast = _Contrast;

            return l;
         }

         half AOContrast(half ao, half scalar)
         {
            scalar += 0.5;  // 0.5 -> 1.5
            scalar *= scalar; // 0.25 -> 2.25
            return pow(ao, scalar);
         }


         half MacroAOContrast(half ao, half scalar)
         {
            #if _MACROAOSCALE
            return AOContrast(ao, scalar);
            #else
            return ao;
            #endif
         }
         #if _USEMACROTEXTURE || _ALPHALAYER
         MegaSplatLayer SampleMacro(float2 uv)
         {
             MegaSplatLayer o = (MegaSplatLayer)0;
             float2 macroUV = uv * _MacroTexScale.xy;
             half4 macAlb = tex2D(_MacroDiff, macroUV);
             o.Albedo = macAlb.rgb;
             o.Height = macAlb.a;
             // defaults
             o.Normal = half3(0,0,1);
             o.Occlusion = 1;
             o.Smoothness = _Glossiness;
             o.Metallic = _Metallic;
             o.Emission = half3(0,0,0);

             // unpack normal
             #if !_NOSPECTEX
             half4 normSample = tex2D(_MacroBump, macroUV);
             #endif

             #if _NORMAL_SMOOTH_AO || _NORMAL_SMOOTH_METAL
             normSample.xy *= 2; 
             normSample.xy -= 1; 
             o.Normal = half3(normSample.x, normSample.y, sqrt(1 - saturate(dot(normSample.xy, normSample.xy))));
             o.Normal.xy *= _MacroTexNormAOScales.x;
             #elif !_NOSPECTEX
             o.Normal = UnpackNormal(normSample);
             o.Normal.xy *= _MacroTexNormAOScales.x;
             #else
             o.Normal = half3(0,0,1);
             #endif

             // Spec
             #if _NORMAL_SMOOTH_AO
             o.Smoothness = normSample.z;
             o.Occlusion = MacroAOContrast(normSample.w, _MacroTexNormAOScales.y);
             #elif _NORMAL_SMOOTH_METAL
             o.Smoothness = normSample.z;
             o.Metallic = normSample.w; 
             #elif _METALLICGLOSSMAP
             half4 glossmap = tex2D(_MetallicGlossMap, macroUV);
             o.Metallic = glossmap.x;
             o.Smoothness = glossmap.w;
             #elif _METAL_SMOOTH_EMISMASK_AO
             half4 MSEO = tex2D(_MetallicGlossMap, macroUV);
             o.Metallic = MSEO.x;
             o.Smoothness = MSEO.y;
             o.Emission = o.Albedo * MSEO.z;
             o.Occlusion = MacroAOContrast(MSEO.w, _MacroTexNormAOScales.y);
             #endif
             #if _ALPHA || _ALPHATEST
             o.Alpha = tex2D(_MacroAlpha, macroUV);
             #endif
             return o;
         }
         #endif

         #if _NORMAL_SMOOTH_AO
           #define SAMPLESPEC(o, params) \
              half4 spec0, spec1, spec2; \
              half4 specFinal = half4(0,0,0,1); \
              specFinal.yw =  norm.zw; \
              specFinal.x = (params.metallic.x * weights.x + params.metallic.y * weights.y + params.metallic.z * weights.z); \
              norm.xy *= 2; \
              norm.xy -= 1; \
              o.Normal = half3(norm.x, norm.y, sqrt(1 - saturate(dot(norm.xy, norm.xy)))); \

         #elif _NORMAL_SMOOTH_METAL
           #define SAMPLESPEC(o, params) \
              half4 spec0, spec1, spec2; \
              half4 specFinal = half4(0,0,0,1); \
              specFinal.yx =  norm.zw; \
              norm.xy *= 2; \
              norm.xy -= 1; \
              o.Normal = half3(norm.x, norm.y, sqrt(1 - saturate(dot(norm.xy, norm.xy)))); 

         #elif _METALLICGLOSSMAP
           #define SAMPLESPEC(o, params) \
              half4 spec0, spec1, spec2; \
              half4 specFinal = half4(0,0,0,1); \
              SAMPLETEXARRAY(spec0, spec1, spec2, _Gloss, params); \
              specFinal = spec0 * weights.x + spec1 * weights.y + spec2 * weights.z; \
              specFinal = half4(specFinal.xw, 0, 1); \
              o.Normal = UnpackNormal(norm); 

         #elif _METAL_SMOOTH_EMISMASK_AO
           #define SAMPLESPEC(o, params) \
              half4 spec0, spec1, spec2; \
              half4 specFinal = half4(0,0,0,1); \
              SAMPLETEXARRAY(spec0, spec1, spec2, _Gloss, params); \
              specFinal = spec0 * weights.x + spec1 * weights.y + spec2 * weights.z; \
              o.Normal = UnpackNormal(norm); 

         #elif _NOSPECTEX
           #define SAMPLESPEC(o, params) \
              half4 specFinal = half4(0,0,0,1); \
              specFinal.x = params.metallic.x * weights.x + params.metallic.y * weights.y + params.metallic.z * weights.z; \
              specFinal.y = params.smoothness.x * weights.x + params.smoothness.y * weights.y + params.smoothness.z * weights.z; \
              o.Normal = UnpackNormal(norm); 

         #else //_NOSPECNORMAL   
           #define SAMPLESPEC(o, params) \
              half4 specFinal = half4(0,0,0,1); \
              specFinal.x = params.metallic.x * weights.x + params.metallic.y * weights.y + params.metallic.z * weights.z; \
              specFinal.y = params.smoothness.x * weights.x + params.smoothness.y * weights.y + params.smoothness.z * weights.z; \
              o.Normal = half3(0,0,1);

         #endif





         void SamplePerTex(sampler2D pt, inout LayerParams params, float2 scaleRange)
         {
            const half cent = 1.0 / 512.0;
            const half pixelStep = 1.0 / 256.0;
            const half vertStep = 1.0 / 8.0;

            // pixel layout for per tex properties
            // metal/smooth/porosity/uv scale
            // flow speed, intensity, alpha, refraction
            // detailNoiseStrength, contrast, displacementAmount, displaceUpBias

            #if _PERTEXMATPARAMS || _PERTEXUV
            {
               half4 props0 = tex2Dlod(pt, half4(params.uv0.z * pixelStep + cent, 0, 0, 0));
               half4 props1 = tex2Dlod(pt, half4(params.uv1.z * pixelStep + cent, 0, 0, 0));
               half4 props2 = tex2Dlod(pt, half4(params.uv2.z * pixelStep + cent, 0, 0, 0));
               params.porosity = half3(0.4, 0.4, 0.4);
               #if _PERTEXMATPARAMS
               params.metallic = half3(props0.r, props1.r, props2.r);
               params.smoothness = half3(props0.g, props1.g, props2.g);
               params.porosity = half3(props0.b, props1.b, props2.b);
               #endif
               #if _PERTEXUV
               float3 uvScale = float3(props0.a, props1.a, props2.a);
               uvScale = lerp(scaleRange.xxx, scaleRange.yyy, uvScale);
               params.uv0.xy *= uvScale.x;
               params.uv1.xy *= uvScale.y;
               params.uv2.xy *= uvScale.z;
               #endif
            }
            #endif


            #if _FLOW || _FLOWREFRACTION
            {
               half4 props0 = tex2Dlod(pt, half4(params.uv0.z * pixelStep + cent, vertStep * 3, 0, 0));
               half4 props1 = tex2Dlod(pt, half4(params.uv1.z * pixelStep + cent, vertStep * 3, 0, 0));
               half4 props2 = tex2Dlod(pt, half4(params.uv2.z * pixelStep + cent, vertStep * 3, 0, 0));

               params.flowIntensity = half3(props0.r, props1.r, props2.r);
               params.flowOn = params.flowIntensity.x + params.flowIntensity.y + params.flowIntensity.z;

               params.flowAlphas = half3(props0.b, props1.b, props2.b);
               params.flowRefracts = half3(props0.a, props1.a, props2.a);
            }
            #endif

            #if _PERTEXDISPLACEPARAMS || _PERTEXCONTRAST || _PERTEXNOISESTRENGTH
            {
               half4 props0 = tex2Dlod(pt, half4(params.uv0.z * pixelStep + cent, vertStep * 5, 0, 0));
               half4 props1 = tex2Dlod(pt, half4(params.uv1.z * pixelStep + cent, vertStep * 5, 0, 0));
               half4 props2 = tex2Dlod(pt, half4(params.uv2.z * pixelStep + cent, vertStep * 5, 0, 0));

               #if _PERTEXDISPLACEPARAMS && (_TESSDISTANCE || _TESSEDGE)
               params.displacementScale = half3(props0.b, props1.b, props2.b);
               params.upBias = half3(props0.a, props1.a, props2.a);
               #endif

               #if _PERTEXCONTRAST
               params.contrast = half3(props0.g, props1.g, props2.g);
               #endif

               #if _PERTEXNOISESTRENGTH
               params.detailNoiseStrength = half3(props0.r, props1.r, props2.r);
               #endif
            }
            #endif

            #if _PERTEXNORMALSTRENGTH || _PERTEXPARALLAXSTRENGTH || _PERTEXAOSTRENGTH
            {
               half4 props0 = tex2Dlod(pt, half4(params.uv0.z * pixelStep + cent, vertStep * 7, 0, 0));
               half4 props1 = tex2Dlod(pt, half4(params.uv1.z * pixelStep + cent, vertStep * 7, 0, 0));
               half4 props2 = tex2Dlod(pt, half4(params.uv2.z * pixelStep + cent, vertStep * 7, 0, 0));

               #if _PERTEXNORMALSTRENGTH
               params.normalStrength = half3(props0.r, props1.r, props2.r);
               #endif

               #if _PERTEXPARALLAXSTRENGTH
               params.parallaxStrength = half3(props0.g, props1.g, props2.g);
               #endif

               #if _PERTEXAOSTRENGTH
               params.aoStrength = half3(props0.b, props1.b, props2.b);
               #endif

            }
            #endif


         }

         float FlowRefract(MegaSplatLayer tex, inout LayerParams main, inout LayerParams second, half3 weights)
         {
            #if _FLOWREFRACTION
            float totalFlow = second.flowIntensity.x * weights.x + second.flowIntensity.y * weights.y + second.flowIntensity.z * weights.z;
            float falpha = second.flowAlphas.x * weights.x + second.flowAlphas.y * weights.y + second.flowAlphas.z * weights.z;
            float frefract = second.flowRefracts.x * weights.x + second.flowRefracts.y * weights.y + second.flowRefracts.z * weights.z;
            float refractOn = min(1, totalFlow * 10000);
            float ratio = lerp(1.0, _FlowAlpha * falpha, refractOn);
            float2 rOff = tex.Normal.xy * _FlowRefraction * frefract * ratio;
            main.uv0.xy += rOff;
            main.uv1.xy += rOff;
            main.uv2.xy += rOff;
            main.dxuv = ddx(main.uv0.xy);
            main.dyuv = ddy(main.uv0.xy);
            return ratio;
            #endif
            return 1;
         }

         #if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_PSSL) || defined(UNITY_COMPILER_HLSLCC)
            #define MSPLAT_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex.SampleGrad (sampler##tex,coord,dx,dy)
            #elif defined(SHADER_API_D3D9)
            #define MSPLAT_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) half4(0,1,0,0) 
            #elif defined(UNITY_COMPILER_HLSL2GLSL) || defined(SHADER_TARGET_SURFACE_ANALYSIS)
            #define MSPLAT_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex2DArray(tex,coord,dx,dy)
            #elif defined(SHADER_API_GLES)
            #define MSPLAT_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) half4(1,1,0,0)
            #elif defined(SHADER_API_D3D11_9X)
            #define MSPLAT_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) half4(0,1,1,0) 
            #else
            #define MSPLAT_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) half4(0,0,1,0) 
         #endif

         #if _DISTANCERESAMPLE
            #if _TRIPLANAR && !_FLOW && !_FLOWREFRACTION
               #define SAMPLETEXARRAY(t0, t1, t2, TA, lp) \
                  t0  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv0_x, lp.dxtpcoords.zy, lp.dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv0_y, lp.dxtpcoords.xz, lp.dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv0_z, lp.dxtpcoords.xy, lp.dytpcoords.xy); \
                  t1  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv1_x, lp.dxtpcoords.zy, lp.dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv1_y, lp.dxtpcoords.xz, lp.dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv1_z, lp.dxtpcoords.xy, lp.dytpcoords.xy); \
                  t2  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv2_x, lp.dxtpcoords.zy, lp.dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv2_y, lp.dxtpcoords.xz, lp.dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv2_z, lp.dxtpcoords.xy, lp.dytpcoords.xy); \
                  { \
                     half4 st0  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv0_x, lp.db_dxtpcoords.zy, lp.db_dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv0_y, lp.db_dxtpcoords.xz, lp.db_dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv0_z, lp.db_dxtpcoords.xy, lp.db_dytpcoords.xy); \
                     half4 st1  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv1_x, lp.db_dxtpcoords.zy, lp.db_dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv1_y, lp.db_dxtpcoords.xz, lp.db_dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv1_z, lp.db_dxtpcoords.xy, lp.db_dytpcoords.xy); \
                     half4 st2  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv2_x, lp.db_dxtpcoords.zy, lp.db_dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv2_y, lp.db_dxtpcoords.xz, lp.db_dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_tpuv2_z, lp.db_dxtpcoords.xy, lp.db_dytpcoords.xy); \
                     t0 = lerp(t0, st0, lp.distanceBlend); \
                     t1 = lerp(t1, st1, lp.distanceBlend); \
                     t2 = lerp(t2, st2, lp.distanceBlend); \
                  }
            #else
               #if _FLOW || _FLOWREFRACTION
                  #define SAMPLETEXARRAY(t0, t1, t2, TA, lp) \
                  UNITY_BRANCH \
                  if (lp.flowOn > 0) \
                  { \
                     t0 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv0a, lp.dxfuva, lp.dyfuva), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv0b, lp.dxfuvb, lp.dyfuvb), lp.flowInterps.x); \
                     t1 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv1a, lp.dxfuva, lp.dyfuva), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv1b, lp.dxfuvb, lp.dyfuvb), lp.flowInterps.y); \
                     t2 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv2a, lp.dxfuva, lp.dyfuva), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv2b, lp.dxfuvb, lp.dyfuvb), lp.flowInterps.z); \
                  } \
                  else \
                  { \
                     t0 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv0, lp.dxuv, lp.dyuv), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_uv0, lp.db_dxuv, lp.db_dyuv), lp.distanceBlend); \
                     t1 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv1, lp.dxuv, lp.dyuv), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_uv1, lp.db_dxuv, lp.db_dyuv), lp.distanceBlend); \
                     t2 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv2, lp.dxuv, lp.dyuv), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_uv2, lp.db_dxuv, lp.db_dyuv), lp.distanceBlend); \
                  }
               #else
                  #define SAMPLETEXARRAY(t0, t1, t2, TA, lp) \
                     t0 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv0, lp.dxuv, lp.dyuv), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_uv0, lp.db_dxuv, lp.db_dyuv), lp.distanceBlend); \
                     t1 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv1, lp.dxuv, lp.dyuv), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_uv1, lp.db_dxuv, lp.db_dyuv), lp.distanceBlend); \
                     t2 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv2, lp.dxuv, lp.dyuv), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.db_uv2, lp.db_dxuv, lp.db_dyuv), lp.distanceBlend); 
               #endif
            #endif
         #else // not distance resample
            #if _TRIPLANAR && !_FLOW && !_FLOWREFRACTION
               #define SAMPLETEXARRAY(t0, t1, t2, TA, lp) \
                  t0  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv0_x, lp.dxtpcoords.zy, lp.dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv0_y, lp.dxtpcoords.xz, lp.dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv0_z, lp.dxtpcoords.xy, lp.dytpcoords.xy); \
                  t1  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv1_x, lp.dxtpcoords.zy, lp.dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv1_y, lp.dxtpcoords.xz, lp.dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv1_z, lp.dxtpcoords.xy, lp.dytpcoords.xy); \
                  t2  = tpw.x * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv2_x, lp.dxtpcoords.zy, lp.dytpcoords.zy) + tpw.y * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv2_y, lp.dxtpcoords.xz, lp.dytpcoords.xz) + tpw.z * MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.tpuv2_z, lp.dxtpcoords.xy, lp.dytpcoords.xy);
            #else
               #if _FLOW || _FLOWREFRACTION
                  #define SAMPLETEXARRAY(t0, t1, t2, TA, lp) \
                  UNITY_BRANCH \
                  if (lp.flowOn > 0) \
                  { \
                     t0 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv0a, lp.dxfuva, lp.dyfuva), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv0b, lp.dxfuvb, lp.dyfuvb), lp.flowInterps.x); \
                     t1 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv1a, lp.dxfuva, lp.dyfuva), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv1b, lp.dxfuvb, lp.dyfuvb), lp.flowInterps.y); \
                     t2 = lerp(MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv2a, lp.dxfuva, lp.dyfuva), MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.fuv2b, lp.dxfuvb, lp.dyfuvb), lp.flowInterps.z); \
                  } \
                  else \
                  { \
                     t0 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv0, lp.dxuv, lp.dyuv); \
                     t1 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv1, lp.dxuv, lp.dyuv); \
                     t2 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv2, lp.dxuv, lp.dyuv); \
                  }
               #else
                  #define SAMPLETEXARRAY(t0, t1, t2, TA, lp) \
                     t0 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv0, lp.dxuv, lp.dyuv); \
                     t1 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv1, lp.dxuv, lp.dyuv); \
                     t2 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(TA, lp.uv2, lp.dxuv, lp.dyuv);
               #endif
            #endif
         #endif


         #if _TRIPLANAR && !_FLOW && !_FLOWREFRACTION
            #define SAMPLETEXARRAYLOD(t0, t1, t2, TA, lp, lod) \
               t0  = tpw.x * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv0_x, lod) + tpw.y * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv0_y, lod) + tpw.z * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv0_z, lod); \
               t1  = tpw.x * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv1_x, lod) + tpw.y * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv1_y, lod) + tpw.z * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv1_z, lod); \
               t2  = tpw.x * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv2_x, lod) + tpw.y * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv2_y, lod) + tpw.z * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv2_z, lod);
         #else
            #define SAMPLETEXARRAYLOD(t0, t1, t2, TA, lp, lod) \
               t0 = UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.uv0, lod); \
               t1 = UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.uv1, lod); \
               t2 = UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.uv2, lod);
         #endif

         #if _TRIPLANAR && !_FLOW && !_FLOWREFRACTION
            #define SAMPLETEXARRAYLODOFFSET(t0, t1, t2, TA, lp, lod, offset) \
               t0  = tpw.x * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv0_x + offset, lod) + tpw.y * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv0_y + offset, lod) + tpw.z * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv0_z + offset, lod); \
               t1  = tpw.x * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv1_x + offset, lod) + tpw.y * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv1_y + offset, lod) + tpw.z * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv1_z + offset, lod); \
               t2  = tpw.x * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv2_x + offset, lod) + tpw.y * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv2_y + offset, lod) + tpw.z * UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.tpuv2_z + offset, lod);
         #else
            #define SAMPLETEXARRAYLODOFFSET(t0, t1, t2, TA, lp, lod, offset) \
               t0 = UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.uv0 + offset, lod); \
               t1 = UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.uv1 + offset, lod); \
               t2 = UNITY_SAMPLE_TEX2DARRAY_LOD(TA, lp.uv2 + offset, lod);
         #endif

         void Flow(float3 uv, half2 flow, half speed, float intensity, out float3 uv1, out float3 uv2, out half interp)
         {
            float2 flowVector = (flow * 2.0 - 1.0) * intensity;
            
            float timeScale = _Time.y * speed;
            float2 phase = frac(float2(timeScale, timeScale + .5));

            uv1.xy = (uv.xy - flowVector * half2(phase.x, phase.x));
            uv2.xy = (uv.xy - flowVector * half2(phase.y, phase.y));
            uv1.z = uv.z;
            uv2.z = uv.z;

            interp = abs(0.5 - phase.x) / 0.5;
         }

         void Flow(float2 uv, half2 flow, half speed, float intensity, out float2 uv1, out float2 uv2, out half interp)
         {
            float2 flowVector = (flow * 2.0 - 1.0) * intensity;
            
            float timeScale = _Time.y * speed;
            float2 phase = frac(float2(timeScale, timeScale + .5));

            uv1.xy = (uv.xy - flowVector * half2(phase.x, phase.x));
            uv2.xy = (uv.xy - flowVector * half2(phase.y, phase.y));

            interp = abs(0.5 - phase.x) / 0.5;
         }

         half3 ComputeWeights(half3 iWeights, half4 tex0, half4 tex1, half4 tex2, half contrast)
         {
             // compute weight with height map
             const half epsilon = 1.0f / 1024.0f;
             half3 weights = half3(iWeights.x * (tex0.a + epsilon), 
                                      iWeights.y * (tex1.a + epsilon),
                                      iWeights.z * (tex2.a + epsilon));

             // Contrast weights
             half maxWeight = max(weights.x, max(weights.y, weights.z));
             half transition = contrast * maxWeight;
             half threshold = maxWeight - transition;
             half scale = 1.0f / transition;
             weights = saturate((weights - threshold) * scale);
             // Normalize weights.
             half weightScale = 1.0f / (weights.x + weights.y + weights.z);
             weights *= weightScale;
             #if _LINEARBIAS
             weights = lerp(weights, iWeights, contrast);
             #endif
             return weights;
         }

         inline void ProjectUVs(float3 vertex, inout float2 coords)
         {
            #if _UVLOCALTOP
            coords.xy = (vertex.xz * _UVProjectOffsetScale.zw) + _UVProjectOffsetScale.xy;
            #elif _UVLOCALSIDE
            coords.xy = (vertex.zy * _UVProjectOffsetScale.zw) + _UVProjectOffsetScale.xy;
            #elif _UVLOCALFRONT
            coords.xy = (vertex.xy * _UVProjectOffsetScale.zw) + _UVProjectOffsetScale.xy;
            #elif _UVWORLDTOP
            float3 worldPos = mul (unity_ObjectToWorld, vertex).xyz;
            coords.xy = (worldPos.xz * _UVProjectOffsetScale.zw) + _UVProjectOffsetScale.xy;
            #elif _UVWORLDFRONT
            float3 worldPos = mul (unity_ObjectToWorld, vertex).xyz;
            coords.xy = (worldPos.zy * _UVProjectOffsetScale.zw) + _UVProjectOffsetScale.xy;
            #elif _UVWORLDSIDE
            float3 worldPos = mul (unity_ObjectToWorld, vertex).xyz;
            coords.xy = (worldPos.xy * _UVProjectOffsetScale.zw) + _UVProjectOffsetScale.xy;
            #endif
         }

         inline void ProjectUV2(float3 vertex, inout float2 coords)
         {
            #if _UVLOCALTOP2
            coords.xy = (vertex.xz * _UVProjectOffsetScale2.zw) + _UVProjectOffsetScale2.xy;
            #elif _UVLOCALSIDE2
            coords.xy = (vertex.zy * _UVProjectOffsetScale2.zw) + _UVProjectOffsetScale2.xy;
            #elif _UVLOCALFRONT2
            coords.xy = (vertex.xy * _UVProjectOffsetScale2.zw) + _UVProjectOffsetScale2.xy;
            #elif _UVWORLDTOP2
            float3 worldPos = mul (unity_ObjectToWorld, vertex).xyz;
            coords.xy = (worldPos.xz * _UVProjectOffsetScale2.zw) + _UVProjectOffsetScale2.xy;
            #elif _UVWORLDFRONT2
            float3 worldPos = mul (unity_ObjectToWorld, vertex).xyz;
            coords.xy = (worldPos.zy * _UVProjectOffsetScale2.zw) + _UVProjectOffsetScale2.xy;
            #elif _UVWORLDSIDE2
            float3 worldPos = mul (unity_ObjectToWorld, vertex).xyz;
            coords.xy = (worldPos.xy * _UVProjectOffsetScale2.zw) + _UVProjectOffsetScale2.xy;
            #endif
         }

         // manually compute barycentric coordinates
         float3 Barycentric(float2 p, float2 a, float2 b, float2 c)
         {
             float2 v0 = b - a;
             float2 v1 = c - a;
             float2 v2 = p - a;
             float d00 = dot(v0, v0);
             float d01 = dot(v0, v1);
             float d11 = dot(v1, v1);
             float d20 = dot(v2, v0);
             float d21 = dot(v2, v1);
             float denom = d00 * d11 - d01 * d01;
             float v = (d11 * d20 - d01 * d21) / denom;
             float w = (d00 * d21 - d01 * d20) / denom;
             float u = 1.0f - v - w;
             return float3(u, v, w);
         }

         // given two height values (from textures) and a height value for the current pixel (from vertex)
         // compute the blend factor between the two with a small blending area between them.
         half HeightBlend(half h1, half h2, half slope, half contrast)
         {
            h2 = 1 - h2;
            half tween = saturate((slope - min(h1, h2)) / max(abs(h1 - h2), 0.001)); 
            return saturate( ( tween - (1-contrast) ) / max(contrast, 0.001));
         }

         void BlendSpec(inout MegaSplatLayer base, MegaSplatLayer macro, half r, float3 albedo)
         {
            base.Metallic = lerp(base.Metallic, macro.Metallic, r);
            base.Smoothness = lerp(base.Smoothness, macro.Smoothness, r);
            base.Emission = albedo * lerp(base.Emission, macro.Emission, r);
         }

         half3 BlendOverlay(half3 base, half3 blend) { return (base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend))); }
         half3 BlendMult2X(half3  base, half3 blend) { return (base * (blend * 2)); }


         MegaSplatLayer SampleDetail(half3 weights, float3 viewDir, inout LayerParams params, half3 tpw)
         {
            MegaSplatLayer o = (MegaSplatLayer)0;
            half4 tex0, tex1, tex2;
            half4 norm0, norm1, norm2;

            SAMPLETEXARRAY(tex0, tex1, tex2, _Diffuse, params);
            fixed4 albedo = tex0 * weights.x + tex1 * weights.y + tex2 * weights.z;

            #if _NOSPECNORMAL
            half4 norm = half4(0,0,1,1);
            #else
            SAMPLETEXARRAY(norm0, norm1, norm2, _Normal, params);
               #if _PERTEXNORMALSTRENGTH
               norm0.xy *= params.normalStrength.x;
               norm1.xy *= params.normalStrength.y;
               norm2.xy *= params.normalStrength.z;
               #endif
            half4 norm = norm0 * weights.x + norm1 * weights.y + norm2 * weights.z;
            #endif

            SAMPLESPEC(o, params);

            o.Emission = albedo.rgb * specFinal.z;
            o.Albedo = albedo.rgb;
            o.Height = albedo.a;
            o.Metallic = specFinal.x;
            o.Smoothness = specFinal.y;
            o.Occlusion = specFinal.w;

            return o;
         }


         float SampleLayerHeight(half3 biWeights, float3 viewDir, inout LayerParams params, half3 tpw, float lod, float contrast)
         { 
            #if _TESSDISTANCE || _TESSEDGE
            half4 tex0, tex1, tex2;

            SAMPLETEXARRAYLOD(tex0, tex1, tex2, _Diffuse, params, lod);
            half3 weights = ComputeWeights(biWeights, tex0, tex1, tex2, contrast);
            params.weights = weights;
            return (tex0.a * params.displacementScale.x * weights.x + 
                    tex1.a * params.displacementScale.y * weights.y + 
                    tex2.a * params.displacementScale.z * weights.z);
            #endif
            return 0.5;
         }

         #if _TESSDISTANCE || _TESSEDGE
         float4 MegaSplatDistanceBasedTess (float d0, float d1, float d2, float tess)
         {
            float3 f;
            f.x = clamp(d0, 0.01, 1.0) * tess;
            f.y = clamp(d1, 0.01, 1.0) * tess;
            f.z = clamp(d2, 0.01, 1.0) * tess;

            return UnityCalcTriEdgeTessFactors (f);
         }
         #endif


         MegaSplatLayer SampleLayer(half3 biWeights, float3 viewDir, inout LayerParams params, half3 tpw)
         { 
            MegaSplatLayer o = (MegaSplatLayer)0;
            half4 tex0, tex1, tex2;
            half4 norm0, norm1, norm2;

            SAMPLETEXARRAY(tex0, tex1, tex2, _Diffuse, params);
            half3 weights = ComputeWeights(biWeights, tex0, tex1, tex2, params.contrast);
            params.weights = weights;
            fixed4 albedo = tex0 * weights.x + tex1 * weights.y + tex2 * weights.z;

            #if _PARALLAX
               float pamt = _Parallax;
               #if _PERTEXPARALLAXSTRENGTH
               // can't really do per-tex, because that would require parallaxing each texture independently. So blend..
               pamt *= (params.parallaxStrength.x * biWeights.x + params.parallaxStrength.y * biWeights.y + params.parallaxStrength.z * biWeights.z); 
               #endif
               float2 pOffset = ParallaxOffset (albedo.a, pamt, viewDir);
               params.uv0.xy += pOffset;
               params.uv1.xy += pOffset;
               params.uv2.xy += pOffset;
               SAMPLETEXARRAY(tex0, tex1, tex2, _Diffuse, params);
               weights = ComputeWeights(biWeights, tex0, tex1, tex2, params.contrast);
               albedo = tex0 * weights.x + tex1 * weights.y + tex2 * weights.z;
            #endif

            #if _ALPHA || _ALPHATEST
            half4 alpha0, alpha1, alpha2;
            SAMPLETEXARRAY(alpha0, alpha1, alpha2, _AlphaArray, params);
            o.Alpha = alpha0.r * weights.x + alpha1.r * weights.y + alpha2.r * weights.z;
            #endif


            #if _NOSPECNORMAL
            half4 norm = half4(0,0,1,1);
            #else
            SAMPLETEXARRAY(norm0, norm1, norm2, _Normal, params);
               #if _PERTEXNORMALSTRENGTH
               norm0.xy *= params.normalStrength.x;
               norm1.xy *= params.normalStrength.y;
               norm2.xy *= params.normalStrength.z;
               #endif
            half4 norm = norm0 * weights.x + norm1 * weights.y + norm2 * weights.z;
            #endif


            SAMPLESPEC(o, params);
            o.Emission = albedo.rgb * specFinal.z;
            #if _EMISMAP
            half3 emis0, emis1, emis2;
            SAMPLETEXARRAY(emis0, emis1, emis2, _Emissive, params);
            o.Emission = emis0 * weights.x + emis1 * weights.y + emis2 * weights.z;
            #endif

            o.Albedo = albedo.rgb;
            o.Height = albedo.a;
            o.Metallic = specFinal.x;
            o.Smoothness = specFinal.y;
            o.Occlusion = specFinal.w;

            #if _PERTEXAOSTRENGTH
            float aoStr = params.aoStrength.x * params.weights.x + params.aoStrength.y * params.weights.y + params.aoStrength.z * params.weights.z;
            o.Occlusion = AOContrast(o.Occlusion, aoStr);
            #endif

            return o;
         }

         half4 SampleSpecNoBlend(LayerParams params, in half4 norm, out half3 Normal)
         {
            half4 specFinal = half4(0,0,0,1);
            Normal = half3(0,0,1);
            specFinal.x = params.metallic.x;
            specFinal.y = params.smoothness.x;
            #if _NORMAL_SMOOTH_AO
               specFinal.yw = norm.zw;
               specFinal.x = params.metallic.x;
               norm.xy *= 2;
               norm.xy -= 1;
               Normal = half3(norm.x, norm.y, sqrt(1 - saturate(dot(norm.xy, norm.xy))));
            #elif _NORMAL_SMOOTH_METAL
               specFinal.yx =  norm.zw;
               norm.xy *= 2;
               norm.xy -= 1;
               Normal = half3(norm.x, norm.y, sqrt(1 - saturate(dot(norm.xy, norm.xy)))); 
            #elif _METALLICGLOSSMAP
               specFinal = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(_Gloss, params.uv0, params.dxuv, params.dyuv); 
               specFinal = half4(specFinal.xw, 0, 1);
               Normal = UnpackNormal(norm); 
            #elif _METAL_SMOOTH_EMISMASK_AO
               specFinal = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(_Gloss, params.uv0, params.dxuv, params.dyuv); 
               Normal = UnpackNormal(norm); 
            #elif _NOSPECTEX
               Normal = UnpackNormal(norm); 
            #endif
            return specFinal;
         }

         MegaSplatLayer SampleLayerNoBlend(float3 viewDir, inout LayerParams params, half3 tpw)
         { 
            MegaSplatLayer o = (MegaSplatLayer)0;
            half4 tex0;
            half4 norm0 = half4(0,0,1,1);

            tex0 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(_Diffuse, params.uv0, params.dxuv, params.dyuv); 
            fixed4 albedo = tex0;

            #if _PARALLAX
               float pamt = _Parallax;
               #if _PERTEXPARALLAXSTRENGTH
               // can't really do per-tex, because that would require parallaxing each texture independently. So blend..
               pamt *= params.parallaxStrength.x;
               #endif
               float2 pOffset = ParallaxOffset (albedo.a, pamt, viewDir);
               params.uv0.xy += pOffset;
               tex0 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(_Diffuse, params.uv0, params.dxuv, params.dyuv); 
               albedo = tex0;
            #endif

            #if _ALPHA || _ALPHATEST
            half4 alpha0 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(_AlphaArray, params.uv0, params.dxuv, params.dyuv); 
            o.Alpha = alpha0.r;
            #endif


            #if !_NOSPECNORMAL
            norm0 = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(_Normal, params.uv0, params.dxuv, params.dyuv); 
               #if _PERTEXNORMALSTRENGTH
               norm0.xy *= params.normalStrength.x;
               #endif
            #endif

            half4 specFinal = SampleSpecNoBlend(params, norm0, o.Normal);
            o.Emission = albedo.rgb * specFinal.z;
            #if _EMISMAP
            o.Emission = MSPLAT_SAMPLE_TEX2DARRAY_GRAD(_Emissive, params.uv0, params.dxuv, params.dyuv).rgb; 
            #endif

            o.Albedo = albedo.rgb;
            o.Height = albedo.a;
            o.Metallic = specFinal.x;
            o.Smoothness = specFinal.y;
            o.Occlusion = specFinal.w;

            #if _PERTEXAOSTRENGTH
            float aoStr = params.aoStrength.x;
            o.Occlusion = AOContrast(o.Occlusion, aoStr);
            #endif

            return o;
         }

         MegaSplatLayer BlendResults(MegaSplatLayer a, MegaSplatLayer b, half r)
         {
            a.Height = lerp(a.Height, b.Height, r);
            a.Albedo = lerp(a.Albedo, b.Albedo, r);
            #if !_NOSPECNORMAL
            a.Normal = lerp(a.Normal, b.Normal, r);
            #endif
            a.Metallic = lerp(a.Metallic, b.Metallic, r);
            a.Smoothness = lerp(a.Smoothness, b.Smoothness, r);
            a.Occlusion = lerp(a.Occlusion, b.Occlusion, r);
            a.Emission = lerp(a.Emission, b.Emission, r);
            #if _ALPHA || _ALPHATEST
            a.Alpha = lerp(a.Alpha, b.Alpha, r);
            #endif
            return a;
         }

         MegaSplatLayer OverlayResults(MegaSplatLayer splats, MegaSplatLayer macro, half r)
         {
            #if !_SPLATSONTOP
            r = 1 - r;
            #endif

            #if _ALPHA || _ALPHATEST
            splats.Alpha = min(macro.Alpha, splats.Alpha);
            #endif

            #if _MACROMULT2X
               splats.Albedo = lerp(BlendMult2X(macro.Albedo, splats.Albedo), splats.Albedo, r);
               #if !_NOSPECNORMAL
               splats.Normal = lerp(BlendNormals(macro.Normal, splats.Normal), splats.Normal, r); 
               #endif
               splats.Occlusion = lerp((macro.Occlusion + splats.Occlusion) * 0.5, splats.Occlusion, r);
               BlendSpec(macro, splats, r, splats.Albedo);
            #elif _MACROOVERLAY
               splats.Albedo = lerp(BlendOverlay(macro.Albedo, splats.Albedo), splats.Albedo, r);
               #if !_NOSPECNORMAL
               splats.Normal  = lerp(BlendNormals(macro.Normal, splats.Normal), splats.Normal, r); 
               #endif
               splats.Occlusion = lerp((macro.Occlusion + splats.Occlusion) * 0.5, splats.Occlusion, r);
               BlendSpec(macro, splats, r, splats.Albedo);
            #elif _MACROMULT
               splats.Albedo = lerp(splats.Albedo * macro.Albedo, splats.Albedo, r);
               #if !_NOSPECNORMAL
               splats.Normal  = lerp(BlendNormals(macro.Normal, splats.Normal), splats.Normal, r); 
               #endif
               splats.Occlusion = lerp((macro.Occlusion + splats.Occlusion) * 0.5, splats.Occlusion, r);
               BlendSpec(macro, splats, r, splats.Albedo);
            #else
               splats.Albedo = lerp(macro.Albedo, splats.Albedo, r);
               #if !_NOSPECNORMAL
               splats.Normal  = lerp(macro.Normal, splats.Normal, r); 
               #endif
               splats.Occlusion = lerp(macro.Occlusion, splats.Occlusion, r);
               BlendSpec(macro, splats, r, splats.Albedo);
            #endif

            return splats;
         }

         MegaSplatLayer BlendDetail(MegaSplatLayer splats, MegaSplatLayer detail, float detailBlend)
         {
            #if _DETAILMAP
            detailBlend *= _DetailTextureStrength;
            #endif
            #if _DETAILMULT2X
               splats.Albedo = lerp(detail.Albedo, BlendMult2X(splats.Albedo, detail.Albedo), detailBlend);
            #elif _DETAILOVERLAY
               splats.Albedo = lerp(detail.Albedo, BlendOverlay(splats.Albedo, detail.Albedo), detailBlend);
            #elif _DETAILMULT
               splats.Albedo = lerp(detail.Albedo, splats.Albedo * detail.Albedo, detailBlend);
            #else
               splats.Albedo = lerp(detail.Albedo, splats.Albedo, detailBlend);
            #endif 

            #if !_NOSPECNORMAL
            splats.Normal = lerp(splats.Normal, BlendNormals(splats.Normal, detail.Normal), detailBlend);
            #endif
            return splats;
         }

         void WaterBRDF (inout half3 Albedo, inout half Smoothness, half metalness, half wetFactor, half surfPorosity) 
         {
            #if _PUDDLES || _PUDDLEFLOW || _WETNESS
            half porosity = saturate((( (1 - Smoothness) - 0.5)) / max(surfPorosity, 0.001));
            half factor = lerp(1, 0.2, (1 - metalness) * porosity);
            Albedo *= lerp(1.0, factor, wetFactor);
            Smoothness = lerp(1.0, Smoothness, lerp(1.0, factor, wetFactor));
            #endif
         }

         float DoPuddles(inout MegaSplatLayer o, float2 uv, half puddleLevel, half2 flowDir, half porosity)
         {
            #if _PUDDLES || _PUDDLEFLOW
            float waterBlend = saturate((puddleLevel - o.Height) * _PuddleBlend);

            #if _PUDDLEFLOW
            uv *= _PuddleUVScales.xy;

            float2 uv1;
            float2 uv2;
            half interp;
            Flow(uv, flowDir, _PuddleFlowParams.y, _PuddleFlowParams.z * saturate(waterBlend-0.1), uv1, uv2, interp);

            half4 fd = lerp(tex2D(_PuddleNormal, uv1), tex2D(_PuddleNormal, uv2), interp);
            half3 waterNorm = UnpackNormal(fd);
            #else 
            half3 waterNorm = half3(0,0,1);
            #endif


            half3 wetAlbedo = o.Albedo * _PuddleTint * 2;
            half wetSmoothness = o.Smoothness;
            WaterBRDF(wetAlbedo, wetSmoothness, o.Metallic, waterBlend, porosity);
            #if _PUDDLEFLOW
            wetAlbedo += _PuddleFlowParams.w * (waterNorm.x + waterNorm.y);
            #endif
            o.Normal = lerp(o.Normal, waterNorm, waterBlend);
            o.Occlusion = lerp(o.Occlusion, 1, waterBlend);
            o.Smoothness = lerp(o.Smoothness, wetSmoothness, waterBlend);
            o.Albedo = lerp(o.Albedo, wetAlbedo, waterBlend);
            return waterBlend;
            #endif
            return 0;
         }

         float DoSnowDisplace(float splat_height, float2 uv, float3 worldNormal, half snowHeightFade, float puddleHeight)
         {
            // could force a branch and avoid texsamples
            #if _SNOW
            uv *= _SnowUVScales.xy;
            half4 snowAlb = tex2D(_SnowDiff, uv);
            half4 snowNsao = tex2D(_SnowNormal, uv);

            float snowAmount, wetnessMask, snowNormalAmount;
            float snowFade = saturate((_SnowAmount - puddleHeight) * snowHeightFade);

            float height = splat_height * _SnowParams.x;
            float erosion = lerp(0, height, _SnowParams.y);
            float snowMask = saturate((snowFade - erosion));
            float snowMask2 = saturate((snowFade - erosion) * 8);
            snowMask *= snowMask * snowMask * snowMask * snowMask * snowMask2;
            snowAmount = snowMask * saturate(dot(worldNormal, _SnowUpVector));

            return snowAmount;
            #endif
            return 0;
         }

         float DoSnow(inout MegaSplatLayer o, float2 uv, float3 worldNormal, half snowHeightFade, float puddleHeight, half surfPorosity)
         {
            // could force a branch and avoid texsamples
            #if _SNOW
            uv *= _SnowUVScales.xy;
            half4 snowAlb = tex2D(_SnowDiff, uv);
            half4 snowNsao = tex2D(_SnowNormal, uv);

            half3 snowNormal = half3(snowNsao.xy * 2 - 1, 1);
            snowNormal.z = sqrt(1 - saturate(dot(snowNormal.xy, snowNormal.xy)));

            float snowAmount, wetnessMask, snowNormalAmount;
            float snowFade = saturate((_SnowAmount - puddleHeight) * snowHeightFade);
            float ao = o.Occlusion;
            if (snowFade > 0)
            {
               float height = o.Height * _SnowParams.x;
               float erosion = lerp(1-ao, (height + ao) * 0.5, _SnowParams.y);
               float snowMask = saturate((snowFade - erosion) * 8);
               snowMask *= snowMask * snowMask * snowMask;
               snowAmount = snowMask * saturate(dot(worldNormal, _SnowUpVector));  // up
               wetnessMask = saturate((_SnowParams.w * (4.0 * snowFade) - (height + snowNsao.b) * 0.5));
               snowAmount = saturate(snowAmount * 8);
               snowNormalAmount = snowAmount * snowAmount;

               float porosity = saturate((((1.0 - o.Smoothness) - 0.5)) / max(surfPorosity, 0.001));
               float factor = lerp(1, 0.4, porosity);

               o.Albedo *= lerp(1.0, factor, wetnessMask);
               o.Normal = lerp(o.Normal, float3(0,0,1), wetnessMask);
               o.Smoothness = lerp(o.Smoothness, 0.8, wetnessMask);

            }
            o.Albedo = lerp(o.Albedo, snowAlb.rgb, snowAmount);
            o.Normal = lerp(o.Normal, snowNormal, snowNormalAmount);
            o.Smoothness = lerp(o.Smoothness, (snowNsao.b) * _SnowParams.z, snowAmount);
            o.Occlusion = lerp(o.Occlusion, snowNsao.w, snowAmount);
            o.Height = lerp(o.Height, snowAlb.a, snowAmount);
            o.Metallic = lerp(o.Metallic, 0.01, snowAmount);
            float crystals = saturate(0.65 - snowNsao.b);
            o.Smoothness = lerp(o.Smoothness, crystals * _SnowParams.z, snowAmount);
            return snowAmount;
            #endif
            return 0;
         }


         half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
         {
            return half4(s.Albedo, 1);
         }

         #if _RAMPLIGHTING
         half3 DoLightingRamp(half3 albedo, float3 normal, float3 emission, half3 lightDir, half atten)
         {
            half NdotL = dot (normal, lightDir);
            half diff = NdotL * 0.5 + 0.5;
            half3 ramp = tex2D (_Ramp, diff.xx).rgb;
            return (albedo * _LightColor0.rgb * ramp * atten) + emission;
         }

         half4 LightingRamp (SurfaceOutput s, half3 lightDir, half atten) 
         {
            half4 c;
            c.rgb = DoLightingRamp(s.Albedo, s.Normal, s.Emission, lightDir, atten);
            c.a = s.Alpha;
            return c;
         }
         #endif

         void ApplyDetailNoise(inout half3 albedo, inout half3 norm, inout half smoothness, LayerParams data, float camDist, float3 tpw)
         {
            #if _DETAILNOISE

               #if _TRIPLANAR
               float2 uv0 = data.tpuv0_x.xy * _DetailNoiseScaleStrengthFade.x;
               float2 uv1 = data.tpuv0_y.xy * _DetailNoiseScaleStrengthFade.x;
               float2 uv2 = data.tpuv1_z.xy * _DetailNoiseScaleStrengthFade.x;
               float2 dx0 = ddx(uv0);
               float2 dy0 = ddy(uv0);
               float2 dx1 = ddx(uv1);
               float2 dy1 = ddy(uv1);
               float2 dx2 = ddx(uv2);
               float2 dy2 = ddy(uv2);
               #else
               float2 uv = data.uv0.xy * _DetailNoiseScaleStrengthFade.x;
               float2 dx = ddx(uv);
               float2 dy = ddy(uv);
               #endif

            
               UNITY_BRANCH
               if (camDist < _DetailNoiseScaleStrengthFade.z)
               {
                  #if _TRIPLANAR
                  half3 noise = (tex2Dgrad(_DetailNoise, uv0, dx0, dy0) * tpw.x + 
                     tex2Dgrad(_DetailNoise, uv1, dx1, dy1) * tpw.y + 
                     tex2Dgrad(_DetailNoise, uv2, dx2, dy2) * tpw.z).rgb; 
                  #else
                  half3 noise = tex2Dgrad(_DetailNoise, uv, dx, dy).rgb;
                  #endif

                  float fade = 1.0 - ((_DetailNoiseScaleStrengthFade.z - camDist) / _DetailNoiseScaleStrengthFade.z);
                  fade = 1.0 - (fade*fade);
                  fade *= _DetailNoiseScaleStrengthFade.y;

                  #if _PERTEXNOISESTRENGTH
                  fade *= (data.detailNoiseStrength.x * data.weights.x + data.detailNoiseStrength.y * data.weights.y + data.detailNoiseStrength.z * data.weights.z);
                  #endif

                  albedo = lerp(albedo, BlendMult2X(albedo, noise.zzz), fade);
                  noise *= 0.5;
                  #if !_NOSPECNORMAL
                  norm.xy += ((noise.xy-0.25) * fade);
                  #endif
                  #if !_NOSPECNORMAL || _NOSPECTEX
                  smoothness += (abs(noise.x-0.25) * fade);
                  #endif
               }
            #endif
         }


         LayerParams InitLayerParams(SplatInput si, float3 values, half2 texScale)
         {
            LayerParams data = NewLayerParams();
            #if _TERRAIN
            int i0 = round(values.x * 255);
            int i1 = round(values.y * 255);
            int i2 = round(values.z * 255);
            #else
            int i0 = round(values.x / max(si.weights.x, 0.00001));
            int i1 = round(values.y / max(si.weights.y, 0.00001));
            int i2 = round(values.z / max(si.weights.z, 0.00001));
            #endif

            #if _TRIPLANAR
            float3 coords = si.triplanarUVW * texScale.x;
            data.tpuv0_x = float3(coords.zy, i0);
            data.tpuv0_y = float3(coords.xz, i0);
            data.tpuv0_z = float3(coords.xy, i0);
            data.tpuv1_x = float3(coords.zy, i1);
            data.tpuv1_y = float3(coords.xz, i1);
            data.tpuv1_z = float3(coords.xy, i1);
            data.tpuv2_x = float3(coords.zy, i2);
            data.tpuv2_y = float3(coords.xz, i2);
            data.tpuv2_z = float3(coords.xy, i2);
            data.dxtpcoords = ddx(coords);
            data.dxtpcoords = ddy(coords);

            float2 splatUV = si.splatUV * texScale.xy;
            data.uv0 = float3(splatUV, i0);
            data.uv1 = float3(splatUV, i1);
            data.uv2 = float3(splatUV, i2);
            data.dxuv = ddx(splatUV);
            data.dyuv = ddy(splatUV);
            #else
            float2 splatUV = si.splatUV.xy * texScale.xy;
            data.uv0 = float3(splatUV, i0);
            data.uv1 = float3(splatUV, i1);
            data.uv2 = float3(splatUV, i2);
            data.dxuv = ddx(splatUV);
            data.dyuv = ddy(splatUV);
            #endif

            #if _FLOW || _FLOWREFRACTION
            data.flowOn = 0;
            #endif

            #if _DISTANCERESAMPLE
            InitDistanceResample(data, si.camDist.y);
            #endif

            return data;
         }

         MegaSplatLayer DoSurf(inout SplatInput si, MegaSplatLayer macro, float3x3 tangentToWorld)
         {
            #if _ALPHALAYER
            LayerParams mData = InitLayerParams(si, si.valuesSecond.xyz, _TexScales.xy);
            #else
            LayerParams mData = InitLayerParams(si, si.valuesMain.xyz, _TexScales.xy);
            #endif

            #if _PARALLAX && (_TESSEDGE || _TESSDISTANCE || _LOWPOLY)
            si.viewDir = mul(si.viewDir, tangentToWorld);
            #endif

            SamplePerTex(_PropertyTex, mData, _PerTexScaleRange);

            half porosity = _GlobalPorosityWetness.x;
            #if _PERTEXMATPARAMS
            porosity = mData.porosity.x * mData.weights.x + mData.porosity.y * mData.weights.y + mData.porosity.z * mData.weights.z;
            #endif

            #if _TWOLAYER
               LayerParams sData = InitLayerParams(si, si.valuesSecond.xyz, _TexScales.zw);
               sData.layerBlend = si.layerBlend;
               SamplePerTex(_PropertyTex, sData, _PerTexScaleRange);

               #if (_FLOW || _FLOWREFRACTION)
                  Flow(sData.uv0, si.flowDir, _FlowSpeed * sData.flowIntensity.x, _FlowIntensity, sData.fuv0a, sData.fuv0b, sData.flowInterps.x);
                  Flow(sData.uv1, si.flowDir, _FlowSpeed * sData.flowIntensity.y, _FlowIntensity, sData.fuv1a, sData.fuv1b, sData.flowInterps.y);
                  Flow(sData.uv2, si.flowDir, _FlowSpeed * sData.flowIntensity.z, _FlowIntensity, sData.fuv2a, sData.fuv2b, sData.flowInterps.z);
                  sData.dxfuva = ddx(sData.fuv0a.xy);
                  sData.dyfuva = ddy(sData.fuv0a.xy);
                  sData.dxfuvb = ddx(sData.fuv0b.xy);
                  sData.dyfuvb = ddy(sData.fuv0b.xy);
                  mData.flowOn = 0;
               #endif

               MegaSplatLayer second = SampleLayer(si.weights, si.viewDir, sData, si.triplanarBlend);

               #if _FLOWREFRACTION
                  float hMod = FlowRefract(second, mData, sData, si.weights);
               #endif
            #else // _TWOLAYER

               #if (_FLOW || _FLOWREFRACTION)
                  Flow(mData.uv0, si.flowDir, _FlowSpeed * mData.flowIntensity.x, _FlowIntensity, mData.fuv0a, mData.fuv0b, mData.flowInterps.x);
                  Flow(mData.uv1, si.flowDir, _FlowSpeed * mData.flowIntensity.y, _FlowIntensity, mData.fuv1a, mData.fuv1b, mData.flowInterps.y);
                  Flow(mData.uv2, si.flowDir, _FlowSpeed * mData.flowIntensity.z, _FlowIntensity, mData.fuv2a, mData.fuv2b, mData.flowInterps.z);
                  mData.dxfuva = ddx(mData.fuv0a.xy);
                  mData.dyfuva = ddy(mData.fuv0a.xy);
                  mData.dxfuvb = ddx(mData.fuv0b.xy);
                  mData.dyfuvb = ddy(mData.fuv0b.xy);
               #endif
            #endif

            #if _NOBLENDBOTTOM
            MegaSplatLayer splats = SampleLayerNoBlend(si.viewDir, mData, si.triplanarBlend);
            #else
            MegaSplatLayer splats = SampleLayer(si.weights, si.viewDir, mData, si.triplanarBlend);
            #endif

            #if _TWOLAYER
               // blend layers together..
               float hfac = HeightBlend(splats.Height, second.Height, sData.layerBlend, _Contrast);
               #if _FLOWREFRACTION
                  hfac *= hMod;
               #endif
               splats = BlendResults(splats, second, hfac);
               porosity = lerp(porosity, 
                     sData.porosity.x * sData.weights.x + 
                     sData.porosity.y * sData.weights.y + 
                     sData.porosity.z * sData.weights.z,
                     hfac);
            #endif

            half macroBlend = 1;
            #if _USEMACROTEXTURE
            macroBlend = saturate(_MacroTextureStrength * si.camDist.x);
            #endif

            #if _DETAILMAP
               float dist = si.camDist.y;
               UNITY_BRANCH
               if (dist > _DistanceFades.w)
               {
                  MegaSplatLayer o = (MegaSplatLayer)0;
                  UNITY_INITIALIZE_OUTPUT(MegaSplatLayer,o);
                  #if _USEMACROTEXTURE
                  splats = OverlayResults(splats, macro, macroBlend);
                  #endif
                  o.Albedo = splats.Albedo;
                  o.Normal = splats.Normal;
                  o.Emission = macro.Emission;
                  o.Occlusion = splats.Occlusion;
                  #if !_RAMPLIGHTING
                  o.Metallic = splats.Metallic;
                  o.Smoothness = splats.Smoothness;
                  #endif

                  return o;
               }

               LayerParams sData = InitLayerParams(si, si.valuesMain, _TexScales.zw);
               MegaSplatLayer second = SampleDetail(mData.weights, si.viewDir, sData, si.triplanarBlend);   // use prev weights for detail

               float detailBlend = 1.0 - saturate((dist - _DistanceFades.z) / (_DistanceFades.w - _DistanceFades.z));
               splats = BlendDetail(splats, second, detailBlend); 
            #endif

            ApplyDetailNoise(splats.Albedo, splats.Normal, splats.Smoothness, mData, si.camDist.y, si.triplanarBlend);

            float pud = 0;
            #if _PUDDLES || _PUDDLEFLOW
               pud = DoPuddles(splats, si.macroUV, si.puddleHeight, si.flowDir, porosity);
            #endif

            #if _SNOW
               DoSnow(splats, si.macroUV, mul(tangentToWorld, normalize(splats.Normal)), si.snowHeightFade, pud, porosity);
            #endif

            #if _WETNESS
            WaterBRDF(splats.Albedo, splats.Smoothness, splats.Metallic, max( si.wetness, _GlobalPorosityWetness.y), porosity); 
            #endif

            #if _ALPHALAYER
            half blend = HeightBlend(splats.Height, 1 - macro.Height, si.layerBlend * macroBlend, _Contrast);
            splats = OverlayResults(splats, macro, blend);
            #elif _USEMACROTEXTURE
            splats = OverlayResults(splats, macro, macroBlend);
            #endif

            return splats;
         }


      // heavility packed structure
      struct Input
      {
          // avoid naming UV because unity magic..
          float2 coords;               // uv, or triplanar UV
          float4 valuesMain;           //index rgb, triplanar W
          #if _TWOLAYER || _ALPHALAYER
          float4 valuesSecond;         //index rgb + alpha
          #endif
          fixed3 weights : COLOR0;     // Causes unity to automagically map this from vertex color, erasing your values.. grr..
          float3 viewDir;              // special unity keyword magic, cannot pack .a, do not pass go and collect $100
          float2 camDist;              // distance from camera (for fades) and fog
          float4 extraData;            // flowdir + fade, or if triplanar triplanarView, .w contains puddle height
          // everything after this requires > 3.5 shader model :(
          #if _SECONDUV
          float2 macroUV;              // special macro UV only used in alphalayer mode
          #endif
          #if _SNOW
          float3 wsNormal;
          float4 wsTangent;
          half snowHeightFade;
          #endif
          #if _WETNESS
          half wetness;
          #endif
      };

      SplatInput ToSplatInput(Input i)
      {
         SplatInput o = (SplatInput)0;
         UNITY_INITIALIZE_OUTPUT(SplatInput,o);
         o.weights = i.weights.xyz;
         o.valuesMain = i.valuesMain.xyz;
         o.viewDir = i.viewDir;
         o.camDist.xy = i.camDist.xy;
         #if _TWOLAYER || _ALPHALAYER
         o.valuesSecond = i.valuesSecond.xyz;
         o.layerBlend = i.valuesSecond.w;
         #endif
         o.splatUV = i.coords.xy;
         o.macroUV = i.coords.xy;
         #if _SECONDUV
         o.macroUV = i.macroUV.xy;
         #endif
         #if _TRIPLANAR
         o.triplanarUVW = float3(i.coords.xy, i.valuesMain.w);
         o.triplanarBlend = i.extraData.xyz;
         #endif
         #if _FLOW || _FLOWREFRACTION || _PUDDLEFLOW
         o.flowDir = i.extraData.xy;
         #endif
         #if _PUDDLES || _PUDDLEFLOW
         o.puddleHeight = i.extraData.w;
         #endif

         #if _TESSDAMPENING
         o.displacementDampening = i.weights.w;
         #endif

         #if _SNOW
         o.snowHeightFade = i.snowHeightFade;
         #endif

         #if _WETNESS
         o.wetness = i.wetness;
         #endif
         return o;
      }


      void vert (inout appdata_full i, out Input o) 
      {
          UNITY_INITIALIZE_OUTPUT(Input,o);
          // select the texture coordinate for the splat texture
          o.coords.xy = i.texcoord.xy;
          #if _UVFROMSECOND
          o.coords.xy = i.texcoord1.xy;
          #endif

          #if _SECONDUV
             o.macroUV = i.texcoord.xy;
             #if _UVFROMSECOND2
             o.macroUV = i.texcoord1.xy;
             #endif
          #endif

          ProjectUVs(i.vertex.xyz, o.coords.xy);
          #if _SECONDUV
          ProjectUV2(i.vertex.xyz, o.macroUV.xy);
          #endif

          // filters in vertex color, main splat in color.a, secondary in uv2.a
          o.weights = i.color.rgb;
          o.valuesMain.xyz = i.color.rgb * i.color.a * 255;
          #if _TWOLAYER || _ALPHALAYER
          o.valuesSecond.xyz = i.color.rgb * i.texcoord3.a * 255;
          o.valuesSecond.a = i.texcoord3.x;
          #endif

          float3 worldPos = mul (unity_ObjectToWorld, i.vertex).xyz;

          float dist = distance(_WorldSpaceCameraPos, worldPos);
          o.camDist.x = 1.0 - saturate((dist - _DistanceFades.x) / (_DistanceFades.y - _DistanceFades.x));
          o.camDist.y = length(mul (UNITY_MATRIX_MV, i.vertex).xyz);

          #if _FLOW || _FLOWREFRACTION || _PUDDLEFLOW
          o.extraData.xy = (i.texcoord2.zw * 2.0 - 1.0);
          #endif

          #if _PUDDLES || _PUDDLEFLOW
          o.extraData.w = i.texcoord3.y;
          #endif

          #if _WETNESS
          o.wetness = i.texcoord1.w;
          #endif

          #if _TRIPLANAR
             float3 norm = i.normal;
             #if _TRIPLANAR_WORLDSPACE
             float3 uvw = worldPos * _TriplanarTexScale + _TriplanarOffset;
             norm = normalize(mul(unity_ObjectToWorld, norm));
             #else
             float3 uvw = i.vertex.xyz * _TriplanarTexScale + _TriplanarOffset;
             #endif
             o.coords.xy = uvw.xy;
             o.valuesMain.w = uvw.z;
             o.extraData.xyz = pow(abs(norm), _TriplanarContrast);
          #endif


         #if _SNOW
         o.snowHeightFade = saturate((worldPos.y - _SnowHeightRange.x) / max(_SnowHeightRange.y, 0.001));
         o.wsNormal = mul((float3x3)unity_ObjectToWorld, i.normal);
         o.wsTangent = mul(unity_ObjectToWorld, i.tangent);
         #endif
      }  


      void surf (Input i, inout SurfaceOutputStandard o) 
      {
         SplatInput si = ToSplatInput(i);
         float3x3 tangentToWorld = (float3x3)0;
         #if _SNOW
         float3 tangent = normalize(i.wsTangent.xyz);
         float3 normal = normalize(i.wsNormal);
         float3 binormal = normalize(cross(normal, tangent) * i.wsTangent.w);
         tangentToWorld = transpose(float3x3(tangent, binormal, normal));
         #endif

         MegaSplatLayer macro = (MegaSplatLayer)0;
         #if _USEMACROTEXTURE || _ALPHALAYER
            macro = SampleMacro(si.macroUV.xy);
            #if _SNOW && _SNOWOVERMACRO
            DoSnow(macro, si.macroUV.xy, mul(tangentToWorld, normalize(macro.Normal)), si.snowHeightFade, 0, _GlobalPorosityWetness.x);
            #endif
            #if _DISABLESPLATSINDISTANCE
            UNITY_BRANCH
            if (i.camDist.x <= 0.0)
            {
               
               o.Albedo = macro.Albedo;
               o.Normal = macro.Normal;
               o.Emission = macro.Emission;
               #if !_RAMPLIGHTING
               o.Smoothness = macro.Smoothness;
               o.Metallic = macro.Metallic;
               o.Occlusion = macro.Occlusion;
               #endif
               #if _ALPHA || _ALPHATEST
               o.Alpha = macro.Alpha;
               #endif
               return;
            }
            #endif
         #endif




         MegaSplatLayer splats = DoSurf(si, macro, tangentToWorld);

         #if _DEBUG_OUTPUT_ALBEDO
         o.Albedo = splats.Albedo;
         #elif _DEBUG_OUTPUT_HEIGHT
         o.Albedo = splats.Height.xxx * saturate(splats.Albedo+1);
         #elif _DEBUG_OUTPUT_NORMAL
         o.Albedo = splats.Normal * 0.5 + 0.5 * saturate(splats.Albedo+1);
         #elif _DEBUG_OUTPUT_SMOOTHNESS
         o.Albedo = splats.Smoothness.xxx * saturate(splats.Albedo+1);
         #elif _DEBUG_OUTPUT_METAL
         o.Albedo = splats.Metallic.xxx * saturate(splats.Albedo+1);
         #elif _DEBUG_OUTPUT_AO
         o.Albedo = splats.Occlusion.xxx * saturate(splats.Albedo+1);
         #elif _DEBUG_OUTPUT_EMISSION
         o.Albedo = splats.Emission * saturate(splats.Albedo+1);
         #elif _RAMPLIGHTING
         o.Albedo = splats.Albedo;
         o.Emission = splats.Emission;
         o.Normal = splats.Normal;
         #else
         o.Albedo = splats.Albedo;
         o.Normal = splats.Normal;
         o.Metallic = splats.Metallic;
         o.Smoothness = splats.Smoothness;
         o.Occlusion = splats.Occlusion;
         o.Emission = splats.Emission;
            #if _ALPHA || _ALPHATEST
               o.Alpha = splats.Alpha;
            #endif

            #if _ALPHATEST
            clip(o.Alpha - 0.5);
            #endif
         #endif
      }

      void fogcolor(Input i, SurfaceOutputStandard o, inout fixed4 col)
      {
         UNITY_APPLY_FOG(i.camDist.y, col);
      }

      ENDCG
   }
   CustomEditor "SplatArrayShaderGUI"
   FallBack "Diffuse"
}
