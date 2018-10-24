using System;
using System.Collections.Generic;
using Terra.Structure;
using Terra.Util;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class ObjectPlacer {
		public float GridSize;
		public TerraConfig Config;

		private List<ObjectPlacementType> ObjectsToPlace;

		public ObjectPlacer(TerraConfig config) {
			Config = config;
			//ObjectsToPlace = Settings.ObjectData;
		}

		/// <summary>
		/// Calculates a grid using the poisson disc sampling method. 
		/// The 2D grid positions fall within the range of [0, 1].
		/// 
		/// Can be called off of Unity's main thread.
		/// </summary>
		/// <param name="opt">Object placement type to sample</param>
		/// <returns>List of vectors within the grid</returns>
		public List<Vector2> GetPoissonGrid(ObjectPlacementType opt) {
			PoissonDiscSampler pds = new PoissonDiscSampler(opt.GridSize, opt.GridSize, opt.Density);
			List<Vector2> total = new List<Vector2>();

			foreach (Vector2 sample in pds.Samples()) {
				total.Add(sample);
			}

			return total;
		}
	}
}