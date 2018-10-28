using System;
using System.Collections.Generic;
using System.Threading;
using Terra.Structure;
using Terra.Util;
using UnityEngine;

namespace Terra.Terrain {
	/// <summary>
	/// References the <see cref="UnityEngine.Terrain"/> Component attached to this Tile and 
	/// handles modifying the heightmap.
	/// </summary>
	[Serializable]
	public class TileMesh: ISerializationCallbackReceiver {
		/// <summary>
		/// Resolution of this mesh
		/// </summary>
		public Resolution MeshResolution { get; private set; }

		/// <summary>
		/// Resolution of the heightmap
		/// </summary>
		public int HeightmapResolution { get; private set; }

		/// <summary>
		/// Maximum height of the most recently calculated heightmap.
		/// Default value is 1.
		/// </summary>
		public float HeightmapMaxHeight = 1;

		/// <summary>
		/// Minimum height of the most recently calculated heightmap.
		/// Default value is 0.
		/// </summary>
		public float HeightmapMinHeight = 0;

		/// <summary>
		/// The last heightmap computed after calling <see cref="CalculateHeightmapAsync"/> or 
		/// <see cref="CalculateHeightmap"/>. To apply this heightmap to the <see cref="UnityEngine.Terrain"/> 
		/// call <see cref="SetTerrainHeightmap"/>.
		/// </summary>
		public float[,] Heightmap { get; private set; }

		public LodData.LodLevel LodLevel {
			get { return _lodLevel; }
			set {
				_lodLevel = value;
				HeightmapResolution = value.MapResolution;
			}
		}

		/// <summary>
		/// The <see cref="UnityEngine.Terrain"/> instance attached to this 
		/// <see cref="Tile"/>'s gameobject. Null until <see cref="CalculateHeightmap"/> or 
		/// <see cref="CalculateHeightmapAsync"/> has been called.
		/// </summary>
		public UnityEngine.Terrain ActiveTerrain {
			get {
				return _tile.gameObject.GetComponent<UnityEngine.Terrain>();
			}
		}

		/// <summary>
		/// The LOD level of this mesh during the last creation of the heightmap. If 
		/// the heightmap hasn't been created yet this value is null.
		/// </summary>
		public LodData.LodLevel LastGeneratedLodLevel {
			get {
				return _lastGeneratedLodLevel;
			}
		}

		/// <summary>
		/// Internal <see cref="LodLevel"/>
		/// </summary>
		[SerializeField]
		private LodData.LodLevel _lodLevel;

		/// <summary>
		/// Tile using this TileMesh
		/// </summary>
		[SerializeField]
		private Tile _tile;

		/// <summary>
		/// LOD of last generated heightmap
		/// </summary>
		[SerializeField]
		private LodData.LodLevel _lastGeneratedLodLevel;

		/// <summary>
		/// If this TileMesh needs to update its generator
		/// </summary>
		private bool _genNeedsUpdating = true;

		private static readonly object _asyncMeshLock;

		static TileMesh() {
			_asyncMeshLock = new object();
		}

		/// <summary>
		/// Constructs a new TileMesh instance
		/// </summary>
		/// <param name="tile">Tile to attach mesh to</param>
		/// <param name="lodLevel">LOD level to reference when creating heightmap and Terrain</param>
		public TileMesh(Tile tile, LodData.LodLevel lodLevel) {
			_tile = tile;
			LodLevel = lodLevel;
		}

