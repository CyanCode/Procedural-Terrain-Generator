// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/MegaSplatGaussianBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
      _Blur ("Blur", Float) = 0.03
      _Contrast("Contrast", Float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D_float _MainTex;
         float _Blur;
         float _Contrast;
         
         float Normpdf(float x, float sigma)
         {
            return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
         }
         //this is the blur function... pass in standard col derived from tex2d(_MainTex,i.uv)
         float4 Blur(sampler2D tex, float2 uv, float blurAmount) 
         {
           float4 col = tex2D(tex, uv);
           const int mSize = 11;
           const int iter = (mSize - 1) / 2;

           for (int i = -iter; i <= iter; ++i) 
           {
               for (int j = -iter; j <= iter; ++j) 
               {
                   col += tex2D(tex, float2(uv.x + i * blurAmount, uv.y + j * blurAmount)) * Normpdf(float(i), 7);
               }
           }

           return col/mSize;
         }


			float4 frag (v2f i) : SV_Target
			{
				return pow(Blur(_MainTex, i.uv, _Blur), _Contrast); 
			}
			ENDCG
		}
	}
}
