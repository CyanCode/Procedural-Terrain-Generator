//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using JBooth.MegaSplat;

namespace JBooth.TerrainPainter
{
   public partial class TerrainPainterWindow : EditorWindow 
   {
      double deltaTime = 0;
      double lastTime = 0;
      bool painting = false;
      Vector3 oldMousePosition;

      public Vector3         oldpos = Vector3.zero;
      public float           brushSize = 1;
      public float           brushFlow = 8;
      public float           brushFalloff = 1; // linear

      public System.Action<TerrainJob[]> OnBeginStroke;
      public System.Action<TerrainJob, bool> OnStokeModified;  // bool is true when doing a fill or other non-bounded opperation
      public System.Action OnEndStroke;
     

      public enum BrushVisualization
      {
         Sphere,
         Disk
      }
      public BrushVisualization brushVisualization = BrushVisualization.Sphere;

      public Vector2 lastHitUV;
      public Vector2 lastMousePosition;
      void OnSceneGUI(SceneView sceneView)
      {
         if (config == null)
            return;
       
         deltaTime = EditorApplication.timeSinceStartup - lastTime;
         lastTime = EditorApplication.timeSinceStartup;

         if (terrains == null || terrains.Length == 0 && Selection.activeGameObject != null)
         {
            InitTerrains();
         }

         if (!enabled || terrains.Length == 0 || Selection.activeGameObject == null)
         {
            return;
         }

         RaycastHit hit;
         float distance = float.MaxValue;
         Vector3 mousePosition = Event.current.mousePosition;
         Vector2 uv = Vector2.zero;

         // So, in 5.4, Unity added this value, which is basically a scale to mouse coordinates for retna monitors.
         // Not all monitors, just some of them.
         // What I don't get is why the fuck they don't just pass me the correct fucking value instead. I spent hours
         // finding this, and even the paid Unity support my company pays many thousands of dollars for had no idea
         // after several weeks of back and forth. If your going to fake the coordinates for some reason, please do
         // it everywhere to not just randomly break things everywhere you don't multiply some new value in. 
         float mult = EditorGUIUtility.pixelsPerPoint;

         mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y * mult;
         mousePosition.x *= mult;
         Vector3 fakeMP = mousePosition;
         fakeMP.z = 20;
         Vector3 point = sceneView.camera.ScreenToWorldPoint(fakeMP);
         Vector3 normal = Vector3.forward;
         Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);

         bool registerUndo = (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt == false);

         for (int i = 0; i < terrains.Length; ++i)
         {
            if (terrains[i] == null)
               continue;
            // Early out if we're not in the area..
            var cld = terrains[i].collider;
            Bounds b = cld.bounds;
            b.Expand(brushSize*2);
            if (!b.IntersectRay(ray))
            {
               continue;
            }

            if (registerUndo)
            {
               painting = true;
               for (int x = 0; x < jobEdits.Length; ++x)
               {
                  jobEdits[x] = false;
               }
               if (i == 0 && OnBeginStroke != null)
               {
                  OnBeginStroke(terrains);
               }
            }

            if (cld.Raycast(ray, out hit, float.MaxValue))
            {
               if (Event.current.shift == false) 
               {
                  if (hit.distance < distance) 
                  {
                     uv = hit.textureCoord;
                     distance = hit.distance;
                     point = hit.point;
                     normal = hit.normal;
                  }
               } 
               else 
               {
                  point = oldpos;
               }
            } 
            else 
            {
               if (Event.current.shift == true) 
               {
                  point = oldpos;
               }
            }  
         }

         if (Event.current.type == EventType.MouseMove && Event.current.shift) 
         {
            brushSize += Event.current.delta.x * (float)deltaTime * 6.0f;
            brushFalloff -= Event.current.delta.y * (float)deltaTime * 48.0f;
         }

         if (Event.current.rawType == EventType.MouseUp)
         {
            EndStroke();
         }
         if (Event.current.type == EventType.MouseMove && Event.current.alt)
         {
            brushSize += Event.current.delta.y * (float)deltaTime;
         }
            

         if (brushVisualization == BrushVisualization.Sphere)
         {
            #if UNITY_5_6_OR_NEWER
            Handles.SphereHandleCap(0, point, Quaternion.identity, brushSize * 2, EventType.Repaint);
            #else
            Handles.SphereCap(0, point, Quaternion.identity, brushSize * 2);
            #endif
         }
         else
         {
            Handles.color = new Color(0.8f, 0, 0, 1.0f);
            float r = Mathf.Pow(0.5f, brushFalloff);
            Handles.DrawWireDisc(point, normal, brushSize * r);
            Handles.color = new Color(0.9f, 0, 0, 0.8f);
            Handles.DrawWireDisc(point, normal, brushSize);
         }
         // eat current event if mouse event and we're painting
         if (Event.current.isMouse && painting)
         {
            Event.current.Use();
         } 

         if (Event.current.type == EventType.Layout)
         {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
         }

         // only paint once per frame
         if (Event.current.type != EventType.Repaint)
         {
            return;
         }