		/// <summary>
		/// Creates a heightmap of resolution <see cref="HeightmapResolution"/>. If a 
		/// <see cref="Heightmap"/> of the same resolution or higher has already been 
		/// created, this method does nothing.
		/// A heightmap is 2D array of floats that represents the Y values (or heights) 
		/// of to-be created vertices in 3D space.
		/// </summary>
		/// <param name="gridPos">Optionally override the GridPosition from the referenced Tile</param>
		public void CalculateHeightmap(GridPosition? gridPos = null) {
			if (!TerraConfig.Instance.Generator.UseMultithreading) {
				_lastGeneratedLodLevel = _tile.GetLodLevel();
				LodLevel = _lastGeneratedLodLevel;
			}
		
			if (Heightmap != null && (int)Math.Sqrt(Heightmap.Length) >= HeightmapResolution)
				return;

			Heightmap = new float[HeightmapResolution, HeightmapResolution];
			float max = float.NegativeInfinity;
			float min = float.PositiveInfinity;

			for (int x = 0; x < HeightmapResolution; x++) {
				for (int z = 0; z < HeightmapResolution; z++) {
					Vector2 localXZ = PositionToLocal(x, z, HeightmapResolution);
					Vector2 worldXZ = LocalToWorld(gridPos == null ? _tile.GridPosition : new GridPosition(), localXZ.x, localXZ.y);

					lock (_asyncMeshLock) {
						float height = HeightAt(worldXZ.x, worldXZ.y);
						Heightmap[z, x] = height;

						if (height > max) {
							max = height;
						}
						if (height < min) {
							min = height;
						}
					}
				}
			}

			//Set calculated min and max heights
			HeightmapMaxHeight = max;
			HeightmapMinHeight = min;
		}

		/// <summary>
		/// Creates a heightmap of resolution <see cref="HeightmapResolution"/> asynchronously. 
		/// If a <see cref="Heightmap"/> of the same resolution or higher has already been 
		/// created, this method does nothing.
		/// A heightmap is 2D array of floats that represents the Y values (or heights) 
		/// of to-be created vertices in 3D space.
		/// </summary>
		/// <param name="onComplete">Called when the heightmap has been created</param>
		public void CalculateHeightmapAsync(Action onComplete) {
			_lastGeneratedLodLevel = _tile.GetLodLevel();
			LodLevel = _lastGeneratedLodLevel;

			ThreadPool.QueueUserWorkItem(d => { //Worker thread
				CalculateHeightmap();

				MTDispatch.Instance().Enqueue(onComplete);
			});
		}

		/// <summary>
		/// Adds a <see cref="UnityEngine.Terrain"/> component to this <see cref="Tile"/>'s 
		/// gameobject and sets it up according to <see cref="TerraConfig"/>. 
		/// Overwrites <see cref="ActiveTerrain"/> if it already exists.
		/// </summary>
		public void AddTerrainComponent() {
			//Destory current Terrain instance if it exists
			if (ActiveTerrain != null) {
#if UNITY_EDITOR
				UnityEngine.Object.DestroyImmediate(ActiveTerrain);
#else
				UnityEngine.Object.Destroy(ActiveTerrain);
#endif
			}

			TerraConfig conf = TerraConfig.Instance;
			int length = conf.Generator.Length;
			UnityEngine.Terrain t = _tile.gameObject.AddComponent<UnityEngine.Terrain>();

			t.terrainData = new TerrainData();
			t.terrainData.size = new Vector3(length, conf.Generator.Amplitude, length);

			TerrainCollider tc = _tile.gameObject.AddComponent<TerrainCollider>();
			tc.terrainData = t.terrainData;
		}

		/// <summary>
		/// Sets the neighboring <see cref="UnityEngine.Terrain"/> types.
		/// </summary>
		public void SetNeighboringTiles(Neighborhood neighbors) {
			Neighborhood n = neighbors;

			UnityEngine.Terrain t = ActiveTerrain;
			if (t == null) {
				return;
			}

			UnityEngine.Terrain left = n.Left == null ? null : n.Left.MeshManager.ActiveTerrain;
			UnityEngine.Terrain top = n.Up == null ? null : n.Up.MeshManager.ActiveTerrain;
			UnityEngine.Terrain right = n.Right == null ? null : n.Right.MeshManager.ActiveTerrain;
			UnityEngine.Terrain bottom = n.Down == null ? null : n.Down.MeshManager.ActiveTerrain;

			t.SetNeighbors(left, top, right, bottom);
		}

