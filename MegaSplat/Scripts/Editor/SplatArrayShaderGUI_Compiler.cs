//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using JBooth.MegaSplat;
using System.Collections.Generic;

public partial class SplatArrayShaderGUI : ShaderGUI 
{
   static TextAsset meshBody;
   static TextAsset meshFooter;
   static TextAsset terrainBody;

   static TextAsset sharedInc;
   static TextAsset structInc;
   static TextAsset debugOutput;

   static TextAsset tessMeshBody;
   static TextAsset tessMeshPass1;
   static TextAsset tessMeshPass2;
   static TextAsset tessMeshPass3;
   static TextAsset tessMeshPass3notess;
   static TextAsset tessMeshPass4;
   static TextAsset tessMeshPass5;
   static TextAsset tessTerrainBody;
   static TextAsset tessTerrainPass1;
   static TextAsset tessTerrainPass2;
   static TextAsset tessTerrainPass3;
   static TextAsset tessTerrainPass3notess;
   static TextAsset tessTerrainPass4;
   static TextAsset tessTerrainPass5;

   static TextAsset properties_detail;
   static TextAsset properties_distanceNoise;
   static TextAsset properties_flow;
   static TextAsset properties_macro;
   static TextAsset properties_geomap;
   static TextAsset properties_pertex;
   static TextAsset properties_glitter;
   static TextAsset properties_splat;
   static TextAsset properties_terrain;
   static TextAsset properties_tess;
   static TextAsset properties_triplanar;
   static TextAsset properties_emission;
   static TextAsset properties_detailNoise;
   static TextAsset properties_project3way;
   static TextAsset properties_project3way2;
   static TextAsset properties_ramp;
   static TextAsset properties_puddle;
   static TextAsset properties_puddleFlow;
   static TextAsset properties_lava;
   static TextAsset properties_raindrops;
   static TextAsset properties_uvproject;
   static TextAsset properties_uvproject2;
   static TextAsset properties_snow;
   static TextAsset properties_snow_distanceNoise;
   static TextAsset properties_snow_distanceResample;
   static TextAsset properties_lowpoly;
   static TextAsset properties_distanceresample;
   static TextAsset properties_wetness;

   static TextAsset customFuncTemplate;
   static TextAsset customPropTemplate;

   static TextAsset lighting;
      
   const string declareMesh            = "      #pragma surface surf Standard vertex:vert fullforwardshadows nofog finalcolor:fogcolor";
   const string declareTerrain        = "      #pragma surface surf Standard vertex:vert fullforwardshadows";
   const string declareMeshNoLight     = "      #pragma surface surf Unlit vertex:vert nofog";
   const string declareTerrainNoLight = "      #pragma surface surf Unlit vertex:vert nofog";
   const string declareMeshRamp       = "      #pragma surface surf Ramp vertex:vert fullforwardshadows nofog finalcolor:fogcolor";
   const string declareTerrainRamp    = "      #pragma surface surf Ramp vertex:vert fullforwardshadows";

   static string GetFeatureName(DefineFeature feature)
   {
      return System.Enum.GetName(typeof(DefineFeature), feature);
   }

   static bool HasFeature(string[] keywords, DefineFeature feature)
   {
      string f = GetFeatureName(feature);
      for (int i = 0; i < keywords.Length; ++i)
      {
         if (keywords[i] == f)
            return true;
      }
      return false;
   }
      

   [MenuItem ("Assets/Create/Shader/MegaSplat Shader")]
   static void NewShader2()
   {
      NewShader();
   }

   [MenuItem ("Assets/Create/MegaSplat/MegaSplat Shader")]
   public static Shader NewShader()
   {
      string path = "Assets";
      foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
      {
         path = AssetDatabase.GetAssetPath(obj);
         if (System.IO.File.Exists(path))
         {
            path = System.IO.Path.GetDirectoryName(path);
         }
         break;
      }
      path = path.Replace("\\", "/");
      path = AssetDatabase.GenerateUniqueAssetPath(path + "/MegaSplat.shader");
      string name = path.Substring(path.LastIndexOf("/"));
      name = name.Substring(0, name.IndexOf("."));
      InitCompiler();
      string ret = Compile(new string[0], name);
      System.IO.File.WriteAllText(path, ret);
      AssetDatabase.Refresh();
      return AssetDatabase.LoadAssetAtPath<Shader>(path);
   }

