//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.MegaSplat
{
   [CustomEditor(typeof(TextureArrayConfig))]
   public class TextureArrayConfigEditor : Editor 
   {

      static bool NeedsAlpha(TextureArrayConfig cfg)
      {
         for (int i = 0; i < cfg.sourceTextures.Count; ++i)
         {
            if (cfg.sourceTextures[i].alpha != null)
               return true;
         }
         return false;
      }

      static bool NeedsEmisMetal(TextureArrayConfig cfg)
      {
         for (int i = 0; i < cfg.sourceTextures.Count; ++i)
         {
            if (cfg.sourceTextures[i].metallic != null || cfg.sourceTextures[i].emissive != null)
               return true;
         }
         return false;
      }

      void DrawHeader(TextureArrayConfig cfg)
      {
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField("", GUILayout.Width(30));
         EditorGUILayout.LabelField("Channel", GUILayout.Width(64));
         EditorGUILayout.EndVertical();
         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Height"), GUILayout.Width(64));
         cfg.allTextureChannelHeight = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelHeight, GUILayout.Width(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Smoothness"), GUILayout.Width(64));
         cfg.allTextureChannelSmoothness = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelSmoothness, GUILayout.Width(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("AO"), GUILayout.Width(64));
         cfg.allTextureChannelAO = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelAO, GUILayout.Width(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Metal"), GUILayout.Width(64));
         cfg.allTextureChannelMetallic = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelMetallic, GUILayout.Width(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Alpha"), GUILayout.Width(64));
         cfg.allTextureChannelAlpha = (TextureArrayConfig.AllTextureChannel)EditorGUILayout.EnumPopup(cfg.allTextureChannelAlpha, GUILayout.Width(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.EndHorizontal();
         GUILayout.Box(Texture2D.blackTexture, GUILayout.Height(3), GUILayout.ExpandWidth(true));

      }

      bool DrawTextureEntry(TextureArrayConfig cfg, TextureArrayConfig.TextureEntry e, int i)
      {
         bool ret = false;

         EditorGUILayout.BeginHorizontal();

         if (e.HasTextures())
         {
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));
            EditorGUILayout.LabelField(e.diffuse != null ? e.diffuse.name : "empty");
            ret = GUILayout.Button("Clear Entry");
         }
         else
         {
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));
            EditorGUILayout.HelpBox("Removing an entry completely can cause texture choices to change on existing terrains. You can leave it blank to preserve the texture order and MegaSplat will put a dummy texture into the array.", MessageType.Warning);
            ret = (GUILayout.Button("Delete Entry"));
         }
         EditorGUILayout.EndHorizontal();

         EditorGUILayout.BeginHorizontal();

         #if !UNITY_2017_3_OR_NEWER
         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Substance"), GUILayout.Width(64));
         e.substance = (ProceduralMaterial)EditorGUILayout.ObjectField(e.substance, typeof(ProceduralMaterial), false, GUILayout.Width(64), GUILayout.Height(64));
         EditorGUILayout.EndVertical();
         #endif

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Diffuse"), GUILayout.Width(64));
         e.diffuse = (Texture2D)EditorGUILayout.ObjectField(e.diffuse, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Normal"), GUILayout.Width(64));
         e.normal = (Texture2D)EditorGUILayout.ObjectField(e.normal, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         EditorGUILayout.EndVertical();


         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Height"), GUILayout.Width(64));
         e.height = (Texture2D)EditorGUILayout.ObjectField(e.height, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         if (cfg.allTextureChannelHeight == TextureArrayConfig.AllTextureChannel.Custom)
         {
            e.heightChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.heightChannel, GUILayout.Width(64));
         }
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Smoothness"), GUILayout.Width(64));
         e.smoothness = (Texture2D)EditorGUILayout.ObjectField(e.smoothness, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         if (cfg.allTextureChannelSmoothness == TextureArrayConfig.AllTextureChannel.Custom)
         {
            e.smoothnessChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.smoothnessChannel, GUILayout.Width(64));
         }
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.LabelField("Invert", GUILayout.Width(35));
         e.isRoughness = EditorGUILayout.Toggle(e.isRoughness, GUILayout.Width(20));
         EditorGUILayout.EndHorizontal();
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("AO"), GUILayout.Width(64));
         e.ao = (Texture2D)EditorGUILayout.ObjectField(e.ao, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         if (cfg.allTextureChannelAO == TextureArrayConfig.AllTextureChannel.Custom)
         {
            e.aoChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.aoChannel, GUILayout.Width(64));
         }
         EditorGUILayout.EndVertical();


         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Metallic"), GUILayout.Width(64));
         e.metallic = (Texture2D)EditorGUILayout.ObjectField(e.metallic, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         if (cfg.allTextureChannelMetallic == TextureArrayConfig.AllTextureChannel.Custom)
         {
            e.metallicChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.metallicChannel, GUILayout.Width(64));
         }
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Emissive"), GUILayout.Width(64));
         e.emissive = (Texture2D)EditorGUILayout.ObjectField(e.emissive, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         EditorGUILayout.EndVertical();

         EditorGUILayout.BeginVertical();
         EditorGUILayout.LabelField(new GUIContent("Alpha"), GUILayout.Width(64));
         e.alpha = (Texture2D)EditorGUILayout.ObjectField(e.alpha, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
         if (cfg.allTextureChannelAlpha == TextureArrayConfig.AllTextureChannel.Custom)
         {
            e.alphaChannel = (TextureArrayConfig.TextureChannel)EditorGUILayout.EnumPopup(e.alphaChannel, GUILayout.Width(64));
         }
         EditorGUILayout.EndVertical();


         EditorGUILayout.EndHorizontal();
         GUILayout.Box(Texture2D.blackTexture, GUILayout.Height(3), GUILayout.ExpandWidth(true));
         return ret;
      }
         

      static TextureArrayConfig staticConfig;
      void DelayedCompileConfig()
      {
         CompileConfig(staticConfig);
      }
         
      public string lastImportFolder;


      string GetNameRoot(TextureArrayConfig cfg, TextureArrayConfig.TextureEntry te)
      {
         string name = "";
         if (te.diffuse != null)
         {
            name = te.diffuse.name;
         }
         else if (te.height != null)
         {
            name = te.height.name;
         }
         else if (te.normal != null)
         {
            name = te.normal.name;
         }
         else if (te.ao != null)
         {
            name = te.ao.name;
         }
         else if (te.smoothness != null)
         {
            name = te.smoothness.name;
         }
         else if (te.metallic != null)
         {
            name = te.metallic.name;
         }
         else if (te.alpha != null)
         {
            name = te.alpha.name;
         }
         name = name.ToLower();

         if (name.EndsWith(cfg.extDiffuse) || name.EndsWith(cfg.extNorm) || name.EndsWith(cfg.extSmoothness) ||
            name.EndsWith(cfg.extAO) || name.EndsWith(cfg.extMetal) || name.EndsWith(cfg.extHeight) ||
            name.EndsWith(cfg.extEmiss) || name.EndsWith(cfg.extAlpha) ||
            name.EndsWith("_normsao"))
         {
            name = name.Substring(0, name.LastIndexOf("_"));
         }
         return name;
      }


      void AssignTexture(TextureArrayConfig cfg, Texture2D tex)
      {
         string name = tex.name;
         name = name.ToLower();
         if (name.EndsWith(cfg.extHeight))
         {
            var compName = name.Substring(0, name.LastIndexOf("_")).ToLower();
            bool found = false;

            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {
               var te = cfg.sourceTextures[i];
               var nm = GetNameRoot(cfg, te);
               if (nm == compName)
               {
                  te.height = tex;
                  found = true;
                  break;
               }
            }
            if (!found)
            {
               TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
               te.height = tex;
               cfg.sourceTextures.Add(te);
            }
         }
         if (name.EndsWith(cfg.extNorm))
         {
            var compName = name.Substring(0, name.LastIndexOf("_")).ToLower();
            bool found = false;
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {
               
               var te = cfg.sourceTextures[i];
               var nm = GetNameRoot(cfg, te);
               if (nm == compName)
               {
                  te.normal = tex;
                  found = true;
                  break;
               }
            }
            if (!found)
            {
               TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
               te.normal = tex;
               cfg.sourceTextures.Add(te);
            }
         }
         if (name.EndsWith(cfg.extAO))
         {
            var compName = name.Substring(0, name.LastIndexOf("_")).ToLower();
            bool found = false;
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {

               var te = cfg.sourceTextures[i];
               var nm = GetNameRoot(cfg, te);
               if (nm == compName)
               {
                  te.ao = tex;
                  found = true;
                  break;
               }
            }
            if (!found)
            {
               TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
               te.ao = tex;
               cfg.sourceTextures.Add(te);
            }
         }
         if (name.EndsWith(cfg.extSmoothness))
         {
            var compName = name.Substring(0, name.LastIndexOf("_")).ToLower();
            bool found = false;
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {

               var te = cfg.sourceTextures[i];
               var nm = GetNameRoot(cfg, te);
               if (nm == compName)
               {
                  te.smoothness = tex;
                  found = true;
                  break;
               }
            }
            if (!found)
            {
               TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
               te.smoothness = tex;
               cfg.sourceTextures.Add(te);
            }
         }
         if (name.EndsWith(cfg.extEmiss))
         {
            var compName = name.Substring(0, name.LastIndexOf("_")).ToLower();
            bool found = false;
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {

               var te = cfg.sourceTextures[i];
               var nm = GetNameRoot(cfg, te);
               if (nm == compName)
               {
                  te.emissive = tex;
                  found = true;
                  break;
               }
            }
            if (!found)
            {
               TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
               te.emissive = tex;
               cfg.sourceTextures.Add(te);
            }
         }
         if (name.EndsWith(cfg.extAlpha))
         {
            var compName = name.Substring(0, name.LastIndexOf("_")).ToLower();
            bool found = false;
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {

               var te = cfg.sourceTextures[i];
               var nm = GetNameRoot(cfg, te);
               if (nm == compName)
               {
                  te.alpha = tex;
                  found = true;
                  break;
               }
            }
            if (!found)
            {
               TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
               te.emissive = tex;
               cfg.sourceTextures.Add(te);
            }
         }
         if (name.EndsWith("_normsao"))
         {
            var compName = name.Substring(0, name.LastIndexOf("_")).ToLower();
            bool found = false;
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {

               var te = cfg.sourceTextures[i];
               var nm = GetNameRoot(cfg, te);
               if (nm == compName)
               {
                  te.smoothness = tex;
                  te.smoothnessChannel = TextureArrayConfig.TextureChannel.B;
                  te.ao = tex;
                  te.aoChannel = TextureArrayConfig.TextureChannel.A;
                  cfg.allTextureChannelSmoothness = TextureArrayConfig.AllTextureChannel.Custom;
                  cfg.allTextureChannelAO = TextureArrayConfig.AllTextureChannel.Custom;
                  found = true;
                  break;
               } 
            }
            if (!found)
            {
               TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
               te.smoothness = tex;
               te.smoothnessChannel = TextureArrayConfig.TextureChannel.B;
               te.ao = tex;
               te.aoChannel = TextureArrayConfig.TextureChannel.A;

               cfg.sourceTextures.Add(te);
            }
         }
      }

      static string ToRel(string path)
      {
         if (path.Contains(Application.dataPath))
         {
            path = "Assets" + path.Replace(Application.dataPath, "");
         }
         path = path.Replace("\\", "/");
         return path;
      }

      void BatchImport(TextureArrayConfig cfg, string folder)
      {
         string[] paths = System.IO.Directory.GetFiles(folder, "*.*", System.IO.SearchOption.AllDirectories);

         cfg.sourceTextures.Clear();
         // put diffuse in
         for (int i = 0; i < paths.Length; ++i)
         {
            if (paths[i].EndsWith(".meta"))
               continue;

            string rel = ToRel(paths[i]);

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(rel);

            if (tex != null)
            {
               if (tex.name.EndsWith(cfg.extDiffuse))
               {
                  TextureArrayConfig.TextureEntry te = new TextureArrayConfig.TextureEntry();
                  te.diffuse = tex;
                  cfg.sourceTextures.Add(te);
               }
            }
         }
         // assign others..
         for (int i = 0; i < paths.Length; ++i)
         {
            if (paths[i].EndsWith(".meta"))
               continue;

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ToRel(paths[i]));

            if (tex != null)
            {
               AssignTexture(cfg, tex);
            }
         }
      }

      public override void OnInspectorGUI()
      {
         var cfg = target as TextureArrayConfig;
         EditorGUI.BeginChangeCheck();

         cfg.uiOpenOutput = MegaSplatUtilities.DrawRollup("Texture Array Output Settings");
         if (cfg.uiOpenOutput)
         {
            DrawDefaultInspector();
         }
         cfg.uiOpenImporter = MegaSplatUtilities.DrawRollup("Batch Importer");
         if (cfg.uiOpenImporter)
         {
            cfg.extDiffuse = EditorGUILayout.TextField("Diffuse Extension", cfg.extDiffuse);
            cfg.extNorm = EditorGUILayout.TextField("Normal Extension", cfg.extNorm);
            cfg.extHeight = EditorGUILayout.TextField("Height Extension", cfg.extHeight);
            cfg.extSmoothness = EditorGUILayout.TextField("Smoothness Extension", cfg.extSmoothness);
            cfg.extAO = EditorGUILayout.TextField("AO Extension", cfg.extAO);
            cfg.extMetal = EditorGUILayout.TextField("Metal Extension", cfg.extMetal);
            cfg.extEmiss = EditorGUILayout.TextField("Emissive Extension", cfg.extEmiss);
            cfg.extAlpha = EditorGUILayout.TextField("Alpha Extension", cfg.extAlpha);

            if (GUILayout.Button("Batch Import"))
            {
               string folder = EditorUtility.OpenFolderPanel("Batch Import", lastImportFolder, "");
               if (!string.IsNullOrEmpty(folder))
               {
                  lastImportFolder = folder;
                  BatchImport(cfg, folder);
               }
            }
         }

         cfg.uiOpenTextures = MegaSplatUtilities.DrawRollup("Textures", true);
         if (cfg.uiOpenTextures)
         {
            EditorGUILayout.HelpBox("Don't have a normal map? Any missing textures will be generated automatically from the best available source texture", MessageType.Info);

            DrawHeader(cfg);
            for (int i = 0; i < cfg.sourceTextures.Count; ++i)
            {
               if (DrawTextureEntry(cfg, cfg.sourceTextures[i], i))
               {
                  var e = cfg.sourceTextures[i];
                  if (!e.HasTextures())
                  {
                     cfg.sourceTextures.RemoveAt(i);
                     i--;
                  }
                  else
                  {
                     e.Reset();
                  }
               }
            }
            if (GUILayout.Button("Add Textures"))
            {
               var entry = new TextureArrayConfig.TextureEntry();
               cfg.sourceTextures.Add(entry);
               entry.aoChannel = cfg.sourceTextures[0].aoChannel;
               entry.heightChannel = cfg.sourceTextures[0].heightChannel;
               entry.smoothnessChannel = cfg.sourceTextures[0].smoothnessChannel;

            }
         }

         if (GUILayout.Button("Update"))
         {
            staticConfig = cfg;
            EditorApplication.delayCall += DelayedCompileConfig;
         }
         if (EditorGUI.EndChangeCheck())
         {
            EditorUtility.SetDirty(cfg);
         }
      

         if (cfg.sourceTextures != null && cfg.sourceTextures.Count > 1)
         {
            if (GUILayout.Button("Sort"))
            {
               cfg.sourceTextures.Sort(
                  delegate(TextureArrayConfig.TextureEntry p1, TextureArrayConfig.TextureEntry p2)
                  {
                     if (p1.diffuse != null && p2.diffuse != null)
                     {
                        return p1.diffuse.name.CompareTo(p2.diffuse.name);
                     }
                     return 0;
                  }
               );
               EditorUtility.SetDirty(cfg);
            }
         }

         cfg.DrawLibraryGUI();
      }

      static Texture2D ResizeTexture(Texture2D source, int width, int height, bool linear)
      {
         RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
         rt.DiscardContents();
         GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear) && !linear;
         Graphics.Blit(source, rt);
         GL.sRGBWrite = false;
         RenderTexture.active = rt;
         Texture2D ret = new Texture2D(width, height, TextureFormat.ARGB32, true, linear);
         ret.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
         ret.Apply(true);
         RenderTexture.active = null;
         rt.Release();
         DestroyImmediate(rt);
         return ret;
      }

      static TextureFormat GetTextureFormat()
      {
         var platform = EditorUserBuildSettings.activeBuildTarget;
         if (platform == BuildTarget.Android)
         {
            return TextureFormat.ETC2_RGBA8;
         }
         else if (platform == BuildTarget.iOS)
         {
            return TextureFormat.PVRTC_RGBA4;
         }
         else
         {
            return TextureFormat.DXT5;
         }
      }

      static Texture2D RenderMissingTexture(Texture2D src, string shaderPath, int width, int height, int channel = -1)
      {
         Texture2D res = new Texture2D(width, height, TextureFormat.ARGB32, true, true);
         RenderTexture resRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
         resRT.DiscardContents();
         Shader s = Shader.Find(shaderPath);
         if (s == null)
         {
            Debug.LogError("Could not find shader " + shaderPath);
            res.Apply();
            return res;
         }
         Material genMat = new Material(Shader.Find(shaderPath));
         if (channel >= 0)
         {
            genMat.SetInt("_Channel", channel);
         }

         GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);
         Graphics.Blit(src, resRT, genMat);
         GL.sRGBWrite = false;

         RenderTexture.active = resRT;
         res.ReadPixels(new Rect(0, 0, width, height), 0, 0);
         res.Apply();
         RenderTexture.active = null;
         resRT.Release();
         DestroyImmediate(resRT);
         DestroyImmediate(genMat);
         return res;
      }

      static void MergeInChannel(Texture2D target, int targetChannel, 
         Texture2D merge, int mergeChannel, bool linear, bool invert = false)
      {
         Texture2D src = ResizeTexture(merge, target.width, target.height, linear);
         Color[] sc = src.GetPixels();
         Color[] tc = target.GetPixels();

         for (int i = 0; i < tc.Length; ++i)
         {
            Color s = sc[i];
            Color t = tc[i];
            t[targetChannel] = s[mergeChannel];
            tc[i] = t;
         }
         if (invert)
         {
            for (int i = 0; i < tc.Length; ++i)
            {
               Color t = tc[i];
               t[targetChannel] = 1.0f - t[targetChannel];
               tc[i] = t;
            }
         }

         target.SetPixels(tc);
         target.Apply();
         DestroyImmediate(src);
      }


      static bool IsLinear(TextureImporter ti)
      {
         #if UNITY_5_5_OR_NEWER
         return ti.sRGBTexture == false;
         #else
         return ti.linearTexture;
         #endif
      }

      static void SetLinear(TextureImporter ti, bool val)
      {
         #if UNITY_5_5_OR_NEWER
         ti.sRGBTexture = !val;
         #else
         ti.linearTexture = val;
         #endif
      }

      #if !UNITY_2017_3_OR_NEWER
      static Texture2D BakeSubstance(string path, ProceduralTexture pt, bool linear = true, bool isNormal = false, bool invert = false)
      {
         string texPath = path + pt.name + ".tga";
         TextureImporter ti = TextureImporter.GetAtPath(texPath) as TextureImporter;
         if (ti != null)
         {
            bool changed = false;
            
            if (!IsLinear(ti) && linear)
            {
               SetLinear(ti, true);
               changed = true;
            }
            else if (IsLinear(ti) && !linear)
            {
               SetLinear(ti, false);
               changed = true;
            }
            if (isNormal && ti.textureType != TextureImporterType.NormalMap)
            {
               ti.textureType = TextureImporterType.NormalMap;
               changed = true;
            }
            if (changed)
            {
               ti.SaveAndReimport();
            }
         }
         var srcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
         return srcTex;
      }

      static void PreprocessTextureEntries(TextureArrayConfig cfg, bool diffuseIsLinear)
      {
         var src = cfg.sourceTextures;
         for (int i = 0; i < src.Count; ++i)
         {
            var e = src[i];
            // fill out substance data if it exists
            if (e.substance != null)
            {
               e.substance.isReadable = true;
               e.substance.RebuildTexturesImmediately();
               string srcPath = AssetDatabase.GetAssetPath(e.substance);

               e.substance.SetProceduralVector("$outputsize", new Vector4(11, 11, 0, 0)); // in mip map space, so 2048

               SubstanceImporter si = AssetImporter.GetAtPath(srcPath) as SubstanceImporter;

               si.SetMaterialScale(e.substance, new Vector2(2048, 2048));
               string path = AssetDatabase.GetAssetPath(cfg);
               path = path.Replace("\\", "/");
               path = path.Substring(0, path.LastIndexOf("/"));
               path += "/SubstanceExports/";
               System.IO.Directory.CreateDirectory(path);
               si.ExportBitmaps(e.substance, path, true);
               AssetDatabase.Refresh();

               Texture[] textures = e.substance.GetGeneratedTextures();
               for (int tidx = 0; tidx < textures.Length; tidx++)
               {
                  ProceduralTexture pt = e.substance.GetGeneratedTexture(textures[tidx].name);

                  if (pt.GetProceduralOutputType() == ProceduralOutputType.Diffuse)
                  {
                     e.diffuse = BakeSubstance(path, pt, diffuseIsLinear);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Height)
                  {
                     e.height = BakeSubstance(path, pt);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.AmbientOcclusion)
                  {
                     e.ao = BakeSubstance(path, pt);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Normal)
                  {
                     e.normal = BakeSubstance(path, pt, true, true);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Smoothness)
                  {
                     e.smoothness = BakeSubstance(path, pt);
                     e.isRoughness = false;
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Roughness)
                  {
                     e.smoothness = BakeSubstance(path, pt, true, false);
                     e.isRoughness = true;
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Metallic)
                  {
                     e.metallic = BakeSubstance(path, pt, true, false);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Opacity)
                  {
                     e.alpha = BakeSubstance(path, pt, true, false);
                  }
                  else if (pt.GetProceduralOutputType() == ProceduralOutputType.Emissive)
                  {
                     e.emissive = BakeSubstance(path, pt, true, false);
                  }
               }
            }

         }
      }
      #endif

      static void SaveArray(Texture2DArray array, string path)
      {
         var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
         if (existing != null)
         {
            EditorUtility.CopySerialized(array, existing);
         }
         else
         {
            AssetDatabase.CreateAsset(array, path);
         }
      }
         
      public static void CompileConfig(TextureArrayConfig cfg)
      {
         bool diffuseIsLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

         #if !UNITY_2017_3_OR_NEWER
         PreprocessTextureEntries(cfg, diffuseIsLinear);
         #endif

         int diffuseWidth = (int)cfg.diffuseTextureSize;
         int diffuseHeight = (int)cfg.diffuseTextureSize;
         int normalWidth = (int)cfg.normalSAOTextureSize;
         int normalHeight = (int)cfg.normalSAOTextureSize;
         int emisWidth = (int)cfg.emisTextureSize;
         int emisHeight = (int)cfg.emisTextureSize;
         int alphaWidth = (int)cfg.alphaTextureSize;
         int alphaHeight = (int)cfg.alphaTextureSize;



         int diffuseAnisoLevel = cfg.diffuseAnisoLevel;
         int normalAnisoLevel = cfg.normalAnisoLevel;
         int emisAnisoLevel = cfg.emisAnisoLevel;
         int alphaAnisoLevel = cfg.alphaAnisoLevel;

         FilterMode diffuseFilter = cfg.diffuseFilterMode;
         FilterMode normalFilter = cfg.normalFilterMode;
         FilterMode emisFilter = cfg.emisFilterMode;
         FilterMode alphaFilter = cfg.alphaFilterMode;



         Texture2DArray diffuseArray = new Texture2DArray(diffuseWidth, diffuseHeight, cfg.sourceTextures.Count, 
            cfg.diffuseCompression == TextureArrayConfig.Compression.AutomaticCompressed ? GetTextureFormat() : TextureFormat.ARGB32, 
            true, diffuseIsLinear);

         diffuseArray.wrapMode = TextureWrapMode.Repeat;
         diffuseArray.filterMode = diffuseFilter;
         diffuseArray.anisoLevel = diffuseAnisoLevel;



         Texture2DArray normalSAOArray = new Texture2DArray(normalWidth, normalHeight, cfg.sourceTextures.Count,
            cfg.normalCompression == TextureArrayConfig.Compression.AutomaticCompressed ? GetTextureFormat() : TextureFormat.ARGB32, 
            true, true);

         normalSAOArray.wrapMode = TextureWrapMode.Repeat;
         normalSAOArray.filterMode = normalFilter;
         normalSAOArray.anisoLevel = normalAnisoLevel;


         Texture2DArray emisArray = new Texture2DArray(emisWidth, emisHeight, cfg.sourceTextures.Count, 
            cfg.emisCompression == TextureArrayConfig.Compression.AutomaticCompressed ? GetTextureFormat() : TextureFormat.ARGB32, 
            true, diffuseIsLinear);

         emisArray.wrapMode = TextureWrapMode.Repeat;
         emisArray.filterMode = emisFilter;
         emisArray.anisoLevel = emisAnisoLevel;


         Texture2DArray alphaArray = new Texture2DArray(alphaWidth, alphaHeight, cfg.sourceTextures.Count, 
            cfg.alphaCompression == TextureArrayConfig.Compression.AutomaticCompressed ? GetTextureFormat() : TextureFormat.ARGB32, 
            true, true);

         alphaArray.wrapMode = TextureWrapMode.Repeat;
         alphaArray.filterMode = alphaFilter;
         alphaArray.anisoLevel = alphaAnisoLevel;

         bool needsEmis = NeedsEmisMetal(cfg);
         bool needsAlpha = NeedsAlpha(cfg);

         for (int i = 0; i < cfg.sourceTextures.Count; ++i)
         {
            try
            {
               EditorUtility.DisplayProgressBar("Packing textures...", "", (float)i/(float)cfg.sourceTextures.Count);

               // first, generate any missing data. We generate a full NSAO map from diffuse or height map
               // if no height map is provided, we then generate it from the resulting or supplied normal. 
               var e = cfg.sourceTextures[i];
               Texture2D diffuse = e.diffuse;
               if (diffuse == null)
               {
                  diffuse = Texture2D.whiteTexture;
               }

               // resulting maps
               Texture2D diffuseHeightTex = ResizeTexture(diffuse, diffuseWidth, diffuseHeight, diffuseIsLinear);
               Texture2D emisTex = ResizeTexture(Texture2D.blackTexture, emisWidth, emisHeight, diffuseIsLinear);
               Texture2D alphaTex = ResizeTexture(Texture2D.whiteTexture, alphaWidth, alphaHeight, true);
               Texture2D normalSAOTex = null;

               int heightChannel = (int)e.heightChannel;
               int aoChannel = (int)e.aoChannel;
               int smoothChannel = (int)e.smoothnessChannel;
               if (cfg.allTextureChannelHeight != TextureArrayConfig.AllTextureChannel.Custom)
               {
                  heightChannel = (int)cfg.allTextureChannelHeight;
               }
               if (cfg.allTextureChannelAO != TextureArrayConfig.AllTextureChannel.Custom)
               {
                  aoChannel = (int)cfg.allTextureChannelAO;
               }
               if (cfg.allTextureChannelSmoothness != TextureArrayConfig.AllTextureChannel.Custom)
               {
                  smoothChannel = (int)cfg.allTextureChannelSmoothness;
               }

               if (e.normal == null)
               {
                  if (e.height == null)
                  {
                     normalSAOTex = RenderMissingTexture(diffuse, "Hidden/MegaSplat/NormalSAOFromDiffuse", normalWidth, normalHeight);
                  }
                  else
                  {
                     normalSAOTex = RenderMissingTexture(e.height, "Hidden/MegaSplat/NormalSAOFromHeight", normalWidth, normalHeight, heightChannel);
                  }
               }
               else
               {
                  // copy, but go ahead and generate other channels in case they aren't provided later.
                  normalSAOTex = RenderMissingTexture(e.normal, "Hidden/MegaSplat/NormalSAOFromNormal", normalWidth, normalHeight);
               }

               bool destroyHeight = false;
               Texture2D height = e.height;
               if (height == null)
               {
                  destroyHeight = true;
                  height = RenderMissingTexture(normalSAOTex, "Hidden/MegaSplat/HeightFromNormal", diffuseHeight, diffuseWidth);
               }

               MergeInChannel(diffuseHeightTex, (int)TextureArrayConfig.TextureChannel.A, height, heightChannel, diffuseIsLinear);


               if (e.ao != null)
               {
                  MergeInChannel(normalSAOTex, (int)TextureArrayConfig.TextureChannel.B, e.ao, aoChannel, true); 
               }

               if (e.smoothness != null)
               {
                  MergeInChannel(normalSAOTex, (int)TextureArrayConfig.TextureChannel.R, e.smoothness, smoothChannel, true, e.isRoughness); 
               }

               if (e.emissive != null)
               {
                  DestroyImmediate(emisTex);
                  emisTex = ResizeTexture(e.emissive, emisWidth, emisHeight, diffuseIsLinear);
               }
               if (e.metallic != null)
               {
                  MergeInChannel(emisTex, (int)TextureArrayConfig.TextureChannel.A, e.metallic, (int)e.metallicChannel, true);
               }

               if (e.alpha != null)
               {
                  DestroyImmediate(alphaTex);
                  alphaTex = ResizeTexture(e.alpha, alphaWidth, alphaHeight, true);
               }



               if (cfg.normalCompression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(normalSAOTex, GetTextureFormat(), TextureCompressionQuality.Normal);
               }

               if (cfg.diffuseCompression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(diffuseHeightTex, GetTextureFormat(), TextureCompressionQuality.Normal);
               }

               if (cfg.emisCompression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(emisTex, GetTextureFormat(), TextureCompressionQuality.Normal);
               }

               if (cfg.alphaCompression != TextureArrayConfig.Compression.Uncompressed)
               {
                  EditorUtility.CompressTexture(alphaTex, GetTextureFormat(), TextureCompressionQuality.Normal);
               }

               normalSAOTex.Apply();
               diffuseHeightTex.Apply();
               emisTex.Apply();
               alphaTex.Apply();

               for (int mip = 0; mip < diffuseHeightTex.mipmapCount; ++mip)
               {
                  Graphics.CopyTexture(diffuseHeightTex, 0, mip, diffuseArray, i, mip);
               }
               for (int mip = 0; mip < normalSAOTex.mipmapCount; ++mip)
               {
                  Graphics.CopyTexture(normalSAOTex, 0, mip, normalSAOArray, i, mip);
               }
               for (int mip = 0; mip < emisTex.mipmapCount; ++mip)
               {
                  Graphics.CopyTexture(emisTex, 0, mip, emisArray, i, mip);
               }
               for (int mip = 0; mip < alphaTex.mipmapCount; ++mip)
               {
                  Graphics.CopyTexture(alphaTex, 0, mip, alphaArray, i, mip);
               }

               DestroyImmediate(diffuseHeightTex);
               DestroyImmediate(normalSAOTex);
               DestroyImmediate(emisTex);
               DestroyImmediate(alphaTex);

               if (destroyHeight)
               {
                  DestroyImmediate(height);
               }
            }
            finally
            {
               EditorUtility.ClearProgressBar();
            }

         }
         EditorUtility.ClearProgressBar();

         diffuseArray.Apply(false, true);
         normalSAOArray.Apply(false, true);
         emisArray.Apply(false, true);
         alphaArray.Apply(false, true);

         string path = AssetDatabase.GetAssetPath(cfg);
         // create array path
         path = path.Replace("\\", "/");
         string diffPath = path.Replace(".asset", "_diff_tarray.asset");
         string normSAOPath = path.Replace(".asset", "_normSAO_tarray.asset");
         string emisPath = path.Replace(".asset", "_emismetal_tarray.asset");
         string alphaPath = path.Replace(".asset", "_alpha_tarray.asset");

         SaveArray(diffuseArray, diffPath);
         SaveArray(normalSAOArray, normSAOPath);
         if (needsEmis)
         {
            SaveArray(emisArray, emisPath);
         }
         if (needsAlpha)
         {
            SaveArray(alphaArray, alphaPath);
         }
            

         cfg.diffuseArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(diffPath);
         cfg.normalSAOArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(normSAOPath);
         if (needsEmis)
         {
            cfg.emissiveArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(emisPath);
         }
         else
         {
            DestroyImmediate(emisArray);
            cfg.emissiveArray = null;
         }
         if (needsAlpha)
         {
            cfg.alphaArray = AssetDatabase.LoadAssetAtPath<Texture2DArray>(alphaPath);
         }
         else
         {
            cfg.alphaArray = null;
            DestroyImmediate(alphaArray);
         }

         EditorUtility.SetDirty(cfg);
         AssetDatabase.Refresh();
         AssetDatabase.SaveAssets();



         Texture2D[] physicsData = null;
         Texture2D scratch = null;

         int physicsMip = 0;
         string physPath = null;


         if (cfg.physicsDataSize != TextureArrayConfig.PhysicsDataSize.None)
         {
            physicsData = new Texture2D[cfg.sourceTextures.Count];
            int size = cfg.diffuseArray.width;
            while (size > (int)cfg.physicsDataSize)
            {
               physicsMip++;
               size /= 2;
            }
            physPath = path.Substring(0, path.LastIndexOf("/"));
            if (!AssetDatabase.IsValidFolder(physPath + "/physics"))
            {
               AssetDatabase.CreateFolder(physPath, "physics");
            }
            physPath += "/physics/";
            physPath = physPath.Replace("Assets/", "/");
            physPath = Application.dataPath + physPath;
         }

        
         List<string> names = new List<string>();
         for (int i = 0; i < cfg.sourceTextures.Count; ++i)
         {
            names.Add(cfg.sourceTextures[i].diffuse == null ? "null" : cfg.sourceTextures[i].diffuse.name);
            if (physicsData != null)
            {
               // copy the texture directly from the source, as compressed. Hopefully this retains the compression
               // artifacts introduced by the compression.
               scratch = new Texture2D((int)cfg.physicsDataSize, (int)cfg.physicsDataSize, cfg.diffuseArray.format, false);
               Graphics.CopyTexture(cfg.diffuseArray, i, physicsMip, scratch, 0, 0);
               scratch.Apply();
               var colors = scratch.GetPixels();
               DestroyImmediate(scratch);

               scratch = new Texture2D((int)cfg.physicsDataSize, (int)cfg.physicsDataSize);
               scratch.SetPixels(colors);

               scratch.Apply();
               var bytes = scratch.EncodeToPNG();
               string texPath = physPath + "/" + names[i] + "_physics.png";
               System.IO.File.WriteAllBytes(texPath, bytes);
               texPath = texPath.Replace(Application.dataPath, "Assets/");
               RenderTexture.active = null;
               GameObject.DestroyImmediate(scratch);

            }

         }
    
         AssetDatabase.Refresh();

         if (cfg.clusterLibrary == null || cfg.clusterLibrary.Count == 0)
         {
            cfg.AutoGenerateClustersNoise();
            EditorUtility.SetDirty(cfg);
         }


         if (physicsData != null)
         {
            for (int i = 0; i < names.Count; ++i)
            {
               string texPath = physPath + names[i] + "_physics.png";
               texPath = texPath.Replace(Application.dataPath, "Assets");
               var ai = AssetImporter.GetAtPath(texPath);
               TextureImporter ti = ai as TextureImporter;
               if (ti == null)
               {
                  Debug.Log(texPath);
               }
               else
               {
                  #if UNITY_5_5_OR_NEWER
                  TextureImporterPlatformSettings def = ti.GetDefaultPlatformTextureSettings();
                  if (ti.textureType != TextureImporterType.Default ||
                     ti.isReadable != true ||
                     ti.mipmapEnabled != false)
                  {
                     ti.textureType = TextureImporterType.Default;
                     def.format = TextureImporterFormat.Alpha8;
                     ti.isReadable = true;
                     ti.mipmapEnabled = false;
                     ti.SaveAndReimport();
                  }
                  #else

                  if (ti.textureType != TextureImporterType.Advanced ||
                      ti.textureFormat != TextureImporterFormat.Alpha8 ||
                      ti.isReadable != true ||
                      ti.mipmapEnabled != false)
                  {
                     ti.textureType = TextureImporterType.Advanced;
                     ti.textureFormat = TextureImporterFormat.Alpha8;
                     ti.isReadable = true;
                     ti.mipmapEnabled = false;
                     ti.SaveAndReimport();
                  }
                  #endif

                  physicsData[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(ti.assetPath);
               }
            }
         }
            
         string tlPath = path.Replace(".asset", "_texlist.asset");
         var tlExisting = AssetDatabase.LoadAssetAtPath<MegaSplatTextureList>(tlPath);
         if (tlExisting != null)
         {
            tlExisting.textureNames = names.ToArray();
            tlExisting.physicsTex = physicsData;
            tlExisting.clusters = cfg.clusterLibrary.ToArray();
            EditorUtility.SetDirty(tlExisting);
         }
         else
         {
            tlExisting = MegaSplatTextureList.CreateInstance<MegaSplatTextureList>();
            tlExisting.textureNames = names.ToArray();
            tlExisting.physicsTex = physicsData;
            tlExisting.clusters = cfg.clusterLibrary.ToArray();
            AssetDatabase.CreateAsset(tlExisting, tlPath);
         }



         AssetDatabase.SaveAssets();



      }
         
   }
}
