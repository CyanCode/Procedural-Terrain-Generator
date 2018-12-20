using System.Collections;
using System.Collections.Generic;
using Terra.CoherentNoise;
using Terra.Graph;
using Terra.Graph.Noise;
using Terra.Structure;
using UnityEngine;
using XNode;

namespace Terra.Graph {
	[CreateNodeMenu("Biomes/Biome")]
	public class BiomeNode: PreviewableNode {
		[Output]
		public BiomeNode Output;

		/// <summary>
		/// Name of the biome
		/// </summary>
		public string Name;

		public Color PreviewColor;
		public bool IsDropdown;

		/// <summary>
		/// Splats that this Biome will display
		/// </summary>
		[Input(ShowBackingValue.Never)] 
		public SplatObjectNode[] SplatObjects;

		[Input]
		public float Blend = 1f;

		[Input(ShowBackingValue.Never, ConnectionType.Override)] 
		public AbsGeneratorNode HeightmapGenerator;
		public bool UseHeightmap;
		public Vector2 HeightmapMinMaxMask = new Vector2(0, 1);

		[Input(ShowBackingValue.Never, ConnectionType.Override)] 
		public AbsGeneratorNode TemperatureGenerator;
		public bool UseTemperature;
		public Vector2 TemperatureMinMaxMask = new Vector2(0, 1);

		[Input(ShowBackingValue.Never, ConnectionType.Override)] 
		public AbsGeneratorNode MoistureGenerator;
		public bool UseMoisture;
		public Vector2 MoistureMinMaxMask = new Vector2(0, 1);

		public override object GetValue(NodePort port) {
			return this;
		}

		public override Texture2D DidRequestTextureUpdate() {
			return Preview(PreviewTextureSize);
		}

		/// <summary>
		/// Gets the heights for the height, temperature, and moisture maps. If 
		/// a map isn't used, 0 is set for its value. The returned float structure 
		/// is filled as follows:
		/// [heightmap val, temperature val, moisture val]
		/// </summary>
		/// <param name="x">X coordinate to sample</param>
		/// <param name="y">Y coordinate to sample</param>
		/// <returns></returns>
		public float[] GetMapHeightsAt(float x, float y) {
			float[] heights = new float[3];

			var hg = GetHeightmapGenerator();
			var tg = GetTemperatureGenerator();
			var mg = GetMoistureGenerator();

			if (hg != null) {
				heights[0] = hg.GetValue(x, y, 0);
			}
			if (tg != null) {
				heights[1] = tg.GetValue(x, y, 0);
			}
			if (mg != null) {
				heights[2] = mg.GetValue(x, y, 0);
			}

			return heights;
		}

		public float[,] GetNormalizedValues(int resolution) {
			//Constraints
			Constraint hc = new Constraint(HeightmapMinMaxMask.x, HeightmapMinMaxMask.y);
			Constraint tc = new Constraint(TemperatureMinMaxMask.x, TemperatureMinMaxMask.y);
			Constraint mc = new Constraint(MoistureMinMaxMask.x, MoistureMinMaxMask.y);

			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;
			float[,,] heights = new float[resolution, resolution, 3];

			//Fill heights structure and set min/max values
			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					float[] generated = GetMapHeightsAt(x / (float)resolution, y / (float)resolution);

					for (int z = 0; z < 3; z++) {
						float height = generated[z];
						heights[x, y, z] = height;

						if (height < min) {
							min = height;
						}
						if (height > max) {
							max = height;
						}
					}
				}
			}

			float[,] normalized = new float[resolution, resolution];

			//Normalize values and set texture
			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					float hv = (heights[x, y, 0] - min) / (max - min);
					float tv = (heights[x, y, 1] - min) / (max - min);
					float mv = (heights[x, y, 2] - min) / (max - min);

					float val = 0;
					int count = 0;

					//Gather heights that fit set min/max
					if (UseHeightmap && hc.Fits(hv)) {
						val += hc.Weight(hv, Blend);
						//val += hv;
						count++;
					}
					if (UseTemperature && tc.Fits(tv)) {
						val += tv;
						count++;
					}
					if (UseMoisture && mc.Fits(mv)) {
						val += mv;
						count++;
					}

					val = count > 0 ? val / count : 0;
					normalized[x, y] = val;
				}
			}

			return normalized;
		}

		/// <summary>
		/// Creates a texture previewing this biome with the passed size used 
		/// for the width and height
		/// </summary>
		/// <param name="size">width & height</param>
		/// <returns></returns>
		public Texture2D Preview(int size) {
			Texture2D tex = new Texture2D(size, size);
			float[,] normalized = GetNormalizedValues(size);

			//Normalize values and set texture
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					float val = normalized[x, y];
					tex.SetPixel(x, y, new Color(val, val, val, 1f));
				}
			}

			tex.Apply();
			return tex;
		}

		public Generator GetHeightmapGenerator() {
			if (!UseHeightmap) {
				return null;
			}

			AbsGeneratorNode gen = GetInputValue<AbsGeneratorNode>("HeightmapGenerator");
			return gen == null ? null : gen.GetGenerator();
		}

		public Generator GetTemperatureGenerator() {
			if (!UseTemperature) {
				return null;
			}

			AbsGeneratorNode gen = GetInputValue<AbsGeneratorNode>("TemperatureGenerator");
			return gen == null ? null : gen.GetGenerator();
		}

		public Generator GetMoistureGenerator() {
			if (!UseMoisture) {
				return null;
			}

			AbsGeneratorNode gen = GetInputValue<AbsGeneratorNode>("MoistureGenerator");
			return gen == null ? null : gen.GetGenerator();
		}
	}
}