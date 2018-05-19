//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using JBooth.MegaSplat;


// Not only a UI, but generates textures for shader parameters as well, because Unity only has minimal support
// for arrays in shaders (no material properties thus no save or load or get)
public partial class SplatArrayShaderGUI : ShaderGUI 
{
   int textureIndex;          // currently editing this sub-texture
   Texture2D displayTex;      // preview display

   bool IsLinear(TextureImporter ti)
   {
      return ti.sRGBTexture == false;
   }

   void SetLinear(TextureImporter ti, bool val)
   {
      ti.sRGBTexture = !val;
   }
      
   Texture2D NewPropTex()
   {
      // pixel layout for per tex properties
      // metal/smooth/porosity/uv scale
      // flow speed, intensity, alpha, refraction
      // detailNoiseStrength, contrast, displacementAmount, displaceUpBias
      // tint
      var tex = new Texture2D(256, 4, TextureFormat.ARGB32, false, true);
      Color c0 = new Color(0, 0, 0.4f, 0.0f);
      Color c1 = new Color(0, 0, 1, 0);
      Color c2 = new Color(1, 1, 1, 0);
      Color c3 = new Color(1, 1, 1, 1);

      for (int i = 0; i < 256; ++i)
      {
         tex.SetPixel(i, 0, c0);
         tex.SetPixel(i, 1, c1);
         tex.SetPixel(i, 2, c2);
         tex.SetPixel(i, 3, c3);
      }
      tex.Apply();
      return tex;
   }

   // makes sure the texture import settings are correct..
   void FixPropTexFlags(Texture2D propTex, Material targetMat)
   {
      var ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(propTex));

      var ti = ai as TextureImporter;

      bool formatCorrect = ti.textureCompression == TextureImporterCompression.Uncompressed;

