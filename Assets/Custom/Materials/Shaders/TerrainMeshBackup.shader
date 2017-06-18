// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4459,x:33847,y:32595,varname:node_4459,prsc:2|diff-8837-OUT,normal-4668-OUT;n:type:ShaderForge.SFN_Tex2d,id:9083,x:32072,y:32584,ptovrint:False,ptlb:Grass,ptin:_Grass,varname:node_9083,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5ce974d8eda42144dbb05d37b1c4fc47,ntxv:0,isnm:False|UVIN-8401-OUT;n:type:ShaderForge.SFN_Tex2d,id:3384,x:32072,y:32781,ptovrint:False,ptlb:Rock,ptin:_Rock,varname:node_3384,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:75b2c51118f22e04695021ae80b9bbe1,ntxv:0,isnm:False|UVIN-8401-OUT;n:type:ShaderForge.SFN_Multiply,id:8401,x:31809,y:32882,varname:node_8401,prsc:2|A-7132-UVOUT,B-9964-OUT;n:type:ShaderForge.SFN_TexCoord,id:7132,x:31613,y:32795,varname:node_7132,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:9964,x:31613,y:32972,ptovrint:False,ptlb:Texture Tiling,ptin:_TextureTiling,varname:node_9964,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_NormalVector,id:476,x:32305,y:32909,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:9786,x:32462,y:32909,varname:node_9786,prsc:2,dt:0|A-476-OUT,B-3492-OUT;n:type:ShaderForge.SFN_Vector3,id:3492,x:32305,y:33063,varname:node_3492,prsc:2,v1:0,v2:1,v3:0;n:type:ShaderForge.SFN_Slider,id:8255,x:32305,y:32826,ptovrint:False,ptlb:Grass Rolloff,ptin:_GrassRolloff,varname:node_8255,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.88,max:1;n:type:ShaderForge.SFN_If,id:8837,x:32737,y:32601,varname:node_8837,prsc:2|A-8255-OUT,B-9786-OUT,GT-3384-RGB,EQ-3384-RGB,LT-9083-RGB;n:type:ShaderForge.SFN_ComponentMask,id:5721,x:32958,y:32784,varname:node_5721,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-8837-OUT;n:type:ShaderForge.SFN_RemapRange,id:4668,x:33166,y:32784,varname:node_4668,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-5721-OUT;proporder:9083-3384-9964-8255;pass:END;sub:END;*/

Shader "Custom/Terrain Mesh" {
    Properties {
        _Grass ("Grass", 2D) = "white" {}
        _Rock ("Rock", 2D) = "white" {}
        _TextureTiling ("Texture Tiling", Float ) = 10
        _GrassRolloff ("Grass Rolloff", Range(0, 1)) = 0.88
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
            uniform float _GrassRolloff;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float node_8837_if_leA = step(_GrassRolloff,dot(i.normalDir,float3(0,1,0)));
                float node_8837_if_leB = step(dot(i.normalDir,float3(0,1,0)),_GrassRolloff);
                float2 node_8401 = (i.uv0*_TextureTiling);
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_8401, _Grass));
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_8401, _Rock));
                float3 normalLocal = (lerp((node_8837_if_leA*_Grass_var.rgb)+(node_8837_if_leB*_Rock_var.rgb),_Rock_var.rgb,node_8837_if_leA*node_8837_if_leB).rgb*0.5+0.5);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
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
                float3 diffuseColor = lerp((node_8837_if_leA*_Grass_var.rgb)+(node_8837_if_leB*_Rock_var.rgb),_Rock_var.rgb,node_8837_if_leA*node_8837_if_leB);
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
            uniform float _GrassRolloff;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float node_8837_if_leA = step(_GrassRolloff,dot(i.normalDir,float3(0,1,0)));
                float node_8837_if_leB = step(dot(i.normalDir,float3(0,1,0)),_GrassRolloff);
                float2 node_8401 = (i.uv0*_TextureTiling);
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_8401, _Grass));
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_8401, _Rock));
                float3 normalLocal = (lerp((node_8837_if_leA*_Grass_var.rgb)+(node_8837_if_leB*_Rock_var.rgb),_Rock_var.rgb,node_8837_if_leA*node_8837_if_leB).rgb*0.5+0.5);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 diffuseColor = lerp((node_8837_if_leA*_Grass_var.rgb)+(node_8837_if_leB*_Rock_var.rgb),_Rock_var.rgb,node_8837_if_leA*node_8837_if_leB);
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
