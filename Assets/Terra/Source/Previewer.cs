using Terra.Data;
using System.Linq;
using UnityEngine;

namespace Terra.Terrain {
	/// <summary>
	/// Acts as a way to preview Terrain within the editor.
	/// </summary>
	public class Previewer {
		private TerraSettings _settings { get { return TerraSettings.Instance; } }
		private Tile _previewedTile = null;

		/// <summary>
		/// Adds a <see cref="Tile"/> at <see cref="GridPosition"/> [0, 0] 
		/// to the <see cref="TilePool"/>.
		/// </summary>
		public void UpdatePreview() {
			AddTile();
		}

		/// <summary>
		/// Adds a "preview" tile to the TilePool. If one already exists 
		/// it is overwritten.
		/// </summary>
		private void AddTile() {
			TilePool pool = _settings.Generator.Pool;
			if (_previewedTile != null && pool.Cache.IsTileActiveAtPosition(_previewedTile.GridPosition)) {
				pool.Cache.RemoveTile(_previewedTile);
			}

			//Create tile synchronously
			bool multiThreaded = _settings.Generator.UseMultithreading;
			_settings.Generator.UseMultithreading = false;

			pool.AddTileAt(new GridPosition(0, 0), tile => {
				_previewedTile = tile;
				Debug.Log("Created preview tile at [0, 0]");
			});

			_settings.Generator.UseMultithreading = multiThreaded;
		}
	}
}
