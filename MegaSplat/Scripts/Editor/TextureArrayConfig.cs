//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using JBooth.VertexPainterPro;


namespace JBooth.MegaSplat
{
   [CreateAssetMenu(menuName = "MegaSplat/Texture Array Config", order = 1)]
   public partial class TextureArrayConfig : VertexPainterCustomBrush 
   {
      [HideInInspector] public bool uiOpenTextures = true;
      [HideInInspector] public bool uiOpenOutput = true;
      [HideInInspector] public bool uiOpenImporter = true;

      public enum AllTextureChannel
      {
         R = 0,
         G,
         B,
         A,
         Custom
      }


      public enum TextureChannel
      {
         R = 0,
         G,
         B,
         A
      }

      public enum Compression
      {
         AutomaticCompressed,
         Uncompressed
      }

      public enum TextureSize
      {
         k4096 = 4096,
         k2048 = 2048,
         k1024 = 1024,
         k512 = 512,
         k256 = 256,
      }
         
      [HideInInspector] public Texture2DArray diffuseArray;
      [HideInInspector] public Texture2DArray normalSAOArray;
      [HideInInspector] public Texture2DArray emissiveArray;
      [HideInInspector] public Texture2DArray alphaArray;

      [HideInInspector] public string extDiffuse = "_diff";
      [HideInInspector] public string extHeight = "_height";
      [HideInInspector] public string extNorm = "_norm";
      [HideInInspector] public string extSmoothness = "_smoothness";
      [HideInInspector] public string extAO = "_ao";
      [HideInInspector] public string extMetal = "_metal";
      [HideInInspector] public string extEmiss = "_emis";
      [HideInInspector] public string extAlpha = "_alpha";

      public TextureSize diffuseTextureSize = TextureSize.k1024;
      public Compression diffuseCompression = Compression.AutomaticCompressed;
      public FilterMode diffuseFilterMode = FilterMode.Bilinear;
      public int diffuseAnisoLevel = 1;

      public TextureSize normalSAOTextureSize = TextureSize.k1024;
      public Compression normalCompression = Compression.AutomaticCompressed;
      public FilterMode normalFilterMode = FilterMode.Trilinear;
      public int normalAnisoLevel = 1;

      public TextureSize emisTextureSize = TextureSize.k1024;
      public Compression emisCompression = Compression.AutomaticCompressed;
      public FilterMode emisFilterMode = FilterMode.Bilinear;
      public int emisAnisoLevel = 1;

      public TextureSize alphaTextureSize = TextureSize.k1024;
      public Compression alphaCompression = Compression.AutomaticCompressed;
      public FilterMode alphaFilterMode = FilterMode.Bilinear;
      public int alphaAnisoLevel = 1;

      [HideInInspector]
      public AllTextureChannel allTextureChannelHeight = AllTextureChannel.G;
      [HideInInspector]
      public AllTextureChannel allTextureChannelSmoothness = AllTextureChannel.G;
      [HideInInspector]
      public AllTextureChannel allTextureChannelAO = AllTextureChannel.G;
      [HideInInspector]
      public AllTextureChannel allTextureChannelMetallic = AllTextureChannel.G;
      [HideInInspector]
      public AllTextureChannel allTextureChannelAlpha = AllTextureChannel.A;


      [System.Serializable]
      public class TextureEntry
      {
         #if !UNITY_2017_3_OR_NEWER
         public ProceduralMaterial substance;
         #endif
         public Texture2D diffuse;
         public Texture2D height;
         public TextureChannel heightChannel = TextureChannel.G;
         public Texture2D normal;
         public Texture2D smoothness;
         public TextureChannel smoothnessChannel = TextureChannel.G;
         public bool isRoughness;
         public Texture2D ao;
         public TextureChannel aoChannel = TextureChannel.G;
         public Texture2D emissive;
         public Texture2D metallic;
         public TextureChannel metallicChannel = TextureChannel.G;
         public Texture2D alpha;
         public TextureChannel alphaChannel = TextureChannel.A;

         public void Reset()
         {
            #if !UNITY_2017_3_OR_NEWER
            substance = null;
            #endif
            diffuse = null;
            height = null;
            normal = null;
            smoothness = null;
            ao = null;
            metallic = null;
            emissive = null;
            alpha = null;
            isRoughness = false;
            heightChannel = TextureChannel.G;
            smoothnessChannel = TextureChannel.G;
            aoChannel = TextureChannel.G;
            metallicChannel = TextureChannel.G;
         }

         public bool HasTextures()
         {
            return (
               #if !UNITY_2017_3_OR_NEWER
               substance != null || 
               #endif
               diffuse != null || 
               height != null || 
               normal != null || 
               smoothness != null || 
               ao != null || 
               alpha != null || 
               metallic != null || 
               emissive != null);
         }
      }

      [HideInInspector]
      public List<TextureEntry> sourceTextures = new List<TextureEntry>();
   
      public enum PhysicsDataSize
      {
         None = 0,
         k16  = 16,
         k32  = 32,
         k64  = 64,
         k128 = 128
      };

      [Tooltip("Size of physics textures for displacement surfaces, 32 is usually more than enough")]
      public PhysicsDataSize physicsDataSize = PhysicsDataSize.None;

      [HideInInspector]
      public GUIContent[] libraryPreviews = new GUIContent[0];
      [HideInInspector]
      public string[] libraryNames = new string[0];

      [HideInInspector]
      public List<TextureCluster> clusterLibrary = new List<TextureCluster>();

      [HideInInspector]
      public int hash;




