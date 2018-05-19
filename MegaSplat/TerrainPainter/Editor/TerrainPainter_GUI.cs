//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using JBooth.MegaSplat;
using System.Reflection;
using System.Linq;

namespace JBooth.TerrainPainter
{
   public partial class TerrainPainterWindow : EditorWindow 
   {

      enum Tab
      {
         Paint = 0,
         Flow,
         Wetness,
         Puddles,
         Dampening,
         Utility,
      }

      string[] tabNames =
      {
         "Paint",
         "Flow",
         "Wetness",
         "Puddles",
         "Dampening",
         "Utility",
      };
      Tab tab = Tab.Paint;



      static Dictionary<string, bool> rolloutStates = new Dictionary<string, bool>();
      static GUIStyle rolloutStyle;
      public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = GUI.skin.box;
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         if (inset == true)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
         }

         if (!rolloutStates.ContainsKey(text))
         {
            rolloutStates[text] = defaultState;
         }
         if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)}))
         {
            rolloutStates[text] = !rolloutStates[text];
         }
         if (inset == true)
         {
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
         }
         return rolloutStates[text];
      }

      Texture2D SaveTexture(string path, Texture2D tex, bool overwrite = false)
      {
         if (overwrite || !System.IO.File.Exists(path))
         {
            var bytes = tex.EncodeToPNG();

            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            AssetImporter ai = AssetImporter.GetAtPath(path);
            TextureImporter ti = ai as TextureImporter;
            #if UNITY_5_5_OR_NEWER
            ti.sRGBTexture = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            var ftm = ti.GetDefaultPlatformTextureSettings();
            ftm.format = TextureImporterFormat.ARGB32;
            ti.SetPlatformTextureSettings(ftm);
            #else
            ti.linearTexture = true;
            ti.textureFormat = TextureImporterFormat.ARGB32;
            #endif
            ti.mipmapEnabled = false;
            ti.isReadable = true;
            ti.filterMode = FilterMode.Point;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.SaveAndReimport();
         }
         return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      }

      void CreateTexture(Terrain t, string texNamePrefix = "", Material customMat = null)
      {
         // find/create manager
         var mgr = t.GetComponent<MegaSplatTerrainManager>();
         if (mgr == null)
         {
            mgr = t.gameObject.AddComponent<MegaSplatTerrainManager>();
         }
         // find best material
         Material mat = customMat;
         if (mat == null)
         {
            mat = mgr.templateMaterial;
         }
         if (mat == null)
         {
            mat = t.materialTemplate;
         }
         string paramPath, path;
         mgr.FindPaths(t, texNamePrefix, mat, out path, out paramPath);
         mgr.FindTextures(t, texNamePrefix, mat);
         Texture2D tex = mgr.splatTexture;
         Texture2D paramTex = mgr.paramTexture;

     
         // if we still don't have a texture, create one
         if (tex == null)
         {
            tex = new Texture2D(t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, TextureFormat.ARGB32, false, true);
            mgr.splatTexture = tex;
            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  tex.SetPixel(x, y, Color.black);
               }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            mgr.splatTexture = SaveTexture(path, tex);
         }
         if (paramTex == null)
         {
            paramTex = new Texture2D(t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, TextureFormat.ARGB32, false, true);
            mgr.paramTexture = tex;
            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  paramTex.SetPixel(x, y, new Color(0.5f, 0.5f, 0, 0));
               }
            }
            paramTex.Apply();
            paramTex.wrapMode = TextureWrapMode.Clamp;
            mgr.paramTexture = SaveTexture(paramPath, paramTex);
         }
         if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(t.materialTemplate)))
         {
            mgr.templateMaterial = mat;
         }
         mgr.Sync();
      }


      void AutoSetup(List<Terrain> terrains)
      {
         if (terrains.Count == 0)
            return;
         // first, see if we have a valid manager, and if so, use it's template material
         Material template = null;
         for (int i = 0; i < terrains.Count; ++i)
         {
            var t = terrains[i];
            var mgr = t.GetComponent<MegaSplatTerrainManager>();
            if (mgr != null && mgr.templateMaterial != null && mgr.templateMaterial.IsKeywordEnabled("_TERRAIN"))
            {
               template = mgr.templateMaterial;
               break;
            }
         }

         // no template? Create a shader and material template
         if (template == null)
         {
            template = CreateNewShaderWithMat(terrains[0].name, terrains[0]);
         }


         for (int i = 0; i < terrains.Count; ++i)
         {
            var t = terrains[i];
            var mgr = t.GetComponent<MegaSplatTerrainManager>();
            if (mgr == null)
            {
               mgr = t.gameObject.AddComponent<MegaSplatTerrainManager>();
            }
         
            t.materialType = Terrain.MaterialType.Custom;
            mgr.templateMaterial = template;
            // this will instanciate a material instance for us..
            mgr.Sync();

            // create textures
            CreateTexture(t, "", mgr.matInstance);
         }

      }

      Material CreateNewShaderWithMat(string name, Terrain t)
      {
         string shader = SplatArrayShaderGUI.Compile(new string[]{ "_TERRAIN", "_TWOLAYER" }, name);

         string shaderPath = AssetDatabase.GetAssetPath(t);
         if (string.IsNullOrEmpty(shaderPath) && t != null && t.terrainData != null)
         {
            shaderPath = AssetDatabase.GetAssetPath(t.terrainData);
         }
         shaderPath = shaderPath.Replace("\\", "/");
         if (shaderPath.Contains("/"))
         {
            shaderPath = shaderPath.Substring(0, shaderPath.LastIndexOf("/") + 1);
         }
         else
         {
            shaderPath = "Assets/";
         }
         string matPath = shaderPath;
         string filePath = shaderPath;
         if (filePath.StartsWith("Assets"))
         {
            filePath = Application.dataPath + filePath.Substring(6);
         }

         System.IO.File.WriteAllText(filePath + name + ".shader", shader);

         AssetDatabase.Refresh();
         Shader s = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath + name + ".shader");
         if (s == null)
         {
            Debug.LogError("Could not find shader at " + shaderPath + name + ".shader   " + shaderPath + name + ".shader");
            return null;
         }
         Material mat = new Material(s);
         mat.shaderKeywords = new string[] {"_TERRAIN" };
         AssetDatabase.CreateAsset(mat, matPath + name + ".mat");
         AssetDatabase.SaveAssets();
         mat = AssetDatabase.LoadAssetAtPath<Material>(matPath + name + ".mat");

         SplatArrayShaderGUI.Compile(mat);
         EditorUtility.SetDirty(mat);
         AssetDatabase.Refresh();
         AssetDatabase.SaveAssets();
         return mat;
      }

      bool VerifyData()
      {
         if (rawTerrains == null || rawTerrains.Count == 0)
            return false;

         for (int i = 0; i < rawTerrains.Count; ++i)
         {
            Terrain t = rawTerrains[i];
            if (t.materialType != Terrain.MaterialType.Custom || t.materialTemplate == null || !t.materialTemplate.HasProperty("_SplatControl"))
            {
               EditorGUILayout.HelpBox("Terrains are not setup for MegaSplat", MessageType.Error);

               EditorGUILayout.HelpBox("This will create a single MegaSplat material and shader to be shared by all terrains in the selection", MessageType.Info);
               if (GUILayout.Button("Setup Terrain(s) for MegaSplat"))
               {
                  AutoSetup(rawTerrains);
               }
               return false;
            }


            if (t.materialTemplate.GetTexture("_SplatControl") == null)
            {
               EditorGUILayout.HelpBox("Terrain material must have a splat control texture assigned", MessageType.Info);
               if (GUILayout.Button("Create it for me"))
               {
                  AutoSetup(rawTerrains);
               }

               return false;
            }

            var mgr = t.GetComponent<MegaSplatTerrainManager>();
            if (mgr == null)
            {
               mgr = t.gameObject.AddComponent<MegaSplatTerrainManager>();
               mgr.templateMaterial = t.materialTemplate;
               mgr.splatTexture = t.materialTemplate.GetTexture("_SplatControl") as Texture2D;
               mgr.paramTexture = t.materialTemplate.GetTexture("_SplatParams") as Texture2D;
               mgr.Sync();
            }

            if (t.materialTemplate.HasProperty("_SplatParams"))
            {
               
               if (mgr.UsesParams() && t.materialTemplate.GetTexture("_SplatParams") == null)
               {
                  CreateTexture(t);
                  return false;
               }

            }

            if (t.materialTemplate.HasProperty("_Diffuse") && t.materialTemplate.GetTexture("_Diffuse") == null)
            {
               EditorGUILayout.HelpBox("Terrain material has no diffuse texture array\nPlease select your material and assign one", MessageType.Error);
               if (GUILayout.Button("Select Material"))
               {
                  Selection.activeObject = mgr.templateMaterial;
                  EditorApplication.ExecuteMenuItem("Window/Inspector");
               }
               return false;
            }
         }

         for (int i = 0; i < rawTerrains.Count; ++i)
         {
            Terrain t = rawTerrains[i];
            var tex = t.materialTemplate.GetTexture("_SplatControl");
            AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            TextureImporter ti = ai as TextureImporter;
            if (ti == null || !ti.isReadable)
            {
               EditorGUILayout.HelpBox("Control texture is not read/write", MessageType.Error);
               if (GUILayout.Button("Fix it!"))
               {
                  ti.isReadable = true;
                  ti.SaveAndReimport();
               }
               return false;
            }

            #if UNITY_5_5_OR_NEWER
            bool isLinear = ti.sRGBTexture == false;
            bool isRGB32 = ti.textureCompression == TextureImporterCompression.Uncompressed && ti.GetDefaultPlatformTextureSettings().format == TextureImporterFormat.ARGB32;
            #else
            bool isLinear = ti.linearTexture;
            bool isRGB32 = ti.textureFormat == TextureImporterFormat.ARGB32;
            #endif


            if (isRGB32 == false || isLinear == false || ti.wrapMode == TextureWrapMode.Repeat)
            {
               EditorGUILayout.HelpBox("Control texture is not in the correct format (Uncompressed, linear, clamp, ARGB)", MessageType.Error);
               if (GUILayout.Button("Fix it!"))
               {
                  
                  #if UNITY_5_5_OR_NEWER
                  ti.sRGBTexture = false;
                  ti.textureCompression = TextureImporterCompression.Uncompressed;
                  var ftm = ti.GetDefaultPlatformTextureSettings();
                  ftm.format = TextureImporterFormat.ARGB32;
                  ti.SetPlatformTextureSettings(ftm);
                  #else
                  ti.linearTexture = true;
                  ti.textureFormat = TextureImporterFormat.ARGB32;
                  #endif
                  ti.mipmapEnabled = false;
                  ti.wrapMode = TextureWrapMode.Clamp;
                  ti.SaveAndReimport();
               }
               return false;
            }
         }
         return true;

      }

      void DrawFlowGUI()
      {
         if (DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }

         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Reset"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(terrains);
            }
            for (int i = 0; i < terrains.Length; ++i)
            {
               FillTerrainParams(terrains[i], 0, 0.5f);
               FillTerrainParams(terrains[i], 1, 0.5f);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }
         EditorGUILayout.EndHorizontal();
      }

      float dampeningValue = 1;
      void DrawDampeningGUI()
      {
         if (DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         EditorGUILayout.BeginHorizontal();
         dampeningValue = EditorGUILayout.Slider("value", dampeningValue, 0, 1);
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(terrains);
            }
            for (int i = 0; i < terrains.Length; ++i)
            {
               FillDampening(terrains[i], 1.0f - dampeningValue);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }
         EditorGUILayout.EndHorizontal();
      }

      float wetnessValue = 1;
      void DrawWetnessGUI()
      {
         if (DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         EditorGUILayout.BeginHorizontal();
         wetnessValue = EditorGUILayout.Slider("value", wetnessValue, 0, 1);
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(terrains);
            }
            for (int i = 0; i < terrains.Length; ++i)
            {
               FillTerrainParams(terrains[i], 2, wetnessValue);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }
         EditorGUILayout.EndHorizontal();
      }

      float puddleValue = 1;
      void DrawPuddlesGUI()
      {
         if (DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         EditorGUILayout.BeginHorizontal();
         puddleValue = EditorGUILayout.Slider("value", puddleValue, 0, 1);
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(terrains);
            }
            for (int i = 0; i < terrains.Length; ++i)
            {
               FillTerrainParams(terrains[i], 3, puddleValue);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }
         EditorGUILayout.EndHorizontal();
      }

      bool HasFeature(SplatArrayShaderGUI.DefineFeature feature)
      {
         string keyword = feature.ToString();
         for (int i = 0; i < terrains.Length; ++i)
         {
            if (terrains[i] != null && terrains[i].terrain != null && terrains[i].terrain.materialTemplate != null)
            {
               if (terrains[i].terrain.materialTemplate.IsKeywordEnabled(keyword))
                  return true;
            }
         }
         return false;
      }

      Vector2 scroll;
      void OnGUI()
      {
         if (VerifyData() == false)
         {
            EditorGUILayout.HelpBox("Please select a terrain to begin", MessageType.Info);
            return;
         }

         DrawSettingsGUI();
         DrawSaveGUI();
         tab = (Tab)GUILayout.Toolbar((int)tab, tabNames);


         if (tab == Tab.Paint)
         {
            DrawPaintGUI();
         }
         else if (tab == Tab.Flow)
         {
            if (HasFeature(SplatArrayShaderGUI.DefineFeature._FLOW) || 
               HasFeature(SplatArrayShaderGUI.DefineFeature._FLOWREFRACTION) ||
               HasFeature(SplatArrayShaderGUI.DefineFeature._PUDDLEFLOW) ||
               HasFeature(SplatArrayShaderGUI.DefineFeature._PUDDLEREFRACT) || 
               HasFeature(SplatArrayShaderGUI.DefineFeature._LAVA)
               )
            {
               DrawFlowGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Flow mapping/puddles are not enabled on any terrain shaders", MessageType.Info);
            }
         }
         else if (tab == Tab.Puddles)
         {
            if (HasFeature(SplatArrayShaderGUI.DefineFeature._PUDDLEREFRACT) || 
               HasFeature(SplatArrayShaderGUI.DefineFeature._PUDDLES) || 
               HasFeature(SplatArrayShaderGUI.DefineFeature._PUDDLEFLOW) ||
               HasFeature(SplatArrayShaderGUI.DefineFeature._LAVA)
            )
            {
               DrawPuddlesGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Puddles are not enabled on any terrain shaders", MessageType.Info);
            }
         }
         else if (tab == Tab.Wetness)
         {
            if (HasFeature(SplatArrayShaderGUI.DefineFeature._WETNESS))
            {
               DrawWetnessGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Wetness is not enabled on any terrain shaders", MessageType.Info);
            }
         }
         else if (tab == Tab.Dampening)
         {
            if (HasFeature(SplatArrayShaderGUI.DefineFeature._TESSDAMPENING))
            {
               DrawDampeningGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Tessellation and Tess Dampening are not enabled on any terrain shaders", MessageType.Info);
            }
         }
         else
         {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawUtilityGUI();
            EditorGUILayout.EndScrollView();
         }

      }

      void DrawSaveGUI()
      {
         EditorGUILayout.Space();
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Save"))
         {
            for (int i = 0; i < terrains.Length; ++i)
            {
               string path = AssetDatabase.GetAssetPath(terrains[i].terrainTex);
               var bytes = terrains[i].terrainTex.EncodeToPNG();
               System.IO.File.WriteAllBytes(path, bytes);

               if (terrains[i].terrainParams != null)
               {
                  path = AssetDatabase.GetAssetPath(terrains[i].terrainParams);

                  bytes = terrains[i].terrainParams.EncodeToPNG();
                  System.IO.File.WriteAllBytes(path, bytes);
               }
            }
            AssetDatabase.Refresh();

         }

         EditorGUILayout.EndHorizontal();
         EditorGUILayout.Space();
      }

      List<ITerrainPainterUtility> utilities = new List<ITerrainPainterUtility>();
      void InitPluginUtilities()
      {
         if (utilities == null || utilities.Count == 0)
         {
            var interfaceType = typeof(ITerrainPainterUtility);
            var all = System.AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(x => x.GetTypes())
               .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
               .Select(x => System.Activator.CreateInstance(x));


            foreach (var o in all)
            {
               ITerrainPainterUtility u = o as ITerrainPainterUtility;
               if (u != null)
               {
                  utilities.Add(u);
               }
            }
            utilities = utilities.OrderBy(o=>o.GetName()).ToList();
         }
      }

      void DrawUtilityGUI()
      {
         InitPluginUtilities();
         for (int i = 0; i < utilities.Count; ++i)
         {
            var u = utilities[i];
            if (DrawRollup(u.GetName(), false))
            {
               u.OnGUI(terrains);
            }
         }
      }

      void DrawSettingsGUI()
      {
         EditorGUILayout.Separator();
         GUI.skin.box.normal.textColor = Color.white;
         if (DrawRollup("Terrain Painter"))
         {
            bool oldEnabled = enabled;
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp)
            {
               enabled = !enabled;
            }
            enabled = GUILayout.Toggle(enabled, "Active (ESC)");
            if (enabled != oldEnabled)
            {
               InitTerrains();
            }

            brushVisualization = (BrushVisualization)EditorGUILayout.EnumPopup("Brush Visualization", brushVisualization);
            EditorGUILayout.Separator();
            GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
            EditorGUILayout.Separator();
         }
      }

      void DrawBrushSettingsGUI()
      {
         brushSize      = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 30.0f);
         brushFlow      = EditorGUILayout.Slider("Brush Flow", brushFlow, 0.1f, 128.0f);
         brushFalloff   = EditorGUILayout.Slider("Brush Falloff", brushFalloff, 0.1f, 3.5f);

         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();

      }



      void FillDampening(TerrainJob t, float val)
      {
         InitTerrains();
         t.RegisterUndo();
         Texture2D tex = t.terrainTex;
         int width = tex.width;
         int height = tex.height;
         for (int x = 0; x < width; ++x)
         {
            for (int y = 0; y < height; ++y)
            {
               var c = tex.GetPixel(x, y);
               c.a = val;

               tex.SetPixel(x, y, c);
            }
         }
         tex.Apply();
      }

      void FillTerrainParams(TerrainJob t, int channel, float val)
      {
         InitTerrains();
         t.RegisterUndo();
         Texture2D tex = t.terrainParams;
         int width = tex.width;
         int height = tex.height;

         for (int x = 0; x < width; ++x)
         {
            for (int y = 0; y < height; ++y)
            {
               var c = tex.GetPixel(x, y);
               if (channel == 0)
               {
                  c.r = val;
               }
               else if (channel == 1)
               {
                  c.g = val;
               }
               else if (channel == 2)
               {
                  c.b = val;
               }
               else if (channel == 3)
               {
                  c.a = val;
               }
               tex.SetPixel(x, y, c);
            }
         }
         tex.Apply();
      }

      void FillTerrain(TerrainJob t)
      {
         t.RegisterUndo();
         Texture2D tex = t.terrainTex;
         int width = tex.width;
         int height = tex.height;

         // no auto fill, as it doesn't do what the user expects
         var lm = config.brushData.layerMode;
         if (lm == TextureArrayConfig.BrushData.LayerMode.Auto)
         {
            config.brushData.layerMode = TextureArrayConfig.BrushData.LayerMode.Bottom;
         }
         for (int x = 0; x < width; ++x)
         {
            for (int y = 0; y < height; ++y)
            {
               float h = t.terrain.terrainData.GetHeight(x, y);
               Vector3 n = t.terrain.terrainData.GetInterpolatedNormal(x, y);
               Color c = config.GetValues(t.terrain, tex.GetPixel(x, y), config.brushData, MegaSplatUtilities.TerrainToWorld(t.terrain, x, y, tex), h, n, 1.0f); 
               tex.SetPixel(x, y, c);
            }
         }
         config.brushData.layerMode = lm;
         tex.Apply();
      }

      TextureArrayConfig config;

      void DrawPaintGUI()
      {
         if (DrawRollup("Brush Settings"))
         {
            config = EditorGUILayout.ObjectField("Config", config, typeof(TextureArrayConfig), false) as TextureArrayConfig;

            DrawBrushSettingsGUI();
         }
         scroll = EditorGUILayout.BeginScrollView(scroll);
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(terrains);
            }
            for (int i = 0; i < terrains.Length; ++i)
            {
               FillTerrain(terrains[i]);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }

         EditorGUILayout.EndHorizontal();

         if (config != null)
         {
            config.DrawGUI();
         }

         EditorGUILayout.EndScrollView();
      }
   }
}
