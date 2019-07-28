using Terra.CoherentNoise;
using Terra.Structures;
using UnityEngine;

namespace Terra.Terrain {
	public class GeneratorSampler {
		private Generator _generator;

		public GeneratorSampler(Generator generator) {
			_generator = generator;
		}

	    /// <summary>
	    /// Get the value of the Generator in world space.
	    /// </summary>
	    /// <param name="i">Index of X/Y location in </param>
	    /// <param name="position">Position in Terra grid units</param>
	    /// <param name="resolution">Resolution of this Tile being sampled</param>
	    /// <param name="spread">What to divide the x & y coordinates by before sampling</param>
	    /// <param name="length">Length of a Tile</param>
	    public float GetValue(int i, GridPosition position, int resolution, float spread, int length) {
            int x = i % resolution;
            int y = i / resolution;

	        Vector2 local = TileMesh.PositionToLocal(x, y, resolution);
	        Vector2 world = TileMesh.LocalToWorld(position, local.x, local.y, length);

	        return _generator.GetValue(world.x / spread, world.y / spread, 0f);
	    }

        /// <summary>
        /// Get the value of the Generator in world space.
        /// </summary>
        /// <param name="x">X coordinate in resolution</param>
        /// <param name="y">Y coordinate in resolution</param>
        /// <param name="position">Position in Terra grid units</param>
        /// <param name="resolution">Resolution of this Tile being sampled</param>
        /// <param name="spread">What to divide the x & y coordinates by before sampling</param>
        /// <param name="length">Length of a Tile</param>
        public float GetValue(int x, int y, GridPosition position, int resolution, float spread, int length) {
			Vector2 local = TileMesh.PositionToLocal(x, y, resolution);
			Vector2 world = TileMesh.LocalToWorld(position, local.x, local.y, length);

			return _generator.GetValue(world.x / spread, world.y / spread, 0f);
		}

		/// <summary>
		/// Get the value of the Generator in world space.
		/// </summary>
		/// <param name="x">X coordinate in resolution</param>
		/// <param name="y">Y coordinate in resolution</param>
		/// <param name="position">Position in Terra grid units</param>
		/// <param name="resolution">Resolution of this Tile being sampled</param>
		public float GetValue(int x, int y, GridPosition position, int resolution) {
			GenerationData gen = TerraConfig.Instance.Generator;
			return GetValue(x, y, position, resolution, gen.Spread, gen.Length);			
		}
        
	    public MinMaxResult GetRemap() {
	        int res = TerraConfig.Instance.Generator.RemapResolution;
	        MinMaxRecorder recorder = new MinMaxRecorder();

	        for (int x = 0; x < res; x++) {
	            for (int y = 0; y < res; y++) {
	                recorder.Register(_generator.GetValue(x / (float)res, y / (float)res, 0));
	            }
	        }

	        return recorder.GetMinMax();
	    }
    }
}