		/// <summary>
		/// Sets the <see cref="UnityEngine.Terrain"/> component's heightmap to this instances'
		/// <see cref="Heightmap"/>. If <see cref="UnityEngine.Terrain"/> hasn't been created, 
		/// it is added as a component.
		/// </summary>
		/// <remarks>Since this method creates and adds a <see cref="UnityEngine.Terrain"/> 
		/// component, it is not thread safe.</remarks>
		/// <param name="heightmap">Optionally use the passed heightmap instead of 
		/// <see cref="TileMesh"/>'s <see cref="Heightmap"/></param>
		public void SetTerrainHeightmap(float[,] heightmap = null) {
			float[,] hm = heightmap ?? Heightmap;

			if (hm == null) {
				return;
			}
			if (ActiveTerrain == null) {
				AddTerrainComponent();
			}
			
			// ReSharper disable once PossibleNullReferenceException
			TerrainData td = ActiveTerrain.terrainData;
			TerraConfig conf = TerraConfig.Instance;
			int length = conf.Generator.Length;

			td.heightmapResolution = HeightmapResolution;
			td.SetHeights(0, 0, hm);
			td.size = new Vector3(length, conf.Generator.Amplitude, length);
		}

		/// <summary>
		/// Remaps each value in the heightmap to the new min and max.
		/// </summary>
		public void RemapHeightmap(float min, float max, float newMin, float newMax) {
			for (int x = 0; x < HeightmapResolution; x++) {
				for (int z = 0; z < HeightmapResolution; z++) {
					//NewValue = (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin
					float val = Heightmap[x, z];
					Heightmap[x, z] = ((val - min) * (newMax - newMin) / (max - min)) + newMin;
				}
			}
		}

		/// <summary>
		/// Polls the Generator from <see cref="TerraConfig.HeightMapData"/> and 
		/// returns the height value found at [x, 0, z]. This method applies the 
		/// spread from <see cref="TerraConfig"/> to the result.
		/// </summary>
		/// <param name="worldX">World x coordinate</param>
		/// <param name="worldZ">World z coordinate</param>
		/// <returns>height</returns>
		private float HeightAt(float worldX, float worldZ) {
			var sett = TerraConfig.Instance;
			var spread = sett.HeightMapData.Spread;

			if (_genNeedsUpdating) {
				sett.HeightMapData.UpdateGenerator();
				_genNeedsUpdating = false;
			}
			
			return sett.HeightMapData.GetValue(worldX, worldZ, spread);
		}

		/// <summary>
		/// Transforms the passed x and z incrementors into local coordinates.
		/// </summary>
		/// <param name="x">x position to transform</param>
		/// <param name="z">z position to transform</param>
		/// <param name="resolution">resolution of structure (mesh or heightmap)</param>
		/// <returns></returns>
		public static Vector2 PositionToLocal(int x, int z, int resolution) {
			float length = TerraConfig.Instance.Generator.Length;
			float xLocal = ((float)x / (resolution - 1) - .5f) * length;
			float zLocal = ((float)z / (resolution - 1) - .5f) * length;

			return new Vector2(xLocal, zLocal);
		}

