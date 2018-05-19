//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;

namespace JBooth.MegaSplat
{
   // How to get a ball to roll down a tessellated terrain. So, when you tessellate terrain on the GPU,
   // the physics engine has no idea. So the idea here is to raycast towards the physics mesh each frame,
   // find the collision point, then figure out which textures are being hit and blend them like we do on
   // the GPU to get a final displacement value. Then we move a small physics object to that point, which is
   // what we actually collide with instead of the terrain.
   // This isn't a 'just works for anything' solution, as the only one which fits that is to make an extremely
   // tessellated collision mesh, which would not be efficient. However, it demonstrates how to use the MegaSplat
   // API to get the information you need to roll whatever solution you need. 
   //
   // Requires the MegaSplatCollisionInfo to be set on the mesh, and the Diffuse texture array to have physics
   // representations generated for it. 

   public class TessellationPhysicsFloor : MonoBehaviour 
   {
      // turning this on will create a bunch of spheres under the object to show an area of terrain tessellated
      // It will also draw the fake physics platform.
      public bool showDebug = false;

      // transform of our fake physics object
      Transform physics;

      void Awake()
      {
         // setup the fake physics object
         physics = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
         if (!showDebug)
         {
            physics.hideFlags = HideFlags.HideAndDontSave;
            physics.GetComponent<Renderer>().enabled = false;
         }
         // make sure we don't accidentily hit it with raycasts.
         physics.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
         physics.localScale = new Vector3(1.0f, 1.0f, 0.1f);
      }

      // each frame, raycast down, get the collider's MegaSplatCollisionInfo, and get the new
      // rotation and offset position for the fake physics object. This might be fine for many uses,
      // but a main player character might want to do multiple of these to test against forward direcitons (walls),
      // or raycast down and in the current motion vector as well.
      void Update () 
      {
         Ray ray = new Ray(transform.position, Vector3.down);
         RaycastHit hit;
         if (Physics.Raycast(ray, out hit, float.MaxValue))
         {
            var info = hit.collider.GetComponent<MegaSplatCollisionInfo>();
            if (info != null)
            {
               physics.position = info.GetPhysicsOffsetPosition(hit);
               physics.rotation = Quaternion.LookRotation(hit.normal, Vector3.right);
            }
         }
      }

      #if UNITY_EDITOR
      // only used for debugging to show a grid of spheres representing the surface
      IEnumerator Start()
      {
         if (showDebug)
         {
            yield return null;
            for (int x = 0; x < 128; ++x)
            {
               for (int y = 0; y < 128; ++y)
               {
                  float fx = (float)x / 12.8f;
                  float fy = (float)y / 12.8f;

                  RaycastHit hit;
                  Ray ray = new Ray(this.transform.position + new Vector3(fx, 150, fy), Vector3.down);
                  if (Physics.Raycast(ray, out hit, float.MaxValue))
                  {
                     var info = hit.collider.GetComponent<MegaSplatCollisionInfo>();
                     if (info != null)
                     {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        go.transform.position = info.GetPhysicsOffsetPosition(hit);
                        go.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                     }
                  }
               }
            }
         }
      }
      #endif
   }
}
