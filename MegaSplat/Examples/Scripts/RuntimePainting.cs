//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

// Example of how to paint at runtime..
// Includes very optimized vertex routines with lots of notes
// You can just use this as a runtime brush object as well..

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JBooth.VertexPainterPro;
using JBooth.MegaSplat;

public class RuntimePainting : MonoBehaviour 
{
   public enum Mode
   {
      Texture,
      TextureCluster,
      Puddles
   }

   public Mode mode = Mode.Texture;
   public MegaSplatTextureList textureList;

   public int textureIndex;         // texture index in array to set
   public enum LayerMode
   {
      Top,
      Bottom
   }
   public LayerMode layerMode = LayerMode.Top;
   public float targetWeight = 1;   // target weight between layers

   // brush params, to match painters..
   public float brushSize = 1;
   public float brushFalloff = 1;
   public float brushFlow = 1;

   public float clusterNoiseScale = 0.05f;

   // because the Unity Editor is stupid, and makes allocations when you call GetComponent that don't exist at runtime.
   // This adds up to 0.6k a frame, which is crazy and distorts profiling, so we use GetComponents when in editor mode,
   // which is totally dumb, but works around the issue..
   // We also store off any textures we collide with at runtime, so we can revert them when we exit play mode.
   // I haven't found a wonderful way to do this, so I copy the texture off before any modifications
   // happen, then restore on application quit. This isn't needed in the real game, but makes
   // editing easier otherwise you have to reload the editor to revert the texture changes.
   #if UNITY_EDITOR
   List<Terrain> noAllocTerrains = new List<Terrain>(1);
   List<VertexInstanceStream> noAllocStreams = new List<VertexInstanceStream>(1);

   public class RestoreEntry
   {
      public Material mat;
      public Texture2D texCopy;
      public Texture2D srcTex;
   }

   List<RestoreEntry> generatedData = new List<RestoreEntry>();

   Texture2D EncounteredTexture(Material mat, Texture2D tex)
   {
      for (int i = 0; i < generatedData.Count; ++i)
      {
         if (generatedData[i].texCopy == tex)
            return tex;
      }
      RestoreEntry re = new RestoreEntry();
      Texture2D ntex = new Texture2D(tex.width, tex.height, tex.format, false, true);
      Graphics.CopyTexture(tex, ntex);
      re.srcTex = tex;
      re.texCopy = ntex;
      re.mat = mat;
      mat.SetTexture(splatControlId, ntex);
      generatedData.Add(re);
      return ntex;
   }

   void OnApplicationQuit()
   {
      for (int i = 0; i < generatedData.Count; ++i)
      {
         generatedData[i].mat.SetTexture(splatControlId, generatedData[i].srcTex);
         DestroyImmediate(generatedData[i].texCopy);
      }
      generatedData.Clear();
   }
   #endif

   // cache off the splat control property id for speed.
   int splatControlId;

   void Start()
   {
      splatControlId = Shader.PropertyToID("_SplatControl");
   }
  

