// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:True,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33745,y:32674,varname:node_4013,prsc:2|emission-9552-OUT,difocc-2251-R,alpha-2625-OUT,clip-7845-OUT;n:type:ShaderForge.SFN_Tex2d,id:2251,x:32294,y:32701,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:node_2251,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_LightAttenuation,id:5187,x:32294,y:32864,varname:node_5187,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1282,x:32575,y:32706,varname:node_1282,prsc:2|A-2251-RGB,B-5187-OUT,C-3867-OUT,D-8908-OUT;n:type:ShaderForge.SFN_Slider,id:3867,x:32137,y:33017,ptovrint:False,ptlb:Diffuse Light,ptin:_DiffuseLight,varname:node_3867,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:3;n:type:ShaderForge.SFN_AmbientLight,id:2159,x:32294,y:33198,varname:node_2159,prsc:2;n:type:ShaderForge.SFN_Multiply,id:685,x:32575,y:32829,varname:node_685,prsc:2|A-2251-RGB,B-2159-RGB,C-5616-OUT,D-1549-OUT,E-3217-OUT;n:type:ShaderForge.SFN_Add,id:6622,x:32805,y:32753,varname:node_6622,prsc:2|A-1282-OUT,B-685-OUT,C-9424-OUT;n:type:ShaderForge.SFN_Vector1,id:1549,x:32575,y:32965,varname:node_1549,prsc:2,v1:3;n:type:ShaderForge.SFN_Slider,id:5616,x:32137,y:33118,ptovrint:False,ptlb:Ambient Light,ptin:_AmbientLight,varname:_DiffuseLight_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:5;n:type:ShaderForge.SFN_RemapRange,id:6141,x:32575,y:33045,varname:node_6141,prsc:2,frmn:0,frmx:1,tomn:-0.5,tomx:2.5|IN-2251-A;n:type:ShaderForge.SFN_Clamp01,id:7983,x:32736,y:33045,varname:node_7983,prsc:2|IN-6141-OUT;n:type:ShaderForge.SFN_RemapRange,id:7845,x:32901,y:33045,varname:node_7845,prsc:2,frmn:0,frmx:1,tomn:0,tomx:5|IN-7983-OUT;n:type:ShaderForge.SFN_RemapRange,id:2625,x:32901,y:32890,varname:node_2625,prsc:2,frmn:0,frmx:1,tomn:0,tomx:1.5|IN-2251-A;n:type:ShaderForge.SFN_NormalVector,id:1402,x:32294,y:32531,prsc:2,pt:False;n:type:ShaderForge.SFN_LightVector,id:5076,x:32294,y:32408,varname:node_5076,prsc:2;n:type:ShaderForge.SFN_Dot,id:8774,x:32532,y:32454,varname:node_8774,prsc:2,dt:1|A-5076-OUT,B-1402-OUT;n:type:ShaderForge.SFN_RemapRange,id:2611,x:32882,y:32454,varname:node_2611,prsc:2,frmn:0,frmx:1,tomn:0,tomx:0.65|IN-9118-OUT;n:type:ShaderForge.SFN_Clamp01,id:9118,x:32706,y:32454,varname:node_9118,prsc:2|IN-8774-OUT;n:type:ShaderForge.SFN_Multiply,id:9424,x:33067,y:32454,varname:node_9424,prsc:2|A-2611-OUT,B-7551-OUT,C-2251-RGB;n:type:ShaderForge.SFN_Vector3,id:7551,x:33067,y:32361,varname:node_7551,prsc:2,v1:0.4587727,v2:0.8014706,v3:0;n:type:ShaderForge.SFN_ObjectPosition,id:3820,x:32294,y:32234,varname:node_3820,prsc:2;n:type:ShaderForge.SFN_Frac,id:2272,x:32470,y:32234,varname:node_2272,prsc:2|IN-3820-XYZ;n:type:ShaderForge.SFN_Multiply,id:9790,x:32993,y:32753,varname:node_9790,prsc:2|A-6622-OUT,B-2026-OUT,C-4135-RGB;n:type:ShaderForge.SFN_RemapRange,id:3642,x:32647,y:32234,varname:node_3642,prsc:2,frmn:0,frmx:1,tomn:0.3,tomx:1.2|IN-2272-OUT;n:type:ShaderForge.SFN_Desaturate,id:2026,x:32838,y:32234,varname:node_2026,prsc:2|COL-3642-OUT,DES-6471-OUT;n:type:ShaderForge.SFN_Vector1,id:6471,x:32838,y:32163,varname:node_6471,prsc:2,v1:0.8;n:type:ShaderForge.SFN_FragmentPosition,id:1307,x:32294,y:32092,varname:node_1307,prsc:2;n:type:ShaderForge.SFN_LightColor,id:4135,x:32993,y:32623,varname:node_4135,prsc:2;n:type:ShaderForge.SFN_FragmentPosition,id:9250,x:31703,y:32645,varname:node_9250,prsc:2;n:type:ShaderForge.SFN_Clamp01,id:3217,x:31933,y:32645,varname:node_3217,prsc:2|IN-6931-OUT;n:type:ShaderForge.SFN_RemapRange,id:6931,x:31703,y:32820,varname:node_6931,prsc:2,frmn:0,frmx:1,tomn:0,tomx:0.1|IN-9250-Y;n:type:ShaderForge.SFN_Blend,id:7043,x:33232,y:32684,varname:node_7043,prsc:2,blmd:10,clmp:True|SRC-9790-OUT,DST-9790-OUT;n:type:ShaderForge.SFN_Blend,id:1845,x:33403,y:32684,varname:node_1845,prsc:2,blmd:10,clmp:True|SRC-7043-OUT,DST-7043-OUT;n:type:ShaderForge.SFN_Lerp,id:9552,x:33403,y:32872,varname:node_9552,prsc:2|A-1845-OUT,B-9790-OUT,T-937-OUT;n:type:ShaderForge.SFN_Vector1,id:937,x:33403,y:32999,varname:node_937,prsc:2,v1:0.8;n:type:ShaderForge.SFN_RemapRange,id:8908,x:31933,y:32820,varname:node_8908,prsc:2,frmn:0,frmx:1,tomn:0.5,tomx:1|IN-3217-OUT;proporder:2251-3867-5616;pass:END;sub:END;*/

