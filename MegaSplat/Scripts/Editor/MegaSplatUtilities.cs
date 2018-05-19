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
   
   public class MegaSplatUtilities 
   {

      public static Vector3 WorldToTerrain(Terrain ter, Vector3 point, Texture2D splatControl)
      {
         float x = (point.x / ter.terrainData.size.x) * splatControl.width;
         float z = (point.z / ter.terrainData.size.z) * splatControl.height;
         float y = ter.terrainData.GetHeight((int)x, (int)z);
         return new Vector3(x, y, z);
      }

      public static Vector3 TerrainToWorld(Terrain ter, int x, int y, Texture2D splatControl)
      {
         Vector3 wp = new Vector3(x, 0, y);
         wp.x *= ter.terrainData.size.x / (float)splatControl.width;
         wp.y = ter.terrainData.GetHeight(x, y);
         wp.z *= ter.terrainData.size.z / (float)splatControl.height;
         return ter.transform.localToWorldMatrix.MultiplyPoint(wp);
      }

      public static int DrawTextureSelector(int textureIndex, TextureArrayConfig cfg)
      {
         Texture2D tex = Texture2D.blackTexture;
         if (cfg.sourceTextures != null && cfg.sourceTextures.Count > 0)
         {
            textureIndex = EditorGUILayout.IntSlider("index", textureIndex, 0, cfg.sourceTextures.Count);
            if (cfg.sourceTextures[textureIndex].diffuse != null)
               tex = cfg.sourceTextures[textureIndex].diffuse;
         }
         EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128)), tex);
         return textureIndex;
      }

      public static int DrawClusterSelector(int index, TextureArrayConfig cfg)
      {
         Texture2D tex = Texture2D.blackTexture;
         string name = "";
         cfg.SyncClusterNames();
         SetupSelectionGrid();
         if (cfg.clusterLibrary != null && cfg.clusterLibrary.Count > 0)
         {
            EditorGUILayout.LabelField("index", GUILayout.Width(45));
            index = EditorGUILayout.IntSlider(index, 0, cfg.clusterLibrary.Count-1);

            if (cfg.libraryPreviews != null && index < cfg.libraryPreviews.Length && cfg.libraryPreviews[index] != null)
            {
               tex = cfg.libraryPreviews[index].image as Texture2D;
               name = cfg.libraryPreviews[index].text;
            }
         }
         Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128));
         GUI.DrawTexture(r, tex);
         r.height = 18;
         var v = r.center;
         v.y += 110;
         r.center = v;

         Color contentColor = GUI.contentColor;
         GUI.DrawTexture(r, labelBackgroundTex, ScaleMode.StretchToFill);
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         GUI.Box(r, name);
         GUI.contentColor = contentColor;
         return index;
      }

      // for caching previews
      public class TextureArrayPreviewCache
      {
         public int hash;
         public Texture2D texture;
      }

      static public List<TextureArrayPreviewCache> previewCache = new List<TextureArrayPreviewCache>(20);

      static Texture2D FindInPreviewCache(int hash)
      {
         for (int i = 0; i < previewCache.Count; ++i)
         {
            if (previewCache[i].hash == hash)
               return previewCache[i].texture;
         }
         return null;
      }

      public static int DrawTextureSelector(int textureIndex, Texture2DArray ta, bool compact = false)
      {
         
         Texture2D disp = Texture2D.blackTexture;
         if (ta != null)
         {
            int hash = ta.GetHashCode() * (textureIndex + 7);
            Texture2D hashed = FindInPreviewCache(hash);
            if (hashed == null)
            {
               hashed = new Texture2D(ta.width, ta.height, ta.format, false);
               Graphics.CopyTexture(ta, textureIndex, 0, hashed, 0, 0);
               hashed.Apply(false, false);

               var hd = new TextureArrayPreviewCache();
               hd.hash = hash;
               hd.texture = hashed;
               previewCache.Add(hd);
               if (previewCache.Count > 20)
               {
                  hd = previewCache[0];
                  previewCache.RemoveAt(0);
                  if (hd.texture != null)
                  {
                     GameObject.DestroyImmediate(hd.texture);
                  }
               }

            }
            disp = hashed;
         }
         if (compact)
         {
            EditorGUILayout.BeginVertical();
            EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(110), GUILayout.Height(96)), disp);
            textureIndex = EditorGUILayout.IntSlider(textureIndex, 0, ta.depth - 1, GUILayout.Width(120));
            EditorGUILayout.EndVertical();

         }
         else
         {
            textureIndex = EditorGUILayout.IntSlider("index", textureIndex, 0, ta.depth - 1);
            EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128)), disp);
         }
         return textureIndex;
      }


      static Dictionary<string, bool> rolloutStates = new Dictionary<string, bool>();
      static GUIStyle rolloutStyle;
      public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = GUI.skin.box;
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         var oldColor = GUI.contentColor;
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
         GUI.contentColor = oldColor;
         return rolloutStates[text];
      }

      static Mesh sSphere;
      static Material sMatStandard;
      static PreviewRenderUtility sPru;
      static Color[] sColors;
      static Vector4[] sUV3;
      static Vector3[] sPositions;
      static Vector3[] sNormals;


      static void RenderMeshPreview (Material mat) 
      {
         sSphere.RecalculateBounds();
         // Measure the mesh's bounds so you know where to put the camera and stuff
         Bounds bounds = sSphere.bounds;

         // setup the ObjectPreview's camera
         sPru.m_Camera.backgroundColor = Color.gray;
         sPru.m_Camera.clearFlags = CameraClearFlags.Color;
         sPru.m_Camera.orthographic = true;
         sPru.m_Camera.nearClipPlane = 0.3f;
         sPru.m_Camera.farClipPlane = 40;
         sPru.m_Camera.rect = new Rect(0, 0, 1, 1);
         sPru.m_Camera.orthographicSize = 0.51f;
         sPru.m_Camera.transform.position = new Vector3(0, 0, -4);
         sPru.m_Camera.transform.LookAt(Vector3.zero);

         sPru.m_Light[0].transform.rotation = Quaternion.Euler(30,30,30f);
         sPru.m_Light[0].intensity = 1.0f;
         sPru.m_Light[0].color = Color.white;
         sPru.m_Light[0].type = LightType.Directional;
         sPru.m_Light[0].enabled = true;

         var oldAmbMode = RenderSettings.ambientMode;
         var oldAmbColor = RenderSettings.ambientLight;
         RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
         RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f);
        
        
         bool fog = RenderSettings.fog;
         Unsupported.SetRenderSettingsUseFogNoDirty(false);
         
         sPru.DrawMesh(sSphere, bounds.center, Quaternion.identity, mat, 0);
         sPru.m_Camera.Render();

         RenderSettings.ambientLight = oldAmbColor;
         RenderSettings.ambientMode = oldAmbMode;


         Unsupported.SetRenderSettingsUseFogNoDirty(fog);
         sPru.m_Light[0].enabled = false;
      } 

      static Texture2DArray GetArrayFromConfig(TextureArrayConfig config)
      {
         var path = AssetDatabase.GetAssetPath(config);
         path = path.Replace(".asset", "_tarray.asset");
         return AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
      }

      public static Texture2D RenderBrushSpherePreview(TextureArrayConfig config, TextureArrayConfig.BrushData brushData, int size = 128)
      {
         if (sPru == null)
         {
            sPru = new PreviewRenderUtility(false);
         }
         if (sSphere == null || sPositions == null || sNormals == null || sColors == null || sUV3 == null)
         {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh srcMesh = go.GetComponent<MeshFilter>().sharedMesh;
            sSphere = MeshProcessorWindow.Process(srcMesh);
            sPositions = sSphere.vertices;
            sNormals = sSphere.normals;
            sColors = sSphere.colors;
            sUV3 = new Vector4[sPositions.Length];
            GameObject.DestroyImmediate(go);
         }
         if (sMatStandard == null)
         {
            sMatStandard = new Material(Shader.Find("MegaSplat/BrushPreview_Standard"));
         }
         Material mat = sMatStandard;


         mat.SetFloat("_Contrast", 0.75f);
         mat.SetVector("_TexScales", new Vector4(1, 1, 1, 1));
         mat.SetTexture("_Diffuse", config.diffuseArray);
         mat.SetTexture("_Normal", config.normalSAOArray);
 
         // paint mesh based on cluster data, would be nice to use the brush here, but that would require
         // vertex painter component, etc.. So simulate..
         if (brushData.brushMode == TextureArrayConfig.BrushData.BrushMode.Single)
         {
            if (brushData.layerMode == TextureArrayConfig.BrushData.LayerMode.Bottom)
            {
               for (int i = 0; i < sColors.Length; ++i)
               {
                  sColors[i].a = (float)brushData.singleBottom / 255.0f;
               }
            }
            else if (brushData.layerMode == TextureArrayConfig.BrushData.LayerMode.Top)
            {
               for (int i = 0; i < sColors.Length; ++i)
               {
                  sColors[i].a = (float)brushData.singleTop / 255.0f;
               }
            }
         }
         else
         {
            if (brushData.brushMode == TextureArrayConfig.BrushData.BrushMode.Cluster || brushData.brushMode == TextureArrayConfig.BrushData.BrushMode.Blended)
            {
               if (brushData.layerMode == TextureArrayConfig.BrushData.LayerMode.Bottom)
               {
                  var tc = config.FindInLibrary(brushData.bottom);
                  if (tc != null)
                  {
                     for (int i = 0; i < sColors.Length; ++i)
                     {
                        if (tc.mode == TextureCluster.Mode.Noise)
                        {
                           int index = tc.RemapIndex(tc.noise.GetNoise(sPositions[i] * 10));
                           sColors[i].a = (float)tc.indexes[index] / 255.0f;
                        }
                        else if (tc.mode == TextureCluster.Mode.Angle)
                        {
                           int index = tc.GetIndexForAngle(sNormals[i]);
                           sColors[i].a = (float)tc.indexes[index] / 255.0f;
                        }
                        else if (tc.mode == TextureCluster.Mode.Height)
                        {
                           int index = (int)((1 - Mathf.Clamp(sPositions[i].y * 0.5f + 0.5f, 0, 1.0f)) * tc.indexes.Count);
                           index = Mathf.Clamp(index, 0, tc.indexes.Count - 1);
                           sColors[i].a = (float)tc.indexes[index] / 255.0f;
                        }
                     }
                  }
               }
               if (brushData.layerMode == TextureArrayConfig.BrushData.LayerMode.Top ||  brushData.brushMode == TextureArrayConfig.BrushData.BrushMode.Blended)
               {
                  var tc = config.FindInLibrary(brushData.top);
                  if (tc != null)
                  {
                     for (int i = 0; i < sUV3.Length; ++i)
                     {
                        Vector4 v = sUV3[i];
                        if (tc.mode == TextureCluster.Mode.Noise)
                        {
                           int index = tc.RemapIndex(tc.noise.GetNoise(sPositions[i] * 10));
                           v.w = (float)tc.indexes[index] / 255.0f;
                        }
                        else if (tc.mode == TextureCluster.Mode.Angle)
                        {
                           int index = tc.GetIndexForAngle(sNormals[i]);
                           v.w = (float)tc.indexes[index] / 255.0f;
                        }
                        else if (tc.mode == TextureCluster.Mode.Height)
                        {
                           int index = (int)((1 - Mathf.Clamp(sPositions[i].y * 0.5f + 0.5f, 0, 1.0f)) * tc.indexes.Count);
                           index = Mathf.Clamp(index, 0, tc.indexes.Count - 1);
                           v.w = (float)tc.indexes[index] / 255.0f;
                        }
                        sUV3[i] = v;
                     }
                  }
               }
            }
            if (brushData.brushMode == TextureArrayConfig.BrushData.BrushMode.Blended)
            {
               for (int i = 0; i < sUV3.Length; ++i)
               {
                  Vector4 v = sUV3[i];
                  var n = brushData.layerNoise.GetNoise(sPositions[i] * 7.7f);
                  v.x = n;
                  sUV3[i] = v;
               }
            }
         }

            
         sSphere.colors = sColors;
         sSphere.SetUVs(3, new List<Vector4>(sUV3));

         Rect r = new Rect(0, 0, size, size);
         sPru.BeginStaticPreview(r);
        
         MegaSplatUtilities.RenderMeshPreview(mat);

         var img = sPru.EndStaticPreview();
         img.Apply();
         img.LoadImage(img.EncodeToJPG());

         return img;
      }

      static Texture2D selectedTex = null;
      static Texture2D labelBackgroundTex = null;
      static void SetupSelectionGrid()
      {
         if (selectedTex == null)
         {
            selectedTex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
            for (int x = 0; x < 128; ++x)
            {
               for (int y = 0; y < 128; ++y)
               {
                  if (x < 1 || x > 126 || y < 1 || y > 126)
                  {
                     selectedTex.SetPixel(x, y, new Color(0, 0, 128));
                  }
                  else
                  {
                     selectedTex.SetPixel(x, y, new Color(0, 0, 0, 0));
                  }
               }
            }
            selectedTex.Apply();
         }
         if (labelBackgroundTex == null)
         {
            labelBackgroundTex = new Texture2D(1, 1);
            labelBackgroundTex.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 0.5f));
            labelBackgroundTex.Apply();
         }
      }

      static int DrawSelectionElement(Rect r, int i, int index, Texture2D image, string label)
      {
         SetupSelectionGrid();

         if (GUI.Button(r, "", GUI.skin.box))
         {
            index = i;
         }
         GUI.DrawTexture(r, image != null ? image : Texture2D.blackTexture, ScaleMode.ScaleToFit, false);
         if (i == index)
         {
            GUI.DrawTexture(r, selectedTex, ScaleMode.ScaleToFit, true);
         }

         r.height = 18;
         var v = r.center;
         v.y += 110;
         r.center = v;

         Color contentColor = GUI.contentColor;
         GUI.DrawTexture(r, labelBackgroundTex, ScaleMode.StretchToFill);
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         GUI.Box(r, label);
         GUI.contentColor = contentColor;

         return index;
      }
         
      public static Vector2 scroll;
      public static int SelectionGrid(int index, Texture2D[] contents, int width)
      {
         scroll = EditorGUILayout.BeginScrollView(scroll);
         int w = 0;
         EditorGUILayout.BeginHorizontal();
         for (int i = 0; i < contents.Length; ++i)
         {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128));
            index = DrawSelectionElement(r, i, index, contents[i], contents[i].name);

            w++;
            if (w >= width)
            {
               EditorGUILayout.EndHorizontal();
               w = 0;
               EditorGUILayout.BeginHorizontal();
            }
         }
         var e = Event.current;
         if (e.isKey && e.type == EventType.keyDown)
         {         
            if (e.keyCode == KeyCode.LeftArrow)
            {
               index--;
               e.Use();
            }
            if (e.keyCode == KeyCode.RightArrow)
            {
               index++;
               e.Use();
            }
            if (e.keyCode == KeyCode.DownArrow)
            {
               index += width;
               e.Use();
            }
            if (e.keyCode == KeyCode.UpArrow)
            {
               index -= width;
               e.Use();
            }

         }
         index = Mathf.Clamp(index, 0, contents.Length - 1);

         EditorGUILayout.EndHorizontal();
         EditorGUILayout.EndScrollView();
         return index;
      }

      public static int SelectionGrid(int index, List<TextureArrayConfig.TextureEntry> contents, int width)
      {
         scroll = EditorGUILayout.BeginScrollView(scroll);
         int w = 0;
         EditorGUILayout.BeginHorizontal();
         for (int i = 0; i < contents.Count; ++i)
         {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128));
            index = DrawSelectionElement(r, i, index, contents[i].diffuse, contents[i].diffuse == null ? "null" : contents[i].diffuse.name);

            w++;
            if (w >= width)
            {
               EditorGUILayout.EndHorizontal();
               w = 0;
               EditorGUILayout.BeginHorizontal();
            }
         }
         var e = Event.current;
         if (e.isKey && e.type == EventType.keyDown)
         {         
            if (e.keyCode == KeyCode.LeftArrow)
            {
               index--;
               e.Use();
            }
            if (e.keyCode == KeyCode.RightArrow)
            {
               index++;
               e.Use();
            }
            if (e.keyCode == KeyCode.DownArrow)
            {
               index += width;
               e.Use();
            }
            if (e.keyCode == KeyCode.UpArrow)
            {
               index -= width;
               e.Use();
            }

         }
         index = Mathf.Clamp(index, 0, contents.Count - 1);

         EditorGUILayout.EndHorizontal();
         EditorGUILayout.EndScrollView();
         return index;
      }

      public static int SelectionGrid(int index, GUIContent[] contents, int width)
      {
         scroll = EditorGUILayout.BeginScrollView(scroll);
         int w = 0;
         EditorGUILayout.BeginHorizontal();
         for (int i = 0; i < contents.Length; ++i)
         {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128));
            index = DrawSelectionElement(r, i, index, contents[i].image as Texture2D, contents[i].text);
            w++;
            if (w >= width)
            {
               EditorGUILayout.EndHorizontal();
               w = 0;
               EditorGUILayout.BeginHorizontal();
            }
         }

         var e = Event.current;
         if (e.isKey && e.type == EventType.keyDown)
         {         
            if (e.keyCode == KeyCode.LeftArrow)
            {
               index--;
               e.Use();
            }
            if (e.keyCode == KeyCode.RightArrow)
            {
               index++;
               e.Use();
            }
            if (e.keyCode == KeyCode.DownArrow)
            {
               index += width;
               e.Use();
            }
            if (e.keyCode == KeyCode.UpArrow)
            {
               index -= width;
               e.Use();
            }

         }
         index = Mathf.Clamp(index, 0, contents.Length - 1);
         EditorGUILayout.EndHorizontal();
         EditorGUILayout.EndScrollView();
         return index;
      }
   }
}
