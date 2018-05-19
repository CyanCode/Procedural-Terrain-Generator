//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

// Example script which shows how to get names from texture collisions. Requires that your
// MegaSplat meshes or terrains have the MegaSplatCollisionInfo on them.

namespace JBooth.MegaSplat
{
   public class DisplayTextureName : MonoBehaviour 
   {
      string texName;
      Vector3 offset;

      // An example of how to align a physics object with a tessellated terrain..
   	void Update () 
      {
         Ray ray = new Ray(transform.position, Vector3.down);
         RaycastHit hit;
         if (Physics.Raycast(ray, out hit, float.MaxValue))
         {
            var info = hit.collider.GetComponent<MegaSplatCollisionInfo>();
            if (info != null)
            {
               texName = info.GetTextureName(hit);
               offset = info.GetPhysicsOffsetPosition(hit);
            }
         }
   	}

      void OnGUI()
      {
         if (texName != null)
         {
            GUI.Label(new Rect(30, 30, 300, 30), texName);
            GUI.Label(new Rect(30, 60, 300, 30), offset.ToString());
         }
      }

   }




}