   void Update()
   {
      // raycast down to find a hit
      RaycastHit hit;
      Ray ray = new Ray(transform.position, Vector3.down);
      if (Physics.Raycast(ray, out hit, 3))
      {
         Terrain terrain = null;
         #if UNITY_EDITOR
         hit.collider.GetComponents<Terrain>(noAllocTerrains);
         if (noAllocTerrains.Count > 0)
            terrain = noAllocTerrains[0];
         #else
         terrain = hit.collider.GetComponent<Terrain>();
         #endif

         if (terrain != null)
         {
            PaintTerrain(terrain, hit.point);
            return;
         }

         // update the stream. Note that the collision info's array should be updated automatically, 
         // since it references this array instead of copying it.
         // Mesh must be read/write for this to work. No way around it.

         // Note the optimizations here; unrolling Vector3.Distance and Mathf.Lerp; doing so
         // cut this from 5ms to 1ms on my machine, using a 30,000 vertex mesh. Obviously
         // splitting the mesh would help, but some very basic optimization can have a huge
         // impact on performance, so don't do anything complex until you know you have too..

         // unity editor allocs on GetComponent, so for profiling I prefer to work around it.
         VertexInstanceStream stream = null;
         #if UNITY_EDITOR
         hit.collider.GetComponents<VertexInstanceStream>(noAllocStreams);
         if (noAllocStreams.Count > 0)
            stream = noAllocStreams[0];
         #else
         stream = hit.collider.GetComponent<VertexInstanceStream>();
         #endif
         if (stream != null)
         {
            // cache as locals, which is faster than referencing every time.
            var positions = stream.positions;
            var colors = stream.colors;
            var uv3 = stream.uv3;
            float delta = Time.deltaTime;

            bool hasUV3 = (uv3 == null || uv3.Count == positions.Length);

            if ((mode == Mode.Texture || mode == Mode.TextureCluster) && layerMode == LayerMode.Top && !hasUV3)
            {
               return;
            }
            if (colors == null || colors.Length != positions.Length)
            {
               return;
            }

            // convert the hit point into local space, rather than having to convert each vertex!
            Vector3 pnt = hit.point;
            var mtx = hit.collider.transform.worldToLocalMatrix;
            pnt = mtx.MultiplyPoint(pnt);
            float scale = 1.0f / Mathf.Abs(hit.collider.transform.lossyScale.x);

            float bz = scale * brushSize;
            // Vector3.Distance is stupidly slow. It casts things to doubles, calls to c++, etc.
            // If we pre-square the brush size and cache off the pnt vector, then unroll the vector math,
            // we make it roughly 10 times as fast.

            bz = bz * bz;

            // flatten to avoid slow vector math..
            float px = pnt.x;
            float py = pnt.y;
            float pz = pnt.z;

            // texture index is stored in 0-1 value, so we remap here.
            float ti = (float)textureIndex / 255.0f;


            // only want to update if things actually change
            bool changed = false;
            // cache local for speed
            int length = positions.Length;
            for (int i = 0; i < length; ++i)
            {
               // This is just Vector3.Distance unrolled withut the sqrt. Over 10 times faster
               // than doing float dist = Vector3.Distance(pnt, positions[i[);
               var p = positions[i];
               float x = px - p.x;
               float y = py - p.y;
               float z = pz - p.z;
               float dist = x * x + y * y + z * z;

               // are we within the brush radius?
               if (dist < bz)
               {
                  // compute falloff
                  float str = 1.0f - dist / bz;
                  str = Mathf.Pow(str, brushFalloff);
                  float finalStr = str * delta * brushFlow;

                  // are we going to effect this vertex?
                  if (finalStr > 0 & textureList != null)
                  {
                     changed = true;
                     TextureCluster cluster = null;
                     // if we're a cluster, get our texture index for this point in space.
                     if (mode == Mode.TextureCluster)
                     {
                        if (textureIndex < textureList.clusters.Length)
                        {
                           // I don't support normal/angle in this example, because that means getting more data and caching it from the mesh.
                           // each to extend to add that though..
                           cluster = textureList.clusters[textureIndex];
                           ti = (float)cluster.GetIndex(p * clusterNoiseScale, Vector3.up, 0.5f) / 255.0f;
                        }
                     }

                     if (mode == Mode.Texture || mode == Mode.TextureCluster)
                     {
                        // update the data
                        if (layerMode == LayerMode.Top)
                        {
                           // top layer is uv3.w for texture, uv3.x for blend weight
                           Vector4 u3 = uv3[i];
                           u3.w = ti;
                           // Unrolled lerp: u3.x = Mathf.Lerp(u3.x, targetWeight, finalStr);
                           u3.x = u3.x * (1.0f - finalStr) + targetWeight * finalStr;   
                           uv3[i] = u3;
                        }
                        else if (layerMode == LayerMode.Bottom)
                        {
                           colors[i].a = ti;
                           if (hasUV3)
                           {
                              Vector4 u3 = uv3[i];
                              // Unrolled lerp; u3.x = Mathf.Lerp(u3.x, 1.0f - targetWeight, finalStr                        );
                              u3.x = u3.x * (1.0f - finalStr) + (1.0f - targetWeight) * finalStr; 
                              uv3[i] = u3;
                           }
                        }
                        else // auto
                        {
                           Color c = new Color(colors[i].a, uv3[i].w, uv3[i].x, 1);
                           c = TextureCluster.AutoColor(c, textureIndex, finalStr, cluster);
                           colors[i].r = c.r;
                           colors[i].g = c.g;
                           colors[i].b = c.b;
                        }
                     }
                     else if (hasUV3)
                     {
                        Vector4 u3 = uv3[i];
                        // Unrolled lerp; u3.x = Mathf.Lerp(u3.x, 1.0f - targetWeight, finalStr                        );
                        u3.y = u3.y * (1.0f - finalStr) + (1.0f - targetWeight) * finalStr; 
                        uv3[i] = u3;

                     }
                  }
               }
            }

            if (changed)
            {
               // note, you don't have to set the arrays/lists back, since you're just modifying them directly..
               stream.Apply(false);
            }
         }
      }
   }

