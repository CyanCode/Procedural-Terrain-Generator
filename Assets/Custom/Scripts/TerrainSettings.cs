using UnityEngine;

public class TerrainSettings : MonoBehaviour {
	[System.Serializable]
	public class MaterialInfo {
		public Material Material;
		public Vector2 TextureSize = new Vector2(2, 2);
		[Tooltip("Height to start showing texture")]
		public float Height;
		[Tooltip("Angle to start showing texture (from 0 to 1. -1 to disable)")]
		public float Angle;
	}

	public GameObject TrackedObject;
	public int GenerationRadius = 3;
	public int GenerationSeed = 1337;

	public int HeightmapResolution = 129;
	public int AlphamapResolution = 129;
	public int Length = 500;
	public int Height = 500;
	public MaterialInfo[] Materials;
}
