//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.TerrainPainter
{
   public class TerrainJob : ScriptableObject
   {
      public Terrain terrain;
      public Texture2D terrainTex;
      public Texture2D terrainParams;
      public Collider collider;

      public byte[] undoBuffer;
      public byte[] undoBufferParams;

      public void RegisterUndo()
      {
         undoBuffer = terrainTex.GetRawTextureData();
         if (terrainParams != null)
         {
            undoBufferParams = terrainParams.GetRawTextureData();
         }
         UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Terrain Edit");
      }

      public void RestoreUndo()
      {
         if (undoBuffer != null && undoBuffer.Length > 0)
         {
            terrainTex.LoadRawTextureData(undoBuffer);
            terrainTex.Apply();
         }
         if (undoBufferParams != null)
         {
            if (terrainParams != null && undoBufferParams != null && undoBufferParams.Length > 0)
            {
               terrainParams.LoadRawTextureData(undoBufferParams);
               terrainParams.Apply();
            }
         }
      }
   }

}
