using System;
using System.Collections.Generic;
using Terra.Terrain.Util;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class ObjectPlacer {
		public TerraSettings Settings;

		private List<ObjectPlacementType> ObjectsToPlace;

		private const int GRID_SIZE = 20;

		public ObjectPlacer(TerraSettings settings) {
			Settings = settings;
			ObjectsToPlace = Settings.ObjectPlacementSettings;
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


	}
}