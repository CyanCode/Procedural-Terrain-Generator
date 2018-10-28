using System;
using Terra.Terrain;
using UnityEngine;

namespace Terra.Structure {
	[Serializable]
	public class GenerationData {
		public GameObject TrackedObject;
		public int GenerationRadius = 3;
		public bool PrecalculateMaxHeight = true;
		public float LinearTransformTValue = 1f;
		public float LinearTransformOffset = 0.1f;

		public bool GenerateOnStart = true;
		public bool UseRandomSeed = false;
		public bool UseMultithreading = true;

		public float ColliderGenerationExtent = 50f;
		public bool GenAllColliders = false;

		public LodData Lod;
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