   static void InitCompiler()
   {
      if (meshBody == null)
      {
         string[] paths = AssetDatabase.FindAssets("megasplat_ t:TextAsset");
         for (int i = 0; i < paths.Length; ++i)
         {
            paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
         }
         for (int i = 0; i < paths.Length; ++i)
         {
            var p = paths[i];
            if (p.EndsWith("megasplat_mesh_body.txt"))
            {
               meshBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_customfunc_template.txt"))
            {
               customFuncTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_customprops_template.txt"))
            {
               customPropTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_glitter.txt"))
            {
               properties_glitter = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_shared.txt"))
            {
               sharedInc = AssetDatabase.LoadAssetAtPath<TextAsset>(p); 
            }
            if (p.EndsWith("megasplat_structs.txt"))
            {
               structInc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_geotex.txt"))
            {
               properties_geomap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_terrain_body.txt"))
            {
               terrainBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_mesh_tess_body.txt"))
            {
               tessMeshBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_mesh_tess_pass1.txt"))
            {
               tessMeshPass1 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_mesh_tess_pass2.txt"))
            {
               tessMeshPass2 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_mesh_tess_pass3.txt"))
            {
               tessMeshPass3 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_mesh_tess_pass3_notess.txt"))
            {
               tessMeshPass3notess = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_mesh_tess_pass4.txt"))
            {
               tessMeshPass4 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_mesh_tess_pass5.txt"))
            {
               tessMeshPass5 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
               
            if (p.EndsWith("megasplat_terrain_tess_body.txt"))
            {
               tessTerrainBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_terrain_tess_pass1.txt"))
            {
               tessTerrainPass1 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_terrain_tess_pass2.txt"))
            {
               tessTerrainPass2 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_terrain_tess_pass3.txt"))
            {
               tessTerrainPass3 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_terrain_tess_pass3_notess.txt"))
            {
               tessTerrainPass3notess = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_terrain_tess_pass4.txt"))
            {
               tessTerrainPass4 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_terrain_tess_pass5.txt"))
            {
               tessTerrainPass5 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_properties_project3way_bottom.txt"))
            {
               properties_project3way = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_properties_project3way_top.txt"))
            {
               properties_project3way2 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }

            if (p.EndsWith("megasplat_properties_lowpoly.txt"))
            {
               properties_lowpoly = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_detail.txt"))
            {
               properties_detail = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_flow.txt"))
            {
               properties_flow = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_macro.txt"))
            {
               properties_macro = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_pertex.txt"))
            {
               properties_pertex = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_splat.txt"))
            {
               properties_splat = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_terrain.txt"))
            {
               properties_terrain = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_tess.txt"))
            {
               properties_tess = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_triplanar.txt"))
            {
               properties_triplanar = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_emission.txt"))
            {
               properties_emission = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_detail_noise.txt"))
            {
               properties_detailNoise = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_distance_noise.txt"))
            {
               properties_distanceNoise = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_snow_distance_noise.txt"))
            {
               properties_snow_distanceNoise = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_snow_distance_resample.txt"))
            {
               properties_snow_distanceResample = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_ramp.txt"))
            {
               properties_ramp = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_distanceresample.txt"))
            {
               properties_distanceresample = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_lighting.txt"))
            {
               lighting = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_puddles.txt"))
            {
               properties_puddle = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_puddles_flow.txt"))
            {
               properties_puddleFlow = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_lava.txt"))
            {
               properties_lava = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_raindrops.txt"))
            {
               properties_raindrops = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_uvproject.txt"))
            {
               properties_uvproject = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_uvproject2.txt"))
            {
               properties_uvproject2 = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_snow.txt"))
            {
               properties_snow = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("megasplat_properties_wetness.txt"))
            {
               properties_wetness = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }
   }

   static void WriteHeader(string[] features, StringBuilder sb)
   {
      sb.AppendLine("   SubShader {");
      if (HasFeature(features, DefineFeature._ALPHATEST))
      {
         sb.AppendLine("      Tags {\"Queue\"=\"AlphaTest\" \"IgnoreProjector\"=\"True\" \"RenderType\"=\"TransparentCutout\"}");
      }
      else if (HasFeature(features, DefineFeature._ALPHA))
      {
         sb.AppendLine("      Tags {\"Queue\"=\"Transparent\" \"IgnoreProjector\"=\"True\" \"RenderType\"=\"Transparent\"}");
         sb.AppendLine("      ZWrite Off");
         sb.AppendLine("      Blend SrcAlpha OneMinusSrcAlpha");
      }
      else
      {
         sb.AppendLine("      Tags {\"RenderType\"=\"Opaque\"}");
      }

      if (UseSurfaceShader(features))
      {
         if (HasFeature(features, DefineFeature._TERRAIN))
         {
            sb.AppendLine("      CGPROGRAM");
            sb.AppendLine("      #pragma exclude_renderers d3d9");
            //sb.AppendLine("      #pragma multi_compile_fog");
            sb.AppendLine("      #define TERRAIN_STANDARD_SHADER");
            sb.AppendLine("      #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard");
         }
         else
         {
            sb.AppendLine("      CGPROGRAM");
            sb.AppendLine("      #pragma exclude_renderers d3d9");
            sb.AppendLine("      #pragma multi_compile_fog");
         }
      }


   }

   static void WriteFeatures(string[] features, StringBuilder sb)
   {
      sb.AppendLine();
      for (int i = 0; i < features.Length; ++i)
      {
         sb.AppendLine("      #define " + features[i] + " 1");
      }
      sb.AppendLine();

      if (HasFeature(features, DefineFeature._USECURVEDWORLD))
      {
         sb.AppendLine("#include \"Assets/VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc\"");
      }

      sb.AppendLine(structInc.text);

      if (HasFeature(features, DefineFeature._CUSTOMUSERFUNCTION))
      {
         sb.AppendLine("#include \"megasplat_custom.cginc\"");
      }

      sb.AppendLine(sharedInc.text);

   }

   static bool HasDebugFeature(string[] features)
   {
      return HasFeature(features, DefineFeature._DEBUG_OUTPUT_ALBEDO) ||
         HasFeature(features, DefineFeature._DEBUG_OUTPUT_NORMAL) ||
         HasFeature(features, DefineFeature._DEBUG_OUTPUT_HEIGHT) ||
         HasFeature(features, DefineFeature._DEBUG_OUTPUT_METAL) ||
         HasFeature(features, DefineFeature._DEBUG_OUTPUT_SMOOTHNESS) ||
         HasFeature(features, DefineFeature._DEBUG_OUTPUT_AO) ||
         HasFeature(features, DefineFeature._DEBUG_OUTPUT_EMISSION) ||
         HasFeature(features, DefineFeature._DEBUG_OUTPUT_SPLATDATA);
   }

   public static void WriteFooter(string[] features, StringBuilder b, string fallbackName, bool needsEnd)
   {
      if (string.IsNullOrEmpty(fallbackName))
      {
         fallbackName = "Diffuse";

         if (HasFeature(features, DefineFeature._USECURVEDWORLD))
         {
            fallbackName = "Hidden/VacuumShaders/Curved World/VertexLit/Diffuse";
         }

         if (HasFeature(features, DefineFeature._ALPHA))
         {
            fallbackName = "Transparent/Diffuse";
            if (HasFeature(features, DefineFeature._USECURVEDWORLD))
            {
               fallbackName = "Hidden/VacuumShaders/Curved World/VertexLit/Transparent";
            }
            
         }
         else if (HasFeature(features, DefineFeature._ALPHATEST))
         {
            fallbackName = "Transparent/Cutout/Diffuse";
            if (HasFeature(features, DefineFeature._USECURVEDWORLD))
            {
               fallbackName = "Hidden/VacuumShaders/Curved World/VertexLit/Cutout";
            }
         }
      }
      if (needsEnd)
      {
         b.AppendLine("      ENDCG");
      }
      b.AppendLine("   }");
      //Dependency "BaseMapShader" = "MegaSplat_Terrain_Base"
      b.AppendLine("   CustomEditor \"SplatArrayShaderGUI\"");
      b.AppendLine("   FallBack \"" + fallbackName + "\"");
      b.AppendLine("}");
   }

   static bool HasTessellation(string[] features)
   {
      return HasFeature(features, DefineFeature._TESSDISTANCE) || HasFeature(features, DefineFeature._TESSEDGE);
   }

   static bool HasFlow(string[] features)
   {
      return HasFeature(features, DefineFeature._FLOW) || HasFeature(features, DefineFeature._FLOWREFRACTION);
   }

   static bool HasPerTex(string[] features)
   {
      return HasFeature(features, DefineFeature._PERTEXCONTRAST) ||
         HasFlow(features) ||
         HasFeature(features, DefineFeature._PERTEXDISPLACEPARAMS) ||
         HasFeature(features, DefineFeature._PERTEXMATPARAMS) || 
         HasFeature(features, DefineFeature._PERTEXNOISESTRENGTH) ||
         HasFeature(features, DefineFeature._PERTEXUV) ||
         HasFeature(features, DefineFeature._PERTEXPARALLAXSTRENGTH) ||
         HasFeature(features, DefineFeature._PERTEXNORMALSTRENGTH) ||
         HasFeature(features, DefineFeature._PERTEXGLITTER);
   }

   static bool HasMacro(string[] features)
   {
      return HasFeature(features, DefineFeature._MACROMULT) || HasFeature(features, DefineFeature._MACROMULT2X) ||
         HasFeature(features, DefineFeature._MACRONORMAL) || HasFeature(features, DefineFeature._MACROOVERLAY);
   }

   static bool HasProjectedUV(string[] features)
   {
      return HasFeature(features, DefineFeature._UVLOCALFRONT) || HasFeature(features, DefineFeature._UVLOCALSIDE) ||
             HasFeature(features, DefineFeature._UVLOCALTOP) || HasFeature(features, DefineFeature._UVWORLDFRONT) ||
             HasFeature(features, DefineFeature._UVWORLDSIDE) || HasFeature(features, DefineFeature._UVWORLDTOP);
   }

   static bool HasProjectedUV2(string[] features)
   {
      return HasFeature(features, DefineFeature._UVLOCALFRONT2) || HasFeature(features, DefineFeature._UVLOCALSIDE2) ||
         HasFeature(features, DefineFeature._UVLOCALTOP2) || HasFeature(features, DefineFeature._UVWORLDFRONT2) ||
         HasFeature(features, DefineFeature._UVWORLDSIDE2) || HasFeature(features, DefineFeature._UVWORLDTOP2);
   }

   /*
   // modify properties to comment out ones that are set to global
   static StringBuilder sStripBuilder = new StringBuilder(1024);
   static string StripProperties(string text, string[] globals)
   {
      if (globals != null && globals.Length > 0)
      {
         sStripBuilder.Length = 0;
         var lines = text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);
         for (int i = 0; i < lines.Length; ++i)
         {
            string line = lines[i];
            for (int x = 0; x < globals.Length; ++x)
            {
               if (lines[i].Contains(globals[x]))
               {
                  lines[i] = "// " + lines[i];
               }
            }
            sStripBuilder.AppendLine(lines[i]);
         }
         return sStripBuilder.ToString();
      }
      return text;
   }
   */

   static string[] GetCustomPropFile(Material mat)
   {
      if (mat == null || mat.shader == null)
         return null;
      
      string path = AssetDatabase.GetAssetPath(mat.shader);
      if (string.IsNullOrEmpty(path))
         return null;
      
      path = path.Substring(6);
      path = Application.dataPath + path;
      path = path.Replace("\\", "/");
      path = path.Substring(0, path.LastIndexOf("/"));
      path += "/megasplat_custom.props";
      if (System.IO.File.Exists(path))
      {
         return System.IO.File.ReadAllLines(path);
      }
      return null;
   }

   // existing mat is optional, and only used for the custom shader extensions
   static void WriteProperties(string[] features, StringBuilder sb, Material existingMat = null)
   {
      sb.AppendLine("   Properties {");

      if (HasFeature(features, DefineFeature._TERRAIN))
      {
         sb.Append(properties_terrain.text);
      }
      if (HasFeature(features, DefineFeature._RAMPLIGHTING))
      {
         sb.Append(properties_ramp.text);
      }
      if (HasMacro(features))
      {
         sb.Append(properties_macro.text);
      }
      if (HasFeature(features, DefineFeature._TRIPLANAR))
      {
         sb.Append(properties_triplanar.text);
      }
      if (HasProjectedUV(features))
      {
         sb.Append(properties_uvproject.text);
      }
      if (HasProjectedUV2(features))
      {
         sb.Append(properties_uvproject2.text);
      }

      if (HasFeature(features, DefineFeature._DISTANCERESAMPLE))
      {
         sb.Append(properties_distanceresample.text);
      }

      if (HasFeature(features, DefineFeature._LOWPOLY))
      {
         sb.Append(properties_lowpoly.text);
      }

      if (HasFeature(features, DefineFeature._PERTEXGLITTER) || HasFeature(features, DefineFeature._SNOWGLITTER) || HasFeature(features, DefineFeature._PUDDLEGLITTER))
      {
         sb.Append(properties_glitter.text);
      }

      sb.Append(properties_splat.text);

      if (HasFeature(features, DefineFeature._EMISMAP))
      {
         sb.Append(properties_emission.text);
      }

      if (HasTessellation(features))
      {
         sb.Append(properties_tess.text);
      }
      if (HasFlow(features))
      {
         sb.Append(properties_flow.text);
      }
      if (HasPerTex(features))
      {
         sb.Append(properties_pertex.text);
      }
      if (HasFeature(features, DefineFeature._GEOMAP))
      {
         sb.Append(properties_geomap.text);
      }
      if (HasFeature(features, DefineFeature._DETAILMAP))
      {
         sb.Append(properties_detail.text);
      }
      if (HasFeature(features, DefineFeature._DETAILNOISE))
      {
         sb.Append(properties_detailNoise.text);
      }
      if (HasFeature(features, DefineFeature._DISTANCENOISE))
      {
         sb.Append(properties_distanceNoise.text);
      }
      if (HasFeature(features, DefineFeature._PUDDLES))
      {
         sb.Append(properties_puddle.text);
      }
      else if (HasFeature(features, DefineFeature._PUDDLEFLOW) || HasFeature(features, DefineFeature._PUDDLEREFRACT))
      {
         sb.Append(properties_puddle.text);
         sb.Append(properties_puddleFlow.text);
      }
      else if (HasFeature(features, DefineFeature._LAVA))
      {
         sb.Append(properties_lava.text);
      }
      if (HasFeature(features, DefineFeature._RAINDROPS))
      {
         sb.Append(properties_raindrops.text);
      }

      if (HasFeature(features, DefineFeature._PROJECTTEXTURE_LOCAL) || HasFeature(features, DefineFeature._PROJECTTEXTURE_WORLD))
      {
         sb.Append(properties_project3way.text);
      }

      if (HasFeature(features, DefineFeature._PROJECTTEXTURE2_LOCAL) || HasFeature(features, DefineFeature._PROJECTTEXTURE2_WORLD))
      {
         sb.Append(properties_project3way2.text);
      }

      if (HasFeature(features, DefineFeature._SNOW))
      {
         sb.Append(properties_snow.text);
         if (HasFeature(features, DefineFeature._SNOWDISTANCENOISE))
         {
            sb.Append(properties_snow_distanceNoise.text);
         }
         if (HasFeature(features, DefineFeature._SNOWDISTANCERESAMPLE))
         {
            sb.Append(properties_snow_distanceResample.text);
         }
      }
      if (HasFeature(features, DefineFeature._WETNESS))
      {
         sb.Append(properties_wetness.text);
      }
      if (HasFeature(features, DefineFeature._CUSTOMUSERFUNCTION))
      {
         var props = GetCustomPropFile(existingMat);
         if (props == null)
         {
            Debug.LogError("Custom User Function could not be compiled because no material is supplied");
         }
         else
         {
            for (int i = 0; i < props.Length; ++i)
            {
               sb.AppendLine(props[i]);
            }

         }
      }
      if (HasFeature(features, DefineFeature._ALPHAHOLE))
      {
         sb.AppendLine("      _AlphaHoleIdx(\"Alpha Hole Index\", int) = 0");
      }
         

      sb.AppendLine("   }");
   }

   static string GetShaderModel(FeatureData fData)
   {
      string[] features = fData.Pack();
      return GetShaderModel(features);
   }

   static string GetShaderModel(string[] features)
   {
      if (HasTessellation(features))
      {
         return "4.6";
      }

      if (HasFeature(features, DefineFeature._LOWPOLY) || HasFeature(features, DefineFeature._PREPROCESSMESH))
      {
         // 45 doesn't support geom shaders, so ignore..
         if (HasFeature(features, DefineFeature._MANUALSHADERLEVEL46))
         {
            return "4.6";
         }
         return "4.0";
      }

      if (HasFeature(features, DefineFeature._SECONDUV) || 
         HasFeature(features, DefineFeature._SNOW) || 
         HasFeature(features, DefineFeature._WETNESS) ||
         HasFeature(features, DefineFeature._PUDDLEGLITTER) ||
         HasFeature(features, DefineFeature._PERTEXGLITTER) ||
         HasDebugFeature(features))

      {
         if (HasFeature(features, DefineFeature._MANUALSHADERLEVEL45))
         {
            return "4.5";
         }
         else if (HasFeature(features, DefineFeature._MANUALSHADERLEVEL46))
         {
            return "4.6";
         }
         return "4.0";
      }
      else
      {
         if (HasFeature(features, DefineFeature._MANUALSHADERLEVEL40))
         {
            return "4.0";
         }
         else if (HasFeature(features, DefineFeature._MANUALSHADERLEVEL45))
         {
            return "4.5";
         }
         else if (HasFeature(features, DefineFeature._MANUALSHADERLEVEL46))
         {
            return "4.6";
         }
         return "3.5";
      }
   }

   public static readonly string MegaSplatVersion = "1.6";

   static void WriteGeomPragma(string[] features, StringBuilder sb)
   {
      if (HasFeature(features, DefineFeature._LOWPOLY) || HasFeature(features, DefineFeature._PREPROCESSMESH))
      {
         sb.AppendLine("         #pragma geometry geom");
      }
   }

   static void WriteTessPragma(string[] features, StringBuilder sb, bool isShadow)
   {
      if (HasTessellation(features))
      {
         if (!isShadow || HasFeature(features, DefineFeature._TESSSHADOWS))
         {
            sb.AppendLine("         #pragma hull hull");
            sb.AppendLine("         #pragma domain domain");
            sb.AppendLine("         #pragma vertex tessvert");
         }
         else
         {
            sb.AppendLine("         #pragma vertex NoTessShadowVertBiased");
         }
         sb.AppendLine("         #pragma fragment frag");
      }
      else
      {
         sb.AppendLine("         #pragma vertex NoTessVert");
         sb.AppendLine("         #pragma fragment frag");
      }
   }

   static bool UseSurfaceShader(string[] features)
   {
      return !(HasFeature(features, DefineFeature._TESSEDGE) ||
      HasFeature(features, DefineFeature._TESSDISTANCE) ||
      HasFeature(features, DefineFeature._LOWPOLY) ||
      HasFeature(features, DefineFeature._PREPROCESSMESH) ||  
      HasFeature(features, DefineFeature._ALPHATEST));
   }
   static StringBuilder sBuilder = new StringBuilder(256000);
   public static string Compile(string[] features, string name, string fallbackName = null, Material existingMat = null)
   {
      InitCompiler();
      sBuilder.Length = 0;
      var sb = sBuilder;
      sb.AppendLine("//////////////////////////////////////////////////////");
      sb.AppendLine("// MegaSplat - 256 texture splat mapping");
      sb.AppendLine("// Copyright (c) Jason Booth, slipster216@gmail.com");
      sb.AppendLine("//");
      sb.AppendLine("// Auto-generated shader code, don't hand edit!");
      sb.AppendLine("//   Compiled with MegaSplat " + MegaSplatVersion);
      sb.AppendLine("//   Unity : " + Application.unityVersion);
      sb.AppendLine("//   Platform : " + Application.platform);
      sb.AppendLine("//////////////////////////////////////////////////////");
      sb.AppendLine();
      sb.Append("Shader \"MegaSplat/");
      while (name.Contains("/"))
      {
         name = name.Substring(name.IndexOf("/") + 1);
      }
      sb.Append(name);
      sb.AppendLine("\" {");


      // props
      WriteProperties(features, sb, existingMat);



      string alphaStr = "";
      if (HasFeature(features, DefineFeature._ALPHA))
      {
         alphaStr = " alpha";
      }
      else if (HasFeature(features, DefineFeature._ALPHATEST))
      {
         alphaStr = " alphatest:_Cutoff";
      }

      WriteHeader(features, sb);

      if (!UseSurfaceShader(features))
      {
         if (HasFeature(features, DefineFeature._TERRAIN))
         {
            // no deferred if alpha
            if (!HasFeature(features, DefineFeature._ALPHA))
            {
               sb.AppendLine(tessTerrainPass5.text);
               WriteGeomPragma(features, sb);
               WriteTessPragma(features, sb, false);
               WriteFeatures(features, sb);
               sb.AppendLine(lighting.text);
               sb.AppendLine(tessTerrainBody.text);
            }


            sb.AppendLine(tessTerrainPass1.text);
            WriteGeomPragma(features, sb);
            WriteTessPragma(features, sb, false);
            WriteFeatures(features, sb);
            sb.AppendLine(lighting.text);
            sb.AppendLine(tessTerrainBody.text);

            sb.AppendLine(tessTerrainPass2.text);
            WriteGeomPragma(features, sb);
            WriteTessPragma(features, sb, false);
            WriteFeatures(features, sb);
            sb.AppendLine(lighting.text);
            sb.AppendLine(tessTerrainBody.text);

            if (HasFeature(features, DefineFeature._TESSSHADOWS))
            {
               sb.AppendLine(tessTerrainPass3.text);
               WriteTessPragma(features, sb, false);
               WriteFeatures(features, sb);

               sb.AppendLine(lighting.text);
               sb.AppendLine(tessTerrainBody.text);
            }
            else if (HasFeature(features, DefineFeature._TESSCENTERBIAS) || HasFeature(features, DefineFeature._ALPHATEST))
            {
               sb.AppendLine(tessTerrainPass3notess.text);
               WriteTessPragma(features, sb, true);
               WriteFeatures(features, sb);

               sb.AppendLine(lighting.text);
               sb.AppendLine(tessTerrainBody.text);
            }

            sb.AppendLine(tessTerrainPass4.text);
            WriteTessPragma(features, sb, false);
            WriteFeatures(features, sb);


            sb.AppendLine(lighting.text);
            sb.AppendLine(tessTerrainBody.text);

            WriteFooter(features, sb, fallbackName, false);
            return sb.ToString();
         }
         else
         {
            // no deferred if alpha
            if (!HasFeature(features, DefineFeature._ALPHA))
            {
               sb.AppendLine(tessMeshPass5.text);
               WriteGeomPragma(features, sb);
               WriteTessPragma(features, sb, false);
               WriteFeatures(features, sb);
               sb.AppendLine(lighting.text);
               sb.AppendLine(tessMeshBody.text);
            }


            sb.AppendLine(tessMeshPass1.text);
            WriteGeomPragma(features, sb);
            WriteTessPragma(features, sb, false);
            WriteFeatures(features, sb);
            sb.AppendLine(lighting.text);
            sb.AppendLine(tessMeshBody.text);

            sb.AppendLine(tessMeshPass2.text);
            WriteGeomPragma(features, sb);
            WriteTessPragma(features, sb, false);
            WriteFeatures(features, sb);
            sb.AppendLine(lighting.text);
            sb.AppendLine(tessMeshBody.text);

            if (HasFeature(features, DefineFeature._TESSSHADOWS))
            {
               sb.AppendLine(tessMeshPass3.text);
               WriteTessPragma(features, sb, true);

               WriteFeatures(features, sb);
               sb.AppendLine(tessMeshBody.text);
            }
            else if (HasFeature(features, DefineFeature._TESSCENTERBIAS) || HasFeature(features, DefineFeature._ALPHATEST))
            {
               sb.AppendLine(tessMeshPass3notess.text);
               WriteTessPragma(features, sb, false);

               WriteFeatures(features, sb);
               sb.AppendLine(tessMeshBody.text);
            }

            sb.AppendLine(tessMeshPass4.text);
            WriteTessPragma(features, sb, false);

            WriteFeatures(features, sb);
            sb.AppendLine(tessMeshBody.text);

            WriteFooter(features, sb, fallbackName, false);
            return sb.ToString();
         }
      }
      else  // Surface shader
      {
         if (HasFeature(features, DefineFeature._TERRAIN))
         {
            if (HasDebugFeature(features))
            {
               sb.AppendLine(declareTerrainNoLight + alphaStr);
            }
            else if (HasFeature(features, DefineFeature._RAMPLIGHTING))
            {
               sb.AppendLine(declareTerrainRamp + alphaStr);
            }
            else
            {
               sb.AppendLine(declareTerrain + alphaStr);
            }
            sb.AppendLine("      #pragma target " + GetShaderModel(features));

            WriteFeatures(features, sb);
            sb.AppendLine(terrainBody.text);
            WriteFooter(features, sb, null, true);
         }
         else
         {
            if (HasDebugFeature(features))
            {
               sb.AppendLine(declareMeshNoLight + alphaStr);
            }
            else if (HasFeature(features, DefineFeature._RAMPLIGHTING))
            {
               sb.AppendLine(declareMeshRamp + alphaStr);
            }
            else
            {
               sb.AppendLine(declareMesh + alphaStr);
            }

            sb.AppendLine("      #pragma target " + GetShaderModel(features));

            WriteFeatures(features, sb);

            sb.AppendLine(meshBody.text);
            WriteFooter(features, sb, null, true);
         }
      }
      string output = sb.ToString();
      if (HasDebugFeature(features) || HasFeature(features, DefineFeature._RAMPLIGHTING))
      {
         output = output.Replace("SurfaceOutputStandard", "SurfaceOutput");
      }
      // fix newline mixing warnings..
      output = System.Text.RegularExpressions.Regex.Replace(output, "\r\n?|\n", System.Environment.NewLine);

      return output;
   }

   public static void Compile(Material m, string shaderName = null)
   {
      var path = AssetDatabase.GetAssetPath(m.shader);
      string nm = m.shader.name;
      if (!string.IsNullOrEmpty(shaderName))
      {
         nm = shaderName;
      }

      // write fallback first, if needed..
      string fallback = null;
      string fallbackPath = null;
      string ret;
      bool exists = false;
      string tessFallback = DefineFeature._TESSFALLBACK.ToString();
      for (int i = 0; i < m.shaderKeywords.Length; ++i)
      {
         if (m.shaderKeywords[i] == tessFallback)
         {
            exists = true;
            break;
         }
      }
      if (exists)
      {
         fallback = nm + "_fallback";
         fallbackPath = path.Replace(".shader", "_fallback.shader");
         List<string> fallbackKeys = new List<string>(m.shaderKeywords);
         if (fallbackKeys.Contains(DefineFeature._TESSDISTANCE.ToString()))
         {
            fallbackKeys.Remove(DefineFeature._TESSDISTANCE.ToString());
         }
         if (fallbackKeys.Contains(DefineFeature._TESSEDGE.ToString()))
         {
            fallbackKeys.Remove(DefineFeature._TESSEDGE.ToString());
         }
         ret = Compile(fallbackKeys.ToArray(), fallback, null, m);
         System.IO.File.WriteAllText(fallbackPath, ret);
      }

      ret = Compile(m.shaderKeywords, nm, fallback, m);
      System.IO.File.WriteAllText(path, ret);

      EditorUtility.SetDirty(m);
      AssetDatabase.Refresh();
   }

}