      public int GetNewHash()
      {
         unchecked
         {
            int h = 17;
            h = h * Application.platform.GetHashCode() * 31;
            h = h * Application.unityVersion.GetHashCode() * 37;
            #if UNITY_EDITOR
            h = h * UnityEditor.EditorUserBuildSettings.activeBuildTarget.GetHashCode() * 13;
            #endif
            return h;
         }
      }
         
      public void SyncClusterNames(bool refresh = false)
      {
         if (refresh || libraryPreviews.Length != clusterLibrary.Count)
         {
            libraryPreviews = new GUIContent[clusterLibrary.Count];
         }
         if (refresh || libraryNames.Length != clusterLibrary.Count)
         {
            libraryNames = new string[clusterLibrary.Count];
         }
         for (int i = 0; i < clusterLibrary.Count; ++i) 
         {
            var lp = libraryPreviews[i];
            libraryNames[i] = clusterLibrary[i].name;
            if (lp == null)
            {
               lp = new GUIContent(clusterLibrary[i].name, clusterLibrary[i].name);
              
            }
            if (lp.image == null || refresh)
            {
               clusterLibrary[i].UpdatePreview(this);
               var bytes = clusterLibrary[i].previewData;
               Texture2D tex = new Texture2D(128, 128);
               tex.LoadImage(bytes);
               tex.Apply();
               lp.image = tex;
            }
            libraryPreviews[i] = lp;
         }
      }

      public TextureCluster FindInLibrary(string name)
      {
         for (int i = 0; i < clusterLibrary.Count; ++i)
         {
            if (clusterLibrary[i].name == name)
            {
               return clusterLibrary[i];
            }
         }
         return null;
      }

      int FindIndexInLibrary(string name)
      {
         for (int i = 0; i < clusterLibrary.Count; ++i)
         {
            if (clusterLibrary[i].name == name)
            {
               return i;
            }
         }
         return -1;
      }

      public void AutoGenerateClustersNoise()
      {
         List<TextureCluster> clusters = new List<TextureCluster>();

         List<string> texNames = new List<string>();
         for (int i = 0; i < sourceTextures.Count; ++i)
         {
            string name = sourceTextures[i].diffuse == null ? "nodiffuse" : sourceTextures[i].diffuse.name;
            name = System.Text.RegularExpressions.Regex.Replace(name, "[0-9]", "");
            texNames.Add(name);
         }
         List<string> skips = new List<string>();
         for (int i = 0; i < texNames.Count; ++i)
         {
            string cur = texNames[i];
            if (skips.Contains(cur))
            {
               continue;
            }
            List<int> indexes = new List<int>();
            indexes.Add(i);
            for (int x = i + 1; x < texNames.Count; ++x)
            {
               if (cur == texNames[x])
               {
                  indexes.Add(x);
               }
            }
            if (indexes.Count > 0)
            {
               TextureCluster tc = new TextureCluster();
               tc.name = cur.Replace("__", "_");
               tc.indexes = indexes;
               tc.noise = new NoiseParams();
               clusters.Add(tc);
               if (FindInLibrary(tc.name) == null)
               {
                  clusterLibrary.Add(tc);
               }
               tc.UpdatePreview(this);
            }
            skips.Add(cur);
         }

         if (clusterLibrary == null)
         {
            clusterLibrary = new List<TextureCluster>();
         }
         
         for (int i = 0; i < clusters.Count; ++i)
         {
            int idx = FindIndexInLibrary(clusters[i].name);
            if (idx < 0)
            {
               clusterLibrary.Add(clusters[i]);
            }
            else
            {
               // exists, overwrite
               clusterLibrary[idx] = clusters[i];
            }
         }


         SyncClusterNames(true);
         EditorUtility.SetDirty(this);
      }



      public void DrawLibraryGUI()
      {
         if (!MegaSplatUtilities.DrawRollup("Cluster Library"))
            return;

         EditorGUILayout.HelpBox("Press to generate clusters based on numeric naming convention\n(same name, with 01, 02, etc)", MessageType.Info);
         if (GUILayout.Button("Clear Clusters"))
         {
            clusterLibrary.Clear();
         }
         if (GUILayout.Button("Auto Generate Clusters"))
         {
            AutoGenerateClustersNoise();
         }

         if (GUILayout.Button("Create New Entry"))
         {
            clusterLibrary.Add(new TextureCluster());
            EditorUtility.SetDirty(this);
         }

         for (int i = 0; i < clusterLibrary.Count; ++i)
         {
            string label = "New Entry";
            if (!string.IsNullOrEmpty(clusterLibrary[i].name))
            {
               label = clusterLibrary[i].name;
            }
            bool draw = clusterLibrary[i].DrawLayer(label, this);
            if (draw && GUILayout.Button("Delete From Library"))
            {
               clusterLibrary.RemoveAt(i);
               i--;
               EditorUtility.SetDirty(this);
               continue;
            }
         }

      }

      public string DrawTextureClusterSelection(string current, string label, bool drawImages = true)
      {
         SyncClusterNames();
         int idx = 0;
         for (int i = 0; i < libraryPreviews.Length; ++i)
         {
            if (libraryPreviews[i].tooltip == current)
            {
               idx = i;
               break;
            }
         }
         if (libraryPreviews.Length > idx)
         {
            if (drawImages)
            {
               int numPerWidth = (int)EditorGUIUtility.currentViewWidth / 128;
               if (numPerWidth < 1)
                  numPerWidth = 1;
               
               int newIdx = MegaSplatUtilities.SelectionGrid(idx, libraryPreviews, numPerWidth);
               return libraryPreviews[newIdx].tooltip;
            }
            else
            {
               
               int newIdx = EditorGUILayout.Popup(label, idx, libraryNames); 
               return libraryPreviews[newIdx].tooltip;
            }
         }
         return "";
      }
   }
}
