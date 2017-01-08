Shader "Majorspot/Terrain Shader" {
	Properties {
		_UnderWater ("Under Water", 2D) = "white" {}
		_UnderWaterNormal ("Under Water Normal", 2D) = "bump" {}

		_Grass ("Grass", 2D) = "white" {}
		_GrassNormal ("Grass Normal", 2D) = "bump" {}

		_Rocks ("Rocks", 2D) = "white" {}
		_RocksNormal ("Rocks Normal", 2D) = "bump" {}

		_Dirt ("Dirt", 2D) = "white" {}
		_DirtNormal ("Dirt Normal", 2D) = "bump" {}

		_Snow ("Snow", 2D) = "white" {}
		_SnowNormal ("Snow Normal", 2D) = "bump" {}

		_Tone ("Tone", Color) = (1,1,1,1)

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Brightness ("Brightness", Range(0,10)) = 1.0

		_Control1 ("Control 1", 2D) = "white" {}
		_Control2 ("Control 2", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		//Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
		LOD 200
		
		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows exclude_path:prepass

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _Control1;
			sampler2D _Grass;
			sampler2D _Dirt;
			sampler2D _Rocks;

			sampler2D _GrassNormal;
			sampler2D _DirtNormal;
			sampler2D _RocksNormal;

			struct Input {
				float2 uv_Control1;
				float2 uv_Grass;
				float2 uv_Dirt;
				float2 uv_Rocks;
			};

			half _Glossiness;
			half _Metallic;
			half _Brightness;
			fixed4 _Tone;

			void surf (Input IN, inout SurfaceOutputStandard o) {
				fixed4 splat_control = tex2D (_Control1, IN.uv_Control1);
				fixed3 col  = fixed3(0, 0, 0);

				float4 s0 = tex2D (_Grass, IN.uv_Grass);
				float4 s1 = tex2D (_Rocks, IN.uv_Rocks);
				float4 s2 = tex2D (_Dirt, IN.uv_Dirt);

				col = lerp(col, s0.rgb, splat_control.r * s0.a);
				col = lerp(col, s1.rgb, splat_control.g * s1.a);
				col = lerp(col, s2.rgb, splat_control.b * s2.a);

				col = saturate( col * _Tone );			
				o.Albedo = col * _Brightness;

				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;

				//o.Alpha = splat_control.r;

				float3 nGrass = UnpackNormal( tex2D( _GrassNormal, IN.uv_Grass ) );
				float3 nDirt = UnpackNormal( tex2D( _DirtNormal, IN.uv_Dirt ) );
				float3 nRocks = UnpackNormal( tex2D( _RocksNormal, IN.uv_Rocks ) );
				o.Normal = clamp( (nGrass.rgb*s0.a + nRocks.rgb*s1.a + nDirt.rgb*s2.a), -1, 1 );
			}
		ENDCG

		Blend SrcAlpha DstAlpha
        ZWrite Off

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows exclude_path:prepass noforwardadd

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _Control2;
			sampler2D _UnderWater;
			sampler2D _Snow;

			struct Input {
				float2 uv_Control2;
				float2 uv_UnderWater;
				float2 uv_Snow;
			};

			half _Glossiness;
			half _Metallic;
			half _Brightness;
			fixed4 _Tone;

			void surf (Input IN, inout SurfaceOutputStandard o) {
				fixed4 splat_control = tex2D (_Control2, IN.uv_Control2);
				fixed3 col  = fixed3(0, 0, 0);

				float4 s0 = tex2D (_UnderWater, IN.uv_UnderWater);
				float4 s1 = tex2D (_Snow, IN.uv_Snow);

				col = lerp(col, s0.rgb, splat_control.r * s0.a);
				col = lerp(col, s1.rgb, splat_control.g * s1.a);

				col = saturate( col * _Tone );			
				o.Albedo = col * _Brightness;

				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				//o.Alpha = splat_control.r;
			}
		ENDCG
	} 
	FallBack "Diffuse"
}
