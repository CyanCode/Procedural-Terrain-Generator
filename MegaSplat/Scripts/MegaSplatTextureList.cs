//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JBooth.MegaSplat;

public class MegaSplatTextureList : ScriptableObject 
{
   public string[] textureNames;
   public Texture2D[] physicsTex;
   public TextureCluster[] clusters;

}
