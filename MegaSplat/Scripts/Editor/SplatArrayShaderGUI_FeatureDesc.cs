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
   public enum DefineFeature
   {
      _TERRAIN = 0,
      _TWOLAYER,
      _ALPHALAYER,
      _NOSPECTEX,
      _NOSPECNORMAL,
      _FLOW,
      _FLOWREFRACTION,
      _USEMACROTEXTURE,
      _DETAILMAP,
      _PARALLAX,
      _TRIPLANAR,
      _UVFROMSECOND,
      _UVFROMSECOND2,
      _UVLOCALTOP,
      _UVWORLDTOP,
      _UVLOCALSIDE,
      _UVWORLDSIDE,
      _UVLOCALFRONT,
      _UVWORLDFRONT,
      _UVLOCALTOP2,
      _UVWORLDTOP2,
      _UVLOCALSIDE2,
      _UVWORLDSIDE2,
      _UVLOCALFRONT2,
      _UVWORLDFRONT2,
      _EMISMAP,
      _MACROMULT2X,
      _MACROOVERLAY,
      _MACROMULT,
      _MACRONORMAL,
      _SPLATSONTOP,
      _DETAILMULT2X,
      _DETAILOVERLAY,
      _DETAILMULT,
      _DETAILNORMAL,
      _DETAILNOISE,
      _DISTANCENOISE,
      _DISABLESPLATSINDISTANCE,
      _DEBUG_OUTPUT_ALBEDO,
      _DEBUG_OUTPUT_HEIGHT,
      _DEBUG_OUTPUT_NORMAL,
      _DEBUG_OUTPUT_METAL,
      _DEBUG_OUTPUT_SMOOTHNESS,
      _DEBUG_OUTPUT_AO,
      _DEBUG_OUTPUT_EMISSION,
      _DEBUG_OUTPUT_SPLATDATA,
      _TESSDISTANCE,
      _TESSEDGE,
      _TESSPHONG,
      _TESSSHADOWS,
      _TESSFALLBACK,
      _TESSCENTERBIAS,
      _TESSDAMPENING, // only needed on terrains, so we don't both sampling params if unneeded
      _SECONDUV,
      _RAMPLIGHTING,
      _PUDDLES,
      _PUDDLEFLOW,
      _PUDDLEREFRACT,
      _PUDDLEFOAM,
      _PUDDLEDEPTHDAMPEN,
      _LAVA,
      _WETNESS,
      _RAINDROPS,
      _RAINDROPFLATONLY,
      _PUDDLESPUSHTERRAIN,
      _SNOW,
      _SNOWDISTANCENOISE,
      _SNOWDISTANCERESAMPLE,
      _TRIPLANAR_WORLDSPACE,
      _SNOWOVERMACRO,
      _MACROAOSCALE,
      _PERTEXCONTRAST,
      _PERTEXUV,
      _PERTEXMATPARAMS,
      _PERTEXNOISESTRENGTH,
      _PERTEXDISPLACEPARAMS,
      _PERTEXNORMALSTRENGTH,
      _PERTEXPARALLAXSTRENGTH,
      _PERTEXAOSTRENGTH,
      _ALPHA,
      _ALPHATEST,
      _LOWPOLY,
      _LOWPOLYADJUST,
      _DISTANCERESAMPLE,
      _LINEARBIAS,
      _NOBLENDBOTTOM,
      _USECURVEDWORLD,
      _PROJECTTEXTURE_LOCAL,
      _PROJECTTEXTURE_WORLD,
      _PROJECTTEXTURE2_LOCAL,
      _PROJECTTEXTURE2_WORLD,
      _PREPROCESSMESH,
      _CUSTOMUSERFUNCTION,
      _MANUALSHADERLEVEL40,
      _MANUALSHADERLEVEL45,
      _MANUALSHADERLEVEL46,
      _PERTEXGLITTER,
      _SNOWGLITTER,
      _PUDDLEGLITTER,
      _GLITTERMOVES,
      _PROJECTTANGENTS,
      _MACROPRESETFADE,
      _MACROPRESETTINT,
      _GEOMAP,
      _ALPHAHOLE,
      kNumFeatures,
   }

   public enum MacroPreset
   {
      None,
      FadeToMacro,
      TintMap,
   }

   public enum ManualShaderLevel
   {
      Auto = 0,
      ShaderModel40,
      ShaderModel45,
      ShaderModel46
   }

   public enum LightingMode
   {
      StandardPBR,
      Ramp
   }

   public enum UVMode
   {
      UV,
      UV2,
      UVProjectAxis,
      Triplanar
   }

   public enum MacroUVMode
   {
      UV, 
      UV2,
      UVProjectAxis
   }

   public enum ProjectAxis
   {
      Top,
      Front,
      Side
   }

   public enum ProjectSpace
   {
      World,
      Local
   }


   public enum PackingMode
   {
      Default,
      NoSpecTex,
      NoSpecOrNormal
   }

   public enum LayerMode
   {
      Single = 0,
      TwoLayer,
      DetailTexture,
      AlphaLayer
   }

   public enum TerrainLayerMode
   {
      Single = 0,
      TwoLayer,
      DetailTexture
   }

   public enum AlphaMode
   {
      Opaque,
      Transparent,
      Cutout
   }

   public enum TextureChoiceMode
   {
      Painted,
      Project3Way
   }

   // pixel layout for per tex properties
   // metal/smooth/porosity/uv scale
   // flow speed, intensity, alpha, refraction
   // detailNoiseStrength, contrast, displacementAmount, displaceUpBias
   // Normal Strength
  

   public enum FlowMode
   {
      None = 0,
      Flow,
      Refractive
   }
      
   public enum PassBlendMode
   {
      Multiply2X = 0,
      Overlay = 1,
      Multiply = 2,
      Normal = 3,
   }

   public enum ShaderType
   {
      Mesh,
      Terrain
   }

   public enum DebugOutput
   {
      None = 0,
      Albedo,
      Height,
      Normal,
      Metallic,
      Smoothness,
      AO,
      Emission,
      SplatData,
   }

   public enum TessellationMode
   {
      None,
      Distance,
      Edge,
   }

   public enum PuddleMode
   {
      None,
      Puddles,
      PuddleFlow,
      PuddleRefraction,
      Lava
   }


   public enum GlitterMode
   {
      Static,
      Animated
   }
      
   // helper class for the GUI
   public class FeatureData
   {
      public ShaderType shaderType;
      public UVMode uvMode;
      public MacroUVMode macroUVMode;
      public ProjectSpace projectSpace;
      public ProjectAxis projectAxis;
      public ProjectSpace macroProjectSpace;
      public ProjectAxis macroProjectAxis;
      public PackingMode packingMode;
      public LayerMode layerMode;
      public FlowMode flowMode;
      public PassBlendMode macroBlend;
      public PassBlendMode detailBlend;
      public LightingMode lightingMode;
      public bool parallax;
      public bool useMacroTexture;
      public bool emission = false;
      public bool splatsOnTop;
      public bool disableSplatsInDistance;
      public TessellationMode tesselationMode = TessellationMode.None;
      public bool tessPhong;
      public bool tessellateShadowPass = false;
      public bool generateFallback = false;
      public bool biasToCenter = true;
      public DebugOutput debugOutput = DebugOutput.None;
      public bool useSecondUV = false;
      public bool detailNoise = false;
      public bool distanceNoise = false;
      public bool perTexContrast = false;
      public bool perTexUV = false;
      public bool perTexMatParams = false;
      public bool perTexNoiseStrength = false;
      public bool perTexDisplaceParams = false;
      public bool perTexNormalStrength = false;
      public bool perTexParallaxStrength = false;
      public bool perTexAOStrength = false;
      public PuddleMode puddleMode = PuddleMode.None;
      public bool terrainTessDampening = false;
      public bool puddlePushTerrain = false;
      public bool puddleFoam = false;
      public bool puddleDepthDampen = false;
      public bool wetness = false;
      public bool snow = false;
      public bool snowDistanceNoise = false;
      public bool snowDistanceResample = false;
      public bool macroAOScale = false;
      public AlphaMode alphaMode = AlphaMode.Opaque;
      public bool lowPoly = false;
      public bool lowPolyAdjust = false;
      public bool distanceResample = false;
      public bool linearBias = false;
      public bool disableBottomBlend = false;
      public bool curvedWorld = false;
      public bool hiQualityTriplanarNormals = false;
      public bool rainDrops = false;
      public bool rainOnFlatOnly = false;
      public TextureChoiceMode projectTextureMode = TextureChoiceMode.Painted;
      public TextureChoiceMode projectTextureMode2 = TextureChoiceMode.Painted;
      public ProjectSpace projectTextureSpace = ProjectSpace.World;
      public ProjectSpace projectTextureSpace2 = ProjectSpace.World;
      public bool preprocessMesh = false;
      public bool snowOverMacro;
      public ProjectSpace triplanarUV = ProjectSpace.Local;
      public bool customUserFunction = false;
      public ManualShaderLevel manualShaderLevel = ManualShaderLevel.Auto;
      public GlitterMode glitterMode = GlitterMode.Static;
      public bool perTexGlitter;
      public bool snowGlitter;
      public bool puddleGlitter;
      public bool projectTangents;
      public bool geoTexture;
      public bool alphaHole;

      public MacroPreset macroPreset = MacroPreset.None;

      // for gui
      string[] blendOverlayOptions = new string[] { "Macro On Top", "Splats On Top" };

      GUIContent CShaderAlphaHole = new GUIContent("Alpha Hole", "Paintable Alpha Holes");
      GUIContent CShaderName = new GUIContent("Name", "Menu path with name for the shader");
      GUIContent CShaderType = new GUIContent("Shader Type", "Is this shader for meshes or terrains?");
      GUIContent CShaderTessellation = new GUIContent("Tessellation", "Type of tessellation to use or None for no tessellation. Edge mode tessellates based on the screen size of an edge. Distance tessellates based on distance from the camera." +
          " Generally speaking, Edge mode swims more, but provides a more consistent triangle size, which Distance mode can over tessellate triangles");
      GUIContent CShaderTessShadow = new GUIContent("Tessellate Shadow Pass", "When true, the shadow pass is tessellated as well as the regular passes. Can be turned off to save performance, but looks great with high quality shadows");
      GUIContent CShaderFallback = new GUIContent("Generate Fallback Shader", "If true, the compiler will generate a fallback shader for systems which don't support tessellation");
      GUIContent CShaderTessPhong = new GUIContent("Phong Curve", "Should we phong tessellate the triangles, giving the overall landscape a rounder feel?");
      GUIContent CShaderDisplaceFromCenter = new GUIContent("Displace From Center", "When true, height maps displace down and up. When false, displacement only happens up");
      GUIContent CShaderUVMode = new GUIContent("Splat UV Mode", "Use mesh UV's, a top down projection, or Triplanar. Note that triplanar texturing is much more expensive than using UVs");
      GUIContent CShaderMacroUVMode = new GUIContent("Macro UV Mode", "UV Mode for macro texture");
      GUIContent CShaderLayerMode = new GUIContent("Layer Mode", "A single layer of splat mapping is the cheapest. But you can use Two Layer mode to blend a second set of splat maps into the first. Detail texture mode acts like a single layer, " +
                              " but tiles a detail texture array inside of your main textures. Alpha Layer mode is used for painting splat maps onto regularly textured surfaces, such as adding dirt on the walls or roof of a house. Alpha Layer mode uses the macro" +
                              " textures as the regular textures on the object");
      GUIContent CShaderTexturePacking = new GUIContent("Texture Packing", "Format your textures are in.  ");
      GUIContent CShaderFlowMode = new GUIContent("Per Tex Flow Mode", "Turn on Flow mapping, which can be regular or refractive. When Refractive flow mapping is used with a two layer shader, the second layer can alpha/refract with the first layer");
      GUIContent CShaderMacroTexture = new GUIContent("Macro Texture", "Macro texturing is used to sample a traditional set of textures that are blended with splat mapping");
      GUIContent CShaderSplatsFadeToMacro = new GUIContent("Splats Fade to Macro", "Sets up the macro blending modes so that the splat maps cross fade to a macro texture in the distance");
      GUIContent CShaderEmissionMap = new GUIContent("Emission/Metallic Array", "Use the Emission/Metallic map");
      GUIContent CShaderDebugOutput = new GUIContent("Debug Output", "Used by Render Baking, but useful when you want to debug the output of the shader. Allows you to see the different channels (albedo, normal, ao, etc) instead of the final result");
      GUIContent CShaderParallax = new GUIContent("Parallax Map", "Turns on parallax mapping, which takes an extra set of texture samples to offset the UVs to create a depth effect");
      GUIContent CShaderSecondUV = new GUIContent("Add Second UV", "uses a second UV set for the macro texture, which can come from UV0, UV1, or a projection. Increases required shader model to 4.0");
      GUIContent CShaderPerTexContrast = new GUIContent("Per Tex Contrast", "When used, splat contrast is controlled per texture. Layer contrast still uses standard contrast control");
      GUIContent CShaderPerTexUV = new GUIContent("Per Tex Scale", "When used, uv scale can be controlled per texture");
      GUIContent CShaderPerTexMat = new GUIContent("Per Tex Material", "When used, metallic, smoothness, and porosity can be set per texture");
      GUIContent CShaderPerTexNoiseStrength = new GUIContent("Per Tex Noise Strength", "When used, detail noise strength can be set per texture");
      GUIContent CShaderPerTexDisplacement = new GUIContent("Per Tex Displacement", "When used, displacement amount and up bias can be controlled per texture");
      GUIContent CShaderPerTexNormalStrength = new GUIContent("Per Tex Normal Strength", "When used, normal strength can be adjusted per texture");
      GUIContent CShaderPerTexParallaxStrength = new GUIContent("Per Tex Parallax Strength", "When used, parallax strength can be adjusted per texture");
      GUIContent CShaderPerTexAOStrength = new GUIContent("Per Tex AO Strength", "When used, AO strength can be adjusted per texture");
      GUIContent CShaderPuddles = new GUIContent("Puddles", "Enabled painting of small puddles on the ground, with optional flow mapping and refraction");
      GUIContent CShaderTessDamp = new GUIContent("Displacement Dampening", "Allows you to paint displacement dampening in the terrain painter, to control displacement amount");
      GUIContent CShaderPuddlePushTerrain = new GUIContent("Puddles Push Terrain Down", "Painting puddles pushes terrain down, instead of water rising up. This looks nicer on static puddles, but water coming up looks better if you're doing runtime water moving across a mesh");
      GUIContent CShaderPuddleFoam = new GUIContent("Puddle Foam", "Pack a foam texture into the B chnanel of the puddle normal to create foam, as found on rivers");
      GUIContent CShaderPuddleDepthDampen = new GUIContent("Puddle Refraction Depth Dampen", "Creates a slider which can be used to control how calm the ater gets in deep areas");
      GUIContent CShaderSnow = new GUIContent("Global Snow", "Enabled Global Snow");
      GUIContent CShaderSnowDistanceNoise = new GUIContent("Snow Distance Noise", "When enabled, a noise texture is sampled in the distance to modify albedo and normal values");
      GUIContent CShaderSnowDistanceResample = new GUIContent("Snow Distance Resample", "When enabled, snow texture is resamples and blended in with itself at different UV scale in the distance");
      GUIContent CShaderSnowOverMacro = new GUIContent("Snow over Macro", "When enabled, snow affects the macro texture as well as the splat maps");
      GUIContent CShaderWetness = new GUIContent("Wetness", "Allows you to paint the minimum wetness of the surface into UV1.w");
      GUIContent CShaderLowPoly = new GUIContent("Low Poly Look", "Allows you to make each triangle have hard edges, giving a low poly look");
      GUIContent CShaderLowPolyAdjust = new GUIContent("Hardness Adjustment", "Allows you to control hardness of the low poly effect");
      GUIContent CShaderDistanceResample = new GUIContent("Distance Resampling", "Resample all splats at a different scale and cross fade over a distance");
      GUIContent CShaderLinearBias = new GUIContent("Linear Blend Bias", "Biases the contrast of texture transitions towards a standard linear blend when contrast is at 0");
      GUIContent CShaderDisableBottomBlend = new GUIContent("Disable Bottom Layer Blend", "Disables blending of textures on the bottom layer. This is an optimization when you are texturing each face with a single texture index, such that each face only ever has one texture on it. ");
      GUIContent CShaderCurvedWorld = new GUIContent("Use Curved World", "Requires Curved World asset to be installed. When checked, shader will be generated with support for curved world rendering");
      GUIContent CShaderRainDrops = new GUIContent("Rain Drops", "When true, raindrops can be generated with a special texture on puddles");
      GUIContent CShaderRainOnFlatOnly = new GUIContent("No Rain on Walls", "When true, raindrop strength is reduced based on surface angle to prevent them from appearing on walls");
      GUIContent CShaderProjectTextureMode = new GUIContent("Bottom Layer Texturing", "How do we choose which texture to put where- via painting, or a procedural mode?");
      GUIContent CShaderProjectTextureMode2 = new GUIContent("Top Layer Texturing", "How do we choose which texture to put where- via painting, or a procedural mode?");
      GUIContent CShaderPreprocessMesh = new GUIContent("Preprocess Mesh", "Preprocess the mesh in the geometry shader. This prevents you from having to preprocess meshes, but makes the shader significantly slower");
      GUIContent CShaderCustomFunction = new GUIContent("Custom User Function", "Creates a .cginc file and .props file that you can use to extend the shader automatically");
      GUIContent CShaderManualLevel = new GUIContent("Shader Level Override", "Allows you to set the shader level manually. Note that the shader level will still be bumped up when feature selection forces it to be.");

      GUIContent CShaderPerTexGlitter = new GUIContent("Per Tex Glitter", "Allows you to specify a glitter amount per texture");
      GUIContent CShaderSnowGlitter = new GUIContent("Snow Glitter", "Glitter on Snow?");
      GUIContent CShaderPuddleGlitter = new GUIContent("Puddle Glitter", "Glitter on Puddles?");
      GUIContent CShaderGlitterMode = new GUIContent("Glitter Mode", "Static or animated; when animated, glitter changes sparkle even when not moving the camera");
      GUIContent CShaderProjectTangents = new GUIContent("Project Tangents", "Should tangents be generated from the texture projection");
      GUIContent CShaderMacroPreset = new GUIContent("Macro Preset", "Select profile to automatically setup macro texture settings for this effect");
      GUIContent CShaderGeoTexture = new GUIContent("Geo Height Texture", "Adds support for a tint texture applied virtically across the terrain");


      static Dictionary<System.Type, string[]> popupOps = new Dictionary<System.Type, string[]>();

      static string[] PopupOptions(System.Type t)
      {
         string[] ret;
         if (popupOps.TryGetValue(t, out ret))
         {
            return ret;
         }
         ret = System.Enum.GetNames(t);
         popupOps.Add(t, ret);
         return ret;
      }


      public bool DrawGUI(Material mat, ref string shaderName)
      {
         bool needsCompile = false;
         if (MegaSplatUtilities.DrawRollup("MegaSplat Shader Compiler"))
         {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.HelpBox("Changes to these values will modify the shader source code!", MessageType.Warning);
            shaderName = EditorGUILayout.DelayedTextField(CShaderName, shaderName);
            shaderType = (ShaderType)EditorGUILayout.EnumPopup(CShaderType, shaderType);
            alphaMode = (AlphaMode)EditorGUILayout.EnumPopup("Alpha Mode", alphaMode);
            tesselationMode = (TessellationMode)EditorGUILayout.EnumPopup(CShaderTessellation, tesselationMode);
            if (tesselationMode != TessellationMode.None)
            {
               EditorGUI.indentLevel++;
               tessPhong = EditorGUILayout.Toggle(CShaderTessPhong, tessPhong);
               if (!tessellateShadowPass)
               {
                  EditorGUILayout.HelpBox("Shadow Pass must be tessellated when using Deferred Rendering, otherwise artifacts will occur", MessageType.Warning);
               }
               tessellateShadowPass = EditorGUILayout.Toggle(CShaderTessShadow, tessellateShadowPass);

               generateFallback = EditorGUILayout.Toggle(CShaderFallback, generateFallback);
               biasToCenter = EditorGUILayout.Toggle(CShaderDisplaceFromCenter, biasToCenter);
               perTexDisplaceParams = EditorGUILayout.Toggle(CShaderPerTexDisplacement, perTexDisplaceParams);
               EditorGUI.indentLevel--;

            }
            if (shaderType == ShaderType.Terrain && tesselationMode != TessellationMode.None)
            {
               EditorGUI.indentLevel++;
               terrainTessDampening = EditorGUILayout.Toggle(CShaderTessDamp, terrainTessDampening);
               EditorGUI.indentLevel--;
            }

            packingMode = (PackingMode)EditorGUILayout.EnumPopup(CShaderTexturePacking, packingMode);

            linearBias = EditorGUILayout.Toggle(CShaderLinearBias, linearBias);
            lightingMode = (LightingMode)EditorGUILayout.EnumPopup("Lighting Mode", (System.Enum)lightingMode);

            uvMode = (UVMode)EditorGUILayout.EnumPopup(CShaderUVMode, uvMode);
            if (uvMode == UVMode.UVProjectAxis)
            {
               EditorGUI.indentLevel++;
               EditorGUILayout.BeginHorizontal();
               projectSpace = (ProjectSpace)EditorGUILayout.EnumPopup("Projection", projectSpace);
               projectAxis = (ProjectAxis)EditorGUILayout.EnumPopup(projectAxis);
               EditorGUILayout.EndHorizontal();
               projectTangents = EditorGUILayout.Toggle(CShaderProjectTangents, projectTangents);
               EditorGUI.indentLevel--;
            }
            if (uvMode == UVMode.Triplanar)
            {
               EditorGUI.indentLevel++;
               triplanarUV = (ProjectSpace) EditorGUILayout.EnumPopup("Space", triplanarUV);
               EditorGUI.indentLevel--;
            }

            geoTexture = EditorGUILayout.Toggle(CShaderGeoTexture, geoTexture);

            bool macroPresentWasNone = macroPreset == MacroPreset.None;
            EditorGUI.BeginChangeCheck();
            macroPreset = (MacroPreset)EditorGUILayout.EnumPopup(CShaderMacroPreset, macroPreset);
            if (EditorGUI.EndChangeCheck())
            {
               if (!macroPresentWasNone && macroPreset == MacroPreset.None)
               {
                  useMacroTexture = false;
                  useSecondUV = false;
               }
            }

            if (macroPreset == MacroPreset.None)
            {
               useSecondUV = EditorGUILayout.Toggle(CShaderSecondUV, useSecondUV);
               if (useSecondUV)
               {
                  EditorGUI.indentLevel++;
                  macroUVMode = (MacroUVMode)EditorGUILayout.EnumPopup(CShaderMacroUVMode, macroUVMode);
                  if (macroUVMode == MacroUVMode.UVProjectAxis)
                  {
                     EditorGUILayout.BeginHorizontal();
                     macroProjectSpace = (ProjectSpace)EditorGUILayout.EnumPopup("Projection", macroProjectSpace);
                     macroProjectAxis = (ProjectAxis)EditorGUILayout.EnumPopup(macroProjectAxis);
                     EditorGUILayout.EndHorizontal();
                  }
                  EditorGUI.indentLevel--;
               }
            }

            if (shaderType == ShaderType.Terrain)
            {
               var lm = (TerrainLayerMode)layerMode;
               if (layerMode == LayerMode.AlphaLayer)
               {
                  lm = TerrainLayerMode.Single;
               }
               lm = (TerrainLayerMode)EditorGUILayout.EnumPopup(CShaderLayerMode, lm);
               layerMode = (LayerMode)lm;
            }
            else
            {
               layerMode = (LayerMode)EditorGUILayout.EnumPopup(CShaderLayerMode, layerMode);
            }

            if (shaderType == ShaderType.Mesh)
            {
               if (layerMode == LayerMode.Single || layerMode == LayerMode.TwoLayer)
               {
                  projectTextureMode = (TextureChoiceMode)EditorGUILayout.EnumPopup(CShaderProjectTextureMode, projectTextureMode);
                  if (projectTextureMode != TextureChoiceMode.Painted)
                  {
                     projectTextureSpace = (ProjectSpace)EditorGUILayout.EnumPopup("Space", projectTextureSpace);
                  }

               }
               if (layerMode == LayerMode.TwoLayer || layerMode == LayerMode.AlphaLayer)
               {
                  projectTextureMode2 = (TextureChoiceMode)EditorGUILayout.EnumPopup(CShaderProjectTextureMode2, projectTextureMode2);
                  if (projectTextureMode2 != TextureChoiceMode.Painted)
                  {
                     projectTextureSpace2 = (ProjectSpace)EditorGUILayout.EnumPopup("Space", projectTextureSpace2);
                  }
               }
            }
            alphaHole = EditorGUILayout.Toggle(CShaderAlphaHole, alphaHole);
            flowMode = (FlowMode)EditorGUILayout.EnumPopup(CShaderFlowMode, flowMode);

            perTexUV = EditorGUILayout.Toggle(CShaderPerTexUV, perTexUV);
            perTexMatParams = EditorGUILayout.Toggle(CShaderPerTexMat, perTexMatParams);
            perTexContrast = EditorGUILayout.Toggle(CShaderPerTexContrast, perTexContrast);
            perTexNormalStrength = EditorGUILayout.Toggle(CShaderPerTexNormalStrength, perTexNormalStrength);
            perTexAOStrength = EditorGUILayout.Toggle(CShaderPerTexAOStrength, perTexAOStrength);
            perTexGlitter = EditorGUILayout.Toggle(CShaderPerTexGlitter, perTexGlitter);


            if (macroPreset == MacroPreset.None)
            {
               if (layerMode != LayerMode.AlphaLayer)
               {
                  useMacroTexture = EditorGUILayout.Toggle(CShaderMacroTexture, useMacroTexture);
               }
               else
               {
                  useMacroTexture = true;
               }
               if (useMacroTexture)
               {
                  EditorGUI.indentLevel++;
                  if (layerMode != LayerMode.AlphaLayer)
                  {
                     disableSplatsInDistance = EditorGUILayout.Toggle(CShaderSplatsFadeToMacro, disableSplatsInDistance);
                  }
                  else
                  {
                     splatsOnTop = true;
                  }

                  if (disableSplatsInDistance)
                  {
                     splatsOnTop = true;
                     macroBlend = PassBlendMode.Normal;
                  }
                  else
                  {
                     if (layerMode != LayerMode.AlphaLayer)
                     {
                        splatsOnTop = (EditorGUILayout.Popup("Layering", (splatsOnTop == true) ? 1 : 0, blendOverlayOptions) == 1) ? true : false;
                     }
                     macroBlend = (PassBlendMode)EditorGUILayout.EnumPopup("Blend Mode", macroBlend);
                  }

                  EditorGUI.indentLevel--;
               }
            }

            if (layerMode == LayerMode.DetailTexture)
            {
               detailBlend = (PassBlendMode)EditorGUILayout.EnumPopup("Detail Blend Mode", detailBlend);
            }
            else
            {
               detailNoise = EditorGUILayout.Toggle("Detail Noise Texture", detailNoise);
               if (detailNoise)
               {
                  perTexNoiseStrength = EditorGUILayout.Toggle(CShaderPerTexNoiseStrength, perTexNoiseStrength);
               }
            }

            distanceNoise = EditorGUILayout.Toggle("Distance Noise Texture", distanceNoise);

            parallax = EditorGUILayout.Toggle(CShaderParallax, parallax);
            if (parallax)
            {
               EditorGUI.indentLevel++;
               perTexParallaxStrength = EditorGUILayout.Toggle(CShaderPerTexParallaxStrength, perTexParallaxStrength);
               EditorGUI.indentLevel--;
            }
            else
            {
               perTexParallaxStrength = false;
            }


            emission = EditorGUILayout.Toggle(CShaderEmissionMap, emission);

            distanceResample = EditorGUILayout.Toggle(CShaderDistanceResample, distanceResample);

            puddleMode = (PuddleMode)EditorGUILayout.EnumPopup(CShaderPuddles, puddleMode);
            if (tesselationMode != TessellationMode.None && puddleMode != PuddleMode.None)
            {
               EditorGUI.indentLevel++;
               puddlePushTerrain = EditorGUILayout.Toggle(CShaderPuddlePushTerrain, puddlePushTerrain);
               EditorGUI.indentLevel--;
            }
            if (puddleMode != PuddleMode.None && puddleMode != PuddleMode.Lava)
            {
               EditorGUI.indentLevel++;
               rainDrops = EditorGUILayout.Toggle(CShaderRainDrops, rainDrops);
               if (rainDrops)
               {
                  rainOnFlatOnly = EditorGUILayout.Toggle(CShaderRainOnFlatOnly, rainOnFlatOnly);
               }
               if (puddleMode != PuddleMode.Puddles)
               {
                  puddleFoam = EditorGUILayout.Toggle(CShaderPuddleFoam, puddleFoam);
               }
               EditorGUI.indentLevel--;
            }

            if (puddleMode == PuddleMode.PuddleRefraction)
            {
               EditorGUI.indentLevel++;
               puddleDepthDampen = EditorGUILayout.Toggle(CShaderPuddleDepthDampen, puddleDepthDampen);
               EditorGUI.indentLevel--;
            } 
            if (puddleMode != PuddleMode.None)
            {
               EditorGUI.indentLevel++;
               puddleGlitter = EditorGUILayout.Toggle(CShaderPuddleGlitter, puddleGlitter);
               EditorGUI.indentLevel--;
            }
            wetness = EditorGUILayout.Toggle(CShaderWetness, wetness);

            snow = EditorGUILayout.Toggle(CShaderSnow, snow);
            if (snow && useMacroTexture)
            {
               EditorGUI.indentLevel++;
               snowOverMacro = EditorGUILayout.Toggle(CShaderSnowOverMacro, snowOverMacro);
               EditorGUI.indentLevel--;
            }
            if (snow)
            {
               EditorGUI.indentLevel++;
               snowGlitter = EditorGUILayout.Toggle(CShaderSnowGlitter, snowGlitter);
               snowDistanceNoise = EditorGUILayout.Toggle(CShaderSnowDistanceNoise, snowDistanceNoise);
               snowDistanceResample = EditorGUILayout.Toggle(CShaderSnowDistanceResample, snowDistanceResample);
               EditorGUI.indentLevel--;
            }

            if (snowGlitter || puddleGlitter || perTexGlitter)
            {
               glitterMode = (GlitterMode)EditorGUILayout.EnumPopup(CShaderGlitterMode, glitterMode);
            }
            disableBottomBlend = EditorGUILayout.Toggle(CShaderDisableBottomBlend, disableBottomBlend);

            lowPoly = EditorGUILayout.Toggle(CShaderLowPoly, lowPoly);
            if (lowPoly)
            {
               EditorGUI.indentLevel++;
               lowPolyAdjust = EditorGUILayout.Toggle(CShaderLowPolyAdjust, lowPolyAdjust);
               EditorGUI.indentLevel--;
            }
            if (shaderType == ShaderType.Mesh)
            {
               preprocessMesh = EditorGUILayout.Toggle(CShaderPreprocessMesh, preprocessMesh);
            }

            curvedWorld = EditorGUILayout.Toggle(CShaderCurvedWorld, curvedWorld);
            bool oldCUF = customUserFunction;
            customUserFunction = EditorGUILayout.Toggle(CShaderCustomFunction, customUserFunction);
            if (oldCUF == false && customUserFunction == true)
            {
               // make sure include file exists. 
               string path = AssetDatabase.GetAssetPath(mat.shader);
               path = path.Substring(6);
               path = Application.dataPath + path;
               path = path.Replace("\\", "/");
               path = path.Substring(0, path.LastIndexOf("/"));
               string path2 = path;
               path2 += "/megasplat_custom.props";
               path += "/megasplat_custom.cginc";
               InitCompiler();
               if (!System.IO.File.Exists(path))
               {
                  System.IO.File.WriteAllText(path, customFuncTemplate.text);
               }

               // now make sure props file exists
               if (!System.IO.File.Exists(path2))
               {
                  System.IO.File.WriteAllText(path2, customPropTemplate.text);
               }

            }
            manualShaderLevel = (ManualShaderLevel)EditorGUILayout.EnumPopup(CShaderManualLevel, manualShaderLevel);
            debugOutput = (DebugOutput)EditorGUILayout.EnumPopup(CShaderDebugOutput, debugOutput);

            needsCompile = EditorGUI.EndChangeCheck();
            if (needsCompile)
            {
               Pack(mat);
            }

         }
         return needsCompile;
      }

      public void Unpack(string[] keywords)
      {
         shaderType = (HasFeature(keywords, DefineFeature._TERRAIN)) ? ShaderType.Terrain : ShaderType.Mesh;

         layerMode = LayerMode.Single;

         if (HasFeature(keywords, DefineFeature._ALPHA))
         {
            alphaMode = AlphaMode.Transparent;
         }
         else if (HasFeature(keywords, DefineFeature._ALPHATEST))
         {
            alphaMode = AlphaMode.Cutout;
         }
         else
         {
            alphaMode = AlphaMode.Opaque;
         }
       
         alphaHole = HasFeature(keywords, DefineFeature._ALPHAHOLE);
         lowPoly = HasFeature(keywords, DefineFeature._LOWPOLY);
         lowPolyAdjust = HasFeature(keywords, DefineFeature._LOWPOLYADJUST);
         preprocessMesh = HasFeature(keywords, DefineFeature._PREPROCESSMESH);

         curvedWorld = HasFeature(keywords, DefineFeature._USECURVEDWORLD);

         bool isTwoLayer = HasFeature(keywords, DefineFeature._TWOLAYER);
         bool isDetail = HasFeature(keywords, DefineFeature._DETAILMAP);
         bool isAlphaLayer = HasFeature(keywords, DefineFeature._ALPHALAYER);
         if (isTwoLayer)
            layerMode = LayerMode.TwoLayer;
         else if (isDetail)
            layerMode = LayerMode.DetailTexture;
         else if (isAlphaLayer)
            layerMode = LayerMode.AlphaLayer;

         if (HasFeature(keywords, DefineFeature._RAMPLIGHTING))
         {
            lightingMode = LightingMode.Ramp;
         }
         else
         {
            lightingMode = LightingMode.StandardPBR;
         }

         if (HasFeature(keywords, DefineFeature._TESSDAMPENING))
         {
            terrainTessDampening = true;
         }

         geoTexture = (HasFeature(keywords, DefineFeature._GEOMAP));

         distanceResample = (HasFeature(keywords, DefineFeature._DISTANCERESAMPLE));
         packingMode = PackingMode.Default;

         if (HasFeature(keywords, DefineFeature._NOSPECTEX))
         {
            packingMode = PackingMode.NoSpecTex;
         }
         else if (HasFeature(keywords, DefineFeature._NOSPECNORMAL))
         {
            packingMode = PackingMode.NoSpecOrNormal;
         }

         linearBias = (HasFeature(keywords, DefineFeature._LINEARBIAS));

         if (HasFeature(keywords, DefineFeature._TESSDISTANCE))
         {
            tesselationMode = TessellationMode.Distance;
         }
         else if (HasFeature(keywords, DefineFeature._TESSEDGE))
         {
            tesselationMode = TessellationMode.Edge;
         }
         else
         {
            tesselationMode = TessellationMode.None;
         }

         if (tesselationMode != TessellationMode.None)
         {
            perTexDisplaceParams = (HasFeature(keywords, DefineFeature._PERTEXDISPLACEPARAMS));
            tessPhong = HasFeature(keywords, DefineFeature._TESSPHONG);
            tessellateShadowPass = HasFeature(keywords, DefineFeature._TESSSHADOWS);
            generateFallback = HasFeature(keywords, DefineFeature._TESSFALLBACK);
            biasToCenter = HasFeature(keywords, DefineFeature._TESSCENTERBIAS);
         }
            
         if (HasFeature(keywords, DefineFeature._MACROPRESETFADE))
         {
            macroPreset = MacroPreset.FadeToMacro;
            useMacroTexture = true;
            macroBlend = PassBlendMode.Normal;
            macroUVMode = MacroUVMode.UV;
            disableSplatsInDistance = true;
            splatsOnTop = false;
         }
         else if (HasFeature(keywords, DefineFeature._MACROPRESETTINT))
         {
            macroPreset = MacroPreset.TintMap;
            useMacroTexture = true;
            macroBlend = PassBlendMode.Multiply2X;
            macroUVMode = MacroUVMode.UV;
            disableSplatsInDistance = false;
            splatsOnTop = false;
         }

         if (HasFeature(keywords, DefineFeature._SECONDUV))
         {
            useSecondUV = true;
         }

         if (HasFeature(keywords, DefineFeature._DETAILNOISE))
         {
            detailNoise = true;
            if (HasFeature(keywords, DefineFeature._PERTEXNOISESTRENGTH))
            {
               perTexNoiseStrength = true;
            }
         }

         distanceNoise = HasFeature(keywords, DefineFeature._DISTANCENOISE);


         if (HasFeature(keywords, DefineFeature._MANUALSHADERLEVEL40))
         {
            manualShaderLevel = ManualShaderLevel.ShaderModel40;
         }
         else if (HasFeature(keywords, DefineFeature._MANUALSHADERLEVEL45))
         {
            manualShaderLevel = ManualShaderLevel.ShaderModel45;
         }
         else if (HasFeature(keywords, DefineFeature._MANUALSHADERLEVEL46))
         {
            manualShaderLevel = ManualShaderLevel.ShaderModel46;
         }

         if (shaderType == ShaderType.Mesh)
         {
            if (HasFeature(keywords, DefineFeature._PROJECTTEXTURE_LOCAL))
            {
               projectTextureMode = TextureChoiceMode.Project3Way;
               projectTextureSpace = ProjectSpace.Local;

            }
            else if (HasFeature(keywords, DefineFeature._PROJECTTEXTURE_WORLD))
            {
               projectTextureMode = TextureChoiceMode.Project3Way;
               projectTextureSpace = ProjectSpace.World;
            }
            else
            {
               projectTextureMode = TextureChoiceMode.Painted;
            }

            if (HasFeature(keywords, DefineFeature._PROJECTTEXTURE2_LOCAL))
            {
               projectTextureMode2 = TextureChoiceMode.Project3Way;
               projectTextureSpace2 = ProjectSpace.Local;

            }
            else if (HasFeature(keywords, DefineFeature._PROJECTTEXTURE2_WORLD))
            {
               projectTextureMode2 = TextureChoiceMode.Project3Way;
               projectTextureSpace2 = ProjectSpace.World;
            }
            else
            {
               projectTextureMode2 = TextureChoiceMode.Painted;
            }
         }

         if (HasFeature(keywords, DefineFeature._PUDDLES))
         {
            puddleMode = PuddleMode.Puddles;
         }
         else if (HasFeature(keywords, DefineFeature._PUDDLEFLOW))
         {
            puddleMode = PuddleMode.PuddleFlow;
            puddleFoam = (HasFeature(keywords, DefineFeature._PUDDLEFOAM));
         }
         else if (HasFeature(keywords, DefineFeature._PUDDLEREFRACT))
         {
            puddleMode = PuddleMode.PuddleRefraction;
            puddleFoam = (HasFeature(keywords, DefineFeature._PUDDLEFOAM));
            puddleDepthDampen = HasFeature(keywords, DefineFeature._PUDDLEDEPTHDAMPEN);
         }
         else if (HasFeature(keywords, DefineFeature._LAVA))
         {
            puddleMode = PuddleMode.Lava;
         }

         if (puddleMode != PuddleMode.None)
         {
            puddleGlitter = HasFeature(keywords, DefineFeature._PUDDLEGLITTER);
         }

         rainDrops = (HasFeature(keywords, DefineFeature._RAINDROPS));
         if (rainDrops)
         {
            rainOnFlatOnly = (HasFeature(keywords, DefineFeature._RAINDROPFLATONLY));
         }
         else
         {
            rainOnFlatOnly = false;
         }
         puddlePushTerrain = (HasFeature(keywords, DefineFeature._PUDDLESPUSHTERRAIN));

         wetness = HasFeature(keywords, DefineFeature._WETNESS);

         snow = (HasFeature(keywords, DefineFeature._SNOW));
         if (snow)
         {
            snowOverMacro = HasFeature(keywords, DefineFeature._SNOWOVERMACRO);
            snowGlitter = HasFeature(keywords, DefineFeature._SNOWGLITTER);
            snowDistanceNoise = HasFeature(keywords, DefineFeature._SNOWDISTANCENOISE);
            snowDistanceResample = HasFeature(keywords, DefineFeature._SNOWDISTANCERESAMPLE);
         }
         flowMode = FlowMode.None;
         if (HasFeature(keywords, DefineFeature._FLOW))
         {
            flowMode = FlowMode.Flow;
         }
         else if (HasFeature(keywords, DefineFeature._FLOWREFRACTION))
         {
            flowMode = FlowMode.Refractive;
         }

         perTexMatParams = (HasFeature(keywords, DefineFeature._PERTEXMATPARAMS));
         perTexContrast = (HasFeature(keywords, DefineFeature._PERTEXCONTRAST));
         perTexUV = (HasFeature(keywords, DefineFeature._PERTEXUV));
         perTexNormalStrength = (HasFeature(keywords, DefineFeature._PERTEXNORMALSTRENGTH));
         perTexAOStrength = (HasFeature(keywords, DefineFeature._PERTEXAOSTRENGTH));
         perTexParallaxStrength = (HasFeature(keywords, DefineFeature._PERTEXPARALLAXSTRENGTH));
         perTexGlitter = (HasFeature(keywords, DefineFeature._PERTEXGLITTER));

         if (HasFeature(keywords, DefineFeature._GLITTERMOVES))
         {
            glitterMode = GlitterMode.Animated;
         }
         else
         {
            glitterMode = GlitterMode.Static;
         }
            
         uvMode = UVMode.UV;
         if (HasFeature(keywords, DefineFeature._UVFROMSECOND))
         {
            uvMode = UVMode.UV2;
         }
         else if (HasFeature(keywords, DefineFeature._TRIPLANAR))
         {
            uvMode = UVMode.Triplanar;
            if (HasFeature(keywords, DefineFeature._TRIPLANAR_WORLDSPACE))
            {
               triplanarUV = ProjectSpace.World;
            }
            else
            {
               triplanarUV = ProjectSpace.Local;
            }
         }
         else if (HasFeature(keywords, DefineFeature._UVLOCALTOP))
         {
            uvMode = UVMode.UVProjectAxis;
            projectSpace = ProjectSpace.Local;
            projectAxis = ProjectAxis.Top;
         }
         else if (HasFeature(keywords, DefineFeature._UVWORLDTOP))
         {
            uvMode = UVMode.UVProjectAxis;
            projectSpace = ProjectSpace.World;
            projectAxis = ProjectAxis.Top;
         }
         else if (HasFeature(keywords, DefineFeature._UVLOCALSIDE))
         {
            uvMode = UVMode.UVProjectAxis;
            projectSpace = ProjectSpace.Local;
            projectAxis = ProjectAxis.Side;
         }
         else if (HasFeature(keywords, DefineFeature._UVWORLDSIDE))
         {
            uvMode = UVMode.UVProjectAxis;
            projectSpace = ProjectSpace.World;
            projectAxis = ProjectAxis.Side;
         }
         else if (HasFeature(keywords, DefineFeature._UVLOCALFRONT))
         {
            uvMode = UVMode.UVProjectAxis;
            projectSpace = ProjectSpace.Local;
            projectAxis = ProjectAxis.Front;
         }
         else if (HasFeature(keywords, DefineFeature._UVWORLDFRONT))
         {
            uvMode = UVMode.UVProjectAxis;
            projectSpace = ProjectSpace.World;
            projectAxis = ProjectAxis.Front;
         }
         projectTangents = (HasFeature(keywords, DefineFeature._PROJECTTANGENTS));


         if (uvMode != UVMode.Triplanar)
         {
            disableBottomBlend = HasFeature(keywords, DefineFeature._NOBLENDBOTTOM);
         }

         if (macroPreset == MacroPreset.None)
         {
            macroUVMode = MacroUVMode.UV;
            if (HasFeature(keywords, DefineFeature._UVFROMSECOND2))
            {
               macroUVMode = MacroUVMode.UV2;
            }
            else if (HasFeature(keywords, DefineFeature._SECONDUV))
            {
               if (HasFeature(keywords, DefineFeature._UVLOCALTOP2))
               {
                  macroUVMode = MacroUVMode.UVProjectAxis;
                  macroProjectSpace = ProjectSpace.Local;
                  macroProjectAxis = ProjectAxis.Top;
               }
               else if (HasFeature(keywords, DefineFeature._UVWORLDTOP2))
               {
                  macroUVMode = MacroUVMode.UVProjectAxis;
                  macroProjectSpace = ProjectSpace.World;
                  macroProjectAxis = ProjectAxis.Top;
               }
               else if (HasFeature(keywords, DefineFeature._UVLOCALSIDE2))
               {
                  macroUVMode = MacroUVMode.UVProjectAxis;
                  macroProjectSpace = ProjectSpace.Local;
                  macroProjectAxis = ProjectAxis.Side;
               }
               else if (HasFeature(keywords, DefineFeature._UVWORLDSIDE2))
               {
                  macroUVMode = MacroUVMode.UVProjectAxis;
                  macroProjectSpace = ProjectSpace.World;
                  macroProjectAxis = ProjectAxis.Side;
               }
               else if (HasFeature(keywords, DefineFeature._UVLOCALFRONT2))
               {
                  macroUVMode = MacroUVMode.UVProjectAxis;
                  macroProjectSpace = ProjectSpace.Local;
                  macroProjectAxis = ProjectAxis.Front;
               }
               else if (HasFeature(keywords, DefineFeature._UVWORLDFRONT2))
               {
                  macroUVMode = MacroUVMode.UVProjectAxis;
                  macroProjectSpace = ProjectSpace.World;
                  macroProjectAxis = ProjectAxis.Front;
               }
            }
         

            macroBlend = PassBlendMode.Normal;

            if (HasFeature(keywords, DefineFeature._MACROMULT))
            {
               macroBlend = PassBlendMode.Multiply;
            }
            else if (HasFeature(keywords, DefineFeature._MACROMULT2X))
            {
               macroBlend = PassBlendMode.Multiply2X;
            }
            else if (HasFeature(keywords, DefineFeature._MACROOVERLAY))
            {
               macroBlend = PassBlendMode.Overlay;
            }
         }

         detailBlend = PassBlendMode.Normal;
         if (HasFeature(keywords, DefineFeature._DETAILMULT))
         {
            detailBlend = PassBlendMode.Multiply;
         }
         else if (HasFeature(keywords, DefineFeature._DETAILMULT2X))
         {
            detailBlend = PassBlendMode.Multiply2X;
         }
         else if (HasFeature(keywords, DefineFeature._DETAILOVERLAY))
         {
            detailBlend = PassBlendMode.Overlay;
         }

         parallax = HasFeature(keywords, DefineFeature._PARALLAX);
         useMacroTexture = HasFeature(keywords, DefineFeature._USEMACROTEXTURE);
         macroAOScale = (useMacroTexture && HasFeature(keywords, DefineFeature._MACROAOSCALE));

         emission = HasFeature(keywords, DefineFeature._EMISMAP);

         if (macroPreset == MacroPreset.None)
         {
            disableSplatsInDistance = HasFeature(keywords, DefineFeature._DISABLESPLATSINDISTANCE);
            splatsOnTop = HasFeature(keywords, DefineFeature._SPLATSONTOP);
         }

         if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_ALBEDO))
         {
            debugOutput = DebugOutput.Albedo;
         }
         else if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_HEIGHT))
         {
            debugOutput = DebugOutput.Height;
         }
         else if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_NORMAL))
         {
            debugOutput = DebugOutput.Normal;
         }
         else if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_SMOOTHNESS))
         {
            debugOutput = DebugOutput.Smoothness;
         }
         else if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_METAL))
         {
            debugOutput = DebugOutput.Metallic;
         }
         else if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_AO))
         {
            debugOutput = DebugOutput.AO;
         }
         else if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_EMISSION))
         {
            debugOutput = DebugOutput.Emission;
         }
         else if (HasFeature(keywords, DefineFeature._DEBUG_OUTPUT_SPLATDATA))
         {
            debugOutput = DebugOutput.SplatData;
         }

         customUserFunction = (HasFeature(keywords, DefineFeature._CUSTOMUSERFUNCTION));

      }

      public string[] Pack()
      {
         // do some verification
         if (flowMode == FlowMode.Refractive && layerMode != LayerMode.TwoLayer)
         {
            flowMode = FlowMode.Flow;
         }

         List<string> features = new List<string>();
         if (shaderType == ShaderType.Terrain)
         {
            features.Add(GetFeatureName(DefineFeature._TERRAIN));
            if (terrainTessDampening)
            {
               features.Add(GetFeatureName(DefineFeature._TESSDAMPENING));
            }
         }
         if (manualShaderLevel != ManualShaderLevel.Auto)
         {
            if (manualShaderLevel == ManualShaderLevel.ShaderModel40)
            {
               features.Add(GetFeatureName(DefineFeature._MANUALSHADERLEVEL40));
            }
            else if (manualShaderLevel == ManualShaderLevel.ShaderModel45)
            {
               features.Add(GetFeatureName(DefineFeature._MANUALSHADERLEVEL45));
            }
            else if (manualShaderLevel == ManualShaderLevel.ShaderModel46)
            {
               features.Add(GetFeatureName(DefineFeature._MANUALSHADERLEVEL46));
            }
         }

         if (lightingMode == LightingMode.Ramp)
         {
            features.Add(GetFeatureName(DefineFeature._RAMPLIGHTING));
         }

         if (alphaMode == AlphaMode.Transparent)
         {
            features.Add(GetFeatureName(DefineFeature._ALPHA));
         }
         else if (alphaMode == AlphaMode.Cutout)
         {
            features.Add(DefineFeature._ALPHATEST.ToString());
         }

         if (alphaHole)
         {
            features.Add(DefineFeature._ALPHAHOLE.ToString());
         }

         if (layerMode == LayerMode.TwoLayer)
         {
            features.Add(GetFeatureName(DefineFeature._TWOLAYER));
         }
         else if (layerMode == LayerMode.DetailTexture)
         {
            features.Add(GetFeatureName(DefineFeature._DETAILMAP));
         }
         else if (layerMode == LayerMode.AlphaLayer)
         {
            features.Add(GetFeatureName(DefineFeature._ALPHALAYER));
         }
         if (shaderType == ShaderType.Mesh)
         {
            if (preprocessMesh)
            {
               features.Add(GetFeatureName(DefineFeature._PREPROCESSMESH));
            }
            if (projectTextureMode == TextureChoiceMode.Project3Way)
            {
               if (projectTextureSpace == ProjectSpace.Local)
               {
                  features.Add(GetFeatureName(DefineFeature._PROJECTTEXTURE_LOCAL));
               }
               else
               {
                  features.Add(GetFeatureName(DefineFeature._PROJECTTEXTURE_WORLD));
               }
            }
            if (projectTextureMode2 == TextureChoiceMode.Project3Way && (layerMode == LayerMode.TwoLayer || layerMode == LayerMode.AlphaLayer))
            {
               if (projectTextureSpace2 == ProjectSpace.Local)
               {
                  features.Add(GetFeatureName(DefineFeature._PROJECTTEXTURE2_LOCAL));
               }
               else
               {
                  features.Add(GetFeatureName(DefineFeature._PROJECTTEXTURE2_WORLD));
               }
            }
         }
            
         if (geoTexture)
         {
            features.Add(GetFeatureName(DefineFeature._GEOMAP));
         }

         if (lowPoly)
         {
            features.Add(GetFeatureName(DefineFeature._LOWPOLY));
            if (lowPolyAdjust)
            {
               features.Add(GetFeatureName(DefineFeature._LOWPOLYADJUST));
            }
         }
         if (macroPreset == MacroPreset.None)
         {
            if (useSecondUV)
            {
               features.Add(GetFeatureName(DefineFeature._SECONDUV));
            }
         }

         if (uvMode == UVMode.Triplanar)
         {
            features.Add(GetFeatureName(DefineFeature._TRIPLANAR));
            if (triplanarUV == ProjectSpace.World)
            {
               features.Add(GetFeatureName(DefineFeature._TRIPLANAR_WORLDSPACE));
            }
            disableBottomBlend = false;
         }
         else if (uvMode == UVMode.UV2)
         {
            features.Add(GetFeatureName(DefineFeature._UVFROMSECOND));
         }
         else if (uvMode == UVMode.UVProjectAxis)
         {
            if (projectSpace == ProjectSpace.Local)
            {
               if (projectAxis == ProjectAxis.Front)
               {
                  features.Add(GetFeatureName(DefineFeature._UVLOCALFRONT));
               }
               else if (projectAxis == ProjectAxis.Side)
               {
                  features.Add(GetFeatureName(DefineFeature._UVLOCALSIDE));
               }
               else if (projectAxis == ProjectAxis.Top)
               {
                  features.Add(GetFeatureName(DefineFeature._UVLOCALTOP));
               }
            }
            else
            {
               if (projectAxis == ProjectAxis.Front)
               {
                  features.Add(GetFeatureName(DefineFeature._UVWORLDFRONT));
               }
               else if (projectAxis == ProjectAxis.Side)
               {
                  features.Add(GetFeatureName(DefineFeature._UVWORLDSIDE));
               }
               else if (projectAxis == ProjectAxis.Top)
               {
                  features.Add(GetFeatureName(DefineFeature._UVWORLDTOP));
               }
            }
         }

         if (uvMode == UVMode.UVProjectAxis && projectTangents)
         {
            features.Add(GetFeatureName(DefineFeature._PROJECTTANGENTS));
         }

         if (disableBottomBlend)
         {
            features.Add(GetFeatureName(DefineFeature._NOBLENDBOTTOM));
         }

         if (macroPreset == MacroPreset.None)
         {
            if (useSecondUV && macroUVMode == MacroUVMode.UV2)
            {
               features.Add(GetFeatureName(DefineFeature._UVFROMSECOND2));
            }
            if (useSecondUV && macroUVMode == MacroUVMode.UVProjectAxis)
            {
               if (macroProjectSpace == ProjectSpace.Local)
               {
                  if (macroProjectAxis == ProjectAxis.Front)
                  {
                     features.Add(GetFeatureName(DefineFeature._UVLOCALFRONT2));
                  }
                  else if (macroProjectAxis == ProjectAxis.Side)
                  {
                     features.Add(GetFeatureName(DefineFeature._UVLOCALSIDE2));
                  }
                  else if (macroProjectAxis == ProjectAxis.Top)
                  {
                     features.Add(GetFeatureName(DefineFeature._UVLOCALTOP2));
                  }
               }
               else
               {
                  if (macroProjectAxis == ProjectAxis.Front)
                  {
                     features.Add(GetFeatureName(DefineFeature._UVWORLDFRONT2));
                  }
                  else if (macroProjectAxis == ProjectAxis.Side)
                  {
                     features.Add(GetFeatureName(DefineFeature._UVWORLDSIDE2));
                  }
                  else if (macroProjectAxis == ProjectAxis.Top)
                  {
                     features.Add(GetFeatureName(DefineFeature._UVWORLDTOP2));
                  }
               }
            }
         }

         if (macroPreset != MacroPreset.None)
         {
            if (macroPreset == MacroPreset.FadeToMacro)
            {
               features.Add(GetFeatureName(DefineFeature._MACROPRESETFADE));
               features.Add(GetFeatureName(DefineFeature._DISABLESPLATSINDISTANCE));
               features.Add(GetFeatureName(DefineFeature._USEMACROTEXTURE));
               useMacroTexture = true;
               disableSplatsInDistance = true;
               splatsOnTop = true;
            }
            else if (macroPreset == MacroPreset.TintMap)
            {
               features.Add(GetFeatureName(DefineFeature._MACROPRESETTINT));
               features.Add(GetFeatureName(DefineFeature._USEMACROTEXTURE));
               features.Add(GetFeatureName(DefineFeature._MACROMULT2X));
               useMacroTexture = true;
               disableSplatsInDistance = false;
               macroBlend = PassBlendMode.Multiply2X;
               splatsOnTop = true;
            }
         }
         if (distanceResample)
         {
            features.Add(GetFeatureName(DefineFeature._DISTANCERESAMPLE));
         }

         if (linearBias)
         {
            features.Add(GetFeatureName(DefineFeature._LINEARBIAS));
         }

         if (packingMode == PackingMode.NoSpecTex)
         {
            features.Add(GetFeatureName(DefineFeature._NOSPECTEX));
         }
         else if (packingMode == PackingMode.NoSpecOrNormal)
         {
            features.Add(GetFeatureName(DefineFeature._NOSPECNORMAL));
         }

         if (packingMode != PackingMode.NoSpecTex)
         {
            features.Add("_NORMALMAP"); // some lighting pathways check this..
         }

         if (tesselationMode == TessellationMode.Distance)
         {
            features.Add(GetFeatureName(DefineFeature._TESSDISTANCE));
         }
         else if (tesselationMode == TessellationMode.Edge)
         {
            features.Add(GetFeatureName(DefineFeature._TESSEDGE));
         }
         if (tesselationMode != TessellationMode.None)
         {
            if (tessPhong)
            {
               features.Add(GetFeatureName(DefineFeature._TESSPHONG));
            }

            if (tessellateShadowPass)
            {
               features.Add(GetFeatureName(DefineFeature._TESSSHADOWS));
            }
            if (generateFallback)
            {
               features.Add(GetFeatureName(DefineFeature._TESSFALLBACK));
            }
            if (biasToCenter)
            {
               features.Add(GetFeatureName(DefineFeature._TESSCENTERBIAS));
            }

            if (perTexDisplaceParams)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXDISPLACEPARAMS));
            }
               
         }

         if (puddleMode == PuddleMode.Puddles)
         {
            features.Add(GetFeatureName(DefineFeature._PUDDLES));
         }
         else if (puddleMode == PuddleMode.PuddleFlow)
         {
            features.Add(GetFeatureName(DefineFeature._PUDDLEFLOW));
            if (puddleFoam)
            {
               features.Add(GetFeatureName(DefineFeature._PUDDLEFOAM));
            }
         }
         else if (puddleMode == PuddleMode.PuddleRefraction)
         {
            features.Add(GetFeatureName(DefineFeature._PUDDLEREFRACT));
            if (puddleFoam)
            {
               features.Add(GetFeatureName(DefineFeature._PUDDLEFOAM));
            }

            if (puddleDepthDampen)
            {
               features.Add(GetFeatureName(DefineFeature._PUDDLEDEPTHDAMPEN));
            }
         }
         else if (puddleMode == PuddleMode.Lava)
         {
            features.Add(GetFeatureName(DefineFeature._LAVA));
            if (puddlePushTerrain && tesselationMode != TessellationMode.None)
            {
               features.Add(GetFeatureName(DefineFeature._PUDDLESPUSHTERRAIN));
            }
         }
         if (puddleMode != PuddleMode.None && puddleGlitter)
         {
            features.Add(GetFeatureName(DefineFeature._PUDDLEGLITTER));
         }

         if (puddleGlitter || snowGlitter || perTexGlitter)
         {
            if (glitterMode == GlitterMode.Animated)
            {
               features.Add(GetFeatureName(DefineFeature._GLITTERMOVES));
            }
         }

         if (puddleMode != PuddleMode.None && puddleMode != PuddleMode.Lava)
         {
            if (rainDrops)
            {
               features.Add(GetFeatureName(DefineFeature._RAINDROPS));
               if (rainOnFlatOnly)
               {
                  features.Add(GetFeatureName(DefineFeature._RAINDROPFLATONLY));
               }
            }

            if (puddlePushTerrain && tesselationMode != TessellationMode.None)
            {
               features.Add(GetFeatureName(DefineFeature._PUDDLESPUSHTERRAIN));
            }
         }

         if (wetness)
         {
            features.Add(GetFeatureName(DefineFeature._WETNESS));
         }

         if (snow)
         {
            features.Add(GetFeatureName(DefineFeature._SNOW));
            if (snowOverMacro)
            {
               features.Add(GetFeatureName(DefineFeature._SNOWOVERMACRO));
            }

            if (snowGlitter)
            {
               features.Add(GetFeatureName(DefineFeature._SNOWGLITTER));
            }

            if (snowDistanceNoise)
            {
               features.Add(GetFeatureName(DefineFeature._SNOWDISTANCENOISE));
            }
            if (snowDistanceResample)
            {
               features.Add(GetFeatureName(DefineFeature._SNOWDISTANCERESAMPLE));
            }
         }

         if (emission)
         {
            features.Add(GetFeatureName(DefineFeature._EMISMAP));
         }
            
         if (flowMode == FlowMode.Flow)
         {
            features.Add(GetFeatureName(DefineFeature._FLOW));
         }
         else if (flowMode == FlowMode.Refractive)
         {
            features.Add(GetFeatureName(DefineFeature._FLOWREFRACTION));
         }

         if (perTexContrast)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXCONTRAST));
         }
         if (perTexMatParams)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXMATPARAMS));
         }
         if (perTexUV)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXUV));
         }
         if (perTexNormalStrength)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXNORMALSTRENGTH));
         }
         if (perTexAOStrength)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXAOSTRENGTH));
         }
         if (perTexParallaxStrength && parallax)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXPARALLAXSTRENGTH));
         }

         if (perTexGlitter)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXGLITTER));
         }

         if (useMacroTexture)
         {
            if (macroBlend == PassBlendMode.Multiply)
            {
               features.Add(GetFeatureName(DefineFeature._MACROMULT));
            }
            else if (macroBlend == PassBlendMode.Multiply2X)
            {
               features.Add(GetFeatureName(DefineFeature._MACROMULT2X));
            }
            else if (macroBlend == PassBlendMode.Overlay)
            {
               features.Add(GetFeatureName(DefineFeature._MACROOVERLAY));
            }
            else
            {
               features.Add(GetFeatureName(DefineFeature._MACRONORMAL));
            }
            if (splatsOnTop)
            {
               features.Add(GetFeatureName(DefineFeature._SPLATSONTOP));
            }
            if (disableSplatsInDistance)
            {
               features.Add(GetFeatureName(DefineFeature._DISABLESPLATSINDISTANCE));
            }
         }

         if (layerMode == LayerMode.DetailTexture)
         {
            if (detailBlend == PassBlendMode.Multiply)
            {
               features.Add(GetFeatureName(DefineFeature._DETAILMULT));
            }
            else if (detailBlend == PassBlendMode.Multiply2X)
            {
               features.Add(GetFeatureName(DefineFeature._DETAILMULT2X));
            }
            else if (detailBlend == PassBlendMode.Overlay)
            {
               features.Add(GetFeatureName(DefineFeature._DETAILOVERLAY));
            }
            else
            {
               features.Add(GetFeatureName(DefineFeature._DETAILNORMAL));
            }
         }

         if (layerMode != LayerMode.DetailTexture)
         {
            if (detailNoise)
            {
               features.Add(GetFeatureName(DefineFeature._DETAILNOISE));
            }
            if (perTexNoiseStrength)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXNOISESTRENGTH));
            }
         }

         if (distanceNoise)
         {
            features.Add(GetFeatureName(DefineFeature._DISTANCENOISE));
         }

         if (parallax == true)
         {
            features.Add(GetFeatureName(DefineFeature._PARALLAX));
         }
         if (useMacroTexture)
         {
            features.Add(GetFeatureName(DefineFeature._USEMACROTEXTURE));
            if (macroAOScale)
            {
               features.Add(GetFeatureName(DefineFeature._MACROAOSCALE));
            }
         }
         if (emission)
         {
            features.Add(GetFeatureName(DefineFeature._EMISMAP));
         }

         if (curvedWorld)
         {
            // Make sure curved world is installed...
            string path = Application.dataPath;
            path = path.Replace("\\", "/");
            path += "/VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc";
           
            if (System.IO.File.Exists(path))
            {
               features.Add(GetFeatureName(DefineFeature._USECURVEDWORLD));
            }
         }

         if (debugOutput != DebugOutput.None)
         {
            string parallaxStr = GetFeatureName(DefineFeature._PARALLAX);
            if (features.Contains(parallaxStr))
            {
               features.Remove(parallaxStr);
            }
            // could contribute, but they can always blend this in photoshop..
            string macroStr = GetFeatureName(DefineFeature._USEMACROTEXTURE);
            if (features.Contains(macroStr))
            {
               features.Remove(macroStr);
            }

            if (debugOutput == DebugOutput.Albedo)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_ALBEDO));
            }
            else if (debugOutput == DebugOutput.Height)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_HEIGHT));
            }
            else if (debugOutput == DebugOutput.Normal)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_NORMAL));
            }
            else if (debugOutput == DebugOutput.Metallic)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_METAL));
            }
            else if (debugOutput == DebugOutput.Smoothness)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_SMOOTHNESS));
            }
            else if (debugOutput == DebugOutput.AO)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_AO));
            }
            else if (debugOutput == DebugOutput.Emission)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_EMISSION));
            }
            else if (debugOutput == DebugOutput.SplatData)
            {
               features.Add(GetFeatureName(DefineFeature._DEBUG_OUTPUT_SPLATDATA));
            }
         }
         if (customUserFunction)
         {
            features.Add(GetFeatureName(DefineFeature._CUSTOMUSERFUNCTION));
         }
         return features.ToArray();
      }

      public void Pack(Material m)
      {
         m.shaderKeywords = Pack();
      }

      public void ComputeSampleCounts(out int arraySampleCount, out int textureSampleCount, out int maxSamples, 
         out int tessellationSamples, out int depTexReadLevel)
      {
         depTexReadLevel = 0;
         tessellationSamples = 0;
         // compute performance help..
         int samplesPerArray = ( layerMode != LayerMode.Single) ? 6 : 3;
         if (disableBottomBlend)
         {
            samplesPerArray = (layerMode != LayerMode.Single) ? 4 : 1;
         }
         int numChannels = 2;

         if (packingMode == PackingMode.NoSpecOrNormal)
         {
            numChannels = 1;
         }
         tessellationSamples += samplesPerArray;

         arraySampleCount = samplesPerArray * numChannels;  // 3 textures per channel, 2/3 texures, 1/2 layers
         textureSampleCount = 0;

         arraySampleCount += parallax ? samplesPerArray : 0;
         arraySampleCount += emission ? samplesPerArray : 0;

         if (shaderType == ShaderType.Terrain)
         {
            depTexReadLevel++;
            textureSampleCount += 3;
            if (layerMode != LayerMode.Single)
               textureSampleCount++;
         }

         if (alphaMode != AlphaMode.Opaque)
         {
            arraySampleCount += samplesPerArray;
         }

         if (detailNoise)
         {
            textureSampleCount++;
            if (uvMode == UVMode.Triplanar)
            {
               textureSampleCount += 2;
            }
         }

         if (distanceNoise)
         {
            textureSampleCount++;
            if (uvMode == UVMode.Triplanar)
            {
               textureSampleCount += 2;
            }
         }

         maxSamples = arraySampleCount;
         if (!distanceResample && (flowMode == FlowMode.Refractive || flowMode == FlowMode.Flow))
         {
            maxSamples *= 2;
         }
         if (distanceResample)
         {
            arraySampleCount *= 2;
         }

         if (useMacroTexture)
         {
            textureSampleCount += numChannels;
         }
         bool addDep = false;
         if (flowMode != FlowMode.None)
         {
            addDep = true;
            textureSampleCount += samplesPerArray;
            tessellationSamples += samplesPerArray;
         }
         if (perTexUV || perTexMatParams)
         {
            addDep = true;
            textureSampleCount += samplesPerArray;
            tessellationSamples += samplesPerArray;
         }
         if (perTexDisplaceParams || perTexNoiseStrength || perTexContrast)
         {
            addDep = true;
            textureSampleCount += samplesPerArray;
            tessellationSamples += samplesPerArray;
         }
         if (perTexNormalStrength || perTexParallaxStrength || perTexAOStrength || perTexGlitter)
         {
            addDep = true;
            textureSampleCount += samplesPerArray;
            tessellationSamples += samplesPerArray;
         }

         if (addDep)
         {
            depTexReadLevel++;
            if (tesselationMode != TessellationMode.None)
            {
               depTexReadLevel++;
            }
         }


         if (puddleMode == PuddleMode.PuddleFlow)
         {
            textureSampleCount += 2;
         }
         else if (puddleMode == PuddleMode.PuddleRefraction)
         {
            textureSampleCount += 2;
            // only count diffuse resample if parallax is off, otherwise counted twice..
            arraySampleCount += parallax ? 0 : samplesPerArray;
         }
         else if (puddleMode == PuddleMode.Lava)
         {
            textureSampleCount += 4;
         }

         if (puddleFoam)
         {
            textureSampleCount += 2;
         }
         if (snow)
         {
            textureSampleCount += 2;
            tessellationSamples += 2;
            if (snowDistanceNoise)
            {
               textureSampleCount++;
            }
            if (snowDistanceResample)
            {
               textureSampleCount += 2;
            }
         }

         if (perTexGlitter || snowGlitter || puddleGlitter)
         {
            textureSampleCount += 2;
            if (glitterMode == GlitterMode.Animated)
            {
               textureSampleCount += 1;
            }
         }

         if (rainDrops)
         {
            textureSampleCount += 4;
         }


         if (tesselationMode == TessellationMode.None)
         {
            tessellationSamples = 0;
            if (perTexContrast)
               textureSampleCount++;
         }
      }
   }
}
