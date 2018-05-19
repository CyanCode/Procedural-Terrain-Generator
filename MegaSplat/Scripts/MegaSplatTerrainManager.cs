using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MegaSplatTerrainManager : MonoBehaviour 
{
   [Tooltip("Material to use for terrain")]
   public Material templateMaterial;
   public Texture2D splatTexture;
   public Texture2D paramTexture;
   public Texture2D macroDiffuseOverride;
   public Texture2D macroNormalOverride;

   [System.NonSerialized]
   [HideInInspector]
   public Material matInstance;

   public delegate void MaterialSyncAll();
   public delegate void MaterialSync(Material m);

   public static event MaterialSyncAll OnMaterialSyncAll;
   public event MaterialSync OnMaterialSync;


   static List<MegaSplatTerrainManager> sInstances = new List<MegaSplatTerrainManager>();

   bool started = false;
   void OnEnable()
   {
      sInstances.Add(this);
      #if UNITY_EDITOR
      if (started)
      {
         Sync();
      }
      #endif
   }
      
   void Start()
   {
      started = true;
      Sync();
   }

   void OnDisable()
   {
      sInstances.Remove(this);
      Cleanup();
   }

   void Cleanup()
   {
      if (matInstance != null && matInstance != templateMaterial)
      {
         DestroyImmediate(matInstance);
      }
   }

   #if UNITY_EDITOR
   public void FindPaths(Terrain t, string texNamePrefix, Material mat, out string path, out string paramPath)
   {
      // find textures
      path = UnityEditor.AssetDatabase.GetAssetPath(t.terrainData);
      if (string.IsNullOrEmpty(path))
      {
         // some landscape systems store terrain data internally or generate it..
         // try the material...
         if (mat)
         {
            path = UnityEditor.AssetDatabase.GetAssetPath(mat);
         }
         else
         {
            path = "Assets/terrain";
         }
      }
      path = path.Replace("\\", "/");
      path = path.Substring(0, path.LastIndexOf("/") + 1);

      if (string.IsNullOrEmpty(texNamePrefix)) 
      { 
         path += t.terrainData.name; 
      }
      else 
      { 
         path += texNamePrefix; 
      }
      paramPath = path + "_splat_params.png";
      path += "_splat_control.png";
   }

   public void FindTextures(Terrain t, string texNamePrefix, Material mat)
   {
      if (splatTexture != null)
      {
         bool uses = UsesParams();
         if (uses && paramTexture != null || !uses)
         {
            return;
         }
      }
      string path, paramPath;
      FindPaths(t, texNamePrefix, mat, out path, out paramPath);

      // not in the manager? Check on the material
      if (splatTexture == null)
      {
         splatTexture = mat.GetTexture("_SplatControl") as Texture2D;
      }
      if (paramTexture == null)
      {
         paramTexture = mat.GetTexture("_SplatParams") as Texture2D;
      }

         
      // finally, check the disk..
      if (splatTexture == null)
      {
         splatTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      }
      if (paramTexture == null)
      {
         paramTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(paramPath);
      }
   }
   #endif

   public void Sync()
   {
      if (templateMaterial == null)
         return;

      Cleanup();
      Terrain t = GetComponent<Terrain>();
      t.materialType = Terrain.MaterialType.Custom;

      Material m = new Material(templateMaterial);
      m.hideFlags = HideFlags.HideAndDontSave;
      t.materialTemplate = m;
      matInstance = m;
      if (!UsesParams())
      {
         paramTexture = null;
      }

      #if UNITY_EDITOR
      FindTextures(GetComponent<Terrain>(), "", m);
      #endif

      m.SetTexture("_SplatControl", splatTexture);
      m.SetTexture("_SplatParams", paramTexture);

      if (m.HasProperty("_MacroDiff"))
      {
         if (macroDiffuseOverride != null)
         {
            m.SetTexture("_MacroDiff", macroDiffuseOverride);
         }
         if (macroNormalOverride != null)
         {
            m.SetTexture("_MacroBump", macroNormalOverride);
         }
      }


      t.basemapDistance = 99999999;

      if (OnMaterialSync != null)
      {
         OnMaterialSync(m);
      }
   }

   public static void SyncAll()
   {
      for (int i = 0; i < sInstances.Count; ++i)
      {
         sInstances[i].Sync();
      }
      if (OnMaterialSyncAll != null)
      {
         OnMaterialSyncAll();
      }
   }

   public bool UsesParams()
   {
      Terrain t = GetComponent<Terrain>();
      if (t == null)
         return false;
      if (t.materialType != Terrain.MaterialType.Custom || t.materialTemplate == null)
         return false;

      var mat = t.materialTemplate;
      if (!mat.HasProperty("_SplatParams"))
         return false;
      
      bool tess = (mat.IsKeywordEnabled("_TESSDAMPENING"));
      bool flow = (mat.IsKeywordEnabled("_FLOW") || mat.IsKeywordEnabled("_FLOWREFRACTION"));
      bool puddles = (mat.IsKeywordEnabled("_PUDDLES") || mat.IsKeywordEnabled("_PUDDLEFLOW") || mat.IsKeywordEnabled("_PUDDLEREFRACT") || mat.IsKeywordEnabled("_LAVA"));
      bool wetness = mat.IsKeywordEnabled("_WETNESS");
      return tess || flow || puddles || wetness;
   }
}
