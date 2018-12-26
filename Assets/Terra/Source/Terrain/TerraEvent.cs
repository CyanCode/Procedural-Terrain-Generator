using Terra.Structures;
using UnityEngine;

namespace Terra.Terrain {
	public static class TerraEvent {
		public delegate void Action(GameObject go);
		public delegate void MeshAction(GameObject go, Mesh mesh);
		public delegate void MeshColliderAction(GameObject go, MeshCollider meshCollider);
		public delegate void SplatAction(GameObject go, Texture2D splat);
		public delegate void TileEvent(Tile tile);

		public static event Action OnMeshWillForm;

		public static event MeshColliderAction OnMeshColliderDidForm;

		public static event Action OnSplatmapWillCalculate;
		public static event SplatAction OnSplatmapDidCalculate;

		public static event Action OnCustomMaterialWillApply;
		public static event Action OnCustomMaterialDidApply;

		/// <summary>
		/// Called when a Tile is deactivated
		/// </summary>
		public static event TileEvent OnTileDeactivated;
		/// <summary>
		/// Called when a Tile is activated
		/// </summary>
		public static event TileEvent OnTileActivated;

		/// <summary>
		/// Called when the <see cref="TilePool"/> in <see cref="TerraConfig"/> 
		/// has completed its queue of tiles to generate.
		/// </summary>
		public static event System.Action<GridPosition[]> OnTilePoolQueueCompleted;

		public static void TriggerOnMeshWillForm(GameObject go) {
			if (OnMeshWillForm != null) OnMeshWillForm(go);
		}

		public static void TriggerOnMeshColliderDidForm(GameObject go, MeshCollider collider) {
			if (OnMeshColliderDidForm != null) OnMeshColliderDidForm(go, collider);
		}

		public static void TriggerOnSplatmapWillCalculate(GameObject go) {
			if (OnSplatmapWillCalculate != null) OnSplatmapWillCalculate(go);
		}

		public static void TriggerOnSplatmapDidCalculate(GameObject go, Texture2D splat) {
			if (OnSplatmapDidCalculate != null) OnSplatmapDidCalculate(go, splat);
		}

		public static void TriggerOnCustomMaterialWillApply(GameObject go) {
			if (OnCustomMaterialWillApply != null) OnCustomMaterialWillApply(go);
		}

		public static void TriggerOnCustomMaterialDidApply(GameObject go) {
			if (OnCustomMaterialDidApply != null) OnCustomMaterialDidApply(go);
		}

		public static void TriggerOnTileDeactivated(Tile tile) {
			if (OnTileDeactivated != null) OnTileDeactivated(tile);
		}

		public static void TriggerOnTileActivated(Tile tile) {
			if (OnTileActivated != null) OnTileActivated(tile);
		}

		public static void TriggerOnTilePoolQueueCompleted(GridPosition[] positions) {
			if (OnTilePoolQueueCompleted != null) OnTilePoolQueueCompleted(positions);
		}
	}
}