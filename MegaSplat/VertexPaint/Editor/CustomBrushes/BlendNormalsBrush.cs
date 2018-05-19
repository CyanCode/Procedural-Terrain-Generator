using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.VertexPainterPro
{
   // Allow the user to create brush objects as assets in an editor folder
   [CreateAssetMenu(menuName = "Vertex Painter Brush/Blend Normal Brush", fileName="VertexNormalBlend_brush")]
   public class BlendNormalsBrush : VertexPainterCustomBrush
   {
      public VertexInstanceStream target;
      public Terrain terrainTarget;

      // return a bitmask of channels in use, so Channels.Colors | Channels.UV0 if you affect those channels with your brush..
      // This will force the channels to be initialized before your brush is applied..
      public override Channels GetChannels()
      {
         return Channels.Normals;
      }

      // preview color for the brush, if we care to provide one
      public override Color GetPreviewColor()
      {
         return Color.yellow;
      }

      // return the data that will be provided to our stamping function, in this case the brush data above..
      public override object GetBrushObject()
      {
         return target;
      }

      // draw any custom GUI we want for this brush in the editor
      public override void DrawGUI()
      {
         target = (VertexInstanceStream)EditorGUILayout.ObjectField("Blend w/ Mesh", target, typeof(VertexInstanceStream),true);
         terrainTarget = (Terrain)EditorGUILayout.ObjectField("Blend w/ Terrain", terrainTarget, typeof(Terrain), true);
      }

      VertexInstanceStream[] streams;

      Vector3 hitPos;
      bool didHit;
      Vector3 normal;
      Vector4 tangent;

      public override void BeginApplyStroke(Ray ray)
      {
         RaycastHit hit;
         if (target != null)
         {
            Mesh msh = target.GetComponent<MeshFilter>().sharedMesh;
            if (msh != null)
            {
               didHit = (RXLookingGlass.IntersectRayMesh(ray, msh, target.transform.localToWorldMatrix, out hit));
               if (didHit)
               {
                  var triangles = msh.triangles;
                  int triangle = hit.triangleIndex;
                  int i0 = triangles[triangle];
                  int i1 = triangles[triangle + 1];
                  int i2 = triangles[triangle + 2];
                  var bary = hit.barycentricCoordinate;

                  Matrix4x4 mtx = target.transform.worldToLocalMatrix;

                  normal = target.GetSafeNormal(i0) * bary.x +
                     target.GetSafeNormal(i1) * bary.y +
                     target.GetSafeNormal(i2) * bary.z;

                  tangent = target.GetSafeTangent(i0) * bary.x +
                     target.GetSafeTangent(i1) * bary.y +
                     target.GetSafeTangent(i2) * bary.z;

                  normal = mtx.MultiplyVector(mtx.MultiplyVector(normal));
                  Vector3 tng = new Vector3(tangent.x, tangent.y, tangent.z);
                  tng = mtx.MultiplyVector(mtx.MultiplyVector(tng));
                  tangent.x = tng.x;
                  tangent.y = tng.y;
                  tangent.z = tng.z;
               }
            }
         }
         else if (terrainTarget != null)
         {
            var col = Physics.RaycastAll(ray);
            for (int i = 0; i < col.Length; ++i)
            {
               if (col[i].collider.gameObject == terrainTarget.gameObject)
               {
                  didHit = true;
                  var uv = col[i].lightmapCoord;

                  Matrix4x4 mtx = terrainTarget.transform.worldToLocalMatrix;

                  normal = terrainTarget.terrainData.GetInterpolatedNormal(uv.x, uv.y);

                  normal = mtx.MultiplyVector(mtx.MultiplyVector(normal));

               }
            }
         }
      }

      void LerpFunc(PaintJob j, int idx, ref object val, float r)
      {
         if (didHit && j.stream.gameObject != target && target != null)
         {
            // convert from world space to local space
            Vector3 norm = j.stream.GetSafeNormal(idx);
            Vector4 tang = j.stream.GetSafeTangent(idx);

            var mtx = j.stream.transform.worldToLocalMatrix;
            var t = tangent;
            t = mtx.MultiplyVector(tangent);
            t.w = tangent.w;

            j.stream.normals[idx] = Vector3.Lerp(norm, mtx.MultiplyVector(norm), r);
            j.stream.tangents[idx] = Vector4.Lerp(tang, t, r);
         }
         if (didHit && terrainTarget != null)
         {
            // retrieve our brush data and get the stream we're painting into

            Vector3 n = j.stream.normals[idx];
            Vector4 t = j.stream.tangents[idx];

            Vector3 pos = j.GetPosition(idx);

            Vector3 iNormal = terrainTarget.terrainData.GetInterpolatedNormal(pos.x,pos.z);
            iNormal = j.stream.transform.InverseTransformDirection(iNormal);

            j.stream.normals[idx] = Vector3.Lerp(n, iNormal, r);
            Vector3 tangentXYZ = Vector3.Cross(j.stream.normals[idx], new Vector3(0, 0, 1));
            Vector4 tangent = new Vector4(tangentXYZ.x, tangentXYZ.y, tangentXYZ.z, -1);

            j.stream.tangents[idx] = Vector4.Lerp(t,tangent,r);

         }

      }
         
      public override VertexPainterWindow.Lerper GetLerper()
      {
         return LerpFunc;
      }

   }
}

