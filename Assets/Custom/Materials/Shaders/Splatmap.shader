// 
//   Author: Evan Pipta "747834"
//   License: GNU GPL: http://www.gnu.org/copyleft/gpl.html
//
Shader "747823/Splatmapped UV"
{
	Properties 
	{
		_Color ("Main Color", Color) 								= (0.8, 0.8, 0.8, 0)
		_Spec ("Specular Power", Float ) 							= 0.2
		_Shininess ("Glossiness", Float ) 							= 0.15
		_BlendSoft ("Texture Blend Softness", Range(0, 1))					= 0.1
		_Tex_Color ("Color map (RGB)", 2D) 							= "white" {}
		_Tex_Splat ("Splat map (RGBA)", 2D) 						= "white" {}
		_Tex_DiffuseR ("Splat Diffuse R (RGB), Height (A)", 2D) 				= "white" {}
		_Tex_NormalR ("Splat Normal R", 2D) 						= "bump" {}
		_Tex_DiffuseG ("Splat Diffuse G (RGB), Height (A)", 2D) 				= "white" {}
		_Tex_NormalG ("Splat Normal G", 2D) 						= "bump" {}
		_Tex_DiffuseB ("Splat Diffuse B (RGB), Height (A)", 2D) 				= "white" {}
		_Tex_NormalB ("Splat Normal B", 2D) 						= "bump" {}
		_Tex_DiffuseA ("Splat Diffuse A (RGB), Height (A)", 2D) 				= "white" {}
		_Tex_NormalA ("Splat Normal A", 2D) 						= "bump" {}
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 300
		
		CGPROGRAM

			#pragma surface surf BlinnPhongSpecMap addshadow
			#pragma target 3.0
			
			sampler2D _Tex_Splat;
			sampler2D _Tex_Color;
			sampler2D _Tex_DiffuseR;
			sampler2D _Tex_DiffuseG;
			sampler2D _Tex_DiffuseB;
			sampler2D _Tex_DiffuseA;
			sampler2D _Tex_NormalR;
			sampler2D _Tex_NormalG;
			sampler2D _Tex_NormalB;
			sampler2D _Tex_NormalA;

			fixed4 _Color;
			half _Spec;
			half _Shininess;
			half _DetailTileX;
			half _DetailTileY;
			half _BlendSoft;

			static const float _PI = 3.14159265359f;
			
			struct Input 
			{
				float2 uv_Tex_Splat;
				float2 uv_Tex_Color;
				float2 uv_Tex_DiffuseR;
				float2 uv_Tex_DiffuseG;
				float2 uv_Tex_DiffuseB;
				float2 uv_Tex_DiffuseA;
				// float2 uv_BumpMap;
			};
			
			struct SurfaceOut 
			{
				fixed3 Albedo;
				fixed3 Normal;
				fixed3 Emission;
				half Specular;
				fixed3 Gloss;
				fixed Alpha;
			};

			//compares input against compares. returns 1 if input is greater than ALL compares, else 0
			float cutoff( float input, float compare1, float compare2, float compare3 )
			{
				return ( input > compare1 && input > compare2 && input > compare3 ) ? 1 : 0;
			}

			float3 blend_overlay( float3 base, float3 blend )
			{
				return lerp((base*blend*2),(1.0-(2.0*(1.0-base)*(1.0-blend))),round(base));
			}

			void surf (Input IN, inout SurfaceOut o)
			{
				//o.Albedo = 0;
				float3 c = {0, 0, 0};

				//Diffuse
				float4 splat = tex2D( _Tex_Splat, IN.uv_Tex_Splat );
				float4 colormap = tex2D( _Tex_Color, IN.uv_Tex_Color );
				float4 dr = tex2D( _Tex_DiffuseR, IN.uv_Tex_DiffuseR );
				float4 dg = tex2D( _Tex_DiffuseG, IN.uv_Tex_DiffuseG );
				float4 db = tex2D( _Tex_DiffuseB, IN.uv_Tex_DiffuseB );
				float4 da = tex2D( _Tex_DiffuseA, IN.uv_Tex_DiffuseA );
				dr.a *= splat.r;
				dg.a *= splat.g;
				db.a *= splat.b;
				da.a *= splat.a;

				//Combine all alphas to equal 1
				float sum = dr.a + dg.a + db.a + da.a;
				if ( sum > 0 )
				{
					dr.a /= sum;
					dg.a /= sum;
					db.a /= sum;
					da.a /= sum;
				}
				else
				{
					dr.a = 1;
				}

				//Cutoff each alpha, comparing against each other
				float dr_cutoff = cutoff( dr.a, dg.a, db.a, da.a );
				float dg_cutoff = cutoff( dg.a, dr.a, db.a, da.a );
				float db_cutoff = cutoff( db.a, dr.a, dg.a, da.a );
				float da_cutoff = cutoff( da.a, dr.a, dg.a, db.a );

				//Lerp between the smooth alpha and cutoff alpha by the softness value
				//Amount to lerp should increase nonlinearly as the difference between the cutoff and smooth increases
				//That way we "round the corners" as we lerp.
				dr.a = lerp( dr_cutoff, dr.a, saturate( _BlendSoft ) );
				dg.a = lerp( dg_cutoff, dg.a, saturate( _BlendSoft ) );
				db.a = lerp( db_cutoff, db.a, saturate( _BlendSoft ) );
				da.a = lerp( da_cutoff, da.a, saturate( _BlendSoft ) );

				c = saturate( (dr.rgb*dr.a + dg.rgb*dg.a + db.rgb*db.a + da.rgb*da.a) * _Color );
				c = blend_overlay( c.rgb, colormap.rgb );

				o.Albedo = c;


				//--------------------------------------------
				//Specular
				o.Gloss = _Spec;
				o.Specular = _Shininess;


				//--------------------------------------------
				//Normal
				float3 nr = UnpackNormal( tex2D( _Tex_NormalR, IN.uv_Tex_DiffuseR ) );
				float3 ng = UnpackNormal( tex2D( _Tex_NormalG, IN.uv_Tex_DiffuseG ) );
				float3 nb = UnpackNormal( tex2D( _Tex_NormalB, IN.uv_Tex_DiffuseB ) );
				float3 na = UnpackNormal( tex2D( _Tex_NormalA, IN.uv_Tex_DiffuseA ) );
				o.Normal = clamp( (nr.rgb*dr.a + ng.rgb*dg.a + nb.rgb*db.a + na.rgb*da.a), -1, 1 );
			}
			
			inline fixed4 LightingBlinnPhongSpecMap (SurfaceOut s, fixed3 lightDir, half3 viewDir, fixed atten)
			{
				half3 h = normalize (lightDir + viewDir);
				fixed diff = max (0, dot (s.Normal, lightDir));
				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, s.Specular*128.0);
				fixed4 c;
				c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec * s.Gloss) * (atten * 2);
				c.a = s.Alpha + _LightColor0.a * (0.2989f * s.Gloss.r + 0.5870f * s.Gloss.g + 0.1140f * s.Gloss.b) * spec * atten;
				return c;
			}

		ENDCG
	}
	
}

