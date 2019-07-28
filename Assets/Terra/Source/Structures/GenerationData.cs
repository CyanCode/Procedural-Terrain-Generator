using System;
using Terra.Terrain;
using UnityEngine;

namespace Terra.Structures {
	[Serializable]
	public class GenerationData {
		public GameObject TrackedObject;
		public int GenerationRadius = 3;
		public float LodChangeRadius = 250f;
		public bool RemapHeightmap = true;
		public int RemapResolution = 128;
		public float RemapPadding = 0.08f;

		public bool GenerateOnStart = true;
		public bool UseRandomSeed = false;
		public bool UseCoroutines = true;
        /// <summary>
        /// Should the terrain disable rendering while generating?
        /// </summary>
        public bool HideWhileGenerating = true;
        public int CoroutineRes = 256;

		public LodData Lod;
		public int LodCount = 0;

		public int Length = 500;
		public float Amplitude = 50f;
		public float Spread = 100f;

		public float BiomeBlendAmount = 1f;
		public float BiomeFalloff = 1f;
        public int DetailmapResolution = 1024;
        public int DetailResolutionPerPatch = 16;
        public int SplatmapResolution = 1024;

        public int DetailDistance = 80;
        public float DetailDensity = 1f;
        public int TreeDistance = 2000;
        public int BillboardStart = 50;
        public int FadeLength = 5;
        public int MaxMeshTrees = 50;

		public TilePool Pool;

		public GenerationData() {
			if (Pool == null) Pool = new TilePool();
			if (Lod == null) Lod = new LodData();
		}
	}
}
