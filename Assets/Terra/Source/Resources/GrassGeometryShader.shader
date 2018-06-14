// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Low Poly Shader developed as part of World of Zero: http://youtube.com/worldofzerodevelopment
// Based upon the example at: http://www.battlemaze.com/?p=153

Shader "Terra/GrassGeometry" {
	Properties{
		[HDR]_BackgroundColor("Background Color", Color) = (1,0,0,1)
		[HDR]_ForegroundColor("Foreground Color", Color) = (0,0,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Cutoff("Cutoff", Range(0,1)) = 0.25
		_GrassHeight("Grass Height", Float) = 0.25
		_GrassWidth("Grass Width", Float) = 0.25
		_WindSpeed("Wind Speed", Float) = 100
		_WindStength("Wind Strength", Float) = 0.05
		_BillboardDistance("Billboard Distance", Float) = 5.0
		_Length("Length of Cloud", Float) = 200
	}
	SubShader{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {

			CULL OFF

			CGPROGRAM
			#include "UnityCG.cginc" 
			#define UNITY_SHADER_NO_UPGRADE 1
			#pragma enable_d3d11_debug_symbols
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			// Use shader model 4.0 target, we need geometry shader support
			#pragma target 4.0

			sampler2D _MainTex;

			struct v2g
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				float2 uv : TEXCOORD0;
				float3 color : TEXCOORD1;
				float distance : TEXCOORD2;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				float2 uv : TEXCOORD0;
				float3 diffuseColor : TEXCOORD1;
				float distance : TEXCOORD2;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _BackgroundColor;
			fixed4 _ForegroundColor;
			half _GrassHeight;
			half _GrassWidth;
			half _Cutoff;
			half _WindStength;
			half _WindSpeed;
			half _BillboardDistance;
			half _Length;

			v2g vert(appdata_full v)
			{
				float3 v0 = v.vertex.xyz;

				v2g OUT;
				OUT.pos = v.vertex;
				OUT.norm = v.normal;
				OUT.uv = v.texcoord;
				OUT.color = tex2Dlod(_MainTex, v.texcoord).rgb;
				
				//Calculate distance from camera
				OUT.distance = distance(mul(unity_ObjectToWorld, v.vertex), _WorldSpaceCameraPos.xyz);

				return OUT;
			}

			[maxvertexcount(24)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> triStream) {
				float3 lightPosition = _WorldSpaceLightPos0;

				float3 perpendicularAngle = float3(0, 0, 1);
				float3 faceNormal = cross(perpendicularAngle, IN[0].norm);

				float3 v0 = IN[0].pos.xyz;
				float3 v1 = IN[0].pos.xyz + IN[0].norm * _GrassHeight;

				float3 wind = float3(sin(_Time.x * _WindSpeed + v0.x) + sin(_Time.x * _WindSpeed + v0.z * 2) + sin(_Time.x * _WindSpeed * 0.1 + v0.x), 0,
					cos(_Time.x * _WindSpeed + v0.x * 2) + cos(_Time.x * _WindSpeed + v0.z));
				v1 += wind * _WindStength;

				float3 color = (IN[0].color);

				float sin30 = 0.5;
				float sin60 = 0.866f;
				float cos30 = sin60;
				float cos60 = sin30;

				g2f OUT;

				//Pass distance for fading in fragment
				OUT.distance = IN[0].distance;
				
				if (IN[0].distance < _BillboardDistance) {
					//Calculate individual quads from vertices
					
				// Quad 1
				
				OUT.pos = UnityObjectToClipPos(v0 + perpendicularAngle * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(1, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1 + perpendicularAngle * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(1, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1 - perpendicularAngle * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0 - perpendicularAngle * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 1);
				triStream.Append(OUT);

				// Quad 2

				OUT.pos = UnityObjectToClipPos(v0 + float3(sin60, 0, -cos60) * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(1, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1 + float3(sin60, 0, -cos60)* 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(1, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0 - float3(sin60, 0, -cos60) * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1 - float3(sin60, 0, -cos60) * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 1);
				triStream.Append(OUT);

				// Quad 3 - Positive

				OUT.pos = UnityObjectToClipPos(v0 + float3(sin60, 0, cos60) * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(1, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1 + float3(sin60, 0, cos60)* 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(1, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 1);
				triStream.Append(OUT);

				// Quad 3 - NEgative

				OUT.pos = UnityObjectToClipPos(v0 - float3(sin60, 0, cos60) * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1 - float3(sin60, 0, cos60) * 0.5 * _GrassHeight);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0, 1);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v0);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 0);
				triStream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(v1);
				OUT.norm = faceNormal;
				OUT.diffuseColor = color;
				OUT.uv = float2(0.5, 1);
				triStream.Append(OUT);
				} else {
					//Calculate billboards only
					// Quad 1

					//Position must be in world space
					float3 wPos = mul(unity_ObjectToWorld, IN[0].pos);

					float3 up = float3(0, 1, 0);
					float3 look = _WorldSpaceCameraPos - wPos;
					look.y = 0;
					look = normalize(look);
					float3 right = cross(up, look);
					float halfS = 2;

					float4 v[4];
					v[0] = float4(wPos + halfS * right - halfS * up, 1.0);
					v[1] = float4(wPos + halfS * right + halfS * up, 1.0);
					v[2] = float4(wPos - halfS * right - halfS * up, 1.0);
					v[3] = float4(wPos - halfS * right + halfS * up, 1.0);

					float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
					
					OUT.pos = mul(vp, v[0]);
					OUT.norm = faceNormal;
					OUT.diffuseColor = color;
					OUT.uv = float2(1.0, 0.0);
					triStream.Append(OUT);

					OUT.pos = mul(vp, v[1]);
					OUT.norm = faceNormal;
					OUT.diffuseColor = color;
					OUT.uv = float2(1.0, 1.0);
					triStream.Append(OUT);

					OUT.pos = mul(vp, v[2]);
					OUT.norm = faceNormal;
					OUT.diffuseColor = color;
					OUT.uv = float2(0.0, 0.0);
					triStream.Append(OUT);

					OUT.pos = mul(vp, v[3]);
					OUT.norm = faceNormal;
					OUT.diffuseColor = color;
					OUT.uv = float2(0.0, 1.0);
					triStream.Append(OUT);
				}
			}

			half4 frag(g2f IN) : COLOR
			{
				fixed4 c = tex2D(_MainTex, IN.uv);
				clip(c.a - _Cutoff);

				//Fade out based on distance
				float midPoint = 0.5 * (_Length + _BillboardDistance);
				float factor = (IN.distance - midPoint) / (_Length - midPoint);
				c.a = 1- saturate(factor);

				return c;
			}
		
			ENDCG

		}
	}
}