Shader "Shader Forge/Foliage Flat" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "white" {}
        _DiffuseLight ("Diffuse Light", Range(0, 3)) = 0
        _AmbientLight ("Ambient Light", Range(0, 5)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float _DiffuseLight;
            uniform float _AmbientLight;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip((saturate((_Diffuse_var.a*3.0+-0.5))*5.0+0.0) - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
////// Emissive:
                float node_3217 = saturate((i.posWorld.g*0.1+0.0));
                float3 node_9790 = (((_Diffuse_var.rgb*attenuation*_DiffuseLight*(node_3217*0.5+0.5))+(_Diffuse_var.rgb*UNITY_LIGHTMODEL_AMBIENT.rgb*_AmbientLight*3.0*node_3217)+((saturate(max(0,dot(lightDirection,i.normalDir)))*0.65+0.0)*float3(0.4587727,0.8014706,0)*_Diffuse_var.rgb))*lerp((frac(objPos.rgb)*0.9+0.3),dot((frac(objPos.rgb)*0.9+0.3),float3(0.3,0.59,0.11)),0.8)*_LightColor0.rgb);
                float3 node_7043 = saturate(( node_9790 > 0.5 ? (1.0-(1.0-2.0*(node_9790-0.5))*(1.0-node_9790)) : (2.0*node_9790*node_9790) ));
                float3 emissive = lerp(saturate(( node_7043 > 0.5 ? (1.0-(1.0-2.0*(node_7043-0.5))*(1.0-node_7043)) : (2.0*node_7043*node_7043) )),node_9790,0.8);
                float3 finalColor = emissive;
                return fixed4(finalColor,(_Diffuse_var.a*1.5+0.0));
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float _DiffuseLight;
            uniform float _AmbientLight;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip((saturate((_Diffuse_var.a*3.0+-0.5))*5.0+0.0) - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float3 finalColor = 0;
                return fixed4(finalColor * (_Diffuse_var.a*1.5+0.0),0);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip((saturate((_Diffuse_var.a*3.0+-0.5))*5.0+0.0) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
