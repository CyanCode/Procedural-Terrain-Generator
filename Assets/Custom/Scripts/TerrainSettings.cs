using System.Collections.Generic;
using UnityEngine;

public class TerrainSettings: MonoBehaviour {
	[System.Serializable]
	public class MaterialInfo {
		public Material Material;
		public Vector2 TextureSize = new Vector2(2, 2);
		[Tooltip("Height to start showing texture")]
		public float Height;
		[Tooltip("Angle to start showing texture (from 0 to 1. -1 to disable)")]
		public float Angle;
	}
	
	public enum ToolbarOptions {
		General = 0,
		Noise = 1,
		Materials = 2
	}
	public ToolbarOptions ToolbarSelection = ToolbarOptions.General;

	//General Tab
	public GameObject TrackedObject;
	public int GenerationRadius = 3;
	public int GenerationSeed = 1337;

	public int HeightmapResolution = 129;
	public int AlphamapResolution = 129;
	public int Length = 500;
	public int Height = 500;
	public MaterialInfo[] Materials;

	public TerrainObject[] Objects;

	//Noise Tab
	public EditorNoiseModule RootNoiseModule;

	void Start() {
		//Set seed for RNG
		Random.InitState(GenerationSeed);
	}

	public void OnDrawGizmosSelected() {
		List<Vector2> positions = TilePool.GetTilePositionsFromRadius(GenerationRadius, transform.position, Length);
		foreach (Vector2 pos in positions) {
			Vector3 pos3d = new Vector3((pos.x * Length) + (Length / 2), Height / 2, (pos.y * Length) + (Length / 2));

			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(pos3d, new Vector3(Length, Height, Length));
		}
	}
}