		/// <summary>
		/// Converts local X and Z coordinates to <see cref="Tile"/> world 
		/// coordinates.
		/// </summary>
		/// <param name="gridPos">Position of the Tile in the grid</param>
		/// <param name="localX">Local x coordinate on mesh</param>
		/// <param name="localZ">Local z coordinate on mesh</param>
		/// <returns>World X and Z coordinates</returns>
		public static Vector2 LocalToWorld(GridPosition gridPos, float localX, float localZ) {
			int length = TerraConfig.Instance.Generator.Length;
			float worldX = localX + (gridPos.X * length);
			float worldZ = localZ + (gridPos.Z * length);

			return new Vector2(worldX, worldZ);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="resolution"></param>
		/// <returns></returns>
		//public static Vector2 PositionToWorld(int x, int z, int resolution) {
		//	float length = TerraConfig.Instance.Generator.Length;
		//	float xLocal = ((float)x / (resolution - 1) - .5f) * length;
		//	float zLocal = ((float)z / (resolution - 1) - .5f) * length;
		//	float xWorld = 
		//}

		/// <summary>
		/// Averages the normals of the passed TileMesh with this TileMesh's ActiveMesh. 
		/// </summary>
		/// <param name="tm">Mesh to average normals with</param>
		/// <param name="orientation">Orientation of the passed mesh in relation to this one.</param>
		//private void AverageNormalsWith(TileMesh tm, Orientation orientation) {
		//	int res = HeightmapResolution;
		//	int tmRes = tm.HeightmapResolution;

		//	if (tmRes == 0 || res == 0) {
		//		//One or more of the referenced tiles hasn't been 
		//		//generated yet. Skip.
		//		return;
		//	}

		//	bool incrX = false;
		//	int x1Start, x2Start, z1Start, z2Start;
		//	x1Start = x2Start = z1Start = z2Start = 0;

		//	switch (orientation) {
		//		case Orientation.Up:
		//			z1Start = res - 1;
		//			incrX = true;
		//			break;
		//		case Orientation.Right:
		//			x1Start = res - 1;
		//			break;
		//		case Orientation.Down:
		//			z2Start = tmRes - 1;
		//			incrX = true;
		//			break;
		//		case Orientation.Left:
		//			x2Start = tmRes - 1;
		//			break;
		//	}

		//	Vector3[] norms1 = ActiveMesh.normals;
		//	Vector3[] norms2 = tm.ActiveMesh.normals;
			
		//	//Since meshes can be different resolutions, x an z 
		//	//vector components across both meshes are 
		//	//incremented independently
		//	int incrAmt1 = 1;
		//	int incrAmt2 = 1;
		//	if (res > tm.HeightmapResolution) {
		//		incrAmt1 = res / tm.HeightmapResolution;
		//	} else {
		//		incrAmt2 = tm.HeightmapResolution / res;
		//	}
		
		//	for (int i = 0; i < Math.Min(res, tm.HeightmapResolution); i++) {
		//		Vector3 average = (norms1[x1Start + z1Start * HeightmapResolution] + 
		//			norms2[x2Start + z2Start * tm.HeightmapResolution]) / 2;
		//		average = average.normalized;
				
		//		norms1[x1Start + z1Start * HeightmapResolution] = average;
		//		norms2[x2Start + z2Start * tm.HeightmapResolution] = average;

		//		if (incrX) {
		//			x1Start += incrAmt1;
		//			x2Start += incrAmt2;
		//		} else {
		//			z1Start += incrAmt1;
		//			z2Start += incrAmt2;
		//		}
		//	}

		//	ActiveMesh.normals = norms1;
		//	tm.ActiveMesh.normals = norms2;
		//}

		#region Serialization

		/// <summary>
		/// One dimensional representation of the heightmap that 
		/// Unity can serialize.
		/// </summary>
		[SerializeField, HideInInspector]
		private float[] _serializedHeightmap;

		[SerializeField, HideInInspector]
		private int[] _serializedMeshResolutions;

		[SerializeField, HideInInspector]
		private MeshData[] _serializedMeshData;

		public void OnBeforeSerialize() {
			//Heightmap
			if (Heightmap != null) {
				_serializedHeightmap = new float[HeightmapResolution * HeightmapResolution];

				for (int x = 0; x < HeightmapResolution; x++) {
					for (int z = 0; z < HeightmapResolution; z++) {
						_serializedHeightmap[x + z * HeightmapResolution] = Heightmap[x, z];
					}
				}
			}
		}

		public void OnAfterDeserialize() {
			//Heightmap
			if (_serializedHeightmap != null) {
				Heightmap = new float[HeightmapResolution, HeightmapResolution];

				for (int x = 0; x < HeightmapResolution; x++) {
					for (int z = 0; z < HeightmapResolution; z++) {
						Heightmap[x, z] = _serializedHeightmap[x + z * HeightmapResolution];
					}
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Enumeration of the three different levels of detail 
	/// a TileMesh can have. Low, medium, and high which 
	/// each correspond to a different mesh resolution.
	/// </summary>
	[Serializable]
	public enum Resolution : int {
		Low = 32, 
		Medium = 64,
		High = 128
	}
}
