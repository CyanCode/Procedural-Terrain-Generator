//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using JBooth.VertexPainterPro;
using UnityEditor;
using System.Collections.Generic;
using JBooth.TerrainPainter;
using JBooth.MegaSplat;

[CreateAssetMenu(menuName = "MegaSplat/Terrain To MegaSplat Config", fileName = "TerrainToSplatConfig")]
public class TerrainToMegaSplatConfig : ScriptableObject 
{
   [System.Serializable]
   public class Entry
   {
      public int mainCluster;
      public float contrast = 1;
      public int secondaryCluster;
      public bool useSecondCluster;
      public float noiseFrequency = 0.05f;
      public Vector2 noiseRange = new Vector2(0.0f, 0.5f);
      public float noiseSeed = 0;
   }

   public TextureArrayConfig config;
   public float clusterNoiseScale = 0.05f;
   public List<Entry> clusterMap = new List<Entry>();
   static Texture2D labelBackgroundTex;

   public float blendBias = 0.0f;
   public float blurSize = 0;


   public void DrawGUI(TerrainJob[] jobs)
   {
      if (labelBackgroundTex == null)
      {
         labelBackgroundTex = new Texture2D(1, 1);
         labelBackgroundTex.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 0.5f));
         labelBackgroundTex.Apply();
      }


      config = (TextureArrayConfig)EditorGUILayout.ObjectField("config", config, typeof(TextureArrayConfig), false);
      clusterNoiseScale = EditorGUILayout.FloatField(new GUIContent("Cluster Noise Scale", "Global Scale for cluster Noise"), clusterNoiseScale);
      blendBias = EditorGUILayout.Slider(new GUIContent("Blend Biasing", "In areas with partial blends between terrains, biases that blend towards 0.5"), blendBias, 0.0f, 1.0f);
      blurSize = EditorGUILayout.Slider(new GUIContent("Blend Blur", "Blurs the input data, creating a wider blend area"), blurSize, 0, 40);


      if (jobs.Length == 0)
         return;

      if (config == null)
      {
         EditorGUILayout.HelpBox("Please assign the config", MessageType.Info);
         return;
      }
      if (config.clusterLibrary == null || config.clusterLibrary.Count == 0)
      {
         EditorGUILayout.HelpBox("Config must have a cluster library; select the config and generate one", MessageType.Info);
         return;
      }
      if (jobs.Length == 0)
         return;
      if (jobs[0].terrain == null || jobs[0].terrain.terrainData == null)
         return;

      if (clusterMap.Count > 0)
      {
         if (GUILayout.Button("Convert"))
         {
            Convert(jobs);
         }
      }
      var prototypes = jobs[0].terrain.terrainData.splatPrototypes;
      for (int i = 0; i < prototypes.Length; ++i)
      {
         var src = prototypes[i].texture;
         if (i >= clusterMap.Count)
         {
            clusterMap.Add(new Entry());
         }
         EditorGUILayout.BeginHorizontal();
         Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128));

         EditorGUI.DrawPreviewTexture(r,  src != null ? src : Texture2D.blackTexture);
         r.height = 18;
         var v = r.center;
         v.y += 110;
         r.center = v;

         Color contentColor = GUI.contentColor;
         GUI.DrawTexture(r, labelBackgroundTex, ScaleMode.StretchToFill);
         GUI.contentColor = Color.white;
         if (src != null)
         {
            GUI.Box(r, src);
         }
         GUI.contentColor = contentColor;

         var map = clusterMap[i];
         map.mainCluster = MegaSplatUtilities.DrawClusterSelector(map.mainCluster, config);
         EditorGUILayout.EndHorizontal();
         map.contrast = EditorGUILayout.Slider("Contrast", map.contrast, 1.5f, 0.5f);
         map.useSecondCluster = EditorGUILayout.Toggle("Secondary Cluster", map.useSecondCluster);


         if (map.useSecondCluster)
         {
            EditorGUI.indentLevel += 4;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Cluster");
            map.secondaryCluster = MegaSplatUtilities.DrawClusterSelector(map.secondaryCluster, config);
            EditorGUILayout.EndHorizontal();
            map.noiseFrequency = EditorGUILayout.FloatField("Noise Frequency", map.noiseFrequency);
            EditorGUILayout.MinMaxSlider(new GUIContent("Noise Range"), ref map.noiseRange.x, ref map.noiseRange.y, 0.0f, 1.0f);
            map.noiseSeed = EditorGUILayout.FloatField("Noise Seed", map.noiseSeed);
            EditorGUI.indentLevel -= 4;
         }


         GUILayout.Box(Texture2D.blackTexture, GUILayout.Height(2), GUILayout.ExpandWidth(true));
      }

      if (clusterMap.Count == 0)
      {
         EditorGUILayout.LabelField("Source Terrain has no textures, there is nothing to convert!");
         return;
      }

      if (GUILayout.Button("Convert"))
      {
         Convert(jobs);
      }
   }

   // guassian blur the terrain data- will change terrain bondaries, but create more blending overall..
   void Blur(float[,,] data, int layers, int width, int height, float[] contrasts)
   {
      Material mat = new Material(Shader.Find("Hidden/MegaSplatGaussianBlur"));
      mat.SetFloat("_Blur", blurSize/(float)width);

      RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      Texture2D tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
      rt.wrapMode = TextureWrapMode.Clamp;
      tex.wrapMode = TextureWrapMode.Clamp;
      for (int l = 0; l < layers; ++l)
      {
         mat.SetFloat("_Contrast", contrasts[l]);
         rt.DiscardContents();
         for (int x = 0; x < width; ++x)
         {
            for (int y = 0; y < height; ++y)
            {
               float v = data[x, y, l];
               tex.SetPixel(x, y, new Color(v,v,v, v));
            }
         }
         tex.Apply();
         Graphics.Blit(tex, rt, mat);
         RenderTexture.active = rt;
         tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
         tex.Apply();
         for (int x = 0; x < width; ++x)
         {
            for (int y = 0; y < height; ++y)
            {
               Color c = tex.GetPixel(x, y);
               data[x, y, l] = c.r;
            }
         }
      }
      RenderTexture.active = null;
      rt.Release();
      DestroyImmediate(rt);
      DestroyImmediate(mat);
      DestroyImmediate(tex);
   }

   void Convert(TerrainJob[] jobs)
   {
      foreach (var job in jobs)
      {
         var tex = job.terrain.materialTemplate.GetTexture("_SplatControl") as Texture2D;
         var path = AssetDatabase.GetAssetPath(tex);
         var terrainData = job.terrain.terrainData;
         int width = terrainData.alphamapWidth;
         int height = terrainData.alphamapHeight;


         if (tex.width != width || tex.height != height || tex.format != TextureFormat.ARGB32)
         {
            tex = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
         }
         var splatmapData = terrainData.GetAlphamaps(0, 0, width, height);
         int alphamaplayers = terrainData.alphamapLayers;

         float[] contrasts = new float[alphamaplayers];
         for (int i = 0; i < alphamaplayers; ++i)
         {
            contrasts[i] = 0;
            if (clusterMap.Count > i)
            {
               contrasts[i] = clusterMap[i].contrast;
            }
         }

         Blur(splatmapData, alphamaplayers, width, height, contrasts);

         for (int x = 0; x < width; ++x)
         {
            EditorUtility.DisplayProgressBar("Processing...", job.terrain.name, (float)x / (float)width);
            for (int y = 0; y < height; ++y)
            {
               int botIdx = 0;
               int topIdx = 0;
               float botWeight = 0;
               float topWeight = 0;

               // get two most dominant textures
               for (int layer = 0; layer < alphamaplayers; layer++)
               {
                  float w = splatmapData[y, x, layer];
                  if (w > botWeight)
                  {
                     topWeight = botWeight;
                     topIdx = botIdx;

                     botWeight = w;
                     botIdx = layer;
                  }
                  else if (w > topWeight)
                  {
                     topIdx = layer;
                     topWeight = w;
                  }

               }
               //swapping indexes to make topIdx always on top
               if (botIdx > topIdx) 
               {
                  int tempIdx = topIdx;
                  topIdx = botIdx;
                  botIdx = tempIdx;

                  float tempWeight = topWeight;
                  topWeight = botWeight;
                  botWeight = tempWeight;
               }

               // remap to 2 cluster choices and a blend weight
               var firstMap = clusterMap[topIdx];
               var secondMap = clusterMap[botIdx];
               topIdx = firstMap.mainCluster;
               botIdx = secondMap.mainCluster;
               float totalWeight = topWeight + botWeight;   
               if (totalWeight < 0.01f)
               {
                  totalWeight = 0.01f;
               }
               float blend = botWeight / totalWeight;    
               if (blend > 1.0f)
               {
                  blend = 1.0f;
               }

               // generate pos/normal/hr for cluster fetch
               Vector3 pos = (MegaSplatUtilities.TerrainToWorld(job.terrain, x, y, tex));
               Vector3 scalePos = pos * clusterNoiseScale;
               Vector3 normal = terrainData.GetInterpolatedNormal(x, y);
               float hr = Mathf.Clamp01(pos.y / terrainData.heightmapHeight);

               // get clusters
               var tc0 = config.clusterLibrary[topIdx];
               var tc1 = config.clusterLibrary[botIdx];

               // get resulting index's in the array, based on cluster noise
               float finalIndex0 = tc0.GetIndex(scalePos, normal, hr) / 255.0f;
               float finalIndex1 = tc1.GetIndex(scalePos, normal, hr) / 255.0f;

               // since megasplat looks best blending, blend in another optional cluster
               // we we have areas of sameness
               if (firstMap.useSecondCluster)
               {
                  // compute weight of secondary cluster based on weight of first chosen cluster
                  // basically, we only want to ramp in the secondary cluster if the weight of the first
                  // cluster is dominant
                  float w = 1-blend;
                  // scale it such that we don't really do this around actual transitions
                  w *= w; w *= w; w *= w;
                  // generate noise and range map it
                  Vector3 nPos = pos;
                  nPos.x += firstMap.noiseSeed;
                  nPos.y -= firstMap.noiseSeed;
                  nPos.z += firstMap.noiseSeed;
                  nPos *= firstMap.noiseFrequency;
                  float n = Noise.Generate(nPos.x, nPos.y, nPos.z);
                  float amt = Mathf.Lerp(firstMap.noiseRange.x, firstMap.noiseRange.y * 2, n);
                  amt *= w;
                  amt = Mathf.Clamp01(amt);
                  // only use if we're more dominant
                  if (amt > blend)
                  {
                     tc1 = config.clusterLibrary[firstMap.secondaryCluster];
                     finalIndex1 = tc1.GetIndex(nPos, normal, hr) / 255.0f;
                     blend = amt;
                  }
               }
               if (blend > 0.01f && blend < 0.99f)
               {
                  blend = Mathf.Lerp(blend, 0.5f, (1.0f - Mathf.Abs((blend - 0.5f) * 2)) * blendBias);
               }
               Color finalColor = new Color(finalIndex0, finalIndex1, blend);
               tex.SetPixel(x, y, finalColor);
            }
         }
         tex.Apply();

         EditorUtility.ClearProgressBar();
         var bytes = tex.EncodeToPNG();
         System.IO.File.WriteAllBytes(path, bytes);

         AssetDatabase.Refresh();

      }
   }
}
