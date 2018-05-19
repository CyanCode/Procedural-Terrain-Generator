using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SnowAnimation : MonoBehaviour 
{
   [Range(0, 1)]
   public float time = 0;

   [System.Serializable]
   public class SnowKey
   {
      [Range(0, 1)]
      public float time;
      [Range(0, 1)]
      public float amount;
      [Range(0, 1)]
      public float heightInfluence;
      [Range(0.5f, 2)]
      public float erosion;
      [Range(0, 0.4f)]
      public float melt;
      [Range(0, 1.5f)]
      public float crystals;
   }

   public Material target;

   public SnowKey[] keys;

   void Blend(SnowKey k0, SnowKey k1, float r)
   {
      float amount = Mathf.Lerp(k0.amount, k1.amount, r);
      float heightInfluence = Mathf.Lerp(k0.heightInfluence, k1.heightInfluence, r);
      float erosion = Mathf.Lerp(k0.erosion, k1.erosion, r);
      float melt = Mathf.Lerp(k0.melt, k1.melt, r);
      float crystals = Mathf.Lerp(k0.crystals, k1.crystals, r);

      //  _SnowParams2 = height influence, erosion, crystals, melt
      Shader.SetGlobalFloat("_SnowAmount", amount);
      Shader.SetGlobalVector("_SnowParams", new Vector4(heightInfluence, erosion, crystals, melt));

      if (target != null)
      {
         target.SetFloat("_SnowAmount", amount);
         target.SetVector("_SnowParams", new Vector4(heightInfluence, erosion, crystals, melt));
      }
   }
      
	void Update () 
   {
      if (keys == null || keys.Length < 1)
         return;

      SnowKey k0 = keys[0];
      SnowKey k1 = keys[0];
      for (int i = 0; i < keys.Length; ++i)
      {
         if (time <= keys[i].time)
         {
            k1 = keys[i];
            if (i >= 1)
               k0 = keys[i - 1];
            break;
         }
      }
      float l = k1.time - k0.time;
      float r = 0;
      if (l > 0)
         r = (time - k0.time) / l;

      Blend(k0, k1, r);
	}
}
