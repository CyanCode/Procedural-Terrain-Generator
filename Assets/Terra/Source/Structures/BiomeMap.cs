using System.Collections.Generic;
using Terra.Graph;
using Terra.Terrain;

namespace Terra.Structure {
	/// <summary>
	/// Represents a 2D structure of biomes within a <see cref="Tile"/>
	/// </summary>
	public class BiomeMap {
		private Tile _tile;
		private float[,] _heightmap;
		private int _heightmapResolution;

		private BiomeCombinerNode _combiner;
		private GridPosition _position;
		private int _resolution;

		private TerraConfig Config { get { return TerraConfig.Instance; } }

		/// <summary>
		/// Map of biomes with each index corresponding to a biome 
		/// at some point. This is populated by calling <see cref="CreateMap"/>.
		/// </summary>
		public BiomeData[,] Map { get; private set; }

		/// <summary>
		/// A set (no duplicates) of biomes that were found while 
		/// calling <see cref="CreateMap"/>
		/// </summary>
		public List<BiomeData> BiomeSet { get; private set; }

		/// <summary>
		/// Readies a biome map for creation and pulls heightmap 
		/// information from the passed <see cref="Tile"/>
		/// </summary>
		/// <param name="tile">Tile to refernce heightmap from</param>
		/// <param name="resolution">Resolution of the map (must be lower 
		/// than or equal to heightmap resolution)</param>
		public BiomeMap(Tile tile, int resolution) {
			_tile = tile;
			_resolution = resolution;
			_heightmap = _tile.MeshManager.Heightmap;
			_heightmapResolution = _tile.MeshManager.HeightmapResolution;

			Map = new BiomeData[resolution, resolution];
			BiomeSet = new List<BiomeData>();
		}

		/// <summary>
		/// Initialize a BiomeMap with the biomes found in the passed combine.
		/// </summary>
		/// <param name="combiner">Combiner containing one or more biomes</param>
		/// <param name="position">Grid position of this BiomeMap</param>
		/// <param name="resolution">Resolution of this BiomeMap</param>
		public BiomeMap(BiomeCombinerNode combiner, GridPosition position, int resolution) {
			_combiner = combiner;
			_position = position;
			_resolution = resolution;
			
		}

		public void CreateMap() {
			for (int x = 0; x < _resolution; x++) {
				for (int z = 0; z < _resolution; z++) {
					//Get biome at this coordinate
					BiomeData biome = GetBiomeAt(x, z);

					Map[x, z] = biome;

					//Add to biome set
					BiomeSet.ForEach(b => {
						if (!BiomeSet.Exists(existing => ReferenceEquals(existing, b))) {
							BiomeSet.Add(b);
						}
					});
				}
			}
		}

		/// <summary>
		/// Gets the bottom-most biome that is active within the list of
		/// biomes stored in <see cref="TerraConfig"/>.
		/// </summary>
		/// <param name="x">X location in heightmap</param>
		/// <param name="z">Z location in heightmap</param>
		public BiomeData GetBiomeAt(int x, int z) {
			BiomeData bottomMost = null;

			//			var tm = Config.TemperatureMapData;
			//			var mm = Config.MoistureMapData;
			//
			//			//TODO Add angle constraint
			//			foreach (BiomeData b in Config.BiomesData) {
			//				if (b.IsTemperatureConstrained && !tm.HasGenerator()) continue;
			//				if (b.IsMoistureConstrained && !mm.HasGenerator()) continue;
			//
			//				//Calculate x and z offsets for checking heightmap
			//				int offset = _resolution / _heightmapResolution;
			//				x *= offset;
			//				z *= offset;
			//
			//				//Calculate height and world x/z positions
			//				var height = _heightmap[x, z];
			//				var local = TileMesh.PositionToLocal(x, z, _resolution);
			//				var world = TileMesh.LocalToWorld(_tile == null ? new GridPosition() : _tile.GridPosition, local.x, local.y);
			//				var wx = world.x;
			//				var wz = world.y;
			//
			//				var temp = tm.GetValue(wx, wz);
			//				var moisture = mm.GetValue(wx, wz);
			//
			//				//If no constraints continue
			//				if (!b.IsHeightConstrained && !b.IsTemperatureConstrained && !b.IsMoistureConstrained) {
			//					bottomMost = b;
			//					continue;
			//				}
			//
			//				//Which map constraints fit the passed value
			//				bool passHeight = b.IsHeightConstrained && b.HeightConstraint.Fits(height);
			//				bool passTemp = b.IsTemperatureConstrained && b.TemperatureConstraint.Fits(temp);
			//				bool passMoisture = b.IsMoistureConstrained && b.MoistureConstraint.Fits(moisture);
			//
			//				if (passHeight && passTemp && passMoisture) {
			//					bottomMost = b;
			//				}
			//			}

			//			return bottomMost;
			return null;
		}
	}
}
