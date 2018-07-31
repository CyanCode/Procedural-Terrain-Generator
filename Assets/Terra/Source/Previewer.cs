using System.Collections.Generic;
using Terra.Data;
using System.Linq;
using UnityEngine;

namespace Terra.Terrain {
	/// <summary>
	/// Acts as a way to preview Terrain within the editor.
	/// </summary>
	public class Previewer {
		private TerraSettings _settings { get { return TerraSettings.Instance; } }
		private List<GridPosition> _existingPositions = null;

		/// <summary>
		/// Adds a <see cref="Tile"/> at <see cref="GridPosition"/> [0, 0] 
		/// to the <see cref="TilePool"/>.
		/// </summary>
		public void UpdatePreview() {
			RemoveExistingTiles();
			AddTilesForRadius(_settings.EditorState.PreviewRadius);
		}

		private void AddTilesForRadius(int radius) {
			int length = _settings.Generator.Length;
			var positions = TilePool.GetTilePositionsFromRadius(radius, new GridPosition(0, 0), length);

			foreach (GridPosition p in positions) {
				AddTile(p);
			}
		}

		/// <summary>
		/// Adds a "preview" tile to the TilePool.
		/// </summary>
		private void AddTile(GridPosition pos) {
			if (_existingPositions == null) {
				_existingPositions = new List<GridPosition>();
			}

			TilePool pool = _settings.Generator.Pool;

			pool.AddTileAt(pos, tile => {
				tile.IsPreviewTile = true;
				_existingPositions.Add(tile.GridPosition);
			});
		}
		
		/// <summary>
		/// List of <see cref="GridPosition"/>s that preview <see cref="Tile"/>s 
		/// are currently occupying.
		/// </summary>
		public List<GridPosition> GetPreviewingPositions() {
			if (_existingPositions != null)
				return _existingPositions;

			_existingPositions = new List<GridPosition>();
			foreach (Tile t in Object.FindObjectsOfType<Tile>()) {
				if (t.IsPreviewTile) {
					_existingPositions.Add(t.GridPosition);
				}
			} 

			return _existingPositions;
		}

		/// <summary>
		/// Remove existing preview Tile gameobject(s) from the scene
		/// </summary>
		private void RemoveExistingTiles() {
			if (_settings != null) {
				foreach (Tile t in Object.FindObjectsOfType<Tile>()) {
					if (t.IsPreviewTile) {
						_existingPositions.Remove(t.GridPosition);
#if UNITY_EDITOR
						Object.DestroyImmediate(t.gameObject);
#else
						Object.Destroy(t);
#endif
					}
				}
			}
		}
	}
}
