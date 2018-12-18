using System;
using Terra.Terrain;
using UnityEngine;

namespace Terra.Structure {
	[Serializable]
	public class GenerationData {
		public GameObject TrackedObject;
		public int GenerationRadius = 3;
		public float LodChangeRadius = 250f;
		public bool RemapHeightmap = true;
		public int RemapResolution = 128;
		public float RemapPadding = 0.1f;

		public bool GenerateOnStart = true;
		public bool UseRandomSeed = false;
		public bool UseMultithreading = true;
		public bool UseCoroutineForHeightmap = true;

		public float ColliderGenerationExtent = 50f;
		public bool GenAllColliders = false;

		public LodData Lod;
		public int LodCount = 0;

		public int Length = 500;
		public float Amplitude = 50f;

		public float BiomeBlendAmount = 1f;
		public float BiomeFalloff = 1f;

		public TilePool Pool;

		public GenerationData() {
			if (Pool == null) Pool = new TilePool();
			if (Lod == null) Lod = new LodData();
		}
	}
}
