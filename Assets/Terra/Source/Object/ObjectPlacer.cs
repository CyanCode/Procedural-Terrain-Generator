using System;
using System.Collections.Generic;
using Terra.Terrain.Util;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class ObjectPlacer {
		private TerraSettings Settings;
		private ObjectPool Pool;
		private bool ObserveTiles;

		public List<ObjectPlacementType> ObjectsToPlace {
			get; private set;
		}

		private const int GRID_SIZE = 20;

		/// <summary>
		/// Creates a new ObjectPlacer that uses mesh information provided 
		/// by TerraSettings to calculate where to place objects on meshes. 
		/// Optionally disable observing TerrainTiles if you wish to 
		/// manage the placement of tiles manually rather than displaying 
		/// and hiding when a TerrainTile activates or deactivates.
		/// </summary>
		/// <param name="settings">TerraSettings instance</param>
		/// <param name="observeTiles">Observe TerrainTile events?</param>
		public ObjectPlacer(TerraSettings settings, bool observeTiles = true) {
			Settings = settings;
			ObserveTiles = observeTiles;
			ObjectsToPlace = Settings.ObjectPlacementSettings;
			Pool = new ObjectPool(this);

			if (ObserveTiles) {
				TerraEvent.OnTileActivated += OnTerrainTileActivate;
				TerraEvent.OnTileDeactivated += OnTerrainTileDeactivate;
			}
		}

		/// <summary>
		/// Calculates a grid using the poisson disc sampling method. 
		/// The 2D grid positions fall within the range of [0, 1].
		/// 
		/// Can be called off of Unity's main thread.
		/// </summary>
		/// <param name="opt">How dense should the samples be</param>
		/// <returns>List of vectors within the grid</returns>
		public List<Vector2> GetPoissonGrid(float density) {
			PoissonDiscSampler pds = new PoissonDiscSampler(GRID_SIZE, GRID_SIZE, density);
			List<Vector2> total = new List<Vector2>();

			foreach (Vector2 sample in pds.Samples()) {
				//Normalize in range of [0, 1] before adding
				total.Add(sample / GRID_SIZE);
			}

			return total;
		}

		/// <summary>
		/// First creates a poisson grid based on the passed density. 
		/// Positions are then filtered based on the passed object placement 
		/// type taking into account height and angle constraints.
		/// 
		/// Unlike the <c>GetFilteredGrid(ObjectPlacementType, float)</c> method 
		/// this method samples from the passed Mesh rather than pulling 
		/// mesh information from TerraSettings.
		/// </summary>
		/// <param name="m">Mesh to sample height and angle values from</param>
		/// <param name="type">object placement type to sample</param>
		/// <returns>List of vectors within the grid and sample constraints</returns>
		public List<Vector3> GetFilteredGrid(Mesh m, ObjectPlacementType type) {
			MeshSampler sampler = new MeshSampler(m, Settings.MeshResolution);
			List<Vector2> grid = GetPoissonGrid(type.Spread / 10);
			List<Vector3> toAdd = new List<Vector3>();

			foreach (Vector2 pos in grid) {
				MeshSampler.MeshSample sample = sampler.SampleAt(pos.x, pos.y);
				
				if (type.ShouldPlaceAt(sample.Height, sample.Angle)) {
					Vector3 newPos = new Vector3(pos.x, sample.Height, pos.y);
					toAdd.Add(newPos);
				}
			}

			return toAdd;
		}

		/// <summary>
		/// First creates a poisson grid based on the passed density. 
		/// Positions are then filtered based on the passed object placement 
		/// type taking into account height and angle constraints.
		/// </summary>
		/// <param name="m">Mesh to sample height and angle values from</param>
		/// <param name="type">object placement type to sample</param>
		/// <param name="density">How dense should the samples be</param>
		/// <returns>List of vectors within the grid and sample constraints</returns>
		public List<Vector3> GetFilteredGrid(TerrainTile tile, ObjectPlacementType type, float density) {
			MeshFilter mf = tile.GetComponent<MeshFilter>();
			if (mf == null) {
				throw new ArgumentException("The passed TerrainTile does not have an attached MeshFilter. Has a mesh been created?");
			}

			return GetFilteredGrid(mf.sharedMesh, type);
		}

		/// <summary>
		/// Called when a TerrainTile has been activated 
		/// </summary>
		/// <param name="tile">Activated tile</param>
		void OnTerrainTileActivate(TerrainTile tile) {
			Pool.ActivateTile(tile);
		}

		/// <summary>
		/// Called when a TerrainTile has been deactivated
		/// </summary>
		/// <param name="tile">Deactivated tile</param>
		void OnTerrainTileDeactivate(TerrainTile tile) {
			Pool.DeactivateTile(tile);
		}
	}
}