   void PaintTerrain(Terrain t, Vector3 point)
   {
      if (t.materialTemplate == null)
         return;
      // convert point into local space, so we don't have to convert every point
      point = t.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
      float scale = 1.0f / Mathf.Abs(t.transform.lossyScale.x);

      float bz = scale * brushSize;
      bz = bz * bz;

      Texture2D tex = t.materialTemplate.GetTexture(splatControlId) as Texture2D;

      #if UNITY_EDITOR
      tex = EncounteredTexture(t.materialTemplate, tex);
      #endif

      Vector3 terPoint = MegaSplatCollisionInfo.LocalPointToTerrain(t, point, tex);

      // Vector3.Distance is stupidly slow. It casts things to doubles, calls to c++, etc.
      // If we pre-square the brush size and cache off the pnt vector, then unroll the vector math,
      // we make it roughly 10 times as fast.
      float px = terPoint.x;
      float py = terPoint.y;
      float pz = terPoint.z;

      float delta = Time.deltaTime;
      float ti = (float)textureIndex / 255.0f;
      bool changed = false;

      // terrain is basically the same, but we don't need to scan through vertices. Instead, 
      // we do a box around the center pixel..
      if (terPoint.x >= 0 && terPoint.z >= 0 && terPoint.x < tex.width || terPoint.z < tex.height)
      {
         // scale brush into texture space
         Vector3 offsetPnt = point - new Vector3(bz, 0, bz);
         Vector3 beginTerPnt = MegaSplatCollisionInfo.LocalPointToTerrain(t, offsetPnt, tex);
         beginTerPnt.x = Mathf.Clamp(beginTerPnt.x, 0, tex.width);
         beginTerPnt.z = Mathf.Clamp(beginTerPnt.z, 0, tex.height);

         Vector3 offset = terPoint - beginTerPnt;
         int pbx = (int)beginTerPnt.x;
         int pby = (int)beginTerPnt.z;
         int pex = (int)(terPoint.x + offset.x * 2.0f);
         int pey = (int)(terPoint.z + offset.z * 2.0f);

         pex = Mathf.Clamp(pex, 0, tex.width);
         pey = Mathf.Clamp(pey, 0, tex.height);

         Vector2 invWorldSize = new Vector2(tex.width / t.terrainData.size.x, tex.height / t.terrainData.size.z);

         for (int x = pbx; x < pex; ++x)
         {
            for (int y = pby; y < pey; ++y)
            {
               float h = t.terrainData.GetHeight(x, y);

               float posx = px - x;
               float posy = py - h;
               float posz = pz - y;
               float d = posx * posx + posy * posy + posz * posz;

              
               float str = 1.0f - d / bz;
               str = Mathf.Pow(str, brushFalloff);
               float finalStr = str * delta * brushFlow;
               if (finalStr > 0)
               {
                  TextureCluster cluster = null;
                  if (mode == Mode.TextureCluster)
                  {
                     if (textureIndex < textureList.clusters.Length)
                     {
                        Vector3 p = new Vector3(x * invWorldSize.x, h, y * invWorldSize.y);

                        // I don't support normal/angle in this example, because that means getting more data and caching it from the mesh.
                        // each to extend to add that though..
                        cluster = textureList.clusters[textureIndex];
                        ti = (float)cluster.GetIndex(p * clusterNoiseScale, Vector3.up, 0.5f) / 255.0f;
                     }
                  }
                  Color c = tex.GetPixel(x, y);
                  changed = true;
                  // update the data
                  if (layerMode == LayerMode.Top)
                  {
                     c.g = ti;
                     c.b = c.b * (1.0f - finalStr) + targetWeight * finalStr;   // Just Mathf.Lerp(u3.x, targetWeight, finalStr);
                  }
                  else if (layerMode == LayerMode.Bottom)
                  {
                     c.r = ti;
                     c.b = c.b * (1.0f - finalStr) + (1.0f - targetWeight) * finalStr; // just Mathf.Lerp(u3.x, 1.0f - targetWeight, finalStr);
                  }
                  else
                  {
                     c = TextureCluster.AutoColor(c, textureIndex, finalStr, cluster);
                  }

                  tex.SetPixel(x, y, c);
               }
            }
         }
      }
      if (changed)
      {
         tex.Apply();
      }
   }

}
