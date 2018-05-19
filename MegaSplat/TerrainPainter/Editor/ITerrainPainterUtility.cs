//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace JBooth.TerrainPainter
{
   public interface ITerrainPainterUtility 
   {
      string GetName();
      void OnGUI(TerrainJob[] jobs);

   }
}