      if (ti.isReadable == false || 
         ti.filterMode != FilterMode.Point ||
         ti.wrapMode != TextureWrapMode.Clamp ||
         !IsLinear(ti) ||
         !formatCorrect)
      {
         ti.textureCompression = TextureImporterCompression.Uncompressed;

         SetLinear(ti, true);
         ti.filterMode = FilterMode.Point;
         ti.mipmapEnabled = false;
         ti.wrapMode = TextureWrapMode.Clamp;
         ti.isReadable = true;
         ti.SaveAndReimport();
         targetMat.SetTexture("_PropertyTex", propTex);
         EditorUtility.SetDirty(targetMat);
      }
      if (propTex.height < 4) // revision to 3 pixels
      {

         var tex = NewPropTex();
         string path = AssetDatabase.GetAssetPath(propTex);
         string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
         fullPath += path;
         var bytes = tex.EncodeToPNG();
         System.IO.File.WriteAllBytes(fullPath, bytes);
         propTex = tex;
      }
   }

   // get, load, or create the property texture for this material.. We don't clean up after ourselves when it's
   // removed, but this allows it to restore properties when you flip the option off and on..
   Texture2D GetPropertyTex(Material targetMat)
   {
      // on the material?
      Texture2D propTex = targetMat.GetTexture("_PropertyTex") as Texture2D;

      if (propTex != null) 
      {
         FixPropTexFlags(propTex, targetMat);
      }
      else
      {
         // look for it next to the material?
         var path = AssetDatabase.GetAssetPath(targetMat);
         path.Replace("\\", "/");
         if (!string.IsNullOrEmpty(path))
         {
            path = path.Substring(0, path.IndexOf("."));
            path += "_properties.png";
            propTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (propTex != null)
            {
               FixPropTexFlags(propTex, targetMat);
            }
            else
            {
               // create a new one..
               propTex = NewPropTex();
               var bytes = propTex.EncodeToPNG();
               string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
               fullPath += path;
               System.IO.File.WriteAllBytes(fullPath, bytes);
               GameObject.DestroyImmediate(propTex);
               AssetDatabase.Refresh();
               propTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
               FixPropTexFlags(propTex, targetMat);
            }
         }
      }

      targetMat.SetTexture("_PropertyTex", propTex);

      return propTex;
   }

   GUIContent CPerTexScaleRange = new GUIContent("Scale Range", "When setting the per-texture scale, this will specify the minimum and maximum to scale between");
   GUIContent CPerTexFlowAlpha = new GUIContent("Per Tex Flow Alpha", "How much of the first layers shows through this texture when using flow mapping");
   GUIContent CPerTexFlowRefraction = new GUIContent("Per Tex Flow Refraction", "How much the normal of the texture distorts the texture underneith it");
   GUIContent CPerTexMetal = new GUIContent("Metallic", "Metallic value to use if no per-pixel texture is provided");
   GUIContent CPerTexSmooth = new GUIContent("Smoothness", "Smoothness value to use if no per-pixel texture is provided");
   GUIContent CPerTexScale = new GUIContent("UV Scale", "UV Scale, between Scale Range value, of this texture");
   GUIContent CPerTexFlowSpeed = new GUIContent("Per Tex Flow Speed", "Speed in which to flow this surface");
   GUIContent CPerTexDisplacementScale = new GUIContent("Displacement Scale", "Scale this textures total displacement");
   GUIContent CPerTexPorosity = new GUIContent("Porosity", "Controls how dark a texture's albedo becomes when wet");
   GUIContent CPerTexUpBias = new GUIContent("Up Bias", "At 0, displacement happens along normal. At 1, displacement always goes up");
   GUIContent CPerTexNormalStrength = new GUIContent("Normal Strength", "How much to apply normal mapping");
   GUIContent CPerTexParallaxStrength = new GUIContent("Parallax Strength", "How much parallax to apply");
   GUIContent CPerTexAOStrength = new GUIContent("AO Strength", "How much ambient occlusion to apply");
   GUIContent CPerTexGlitterStrength = new GUIContent("Glitter Reflectivity", "How much glitter to apply");
   GUIContent CMacroNormal = new GUIContent("Macro Normal", "Normal Map for Macro Texture");
   GUIContent CNormal = new GUIContent("Normal", "Texture Array with Normal Maps");
   GUIContent CDetailNormal = new GUIContent("Detail Normal", "Texture Array for Detail Texture Normals");

   GUIContent CMacroDiffuse = new GUIContent("Macro Diffuse", "Diffuse texture to cover your entire model with");

   GUIContent CFlowSpeed = new GUIContent("Per Tex Flow Speed", "Global setting for maximum flow speed");
   GUIContent CFlowAlpha = new GUIContent("Per Tex Flow Alpha", "Global setting for maximum flow alpha");
   GUIContent CFlowIntensity = new GUIContent("Per Tex Flow Intensity", "Global setting for how far flow appart the two flow UVs can be");
   GUIContent CFlowRefraction = new GUIContent("Per Tex Flow Refraction", "Global setting for maximum distance a flow can distort a texture below it");

   GUIContent CMacroTextureScale = new GUIContent("Macro Texture Scale", "UV scale for macro texture");
   GUIContent CMacroFade = new GUIContent("Fade", "Turns on distance fading for macro texturing");
   GUIContent CMacroFadeBegin = new GUIContent("Begin", "Distance at which to start fading macro texture");
   GUIContent CMacroFadeEnd = new GUIContent("End", "Distance at which macro texture is completely faded");
   GUIContent CMacroStrength = new GUIContent("Macro Strength", "How much the macro texture is blended in");
   GUIContent CInterpContrast = new GUIContent("Interpolation Contrast", "How quickly splat maps blend into each other");
   GUIContent CTexScale = new GUIContent("Texture Scale", "Texture Scale for bottom splat layer");
   GUIContent CTexScale2 = new GUIContent("Second Texture Scale", "Texture Scale top splat layer");
   GUIContent CParallaxHeight = new GUIContent("Parallax Height", "How much parallax effect to apply");
   GUIContent CParallaxFade = new GUIContent("Parallax Fade", "Distance at which parallax effect begins to fade, and distance over which it fades");
   GUIContent CTessDisplacement = new GUIContent("Displacement", "How far to displace the surface from it's original position");
   GUIContent CTessMipBias = new GUIContent("Mip Bias", "Allows you to use lower mip map levels for displacement, which often produces a better looking result and is slightly faster");
   GUIContent CTessShaping = new GUIContent("Shaping", "A seperate contrast blend for tessellation, lower values tend to look best");
   GUIContent CTessMinDistance = new GUIContent("Min Distance", "Distance in which distance based tessellation is at maximum. Also acts as the distance in which tessellation amount begins to fade when fade tessellation is on");
   GUIContent CTessMaxDistance = new GUIContent("Max Distance", "Distance in which distance based tessellation is at minimum. Also acts as the distance in which tessellation amount begins is completely faded when fade tessellation is on");
   GUIContent CTessEdgeLength = new GUIContent("Edge Length", "Size of edge in screen space in which edge will begin tessellating. Higher values are more performant");
   GUIContent CTessTessellation = new GUIContent("Tessellation", "How much to tesselate the mesh at min distance, lower values are more performant");
   GUIContent CTessUpBias = new GUIContent("Up Bias", "Global Up Bias when per-tex properties are not used, when 0, displacement happens along the normal, when 1, displacement goes up");
   GUIContent CTerrainSplatControl = new GUIContent("Splat Control", "Control texture to store splat map information in");
   GUIContent CTerrainSplatParams = new GUIContent("Splat Params", "Control texture to store flow directions, displacement dampening, and puddles");

   GUIContent CDetailNoise = new GUIContent("Noise", "A mostly greyscale linear texture with the center around 0.5");
   GUIContent CDistanceNoise = new GUIContent("Noise", "A mostly greyscale linear texture with the center around 0.5");
   GUIContent CProjectOffset = new GUIContent("UV Offset", "UV offset");
   GUIContent CProjectScale = new GUIContent("UV Scale", "UV scale");
   GUIContent CProjectOffset2 = new GUIContent("UV Offset 2", "UV offset");
   GUIContent CProjectScale2 = new GUIContent("UV Scale 2", "UV scale");
   GUIContent CPuddleDepthCalm = new GUIContent("Refraction Depth Calm", "Calms refraction in areas of the water which are deeper");

   GUIContent CRainDropTexture = new GUIContent("Rain Drop", "Special Texture for rain drops- contains (R) distance inversed, (G/B) normal, (A) phase");

   GUIContent CLowPolyHardness = new GUIContent("Hardness", "how hard to make the low poly edges");

   GUIContent CLavaDiffuse = new GUIContent("Lava Texture", "Normal (RG), Height (B), Hardening(A)");
   GUIContent CLavaBlend = new GUIContent("Blend Width", "Blend border for lava");
   GUIContent CLavaMax = new GUIContent("Max", "Maximum amount of lava allowed.");
   GUIContent CLavaSpeed = new GUIContent("Speed", "Speed of lava flow");
   GUIContent CLavaIntensity = new GUIContent("Intensity", "Intensity of Lava flow");
   GUIContent CLavaDistSize = new GUIContent("Distortion Size", "Size of distortion waves");
   GUIContent CLavaDistRate = new GUIContent("Distortion Rate", "Rate of distortion motion");
   GUIContent CLavaDistScale = new GUIContent("Distortion Scale", "UV scale of distortion");
   GUIContent CLavaColor = new GUIContent("Color", "Central color for lava");
   GUIContent CLavaEdgeColor = new GUIContent("Edge Color", "Color for glow around edges");
   GUIContent CLavaHighlightColor = new GUIContent("Highlight Color", "Color for highlights on lava");
   GUIContent CLavaUVScale = new GUIContent("UV Scale", "Scale of lava texture");
   GUIContent CLavaDarkening = new GUIContent("Drying", "Controls amount of drying lava to appear in flow");

   GUIContent CGlitterUV = new GUIContent("UV Scale", "UV scale for glitter texture");
   GUIContent CGlitterTex = new GUIContent("Noise Texture", "Noise Texture for glitter effect, glitter in R, flutter in B");
   GUIContent CGlitterFlutterSpeed = new GUIContent("Flutter Speed", "Rate at which flutter texture is scrolled");
   GUIContent CGlitterFlutterScale = new GUIContent("Flutter Scale", "Scale for flutter texture");
   GUIContent CGlitterSnowShine = new GUIContent("Snow Shininess", "Glitter Shine");
   GUIContent CGlitterSnowReflect = new GUIContent("Snow Reflectivity", "Glitter reflectivity");
   GUIContent CGlitterPuddleShine = new GUIContent("Puddle Shininess", "Glitter Shine");
   GUIContent CGlitterPuddleReflect = new GUIContent("Puddle Reflectivity", "Glitter reflectivity");
  

   enum Channel
   {
      R = 0,
      G,
      B,
      A
   }

   bool SliderWithReset(GUIContent label, ref float value, Texture2D tex, int pixel, Channel channel, float min = 0, float max = 1.0f)
   {
      EditorGUILayout.BeginHorizontal();
      value = EditorGUILayout.Slider(label, value, min, max);
      bool reset = (GUILayout.Button("All", GUILayout.Width(40)));
      EditorGUILayout.EndHorizontal();

      if (reset)
      {
         for (int i = 0; i < 256; ++i)
         {
            Color c = tex.GetPixel(i, pixel);
            if (channel == Channel.R)
            {
               c.r = value;
            }
            else if (channel == Channel.G)
            {
               c.g = value;
            }
            else if (channel == Channel.B)
            {
               c.b = value;
            }
            else if (channel == Channel.A)
            {
               c.a = value;
            }
            tex.SetPixel(i, pixel, c);
         }
         tex.Apply();
      }
      
      return reset;
   }


   void DrawTextureEditor(MaterialEditor materialEditor, MaterialProperty[] props, Material targetMat, FeatureData fData)
   {
      // pixel layout for per tex properties
      // metal/smooth/porosity/uv scale
      // flow speed, intensity, alpha, refraction
      // detailNoiseStrength, contrast, displacementAmount, displaceUpBias
      // normal strength, parallax strength, ao strength

      var albedoMap = FindProperty("_Diffuse", props);
      Texture2DArray ta = albedoMap.textureValue as Texture2DArray;

      if (ta == null || ta.depth == 0)
         return;

      if (MegaSplatUtilities.DrawRollup("Texture Settings Editor"))
      {
         var scaleVecProp = FindProperty("_PerTexScaleRange", props);
         Vector2 scaleVec = new Vector2(scaleVecProp.vectorValue.x, scaleVecProp.vectorValue.y);
         scaleVec = EditorGUILayout.Vector2Field(CPerTexScaleRange, scaleVec);
         scaleVecProp.vectorValue = new Vector4(scaleVec.x, scaleVec.y, 0, 0);

         // init/find propTex
         Texture2D propTex = GetPropertyTex(targetMat);

         textureIndex = MegaSplatUtilities.DrawTextureSelector(textureIndex, ta);

         if (propTex != null)
         {
            Color c0 = propTex.GetPixel(textureIndex, 0);
            Color c1 = propTex.GetPixel(textureIndex, 1);
            Color c2 = propTex.GetPixel(textureIndex, 2);
            Color c3 = propTex.GetPixel(textureIndex, 3);

            // pixel layout for per tex properties
            // metal/smooth/porosity/uv scale
            // flow speed, intensity, alpha, refraction
            // detailNoiseStrength, contrast, displacementAmount, displaceUpBias
            // normal map strength, parallax strength, ao strength, glitter strength
            float metal =  c0.r;
            float smoothness = c0.g;
            float porosity = c0.b;
            float uvScale = c0.a;

            float flowSpeed = c1.r;
            float flowIntensity = c1.g;
            float flowAlpha = c1.b;
            float flowRefract = c1.a;

            float detailNoiseStrength = c2.r;
            float contrast = c2.g;
            float tessAmount = c2.b;
            float tessUpBias = c2.a;

            float normalStr = c3.r;
            float parallaxStr = c3.g;
            float aoStr = c3.b;
            float glitterStr = c3.a;

            EditorGUI.BeginChangeCheck();

            bool changed = false;

            if (fData.flowMode != FlowMode.None)
            {
               changed = changed || SliderWithReset(CPerTexFlowSpeed, ref flowSpeed, propTex, 1, Channel.R, 0.0f, 1.0f);
               changed = changed || SliderWithReset(CPerTexFlowAlpha, ref flowAlpha, propTex, 1, Channel.B, 0.0f, 1.0f);
               changed = changed || SliderWithReset(CPerTexFlowRefraction, ref flowRefract, propTex, 1, Channel.A, 0.0f, 1.0f);
            }

            if (fData.perTexUV)
            {
               changed = changed || SliderWithReset(CPerTexScale, ref uvScale, propTex, 0, Channel.A, 0.0f, 1.0f);
            }
            if (fData.perTexContrast)
            {
               float min = 2.0f / 255.0f;
               float max = 253.0f / 255.0f;
               changed = changed || SliderWithReset(new GUIContent("Blend Contrast"), ref contrast, propTex, 2, Channel.G, min, max);
            }
            if (fData.perTexMatParams)
            {
               if (!fData.emission)
               {
                  changed = changed || SliderWithReset(CPerTexMetal, ref metal, propTex, 0, Channel.R);
               }
               if (fData.packingMode == PackingMode.NoSpecTex || fData.packingMode == PackingMode.NoSpecOrNormal)
               {
                  changed = changed || SliderWithReset(CPerTexSmooth, ref smoothness, propTex, 0, Channel.G);
               }
               changed = changed || SliderWithReset(CPerTexPorosity, ref porosity, propTex, 0, Channel.B);
            }
            if (fData.perTexNoiseStrength)
            {
               changed = changed || SliderWithReset(new GUIContent("Per Tex Noise Strength"), ref detailNoiseStrength, propTex, 2, Channel.R);
            }
            if (fData.perTexDisplaceParams)
            {
               changed = changed || SliderWithReset(CPerTexDisplacementScale, ref tessAmount, propTex, 2, Channel.B);
               changed = changed || SliderWithReset(CPerTexUpBias, ref tessUpBias, propTex, 2, Channel.A);
            }
            if (fData.perTexNormalStrength)
            {
               changed = changed || SliderWithReset(CPerTexNormalStrength, ref normalStr, propTex, 3, Channel.R);
            }
            if (fData.perTexParallaxStrength)
            {
               changed = changed || SliderWithReset(CPerTexParallaxStrength, ref parallaxStr, propTex, 3, Channel.G);
            }
            if (fData.perTexAOStrength)
            {
               changed = changed || SliderWithReset(CPerTexAOStrength, ref aoStr, propTex, 3, Channel.B);
            }
            if (fData.perTexGlitter)
            {
               changed = changed || SliderWithReset(CPerTexGlitterStrength, ref glitterStr, propTex, 3, Channel.A);
            }

            if (changed || EditorGUI.EndChangeCheck())
            {
               c0.r = metal;
               c0.g = smoothness;
               c0.b = porosity;
               c0.a = uvScale;
               c1.r = flowSpeed;
               c1.g = flowIntensity;
               c1.b = flowAlpha;
               c1.a = flowRefract;
               c2.r = detailNoiseStrength;
               c2.g = contrast;
               c2.b = tessAmount;
               c2.a = tessUpBias;
               c3.r = normalStr;
               c3.g = parallaxStr;
               c3.b = aoStr;
               c3.a = glitterStr;

               propTex.SetPixel(textureIndex, 0, c0);
               propTex.SetPixel(textureIndex, 1, c1);
               propTex.SetPixel(textureIndex, 2, c2);
               propTex.SetPixel(textureIndex, 3, c3);

               propTex.Apply();
               var bytes = propTex.EncodeToPNG();
               string path = AssetDatabase.GetAssetPath(propTex);
               path = path.Replace("\\", "/");
               path = path.Substring(7); // strip assets
               path = Application.dataPath + "/" + path;
               System.IO.File.WriteAllBytes(path, bytes);
               //AssetDatabase.Refresh(); // no need to refresh, we've written to memory and saved the file, so that's enough..
            }
         }
      }
   }




   private Texture TexturePropertyBody(Rect position, MaterialProperty prop)
   {
      bool enabled = GUI.enabled;
      EditorGUI.BeginChangeCheck();
      if ((prop.flags & MaterialProperty.PropFlags.PerRendererData) != MaterialProperty.PropFlags.None)
      {
         GUI.enabled = false;
      }
      EditorGUI.showMixedValue = prop.hasMixedValue;
      Texture textureValue = EditorGUI.ObjectField(position, prop.textureValue, typeof(Texture), false) as Texture;
      EditorGUI.showMixedValue = false;
      if (EditorGUI.EndChangeCheck())
      {
         prop.textureValue = textureValue;
      }
      GUI.enabled = enabled;
      return prop.textureValue;
   }

   public Texture TexturePropertyMiniThumbnail(MaterialEditor editor, Rect position, MaterialProperty prop, string label, string tooltip)
   {
      //editor.BeginAnimatedCheck(prop);
      Rect position2;
      Rect labelPosition;
      GetRectsForMiniThumbnailField(position, out position2, out labelPosition);
      EditorGUI.HandlePrefixLabel(position, labelPosition, new GUIContent(label, tooltip), 0, EditorStyles.label);
      //editor.EndAnimatedCheck();
      Texture result = TexturePropertyBody(position2, prop);
      Rect rect = position;
      rect.y += position.height;
      rect.height = 27f;
      return result;
   }

   internal static void GetRectsForMiniThumbnailField(Rect position, out Rect thumbRect, out Rect labelRect)
   {
      thumbRect = EditorGUI.IndentedRect(position);
      thumbRect.y -= 1f;
      thumbRect.height = 18f;
      thumbRect.width = 32f;
      float num = thumbRect.x + 30f;
      labelRect = new Rect(num, position.y, thumbRect.x + EditorGUIUtility.labelWidth - num, position.height);
   }

   public Rect TexturePropertySingleLine(MaterialEditor editor, GUIContent label, MaterialProperty textureProp)
   {
      Rect controlRectForSingleLine = EditorGUILayout.GetControlRect(true, 18f, EditorStyles.layerMaskField, new GUILayoutOption[0]);

      TexturePropertyMiniThumbnail(editor, controlRectForSingleLine, textureProp, label.text, label.tooltip);

      return controlRectForSingleLine;
   }

   FeatureData fData = new FeatureData();

   System.Text.StringBuilder builder = new System.Text.StringBuilder(1024);
   public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
   {
      EditorGUI.BeginChangeCheck();
      Material targetMat = materialEditor.target as Material;

      fData.Unpack(targetMat.shaderKeywords);

      var albedoMap = FindProperty("_Diffuse", props);
      var normalMap = FindProperty("_Normal", props);
      var glossDefault = FindProperty("_Glossiness", props);
      var metallicDefault = FindProperty("_Metallic", props);


      Vector4 distanceFades = Vector4.zero;
      Vector4 actualDistances = Vector4.zero;
      bool useMacroFade = false;
      int kLargeValue = 9999999;
      int kLessThanValue = 999900;

      distanceFades = FindProperty("_DistanceFadesCached", props).vectorValue;
      actualDistances = FindProperty("_DistanceFades", props).vectorValue;

      useMacroFade = (actualDistances.x < kLessThanValue || actualDistances.y < kLessThanValue);

      // convert from length format to save processing in the shader, but still display nicely

      distanceFades.z = Mathf.Sqrt(distanceFades.z);
      distanceFades.w = Mathf.Sqrt(distanceFades.w);
      actualDistances.z = Mathf.Sqrt(actualDistances.z);
      actualDistances.w = Mathf.Sqrt(actualDistances.w);


      string shaderName = targetMat.shader.name;
      bool needsCompile = fData.DrawGUI(targetMat, ref shaderName);

      var macroNormalLabel = CMacroNormal;
      var normalLabel = CNormal;
      var detailNormalLabel = CDetailNormal;


      EditorGUI.BeginChangeCheck();

      if (fData.shaderType == ShaderType.Terrain)
      {
         if (MegaSplatUtilities.DrawRollup("Terrain Data", false))
         {
            var controlTexture = FindProperty("_SplatControl", props);
            TexturePropertySingleLine(materialEditor, CTerrainSplatControl, controlTexture);

            if (targetMat.HasProperty("_SplatParams"))
            {
               var paramTexture = FindProperty("_SplatParams", props);
               TexturePropertySingleLine(materialEditor, CTerrainSplatParams, paramTexture);
            }
         }
      }

      if (fData.lightingMode != LightingMode.StandardPBR)
      {
         if (MegaSplatUtilities.DrawRollup("Lighting"))
         {
            if (targetMat.HasProperty("_Ramp"))
            {
               TexturePropertySingleLine(materialEditor, new GUIContent("Ramp Texture"), FindProperty("_Ramp", props));
            }
         }
      }

      if (fData.geoTexture && targetMat.HasProperty("_GeoTex"))
      {
         if (MegaSplatUtilities.DrawRollup("Geo Texture"))
         {
            TexturePropertySingleLine(materialEditor, new GUIContent("GeoTexture", "Virtical striping texture for terrain"), FindProperty("_GeoTex", props));
            Vector4 parms = targetMat.GetVector("_GeoParams");
            EditorGUI.BeginChangeCheck();
            parms.x = EditorGUILayout.Slider("Blend", parms.x, 0, 1);
            parms.y = EditorGUILayout.FloatField("World Scale", parms.y);
            parms.z = EditorGUILayout.FloatField("World Offset", parms.z);
            if (EditorGUI.EndChangeCheck())
            {
               targetMat.SetVector("_GeoParams", parms);
               EditorUtility.SetDirty(targetMat);
            }
         }
      }

      if (fData.useMacroTexture && targetMat.HasProperty("_MacroDiff"))
      {
         var macroDiffuse = FindProperty("_MacroDiff", props);
         var macroNormal = FindProperty("_MacroBump", props);

         if (MegaSplatUtilities.DrawRollup("Macro Texture"))
         {
            TexturePropertySingleLine(materialEditor, CMacroDiffuse, macroDiffuse);
            if (fData.packingMode != PackingMode.NoSpecOrNormal)
            {
               TexturePropertySingleLine(materialEditor, macroNormalLabel, macroNormal);
            }

            Vector4 macroTexScale = FindProperty("_MacroTexScale", props).vectorValue;
            bool needUpdate = false;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(CMacroTextureScale);
            macroTexScale.x = EditorGUILayout.FloatField(macroTexScale.x);
            macroTexScale.y = EditorGUILayout.FloatField(macroTexScale.y);
            if (macroTexScale.x == 0)
            {
               macroTexScale.x = 1;
            }
            if (macroTexScale.y == 0)
            {
               macroTexScale.y = 1;
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck() || needUpdate)
            {
               FindProperty("_MacroTexScale", props).vectorValue = macroTexScale;
               EditorUtility.SetDirty(targetMat);
            }
               
            if (fData.alphaMode != AlphaMode.Opaque)
            {
               TexturePropertySingleLine(materialEditor, new GUIContent("Alpha Texture (R)"), FindProperty("_MacroAlpha", props)); 
            }

            if (fData.disableSplatsInDistance == false)
            {
               materialEditor.ShaderProperty(FindProperty("_MacroTextureStrength", props), CMacroStrength);
            }
            else
            {
               if (targetMat.GetFloat("_MacroTextureStrength") != 1)
               {
                  targetMat.SetFloat("_MacroTextureStrength", 1.0f);
               }
            }

            useMacroFade = EditorGUILayout.Toggle(CMacroFade, useMacroFade);
            if (!useMacroFade)
            {
               GUI.enabled = false;
            }
            distanceFades.x = EditorGUILayout.FloatField(CMacroFadeBegin, distanceFades.x);
            distanceFades.y = EditorGUILayout.FloatField(CMacroFadeEnd, distanceFades.y);
            GUI.enabled = true;

            if ((fData.macroUVMode == MacroUVMode.UVProjectAxis) && targetMat.HasProperty("_UVProjectOffsetScale2"))
            {
               EditorGUI.BeginChangeCheck();
               Vector4 vec = targetMat.GetVector("_UVProjectOffsetScale2");
               Vector2 xy = EditorGUILayout.Vector2Field(CProjectOffset2, new Vector2(vec.x, vec.y));
               Vector2 zw = EditorGUILayout.Vector2Field(CProjectScale2, new Vector2(vec.z, vec.w));
               if (EditorGUI.EndChangeCheck())
               {
                  vec.x = xy.x;
                  vec.y = xy.y;
                  vec.z = zw.x;
                  vec.w = zw.y;
                  targetMat.SetVector("_UVProjectOffsetScale2", vec);
                  EditorUtility.SetDirty(targetMat);
               }
            }
            if (targetMat.HasProperty("_MacroTexNormAOScales"))
            {
               Vector4 nas = FindProperty("_MacroTexNormAOScales", props).vectorValue;
               EditorGUI.BeginChangeCheck();
               nas.x = EditorGUILayout.Slider("Normal Strength", nas.x, 0.0f, 1.0f);

               if (fData.macroAOScale)
               {
                  nas.y = EditorGUILayout.Slider("AO Strength", nas.y, 0.0f, 1.0f);
               }

               if (EditorGUI.EndChangeCheck())
               {
                  FindProperty("_MacroTexNormAOScales", props).vectorValue = nas;
               }
            }
         }
      }


      if (MegaSplatUtilities.DrawRollup("Splats"))
      {
         if (fData.alphaMode != AlphaMode.Opaque)
         {
            if (targetMat.HasProperty("_AlphaArray"))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent("Alpha Array"), FindProperty("_AlphaArray", props));
            }
         }
         materialEditor.TexturePropertySingleLine(new GUIContent("Albedo/Height Array"), albedoMap);
         if (fData.packingMode != PackingMode.NoSpecOrNormal)
         {
            materialEditor.TexturePropertySingleLine(normalLabel, normalMap);
         }
            
         if (fData.emission && targetMat.HasProperty("_Emissive"))
         {
            materialEditor.TexturePropertySingleLine(new GUIContent("Emissive Array"), FindProperty("_Emissive", props));
         }

         if (fData.uvMode == UVMode.Triplanar && targetMat.HasProperty("_TriplanarContrast"))
         {
            materialEditor.ShaderProperty(FindProperty("_TriplanarContrast", props), "Triplanar Contrast");
            if (targetMat.HasProperty("_TriplanarOffset"))
            {
               
               Vector3 v3 = FindProperty("_TriplanarOffset", props).vectorValue;
               Vector3 v = EditorGUILayout.Vector3Field("UV offset", v3);
               if (v != v3)
               {
                  FindProperty("_TriplanarOffset", props).vectorValue = v;
                  EditorUtility.SetDirty(targetMat);
               }
            }
         }
         if ((fData.uvMode == UVMode.UVProjectAxis) && targetMat.HasProperty("_UVProjectOffsetScale"))
         {
            EditorGUI.BeginChangeCheck();
            Vector4 vec = targetMat.GetVector("_UVProjectOffsetScale");
            Vector2 xy = EditorGUILayout.Vector2Field(CProjectOffset, new Vector2(vec.x, vec.y));
            Vector2 zw = EditorGUILayout.Vector2Field(CProjectScale, new Vector2(vec.z, vec.w));
            if (EditorGUI.EndChangeCheck())
            {
               vec.x = xy.x;
               vec.y = xy.y;
               vec.z = zw.x;
               vec.w = zw.y;
               targetMat.SetVector("_UVProjectOffsetScale", vec);
               EditorUtility.SetDirty(targetMat);
            }
         }

         if (fData.uvMode == UVMode.Triplanar && targetMat.HasProperty("_TriplanarTexScale"))
         {
            materialEditor.ShaderProperty(FindProperty("_TriplanarTexScale", props), "Triplanar UV Scale");
         }

         if (fData.distanceResample && targetMat.HasProperty("_ResampleDistanceParams"))
         {
            EditorGUI.BeginChangeCheck();
            Vector4 vec = targetMat.GetVector("_ResampleDistanceParams");
            vec.x = EditorGUILayout.FloatField("Resample UV Scale", vec.x);

            Vector2 xy = EditorGUILayout.Vector2Field("Resample Begin/End", new Vector2(vec.y, vec.z));
            if (EditorGUI.EndChangeCheck())
            {
               vec.y = xy.x;
               vec.z = xy.y;
               targetMat.SetVector("_ResampleDistanceParams", vec);
               EditorUtility.SetDirty(targetMat);
            }
         }

         if (fData.packingMode == PackingMode.NoSpecTex || fData.packingMode == PackingMode.NoSpecOrNormal)
         {
            materialEditor.ShaderProperty(glossDefault, "Smoothness");
            materialEditor.ShaderProperty(metallicDefault, "Metallic");
         }
         else if (!fData.emission && !fData.perTexMatParams)
         {
            materialEditor.ShaderProperty(metallicDefault, "Metallic");
         }
         var contrastProp = FindProperty("_Contrast", props);
         if (fData.shaderType == ShaderType.Terrain)
         {
            // allow for overdrive of contrast on terrain
            contrastProp.floatValue = EditorGUILayout.Slider(CInterpContrast, contrastProp.floatValue, 1.5f, 0.001f);
         }
         else
         {
            contrastProp.floatValue = EditorGUILayout.Slider(CInterpContrast, contrastProp.floatValue, 1, 0.001f);
         }

         Vector4 textureScales = targetMat.GetVector("_TexScales");

         EditorGUI.BeginChangeCheck();
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.PrefixLabel(CTexScale);
         textureScales.x = EditorGUILayout.FloatField(textureScales.x);
         textureScales.y = EditorGUILayout.FloatField(textureScales.y);
         EditorGUILayout.EndHorizontal();

         if (EditorGUI.EndChangeCheck())
         {
            targetMat.SetVector("_TexScales", textureScales);
            EditorUtility.SetDirty(targetMat);
         }

         if (fData.layerMode == LayerMode.TwoLayer)
         {

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(CTexScale2);

            textureScales.z = EditorGUILayout.FloatField(textureScales.z);
            textureScales.w = EditorGUILayout.FloatField(textureScales.w);
         
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
               targetMat.SetVector("_TexScales", textureScales);
               EditorUtility.SetDirty(targetMat);
            }

         }
         if (fData.parallax)
         {
            var parallaxProp = FindProperty("_Parallax", props);
            Vector4 parallaxVars = parallaxProp.vectorValue;
            EditorGUI.BeginChangeCheck();
            parallaxVars.x = EditorGUILayout.Slider(CParallaxHeight, parallaxVars.x, 0.0f, 0.12f);
            parallaxVars.y = EditorGUILayout.FloatField(CParallaxFade, parallaxVars.y);

            if (parallaxVars.y < 1)
               parallaxVars.y = 1;
            if (EditorGUI.EndChangeCheck())
            {
               targetMat.SetVector("_Parallax", parallaxVars);
               EditorUtility.SetDirty(targetMat);
            }
         }

         if (fData.tesselationMode != TessellationMode.None)
         {
            if (targetMat.HasProperty("_TessData1"))
            {
               if (MegaSplatUtilities.DrawRollup("Tessellation"))
               {
                  var td1 = FindProperty("_TessData1", props);
                  var td2 = FindProperty("_TessData2", props);
                  EditorGUI.BeginChangeCheck();
                  var td1v = td1.vectorValue;
                  var td2v = td2.vectorValue;
                  td1v.y = EditorGUILayout.Slider(CTessDisplacement, td1v.y, 0, 3);
                  td1v.z = (float)EditorGUILayout.IntSlider(CTessMipBias, (int)td1v.z, 0, 6);
                  td2v.z = 1.0f - (float)EditorGUILayout.Slider(CTessShaping, 1.0f - td2v.z, 0.0f, 0.999f);
                  td2v.w = (float)EditorGUILayout.Slider(CTessUpBias, td2v.w, 0.0f, 1);
                  if (fData.tesselationMode == TessellationMode.Distance)
                  {
                     td1v.x = EditorGUILayout.Slider(CTessTessellation, td1v.x, 0, 32);
                  }
                  else
                  {
                     td1v.w = EditorGUILayout.Slider(CTessEdgeLength, td1v.w, 4, 64);
                  }
                  td2v.x = EditorGUILayout.FloatField(CTessMinDistance, td2v.x);
                  td2v.y = EditorGUILayout.FloatField(CTessMaxDistance, td2v.y);
                  if (EditorGUI.EndChangeCheck())
                  {
                     td1.vectorValue = td1v;
                     td2.vectorValue = td2v;
                  }
               }
            }
         }
         if (fData.alphaHole && targetMat.HasProperty("_AlphaHoleIdx"))
         {
            materialEditor.ShaderProperty(ShaderGUI.FindProperty("_AlphaHoleIdx", props), "Alpha Hole Index");
         }

      }

      if (fData.projectTextureMode == TextureChoiceMode.Project3Way || fData.projectTextureMode2 == TextureChoiceMode.Project3Way)
      {
         Draw3WayEditor(materialEditor, props, targetMat, fData);
      }
         

      if (fData.layerMode == LayerMode.DetailTexture)
      {
         if (MegaSplatUtilities.DrawRollup("Detail Texturing"))
         {
            if (targetMat.HasProperty("_DetailAlbedo"))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent("Detail Albedo Array"), FindProperty("_DetailAlbedo", props));
               if (fData.packingMode != PackingMode.NoSpecOrNormal)
               {
                  materialEditor.TexturePropertySingleLine(detailNormalLabel, FindProperty("_DetailNormal", props));
               }

               Vector4 textureScales = targetMat.GetVector("_TexScales");
               EditorGUI.BeginChangeCheck();
               EditorGUILayout.BeginHorizontal();
               EditorGUILayout.PrefixLabel("Detail Texture Scale");
               textureScales.z = EditorGUILayout.FloatField(textureScales.z);
               textureScales.w = EditorGUILayout.FloatField(textureScales.w);
               EditorGUILayout.EndHorizontal();

               if (EditorGUI.EndChangeCheck())
               {
                  targetMat.SetVector("_TexScales", textureScales);
                  EditorUtility.SetDirty(targetMat);
               }

               materialEditor.ShaderProperty(FindProperty("_DetailTextureStrength", props), "Detail Strength");

               distanceFades.z = EditorGUILayout.FloatField("Begin", distanceFades.z);
               distanceFades.w = EditorGUILayout.FloatField("End", distanceFades.w);

               // convert back to length space
               distanceFades.z *= distanceFades.z;
               distanceFades.w *= distanceFades.w;
            }
         }
      }
      if (fData.detailNoise)
      {
         if (MegaSplatUtilities.DrawRollup("Detail Noise"))
         {
            if (targetMat.HasProperty("_DetailNoise"))
            {
               materialEditor.TexturePropertySingleLine(CDetailNoise, FindProperty("_DetailNoise", props));
               Vector4 scaleStr = FindProperty("_DetailNoiseScaleStrengthFade", props).vectorValue;
               Vector4 newScaleStr = scaleStr;
               newScaleStr.x = EditorGUILayout.FloatField("Scale", scaleStr.x);
               newScaleStr.y = EditorGUILayout.FloatField("Strength", scaleStr.y);
               newScaleStr.z = EditorGUILayout.FloatField("Fade Distance", scaleStr.z);
               if (newScaleStr != scaleStr)
               {
                  FindProperty("_DetailNoiseScaleStrengthFade", props).vectorValue = newScaleStr;
               }

            }
         }
         
      }

      if (fData.distanceNoise)
      {
         if (MegaSplatUtilities.DrawRollup("Distance Noise"))
         {
            if (targetMat.HasProperty("_DistanceNoise"))
            {
               materialEditor.TexturePropertySingleLine(CDistanceNoise, FindProperty("_DistanceNoise", props));
               Vector4 scaleStr = FindProperty("_DistanceNoiseScaleStrengthFade", props).vectorValue;
               Vector4 newScaleStr = scaleStr;
               newScaleStr.x = EditorGUILayout.FloatField("Scale", scaleStr.x);
               newScaleStr.y = EditorGUILayout.FloatField("Strength", scaleStr.y);
               newScaleStr.z = EditorGUILayout.FloatField("Fade Start", scaleStr.z);
               newScaleStr.w = EditorGUILayout.FloatField("Fade End", scaleStr.w);
               if (newScaleStr != scaleStr)
               {
                  FindProperty("_DistanceNoiseScaleStrengthFade", props).vectorValue = newScaleStr;
               }

            }
         }
      }

      actualDistances = distanceFades;
      if (useMacroFade == false)
      {
         actualDistances.x = kLargeValue;
         actualDistances.y = kLargeValue;
      }


      if (fData.uvMode == UVMode.UV && (fData.flowMode == FlowMode.Flow || fData.flowMode == FlowMode.Refractive))
      {
         if (MegaSplatUtilities.DrawRollup("Per Tex Flow Mapping"))
         {
            if (targetMat.HasProperty("_FlowSpeed"))
            {
               materialEditor.ShaderProperty(FindProperty("_FlowSpeed", props), CFlowSpeed);
               materialEditor.ShaderProperty(FindProperty("_FlowIntensity", props), CFlowIntensity);
           
               if (fData.flowMode == FlowMode.Refractive)
               {
                  materialEditor.ShaderProperty(FindProperty("_FlowAlpha", props), CFlowAlpha);
                  materialEditor.ShaderProperty(FindProperty("_FlowRefraction", props), CFlowRefraction);
               }
            }
         }
      }

      bool drawPerTex = fData.perTexContrast || fData.perTexDisplaceParams || fData.perTexMatParams ||
                        fData.perTexNoiseStrength || fData.perTexNormalStrength || fData.perTexUV ||
                        fData.perTexParallaxStrength || fData.perTexAOStrength ||
                        fData.flowMode != FlowMode.None || fData.perTexGlitter;

      if (drawPerTex && targetMat.HasProperty("_PropertyTex") )
      {
         DrawTextureEditor(materialEditor, props, targetMat, fData);

         var propTex = FindProperty("_PropertyTex", props);
         // in theory, this should never happen.. but if it does, show the control to allow recovery..
         if (propTex.textureValue == null && fData.flowMode != FlowMode.None)
         {
            materialEditor.TexturePropertySingleLine(new GUIContent("property tex"), propTex);
         }
      }

      if ((fData.perTexGlitter || fData.puddleGlitter || fData.snowGlitter) && targetMat.HasProperty("_GlitterTexture"))
      {
         if (MegaSplatUtilities.DrawRollup("Glitter"))
         {
            materialEditor.TexturePropertySingleLine(CGlitterTex, FindProperty("_GlitterTexture", props));
            Vector4 glitterParams = targetMat.GetVector("_GlitterParams");
            Vector2 glitterUV = new Vector2(glitterParams.x, glitterParams.y);
            EditorGUI.BeginChangeCheck();
            glitterUV = EditorGUILayout.Vector2Field(CGlitterUV, glitterUV);
            if (fData.glitterMode == GlitterMode.Animated)
            {
               glitterParams.z = EditorGUILayout.FloatField(CGlitterFlutterSpeed, glitterParams.z);
            }
            glitterParams.w = EditorGUILayout.FloatField(CGlitterFlutterScale, glitterParams.w);
            if (EditorGUI.EndChangeCheck())
            {
               glitterParams.x = glitterUV.x;
               glitterParams.y = glitterUV.y;
               targetMat.SetVector("_GlitterParams", glitterParams);
               EditorUtility.SetDirty(targetMat);
            }
            if (fData.puddleGlitter || fData.snowGlitter)
            {
               Vector4 glitterSurfParams = targetMat.GetVector("_GlitterSurfaces");
               EditorGUI.BeginChangeCheck();
               if (fData.snowGlitter)
               {
                  glitterSurfParams.x = EditorGUILayout.Slider(CGlitterSnowShine, glitterSurfParams.x, 0, 10);
                  glitterSurfParams.y = EditorGUILayout.Slider(CGlitterSnowReflect, glitterSurfParams.y, 0, 14);
               }
               if (fData.puddleGlitter)
               {
                  glitterSurfParams.z = EditorGUILayout.Slider(CGlitterPuddleShine, glitterSurfParams.z, 0, 10);
                  glitterSurfParams.w = EditorGUILayout.Slider(CGlitterPuddleReflect, glitterSurfParams.w, 0, 14);
               }
               if (EditorGUI.EndChangeCheck())
               {
                  targetMat.SetVector("_GlitterSurfaces", glitterSurfParams);
                  EditorUtility.SetDirty(targetMat);
               }
            }

         }
      }

      if (fData.puddleMode == PuddleMode.Lava && targetMat.HasProperty("_LavaParams2") && MegaSplatUtilities.DrawRollup("Lava", true))
      {
         materialEditor.TexturePropertySingleLine(CLavaDiffuse, FindProperty("_LavaDiffuse", props));

         EditorGUI.BeginChangeCheck();
         Vector4 lavaUVs = targetMat.GetVector("_LavaUVScale");
         Vector2 luv = new Vector2(lavaUVs.x, lavaUVs.y);
         luv = EditorGUILayout.Vector2Field(CLavaUVScale, luv);
         lavaUVs.x = luv.x;
         lavaUVs.y = luv.y;
         Vector4 lavaParams = targetMat.GetVector("_LavaParams");


         lavaParams.x = EditorGUILayout.Slider(CLavaBlend, lavaParams.x, 2.0f, 40.0f);
         lavaParams.y = EditorGUILayout.Slider(CLavaMax, lavaParams.y, 0.0f, 1.0f);
         lavaParams.z = EditorGUILayout.FloatField(CLavaSpeed, lavaParams.z);
         lavaParams.w = EditorGUILayout.FloatField(CLavaIntensity, lavaParams.w);

         Vector4 lavaParams2 = targetMat.GetVector("_LavaParams2");
         lavaParams2.w = EditorGUILayout.Slider(CLavaDarkening, lavaParams2.w, 0.0f, 6.0f);
         lavaParams2.x = EditorGUILayout.Slider(CLavaDistSize, lavaParams2.x, 0.0f, 0.3f);
         lavaParams2.y = EditorGUILayout.Slider(CLavaDistRate, lavaParams2.y, 0.0f, 0.08f);
         lavaParams2.z = EditorGUILayout.Slider(CLavaDistScale, lavaParams2.z, 0.02f, 1.0f);

         if (EditorGUI.EndChangeCheck())
         {
            targetMat.SetVector("_LavaParams", lavaParams);
            targetMat.SetVector("_LavaParams2", lavaParams2);
            targetMat.SetVector("_LavaUVScale", lavaUVs);
            EditorUtility.SetDirty(targetMat);
         }
         materialEditor.ShaderProperty(FindProperty("_LavaColorLow", props), CLavaColor);
         materialEditor.ShaderProperty(FindProperty("_LavaColorHighlight", props), CLavaHighlightColor);
         materialEditor.ShaderProperty(FindProperty("_LavaEdgeColor", props), CLavaEdgeColor);
      }       
      else if (fData.puddleMode != PuddleMode.None && targetMat.HasProperty("_PuddleBlend") && MegaSplatUtilities.DrawRollup("Puddles"))
      {
         materialEditor.ShaderProperty(FindProperty("_PuddleBlend", props), "Blend");
         materialEditor.ShaderProperty(FindProperty("_PuddleTint", props), "Tint");

         if (targetMat.HasProperty("_PuddleFlowParams"))
         {
            EditorGUI.BeginChangeCheck();
            Vector4 pudp = targetMat.GetVector("_PuddleFlowParams");
            if ((fData.puddleMode == PuddleMode.PuddleFlow || fData.puddleMode == PuddleMode.PuddleRefraction) && targetMat.HasProperty("_PuddleNormal"))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent("Normal"), FindProperty("_PuddleNormal", props));
            }
            if (targetMat.HasProperty("_PuddleNormalFoam"))
            {
               Vector4 pudNF = targetMat.GetVector("_PuddleNormalFoam");
               EditorGUI.BeginChangeCheck();
               pudNF.x = EditorGUILayout.Slider("Normal Blend", pudNF.x, 0, 1.0f);
               if (fData.puddleFoam)
               {
                  pudNF.y = EditorGUILayout.Slider("Foam", pudNF.y, 0.0f, 25.0f);
               }
               if (EditorGUI.EndChangeCheck())
               {
                  targetMat.SetVector("_PuddleNormalFoam", pudNF);
                  EditorUtility.SetDirty(targetMat);
               }
            }

            if (targetMat.HasProperty("_PuddleUVScales"))
            {
               Vector4 pudUV = targetMat.GetVector("_PuddleUVScales");
               Vector2 puv = new Vector2(pudUV.x, pudUV.y);
               EditorGUI.BeginChangeCheck();
               puv = EditorGUILayout.Vector2Field("Puddle UV Scale", puv);
               if (EditorGUI.EndChangeCheck())
               {
                  targetMat.SetVector("_PuddleUVScales", new Vector4(puv.x, puv.y, 0, 0));
                  EditorUtility.SetDirty(targetMat);
               }
            }

            if (fData.puddleMode == PuddleMode.PuddleFlow || fData.puddleMode == PuddleMode.PuddleRefraction)
            {
               if (fData.puddleMode == PuddleMode.PuddleRefraction)
               {
                  pudp.x = EditorGUILayout.Slider("Refraction Strength", pudp.x, 0.0f, 0.25f);
                  if (fData.puddleDepthDampen)
                  {
                     pudp.w = EditorGUILayout.Slider(CPuddleDepthCalm, pudp.w, 0.0f, 1.0f);
                  }
               }
               pudp.y = EditorGUILayout.FloatField("Flow Speed", pudp.y);
               pudp.z = EditorGUILayout.FloatField("Flow Intensity", pudp.z);
            }

            if (EditorGUI.EndChangeCheck())
            {
               targetMat.SetVector("_PuddleFlowParams", pudp);
               EditorUtility.SetDirty(targetMat);
            }
         }

         if (fData.rainDrops && targetMat.HasProperty("_RainDropTexture"))
         {
            materialEditor.TexturePropertySingleLine(CRainDropTexture, FindProperty("_RainDropTexture", props));
            materialEditor.ShaderProperty(FindProperty("_RainIntensity", props), "Rain Intensity");
            if (targetMat.HasProperty("_RainUVScales"))
            {
               EditorGUI.BeginChangeCheck();
               Vector4 rainUV = FindProperty("_RainUVScales", props).vectorValue;
               rainUV = EditorGUILayout.Vector2Field("Rain UV Scale", rainUV);
               if (EditorGUI.EndChangeCheck())
               {
                  FindProperty("_RainUVScales", props).vectorValue = rainUV;
               }
            }
         }
      }

      if (fData.snow && targetMat.HasProperty("_SnowParams") && MegaSplatUtilities.DrawRollup("Snow"))
      {
         TexturePropertySingleLine(materialEditor, new GUIContent("Diffuse/Height"), FindProperty("_SnowDiff", props));
         TexturePropertySingleLine(materialEditor, new GUIContent("Snow NormalSAO"), FindProperty("_SnowNormal", props));
         // influence, erosion, crystal, melt
         Vector4 p1 = FindProperty("_SnowParams", props).vectorValue;
         Vector4 hr = FindProperty("_SnowHeightRange", props).vectorValue;

         if (targetMat.HasProperty("_SnowUVScales"))
         {
            Vector4 snowUV = FindProperty("_SnowUVScales", props).vectorValue;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("UV Scale");
            snowUV.x = EditorGUILayout.FloatField(snowUV.x);
            snowUV.y = EditorGUILayout.FloatField(snowUV.y);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
               FindProperty("_SnowUVScales", props).vectorValue = snowUV;
               EditorUtility.SetDirty(targetMat);
            }
         }
         else
         {
            needsCompile = true;
         }
            
         EditorGUI.BeginChangeCheck();

         materialEditor.ShaderProperty(FindProperty("_SnowAmount", props), "Amount");
         p1.x = EditorGUILayout.Slider("Height Map Influence", p1.x, 0, 1);
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.PrefixLabel("Height Range");
         hr.x = EditorGUILayout.FloatField(hr.x);
         hr.y = EditorGUILayout.FloatField(hr.y);
         EditorGUILayout.EndHorizontal();
         p1.y = EditorGUILayout.FloatField("Erosion", p1.y);
         p1.z = EditorGUILayout.FloatField("Crystals", p1.z);
         p1.w = EditorGUILayout.Slider("Melt", p1.w, 0, 0.6f);

         if (targetMat.HasProperty("_SnowUpVector"))
         {
            Vector4 up = targetMat.GetVector("_SnowUpVector");
            EditorGUI.BeginChangeCheck();
            Vector3 newUp = EditorGUILayout.Vector3Field("Snow Up Vector", new Vector3(up.x, up.y, up.z));
            if (EditorGUI.EndChangeCheck())
            {
               newUp.Normalize();
               targetMat.SetVector("_SnowUpVector", new Vector4(newUp.x, newUp.y, newUp.z, 0));
               EditorUtility.SetDirty(targetMat);
            }
         }

         if (EditorGUI.EndChangeCheck())
         {
            FindProperty("_SnowParams", props).vectorValue = p1;
            FindProperty("_SnowHeightRange", props).vectorValue = hr;
         }

         if (fData.snowDistanceNoise)
         {
            if (targetMat.HasProperty("_SnowDistanceNoise"))
            {
               materialEditor.TexturePropertySingleLine(CDistanceNoise, FindProperty("_SnowDistanceNoise", props));
               Vector4 scaleStr = FindProperty("_SnowDistanceNoiseScaleStrengthFade", props).vectorValue;
               Vector4 newScaleStr = scaleStr;
               newScaleStr.x = EditorGUILayout.FloatField("Noise UV Scale", scaleStr.x);
               newScaleStr.y = EditorGUILayout.FloatField("Noise Strength", scaleStr.y);
               newScaleStr.z = EditorGUILayout.FloatField("Noise Fade Start", scaleStr.z);
               newScaleStr.w = EditorGUILayout.FloatField("Noise Fade End", scaleStr.w);
               if (newScaleStr != scaleStr)
               {
                  FindProperty("_SnowDistanceNoiseScaleStrengthFade", props).vectorValue = newScaleStr;
               }
            }
         }
         if (fData.snowDistanceResample)
         {
            if (targetMat.HasProperty("_SnowDistanceResampleScaleStrengthFade"))
            {
               Vector4 scaleStr = FindProperty("_SnowDistanceResampleScaleStrengthFade", props).vectorValue;
               Vector4 newScaleStr = scaleStr;
               newScaleStr.x = EditorGUILayout.FloatField("Resample UV Scale", scaleStr.x);
               newScaleStr.y = EditorGUILayout.FloatField("Resample Strength", scaleStr.y);
               newScaleStr.z = EditorGUILayout.FloatField("Resample Fade Start", scaleStr.z);
               newScaleStr.w = EditorGUILayout.FloatField("Resample Fade End", scaleStr.w);
               if (newScaleStr != scaleStr)
               {
                  FindProperty("_SnowDistanceResampleScaleStrengthFade", props).vectorValue = newScaleStr;
               }
            }
         }
      }

      if ((fData.snow || fData.puddleMode != PuddleMode.None || fData.wetness) && MegaSplatUtilities.DrawRollup("Wetness"))
      {
         if (targetMat.HasProperty("_GlobalPorosityWetness"))
         {
            Vector4 pw = FindProperty("_GlobalPorosityWetness", props).vectorValue;
            EditorGUI.BeginChangeCheck();
            if (!fData.perTexMatParams)
            {
               pw.x = EditorGUILayout.Slider(new GUIContent("Porosity", "Porosity amount for all textures"), pw.x, 0.0f, 1.0f);
            }

            if (fData.wetness)
            {
               pw.y = EditorGUILayout.Slider(new GUIContent("Global Wetness", "Minimum Wetness for the material"), pw.y, 0.0f, 1.0f);
               if (targetMat.HasProperty("_MaxWetness"))
               {
                  materialEditor.ShaderProperty(FindProperty("_MaxWetness", props), "Maximum Wetness");
               }
            }

            if (fData.puddleMode != PuddleMode.None && targetMat.HasProperty("_MaxPuddles"))
            {
               materialEditor.ShaderProperty(FindProperty("_MaxPuddles", props), "Maximum Puddles");
            }


            if (EditorGUI.EndChangeCheck())
            {
               FindProperty("_GlobalPorosityWetness", props).vectorValue = pw;
            }
         }
      }

      if (fData.lowPolyAdjust && MegaSplatUtilities.DrawRollup("Low Poly", true))
      {
         materialEditor.ShaderProperty(FindProperty("_EdgeHardness", props), CLowPolyHardness);
      }

      // draw custom properties..
      if (fData.customUserFunction)
      {
         if (MegaSplatUtilities.DrawRollup("Custom Properties"))
         {
            string[] cProps = GetCustomPropFile(targetMat);
            if (cProps != null && cProps.Length > 0)
            {
               for (int i = 0; i < cProps.Length; ++i)
               {
                  string prop = cProps[i].Trim();
                  if (prop.StartsWith("_Custom_"))
                  {
                     string propName = prop.Substring(0, prop.IndexOf("("));
                     if (targetMat.HasProperty(propName))
                     {
                        var p = FindProperty(propName, props);
                        materialEditor.ShaderProperty(p, p.name);
                     }
                  }
               }
            }
         }
      }


      int arraySampleCount;
      int textureSampleCount;
      int maxSamples;
      int tessSamples;
      int depTexReadLevel;
      builder.Length = 0;
      fData.ComputeSampleCounts(out arraySampleCount, out textureSampleCount, out maxSamples, out tessSamples, out depTexReadLevel);
      if (MegaSplatUtilities.DrawRollup("Debug"))
      {
         string shaderModel = GetShaderModel(fData);
         builder.Append("Shader Model : ");
         builder.AppendLine(shaderModel);
         if (maxSamples != arraySampleCount)
         {
            builder.Append("Texture Array Samples : ");
            builder.Append(arraySampleCount.ToString());
            builder.Append(", ");
            builder.Append(maxSamples.ToString());
            builder.AppendLine(" in areas with flow mapping");

            builder.Append("Regular Samples : ");
            builder.AppendLine(textureSampleCount.ToString());
         }
         else
         {
            builder.Append("Texture Array Samples : ");
            builder.AppendLine(arraySampleCount.ToString());
            builder.Append("Regular Samples : ");
            builder.AppendLine(textureSampleCount.ToString());
         }
         if (fData.tesselationMode != TessellationMode.None)
         {
            builder.Append("Tessellation Samples : ");
            builder.AppendLine(tessSamples.ToString());
         }
         if (depTexReadLevel > 0)
         {
            builder.Append(depTexReadLevel.ToString());
            builder.AppendLine(" areas with dependent texture reads");
         }
         if (fData.lowPoly)
         {
            builder.AppendLine("\nGeometry Shader is Active");
         }

         EditorGUILayout.HelpBox(builder.ToString(), MessageType.Info);
      }
      if (EditorGUI.EndChangeCheck())
      {
         FindProperty("_DistanceFades", props).vectorValue = actualDistances;
         FindProperty("_DistanceFadesCached", props).vectorValue = distanceFades;
      }



      if (needsCompile)
      {
         fData.Pack(targetMat);
         Compile(targetMat, shaderName);
      }

      if (EditorGUI.EndChangeCheck() && targetMat.IsKeywordEnabled("_TERRAIN"))
      {
         MegaSplatTerrainManager.SyncAll();
      }
   }
}