         if (terrains.Length > 0 && painting)
         {
            for (int i = 0; i < terrains.Length; ++i)
            {
               Bounds b = terrains[i].collider.bounds;
               b.Expand(brushSize * 2);
               if (!b.IntersectRay(ray))
               {
                  continue;
               }
               if (jobEdits[i] == false)
               {
                  jobEdits[i] = true;
                  terrains[i].RegisterUndo();
               }
               PaintTerrain(terrains[i].terrain, point, uv);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], false);
               }
            }
         }

         lastHitUV = uv;
         lastMousePosition = Event.current.mousePosition;
         // update views
         sceneView.Repaint();
         HandleUtility.Repaint();
      }


      void EndStroke()
      {
         painting = false;
         if (OnEndStroke != null)
         {
            OnEndStroke();
         }
      }

      void PaintTerrain(Terrain t, Vector3 worldPoint, Vector2 uv)
      {
         if (t.materialTemplate == null)
            return;
         // convert point into local space, so we don't have to convert every point
         Vector3 localPoint = t.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
         // for some reason this doesn't handle scale, seems like it should
         // we handle it poorly until I can find a better solution
         float scale = 1.0f / Mathf.Abs(t.transform.lossyScale.x);

         float bz = scale * brushSize;

         float pressure = Event.current.pressure > 0 ? Event.current.pressure : 1.0f;

         Texture2D splatTex = t.materialTemplate.GetTexture("_SplatControl") as Texture2D;
         Texture2D paramTex = t.materialTemplate.GetTexture("_SplatParams") as Texture2D;
         if (splatTex == null)
            return;
         Vector3 terPoint = MegaSplatUtilities.WorldToTerrain(t, localPoint, splatTex);

         if (terPoint.x >= 0 && terPoint.z >= 0 && terPoint.x < splatTex.width || terPoint.z < splatTex.height)
         {
            // scale brush into texture space
            Vector3 offsetPnt = localPoint - new Vector3(bz, 0, bz);
            Vector3 beginTerPnt = MegaSplatUtilities.WorldToTerrain(t, offsetPnt, splatTex);
            beginTerPnt.x = Mathf.Clamp(beginTerPnt.x, 0, splatTex.width);
            beginTerPnt.z = Mathf.Clamp(beginTerPnt.z, 0, splatTex.height);

            Vector3 offset = terPoint - beginTerPnt;
            int pbx = (int)beginTerPnt.x;
            int pby = (int)beginTerPnt.z;
            int pex = (int)(terPoint.x + offset.x * 2.0f);
            int pey = (int)(terPoint.z + offset.z * 2.0f);

            pex = Mathf.Clamp(pex, 0, splatTex.width);
            pey = Mathf.Clamp(pey, 0, splatTex.height);

            for (int x = pbx; x < pex; ++x)
            {
               for (int y = pby; y < pey; ++y)
               {
                  float h = t.terrainData.GetHeight(x, y);
                  Vector3 n = t.terrainData.GetInterpolatedNormal(x, y);
                  float d = Vector3.Distance(terPoint, new Vector3(x, h, y));
                  float str = 1.0f - d / bz;
                  str = Mathf.Pow(str, brushFalloff);
                  float finalStr = str * (float)deltaTime * brushFlow * pressure;
                  if (finalStr > 0)
                  {
                     
                     if (tab == Tab.Paint)
                     {
                        Color c = splatTex.GetPixel(x, y);
                        c = config.GetValues(t, c, config.brushData, MegaSplatUtilities.TerrainToWorld(t, x, y, splatTex), h, n, finalStr);
                        splatTex.SetPixel(x, y, c);
                     }
                     else if (tab == Tab.Dampening)
                     {
                        Color c = splatTex.GetPixel(x, y);
                        c.a = Mathf.Lerp(c.a, 1 - dampeningValue, finalStr);
                        splatTex.SetPixel(x, y, c);
                     }
                     else if (tab == Tab.Wetness && paramTex != null)
                     {
                        Color c = paramTex.GetPixel(x, y);
                        c.b = Mathf.Lerp(c.b, wetnessValue, finalStr);
                        paramTex.SetPixel(x, y, c);
                     }
                     else if (tab == Tab.Puddles && paramTex != null)
                     {
                        Color c = paramTex.GetPixel(x, y);
                        c.a = Mathf.Lerp(c.a, puddleValue, finalStr);
                        paramTex.SetPixel(x, y, c);
                     }
                     else if (tab == Tab.Flow && paramTex != null)
                     {
                        // need to move into terrain space, etc..
                        Color c = paramTex.GetPixel(x, y);
                        Vector2 delta = uv - lastHitUV;
                        float l = (Event.current.mousePosition - lastMousePosition).magnitude;
                        delta *= (l * brushFlow);
                        delta.x = Mathf.Clamp(delta.x, -1, 1);
                        delta.y = Mathf.Clamp(delta.y, -1, 1);
                        c.r = Mathf.Lerp(c.r, (delta.x * 0.5f + 0.5f), finalStr);
                        c.g = Mathf.Lerp(c.g, (delta.y * 0.5f + 0.5f), finalStr);
                        paramTex.SetPixel(x, y, c);
                     }


                  }
               }
            }
            if (tab == Tab.Dampening || tab == Tab.Paint)
            {
               splatTex.Apply();
            }
            else if (paramTex != null)
            {
               paramTex.Apply();
            }

         }
      }

   }
}