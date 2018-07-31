using System;
using Terra.Terrain;
using UnityEngine;

namespace Terra.Data {
	[Serializable]
	public class GenerationData {
		public GameObject TrackedObject;
		public int GenerationRadius = 3;

		public bool GenerateOnStart = true;
		public bool UseRandomSeed = false;
		public bool UseMultithreading = true;

		public float ColliderGenerationExtent = 50f;
		public bool GenAllColliders = false;

		public LodData Lod;
		public int Length = 500;
		public float Spread = 100f;
		public float Amplitude = 50f;

		public TilePool Pool;

		public GenerationData() {
			if (Pool == null) Pool = new TilePool();
			if (Lod == null) Lod = new LodData();
		}
	}
}
