using System;
using System.Collections;
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
		/// The minimum value in the heightmap after the last call 
		/// to <see cref="CalculateHeightmap"/>. Default value is 
		/// <see cref="float.PositiveInfinity"/>
		/// </summary>	
		public float HeightmapMin = float.PositiveInfinity;

		/// <summary>
		/// The maximum value in the heightmap after the last call 
		/// to <see cref="CalculateHeightmap"/>. Default value is 
		/// <see cref="float.NegativeInfinity"/>
		/// </summary>
		public float HeightmapMax = float.NegativeInfinity;

		/// <summary>
		/// Resolution of the heightmap
		/// </summary>
		public int HeightmapResolution {
			get { return _heightmapResolution; }
			private set { _heightmapResolution = value; }
		}

		/// <summary>
		/// The last heightmap computed after calling <see cref="CalculateHeightmapAsync"/> or 
		/// <see cref="CalculateHeightmap"/>. To apply this heightmap to the <see cref="UnityEngine.Terrain"/> 
		/// call <see cref="SetTerrainHeightmap"/>.
		/// </summary>
		public float[,] Heightmap { get; private set; }

		public LodData.Lod Lod {
			get { return _lod; }
			set {
				_lod = value;
				HeightmapResolution = value.Resolution;
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
		/// The LOD of this mesh during the last creation of the heightmap. If 
		/// the heightmap hasn't been created yet this value is null.
		/// </summary>
		public LodData.Lod LastGeneratedLodLevel {
			get {
				return _lastGeneratedLodLevel;
			}
		}

		/// <summary>
		/// Internal <see cref="Lod"/>
		/// </summary>
		[SerializeField]
		private LodData.Lod _lod;

		/// <summary>
		/// Internal <see cref="HeightmapResolution"/>
		/// </summary>
		[SerializeField]
		private int _heightmapResolution;

		/// <summary>
		/// Tile using this TileMesh
		/// </summary>
		[SerializeField]
		private Tile _tile;
		
		/// <summary>
		/// LOD of last generated heightmap
		/// </summary>
		[SerializeField]
		private LodData.Lod _lastGeneratedLodLevel;

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
		/// <param name="lod">LOD level to reference when creating heightmap and Terrain</param>
		public TileMesh(Tile tile, LodData.Lod lod) {
			_tile = tile;
			Lod = lod;
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
		/// Creates a heightmap of resolution <see cref="HeightmapResolution"/>. If a 
		/// <see cref="Heightmap"/> of the same resolution or higher has already been 
		/// created, this method does nothing.
		/// A heightmap is 2D array of floats that represents the Y values (or heights) 
		/// of to-be created vertices in 3D space.
		/// </summary>
		/// <param name="gridPos">Optionally override the GridPosition from the referenced Tile</param>
		/// <param name="remapMin">Optionally linear transform the heightmap from [min, max] to [0, 1]</param>
		/// <param name="remapMax">Optionally linear transform the heightmap from [min, max] to [0, 1]</param>
		public void CalculateHeightmap(GridPosition? gridPos = null, float remapMin = 0f, float remapMax = 1f) {
			if (!TerraConfig.Instance.Generator.UseMultithreading) {
				_lastGeneratedLodLevel = _tile.GetLodLevel();
				Lod = _lastGeneratedLodLevel;
			}
		
			if (Heightmap != null && (int)Math.Sqrt(Heightmap.Length) >= HeightmapResolution)
				return;

			Heightmap = new float[HeightmapResolution, HeightmapResolution];
			
			float offset = TerraConfig.Instance.Generator.RemapPadding;
			float newMin = offset;
			float newMax = 1 - offset;

			for (int x = 0; x < HeightmapResolution; x++) {
				for (int z = 0; z < HeightmapResolution; z++) {
					Vector2 localXZ = PositionToLocal(x, z, HeightmapResolution);
					Vector2 worldXZ = LocalToWorld(gridPos == null ? _tile.GridPosition : new GridPosition(), localXZ.x, localXZ.y);

					lock (_asyncMeshLock) {
						float height = HeightAt(worldXZ.x, worldXZ.y);

						//Set this instances min and max
						if (height > HeightmapMax) {
							HeightmapMax = height;
						}
						if (height < HeightmapMin) {
							HeightmapMin = height;
						}

						//Transform height
						if (remapMin != 0f && remapMax != 1f) {
							height = (height - remapMin) * (newMax - newMin) / (remapMax - remapMin) + newMin;
						}

						Heightmap[z, x] = height;
					}
				}
			}
		}

		/// <summary>
		/// Creates a heightmap of resolution <see cref="HeightmapResolution"/> asynchronously. 
		/// If a <see cref="Heightmap"/> of the same resolution or higher has already been 
		/// created, this method does nothing.
		/// A heightmap is 2D array of floats that represents the Y values (or heights) 
		/// of to-be created vertices in 3D space.
		/// </summary>
		/// <param name="onComplete">Called when the heightmap has been created</param>
		/// <param name="remapMin">Optionally linear transform the heightmap from [min, max] to [0, 1]</param>
		/// <param name="remapMax">Optionally linear transform the heightmap from [min, max] to [0, 1]</param>
		public void CalculateHeightmapAsync(Action onComplete, float remapMin = 0f, float remapMax = 1f) {
			_lastGeneratedLodLevel = _tile.GetLodLevel();
			Lod = _lastGeneratedLodLevel;

			ThreadPool.QueueUserWorkItem(d => { //Worker thread
				CalculateHeightmap(null, remapMin, remapMax);

				MTDispatch.Instance().Enqueue(onComplete);
			});
		}

		/// <summary>
		/// Remaps each value in the heightmap to the new min and max.
		/// </summary>
		public void RemapHeightmap(float min, float max, float newMin, float newMax) {
			for (int x = 0; x < HeightmapResolution; x++) {
				for (int z = 0; z < HeightmapResolution; z++) {
					float val = Heightmap[x, z];
					Heightmap[x, z] = (val - min) * (newMax - newMin) / (max - min) + newMin;
				}
			}
		}

		/// <summary>
		/// Sets the neighboring <see cref="UnityEngine.Terrain"/> types.
		/// </summary>
		/// <param name="neighbors">Neighboring tiles</param>
		/// <param name="weldNeighbors">Should neighboring Tiles of different resolutions be welded together</param>
		/// <param name="hideTerrain">Should the terrain be hid when setting neighboring tiles?</param>
		/// <param name="onComplete">Called when the neighboring tiles have been set and welded together</param>
		public void SetNeighboringTiles(Neighborhood neighbors, bool weldNeighbors, bool hideTerrain, Action onComplete = null) {
			Neighborhood n = neighbors;

			UnityEngine.Terrain t = ActiveTerrain;
			if (t == null) {
				return;
			}

			if (weldNeighbors) {
				if (onComplete != null) {
					WeldNeighbors(neighbors, hideTerrain, () => {
						UnityEngine.Terrain[] tiles = GetValidNeighbors(n);

						t.SetNeighbors(tiles[0], tiles[1], tiles[2], tiles[3]);
						onComplete();
					});
				} else {
					WeldNeighbors(neighbors, hideTerrain);
				}
			}

			if (onComplete == null) {
				UnityEngine.Terrain[] tiles = GetValidNeighbors(n);

				t.SetNeighbors(tiles[0], tiles[1], tiles[2], tiles[3]);
			}
		}

		/// <summary>
		/// Sets the <see cref="UnityEngine.Terrain"/> component's heightmap to this instances'
		/// <see cref="Heightmap"/>. If <see cref="UnityEngine.Terrain"/> hasn't been created, 
		/// it is added as a component.
		/// </summary>
		/// <remarks>Since this method creates and adds a <see cref="UnityEngine.Terrain"/> 
		/// component, it is not thread safe.</remarks>
		/// <param name="useCoroutine">Should the heightmap be set over a series of frames?</param>
		/// <param name="hideTerrain">Should the terrain be disabled when setting the heightmap?</param>
		/// <param name="onComplete">Called when the coroutine has finished</param>
		/// <param name="heightmap">Optionally use the passed heightmap instead of 
		/// <see cref="TileMesh"/>'s <see cref="Heightmap"/></param>
		public void SetTerrainHeightmap(bool useCoroutine, bool hideTerrain, Action onComplete = null, float[,] heightmap = null) {
			float[,] hm = heightmap ?? Heightmap;

			if (hm == null) {
				return;
			}
			if (ActiveTerrain == null) {
				AddTerrainComponent();
			}
			
			//ReSharper disable once PossibleNullReferenceException
			TerrainData td = ActiveTerrain.terrainData;
			TerraConfig conf = TerraConfig.Instance;
			int length = conf.Generator.Length;

			//Disable terrain rendering while generating
			ActiveTerrain.enabled = !hideTerrain;

			td.heightmapResolution = HeightmapResolution;

			if (useCoroutine) {
				td.size = new Vector3(length, conf.Generator.Amplitude, length);
				_tile.StartCoroutine(SetTerrainHeightmap_Coroutine(hm, () => {
					td.size = new Vector3(length, conf.Generator.Amplitude, length);
					ActiveTerrain.enabled = true;

					if (onComplete != null) {	
						onComplete();
					}
				}));
			} else {
				td.SetHeights(0, 0, hm);
				td.size = new Vector3(length, conf.Generator.Amplitude, length);
				ActiveTerrain.Flush();
				ActiveTerrain.enabled = true;

				if (onComplete != null) {	
					onComplete();
				}
			}
		}

		private IEnumerator SetTerrainHeightmap_Coroutine(float[,] heightmap, Action onComplete) {
			TerrainData td = ActiveTerrain.terrainData;
			const int maxResPerFrame = 64;
			int hmRes = heightmap.GetLength(0) - 1;

			if (hmRes <= maxResPerFrame) {
				td.SetHeights(0, 0, heightmap);
				
				if (onComplete != null) {
					onComplete();
				}

				yield break;
			}

			int resFactor = hmRes / maxResPerFrame;
			int subResolution = hmRes / resFactor;

			//Loop through first chunk of the resolution
			for (int ix = 0; ix < resFactor; ix++) {
				for (int iy = 0; iy < resFactor; iy++) {
					int xPlus1 = ix == resFactor - 1 ? 1 : 0;
					int yPlus1 = iy == resFactor - 1 ? 1 : 0;

					float[,] subheights = new float[subResolution + yPlus1, subResolution + xPlus1];
					
					//Copy heights into new subdivision array
					for (int x = 0; x < subResolution + xPlus1; x++) {
						for (int y = 0; y < subResolution + yPlus1; y++) {
							int thisHmX = ix * subResolution + x;
							int thisHmY = iy * subResolution + y;
							subheights[y, x] = heightmap[thisHmY, thisHmX];
						}
					}

					//Set heights for this subsection
					//td.SetHeightsDelayLOD(subResolution * ix, subResolution * iy, subheights);
					td.SetHeights(subResolution * ix, subResolution * iy, subheights);

					//Wait for next frame
					yield return null;
				}
			}

			if (onComplete != null) {
				onComplete();
			}
		}

		/// <summary>
		/// todo write description
		/// </summary>
		/// <param name="neighbors"></param>
		/// <param name="hideTerrain">Hide the terrain when welding neighbors?</param>
		/// <param name="onComplete">Called when the heightmap has finished applying</param>
		private void WeldNeighbors(Neighborhood neighbors, bool hideTerrain, Action onComplete = null) {
			if (HeightmapResolution == 0) {
				return;
			}

			foreach (Tile t in neighbors) { //todo remove
				if (t.GridPosition == new GridPosition(0,0) && System.Diagnostics.Debugger.IsAttached)
					System.Diagnostics.Debugger.Break();
			}

			if (!(neighbors.Right == null || neighbors.Right.MeshManager.HeightmapResolution == 0)) {
				// x[1 -> max] z[max]
				WeldEdges(neighbors.Right, true, false, hideTerrain);
			}
			if (!(neighbors.Up == null || neighbors.Up.MeshManager.HeightmapResolution == 0)) {
				// x[max] z[1 -> max]
				WeldEdges(neighbors.Up, false, false, hideTerrain);
			}
			if (!(neighbors.Left == null || neighbors.Left.MeshManager.HeightmapResolution == 0)) {
				// x[1 -> max] z[1]
				WeldEdges(neighbors.Left, true, true, hideTerrain);
			}
			if (!(neighbors.Down == null || neighbors.Down.MeshManager.HeightmapResolution == 0)) {
				// x[1] z[1 -> max]
				WeldEdges(neighbors.Down, false, true, hideTerrain);
			}

			if (_tile.GridPosition == new GridPosition(0, 0) && System.Diagnostics.Debugger.IsAttached)
				System.Diagnostics.Debugger.Break();

			bool useCoroutine = TerraConfig.Instance.Generator.UseCoroutineForHeightmap &&
				!TerraConfig.IsInEditMode;
			SetTerrainHeightmap(useCoroutine, false, onComplete);
		}

		/// <summary>
		/// todo write description
		/// </summary>
		/// <param name="neighbor"></param>
		/// <param name="incrX"></param>
		/// <param name="incrStart0"></param>
		private void WeldEdges(Tile neighbor, bool incrX, bool incrStart0, bool hideTerrain) {
			//todo optimize to only setheights on modified stuff
			if (HeightmapResolution > neighbor.MeshManager.HeightmapResolution) {
				int incrStart = incrStart0 ? 0 : HeightmapResolution - 1;
				int neighborIncrStart = incrStart0 ? neighbor.MeshManager.HeightmapResolution - 1 : 0;
				int resDifference = (HeightmapResolution - 1) / (neighbor.MeshManager.HeightmapResolution - 1);
				float[,] neighborHm = neighbor.MeshManager.Heightmap;

				for (int cursor = 1; cursor < HeightmapResolution; cursor++) {
					//Find neighbors start and end vertices
					int neighborStartIdx = (cursor - 1) / resDifference;
					int neighborEndIdx = neighborStartIdx + 1;

					float neighborStartY, neighborEndY;
					if (incrX) {
						neighborStartY = neighborHm[neighborStartIdx, neighborIncrStart];
						neighborEndY = neighborHm[neighborEndIdx, neighborIncrStart];
					} else {
						neighborStartY = neighborHm[neighborIncrStart, neighborStartIdx];
						neighborEndY = neighborHm[neighborIncrStart, neighborEndIdx];
					}

					//Weld vertices in-between neighboring vertices
					int j;
					for (j = 0; j < resDifference - 1; j++) {
						if (cursor >= HeightmapResolution - 1) {
							break;
						}

						float distance = (j + 1) / (float)resDifference;
						float newHeight = 0;

						if (neighborStartY > neighborEndY) {
							float heightDiff = neighborStartY - neighborEndY;
							newHeight = heightDiff * (1 - distance);
							newHeight += neighborEndY;
						}
						if (neighborStartY < neighborEndY) {
							float heightDiff = neighborEndY - neighborStartY;
							newHeight = heightDiff * (distance);
							newHeight += neighborStartY;
						}

						if (incrX) {
							Heightmap[cursor + j, incrStart] = newHeight;
						} else {
							Heightmap[incrStart, cursor + j] = newHeight;
						}
					}

					cursor += j;
				}

				//SetTerrainHeightmapEdges(null, incrX, incrStart0);
			}

			if (HeightmapResolution == neighbor.MeshManager.HeightmapResolution) {
				int incrStart = incrStart0 ? 0 : HeightmapResolution - 1;
				int neighborIncrStart = incrStart0 ? neighbor.MeshManager.HeightmapResolution - 1 : 0;
				float[,] neighborHm = neighbor.MeshManager.Heightmap;

				for (int cursor = 0; cursor < HeightmapResolution; cursor++) {
					//Find neighbors start and end vertices
					if (incrX) {
						float neighborY = neighborHm[cursor, neighborIncrStart];
						float thisY = Heightmap[cursor, incrStart];
						float avg = (neighborY + thisY) / 2;

						neighborHm[cursor, neighborIncrStart] = avg;
						Heightmap[cursor, incrStart] = avg;
					} else {
						float neighborY = neighborHm[neighborIncrStart, cursor];
						float thisY = Heightmap[incrStart, cursor];
						float avg = (neighborY + thisY) / 2;

						neighborHm[neighborIncrStart, cursor] = avg;
						Heightmap[incrStart, cursor] = avg;
					}
				}

				//Update neighbor's heightmap
				GameObject tracked = TerraConfig.Instance.Generator.TrackedObject;
				bool trackedOnNeighbor = false;
				if (tracked != null) {
					GridPosition gp = new GridPosition(tracked, TerraConfig.Instance.Generator.Length);
					if (gp == neighbor.GridPosition) {
						trackedOnNeighbor = true;
					}
				}

				bool useCoroutine = TerraConfig.Instance.Generator.UseCoroutineForHeightmap &&
					!TerraConfig.IsInEditMode;
				neighbor.MeshManager.SetTerrainHeightmap(useCoroutine && !trackedOnNeighbor, hideTerrain && !trackedOnNeighbor);
			}
		}

		/// <summary>
		/// //todo write description
		/// </summary>
		/// <param name="neighbor"></param>
		/// <param name="incrX"></param>
		/// <param name="incrStart0"></param>
		private void SetTerrainHeightmapEdges(Tile neighbor, bool incrX, bool incrStart0) {
			int incrStart = incrStart0 ? 0 : HeightmapResolution - 1;
			int neighborIncrStart = 0;
			int resDifference = 0;
			float[,] neighborHm = null;

			if (neighbor != null) {
				neighborIncrStart = incrStart0 ? neighbor.MeshManager.HeightmapResolution - 1 : 0;
				resDifference = (HeightmapResolution - 1) / (neighbor.MeshManager.HeightmapResolution - 1);
				neighborHm = neighbor.MeshManager.Heightmap;
			}

			for (int cursor = 1; cursor < HeightmapResolution; cursor++) {
				if (incrX) {
					float[,] height = { { Heightmap[cursor, incrStart] } };
					ActiveTerrain.terrainData.SetHeightsDelayLOD(incrStart, cursor, height);
				} else {
					float[,] height = { { Heightmap[incrStart, cursor] } };
					ActiveTerrain.terrainData.SetHeightsDelayLOD(incrStart, cursor, height);
				}

				if (neighbor != null && cursor % resDifference == 0) {
					int neighborStartIdx = (cursor - 1) / resDifference;

					if (incrX) {
						float[,] height = { { neighborHm[neighborStartIdx, neighborIncrStart] } };
						ActiveTerrain.terrainData.SetHeightsDelayLOD(neighborIncrStart, neighborStartIdx, height);
					} else {
						float[,] height = { { neighborHm[neighborIncrStart, neighborStartIdx] } };
						ActiveTerrain.terrainData.SetHeightsDelayLOD(neighborStartIdx, neighborIncrStart, height);
					}
				}
			}

			ActiveTerrain.ApplyDelayedHeightmapModification();

			if (neighbor != null) {
				neighbor.MeshManager.ActiveTerrain.ApplyDelayedHeightmapModification();
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
			var spread = sett.HeightMapData.SpreadAdjusted;

			if (_genNeedsUpdating) {
				sett.HeightMapData.UpdateGenerator();
				_genNeedsUpdating = false;
			}
			
			return sett.HeightMapData.GetValue(worldX, worldZ, spread);
		}

		/// <summary>
		/// Creates an array of valid neighboring Terrains that fit the following 
		/// requirements:
		/// - not null
		/// - heightmap resolutions match between neighboring tile and this one
		/// </summary>
		/// <param name="n">Neighborhood of tiles</param>
		private UnityEngine.Terrain[] GetValidNeighbors(Neighborhood n) {
			UnityEngine.Terrain left = n.Left == null || n.Left.MeshManager.HeightmapResolution != HeightmapResolution ?
				null : n.Left.MeshManager.ActiveTerrain;
			UnityEngine.Terrain top = n.Up == null || n.Up.MeshManager.HeightmapResolution != HeightmapResolution ?
				null : n.Up.MeshManager.ActiveTerrain;
			UnityEngine.Terrain right = n.Right == null || n.Right.MeshManager.HeightmapResolution != HeightmapResolution ?
				null : n.Right.MeshManager.ActiveTerrain;
			UnityEngine.Terrain bottom = n.Down == null || n.Down.MeshManager.HeightmapResolution != HeightmapResolution ?
				null : n.Down.MeshManager.ActiveTerrain;

			return new[] { left, top, right, bottom };
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
