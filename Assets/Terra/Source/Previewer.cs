using System;
using System.Collections.Generic;
using Terra.Data;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Terrain {
	/// <summary>
	/// Acts as a way to preview Terrain within the editor.
	/// </summary>
	[Serializable]
	public class Previewer {
		private const string PREV_GO_NAME = "PREVIEW TILES";

		private TerraSettings _settings { get { return TerraSettings.Instance; } }

		/// <summary>
		/// Adds a <see cref="Tile"/> at <see cref="GridPosition"/> [0, 0] 
		/// to the <see cref="TilePool"/>.
		/// </summary>
		public void UpdatePreview() {
			RemoveExistingTiles();
			AddTilesForRadius(_settings.EditorState.PreviewRadius);
		}

		/// <summary>
		/// List of <see cref="GridPosition"/>s that preview <see cref="Tile"/>s 
		/// are currently occupying.
		/// </summary>
		public List<GridPosition> GetPreviewingPositions() {
			var positions = new List<GridPosition>();	

			foreach (Tile t in GetPreviewGameObject().GetComponentsInChildren<Tile>()) {
				positions.Add(t.GridPosition);
			} 

			return positions;
		}

		/// <summary>
		/// Remove existing preview Tile gameobject(s) from the scene
		/// </summary>
		public void RemoveExistingTiles() {
			if (_settings != null) {
				foreach (Tile t in GetPreviewGameObject().GetComponentsInChildren<Tile>()) {
#if UNITY_EDITOR
					Object.DestroyImmediate(t.gameObject);
#else
					Object.Destroy(t);
#endif
				}
			}
		}

		private void AddTilesForRadius(int radius) {
			int length = _settings.Generator.Length;
			var positions = TilePool.GetTilePositionsFromRadius(radius, new GridPosition(0, 0), length);

			foreach (GridPosition p in positions) {
				AddTile(p);
			}
		}

		/// <summary>
		/// Get the gameobject that holds all previewed Tiles
		/// </summary>
		private GameObject GetPreviewGameObject() {
			foreach (Component comp in _settings.gameObject.transform.GetComponentsInChildren<Component>()) {
				if (comp.gameObject.name == PREV_GO_NAME) {
					return comp.gameObject;
				}
			}

			GameObject preview = new GameObject(PREV_GO_NAME);
			preview.transform.parent = _settings.gameObject.transform;

			return preview;
		}

		/// <summary>
		/// Adds a "preview" tile to the TilePool.
		/// </summary>
		private void AddTile(GridPosition pos) {
			//Generate synchronously
			bool isMultiThreaded = _settings.Generator.UseMultithreading;
			_settings.Generator.UseMultithreading = false;

			TilePool pool = _settings.Generator.Pool;
			pool.AddTileAt(pos, tile => {
				tile.transform.parent = GetPreviewGameObject().transform;
			});

			_settings.Generator.UseMultithreading = isMultiThreaded;
		}
	}
}
