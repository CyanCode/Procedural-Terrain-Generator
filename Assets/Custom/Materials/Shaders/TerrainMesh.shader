// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4459,x:32951,y:32724,varname:node_4459,prsc:2|diff-7275-OUT;n:type:ShaderForge.SFN_Tex2d,id:9083,x:31489,y:32628,ptovrint:False,ptlb:Grass,ptin:_Grass,varname:node_9083,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c25a06c9c28fd4e4292b3e9a129715d5,ntxv:0,isnm:False|UVIN-8401-OUT;n:type:ShaderForge.SFN_Tex2d,id:3384,x:31489,y:32817,ptovrint:False,ptlb:Rock,ptin:_Rock,varname:node_3384,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e13ad7a9d4dc84697aaa540e7548d65b,ntxv:0,isnm:False|UVIN-8401-OUT;n:type:ShaderForge.SFN_Multiply,id:8401,x:31241,y:32712,varname:node_8401,prsc:2|A-7132-UVOUT,B-9964-OUT;n:type:ShaderForge.SFN_TexCoord,id:7132,x:31045,y:32625,varname:node_7132,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:9964,x:31045,y:32802,ptovrint:False,ptlb:Texture Tiling,ptin:_TextureTiling,varname:node_9964,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_NormalVector,id:476,x:31494,y:32102,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:9786,x:31651,y:32102,varname:node_9786,prsc:2,dt:0|A-476-OUT,B-3492-OUT;n:type:ShaderForge.SFN_Vector3,id:3492,x:31494,y:32257,varname:node_3492,prsc:2,v1:0,v2:1,v3:0;n:type:ShaderForge.SFN_Slider,id:8255,x:32142,y:32276,ptovrint:False,ptlb:Transition Start,ptin:_TransitionStart,varname:node_8255,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:75,max:90;n:type:ShaderForge.SFN_Add,id:5337,x:32478,y:32319,varname:node_5337,prsc:2|A-8255-OUT,B-5014-OUT;n:type:ShaderForge.SFN_Slider,id:5014,x:32142,y:32385,ptovrint:False,ptlb:Transition Length,ptin:_TransitionLength,varname:node_5014,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:11,max:30;n:type:ShaderForge.SFN_Multiply,id:1318,x:31818,y:32102,varname:node_1318,prsc:2|A-9786-OUT,B-2921-OUT;n:type:ShaderForge.SFN_Vector1,id:2921,x:31651,y:32257,varname:node_2921,prsc:2,v1:90;n:type:ShaderForge.SFN_Set,id:2180,x:31994,y:32102,varname:NormalAngle,prsc:2|IN-1318-OUT;n:type:ShaderForge.SFN_Set,id:7603,x:31674,y:32704,varname:Grass,prsc:2|IN-9083-RGB;n:type:ShaderForge.SFN_Set,id:2092,x:31674,y:32766,varname:Rock,prsc:2|IN-3384-RGB;n:type:ShaderForge.SFN_Get,id:8490,x:32457,y:32768,varname:node_8490,prsc:2|IN-7603-OUT;n:type:ShaderForge.SFN_Get,id:5121,x:32457,y:32723,varname:node_5121,prsc:2|IN-2092-OUT;n:type:ShaderForge.SFN_Get,id:3389,x:32478,y:32276,varname:node_3389,prsc:2|IN-2180-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:5042,x:32687,y:32346,varname:node_5042,prsc:2|IN-3389-OUT,IMIN-8255-OUT,IMAX-5337-OUT,OMIN-283-OUT,OMAX-1003-OUT;n:type:ShaderForge.SFN_Vector1,id:283,x:32478,y:32457,varname:node_283,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:1003,x:32478,y:32507,varname:node_1003,prsc:2,v1:1;n:type:ShaderForge.SFN_Set,id:7155,x:32998,y:32346,varname:BaseAlpha,prsc:2|IN-4101-OUT;n:type:ShaderForge.SFN_Get,id:7342,x:32456,y:32814,varname:node_7342,prsc:2|IN-7155-OUT;n:type:ShaderForge.SFN_Lerp,id:7275,x:32681,y:32723,varname:node_7275,prsc:2|A-5121-OUT,B-8490-OUT,T-7342-OUT;n:type:ShaderForge.SFN_Clamp01,id:4101,x:32847,y:32346,varname:node_4101,prsc:2|IN-5042-OUT;proporder:9964-8255-5014-3384-9083;pass:END;sub:END;*/

Shader "Custom/Terrain Mesh" {
    Properties {
        _TextureTiling ("Texture Tiling", Float ) = 10
        _TransitionStart ("Transition Start", Range(0, 90)) = 75
        _TransitionLength ("Transition Length", Range(0, 30)) = 11
        _Rock ("Rock", 2D) = "white" {}
        _Grass ("Grass", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _Grass; uniform float4 _Grass_ST;
            uniform sampler2D _Rock; uniform float4 _Rock_ST;
            uniform float _TextureTiling;
            uniform float _TransitionStart;
            uniform float _TransitionLength;
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
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float2 node_8401 = (i.uv0*_TextureTiling);
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_8401, _Rock));
                float3 Rock = _Rock_var.rgb;
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_8401, _Grass));
                float3 Grass = _Grass_var.rgb;
                float NormalAngle = (dot(i.normalDir,float3(0,1,0))*90.0);
                float node_283 = 0.0;
                float BaseAlpha = saturate((node_283 + ( (NormalAngle - _TransitionStart) * (1.0 - node_283) ) / ((_TransitionStart+_TransitionLength) - _TransitionStart)));
                float3 diffuseColor = lerp(Rock,Grass,BaseAlpha);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _Grass; uniform float4 _Grass_ST;
            uniform sampler2D _Rock; uniform float4 _Rock_ST;
            uniform float _TextureTiling;
            uniform float _TransitionStart;
            uniform float _TransitionLength;
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
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float2 node_8401 = (i.uv0*_TextureTiling);
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_8401, _Rock));
                float3 Rock = _Rock_var.rgb;
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_8401, _Grass));
                float3 Grass = _Grass_var.rgb;
                float NormalAngle = (dot(i.normalDir,float3(0,1,0))*90.0);
                float node_283 = 0.0;
                float BaseAlpha = saturate((node_283 + ( (NormalAngle - _TransitionStart) * (1.0 - node_283) ) / ((_TransitionStart+_TransitionLength) - _TransitionStart)));
                float3 diffuseColor = lerp(Rock,Grass,BaseAlpha);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
