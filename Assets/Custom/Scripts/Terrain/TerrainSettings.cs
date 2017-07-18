using CoherentNoise;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BonLauncher))]
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

	public int MeshResolution = 129;
	public int Length = 500;
	public float Spread = 1f;
	public float Amplitude = 1f;
	public MaterialInfo[] Materials;

	public TerrainObject[] Objects;

	//Noise Tab
	public string SelectedFile = "";
	public Graph LoadedGraph = null;
	public Generator Generator;
	public Mesh PreviewMesh;
	public bool IsWireframePreview = true;

	void Start() {
		//Set seed for RNG
		Random.InitState(GenerationSeed);

		BonLauncher launcher = gameObject.GetComponent<BonLauncher>();
		if (launcher != null) {
			Generator = launcher.GetGraphGenerator();
		} else {
			Debug.LogError("This gameobject has no associated graph launcher and cannot function.");
		}
	}

	public void OnDrawGizmosSelected() {
		if (ToolbarSelection == ToolbarOptions.Noise && PreviewMesh != null) {
			Gizmos.color = Color.white;

			if (IsWireframePreview) Gizmos.DrawWireMesh(PreviewMesh, Vector3.zero);
			else Gizmos.DrawMesh(PreviewMesh, Vector3.zero);
		} else {
			List<Vector2> positions = TilePool.GetTilePositionsFromRadius(GenerationRadius, transform.position, Length);

			foreach (Vector2 pos in positions) {
				Vector3 pos3d = new Vector3((pos.x * Length) + (Length / 2), 0, (pos.y * Length) + (Length / 2));

				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(pos3d, new Vector3(Length, 0, Length));
			}
		}
	}
}
