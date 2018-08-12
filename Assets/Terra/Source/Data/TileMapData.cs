using System;
using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Graph.Noise;
using UnityEngine;

namespace Terra.Data {
	/// <summary>
	/// Holds data relating to various types of maps (ie height, temperature, etc). 
	/// Used by <see cref="TerraSettings"/> for storing information.
	/// </summary>
	[Serializable]
	public class TileMapData {
		/// <summary>
		/// The CoherentNoise Generator attached to this instance. If 
		/// <see cref="MapType"/> is set to <see cref="MapGeneratorType.Custom"/>, 
		/// <see cref="CustomGenerator"/> is returned.
		/// </summary>
		public Generator Generator {
			get {
				if (_generator == null) {
					UpdateGenerator();
				}

				return _generator;
			}
		}

		/// <summary>
		/// The type of Generator to use when constructing a map.
		/// </summary>
		public MapGeneratorType MapType {
			get { return _mapType; }
			set {
				if (_mapType != value) {
					//Update generator on MapType change
					_mapType = value;
					UpdateGenerator();
				}
			}
		}

		/// <summary>
		/// The NoiseGraph attached to this map when using 
		/// <see cref="MapGeneratorType.Custom"/>
		/// </summary>
		public NoiseGraph Graph;

		/// <summary>
		/// "Zoom" level applied to the preview texture
		/// </summary>
		public float TextureZoom = 25f;

		/// <summary>
		/// Lower color in the preview texture gradient
		/// </summary>
		public Color RampColor1 = Color.black;

		/// <summary>
		/// Higher color in the preview texture gradient
		/// </summary>
		public Color RampColor2 = Color.white;

		/// <summary>
		/// Last generated preview texture. Assuming <see cref="UpdatePreviewTexture(int,int,UnityEngine.Color,UnityEngine.Color)"/> 
		/// has already been called.
		/// </summary>
		public Texture2D PreviewTexture;

		/// <summary>
		/// Name of this map
		/// </summary>
		public string Name = "";

		/// <summary>
		/// Value that the X & Z values are multiplied by when 
		/// sampling <see cref="Generator"/>
		/// </summary>
		public float Spread = 50f;

		/// <summary>
		/// Value that the Y value is multiplied by when 
		/// sampling <see cref="Generator"/>
		/// </summary>
		public float Amplitude = 100f; //TODO Remove? Why is there an amp here and in settings

		/// <summary>
		/// Internal <see cref="Generator"/>
		/// </summary>
		private Generator _generator;

		/// <summary>
		/// Internal <see cref="MapType"/>
		/// </summary>
		[SerializeField]
		private MapGeneratorType _mapType;

		public TileMapData() {
			MapType = MapGeneratorType.Perlin;
			UpdateGenerator();
		}

		/// <summary>
		/// Updates the preview texture using the two passed colors 
		/// to form a gradient where -1 is color 1 and 1 is color 2. 
		/// Data is taken from <see cref="Generator"/>.
		/// </summary>
		/// <param name="width">Width of texture in pixels</param>
		/// <param name="height">Height of texture in pixels</param>
		/// <param name="c1">Color 1 in gradient</param>
		/// <param name="c2">Color 2 in gradient</param>
		public void UpdatePreviewTexture(int width, int height, Color c1, Color c2) {
			Texture2D tex = new Texture2D(width, height);

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					float v = GetValue(x, y, TextureZoom);
					Color c = Color.Lerp(c1, c2, v);

					tex.SetPixel(x, y, c);
				}
			}

			tex.Apply();
			PreviewTexture = tex;
		}

		/// <summary>
		/// Updates the preview texture with data from <see cref="Generator"/>. 
		/// The texture is colored as a gradient between the two colors 
		/// from <see cref="RampColor1"/> and <see cref="RampColor2"/>
		/// </summary>
		/// <param name="width">Width of texture in pixels</param>
		/// <param name="height">Height of texture in pixels</param>
		public void UpdatePreviewTexture(int width, int height) {
			UpdatePreviewTexture(width, height, RampColor1, RampColor2);
		}

		/// <summary>
		/// Calls GetValue on <see cref="Generator"/> at the passed 
		/// x / z coordinates. The value returned is between 0 and 1.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <param name="zoom">Optionally specify a zoom amount that scales the generator polling horizontally</param>
		/// <returns>Polled value, 0 if <see cref="Generator"/> is null</returns>
		public float GetValue(float x, float z, float zoom = 1f) {
			if (Generator == null)
				return 0f;

			return Generator.GetValue(x / zoom, z / zoom, 0);
		}

		/// <summary>
		/// Updates the <see cref="Generator"/> assigned to this instance based on the 
		/// set <see cref="MapGeneratorType"/> and returns it.
		/// </summary>
		/// <returns>Generator if set, null if <see cref="MapGeneratorType.Custom"/> 
		/// is set and no <see cref="CustomGenerator"/> is specified</returns>
		public void UpdateGenerator() {
			int seed = TerraSettings.GenerationSeed;
			Generator gen;

			switch (MapType) {
				case MapGeneratorType.Perlin:
					gen = new GradientNoise(seed).ScaleShift(0.5f, 0.5f);
					break;
				case MapGeneratorType.Fractal:
					gen = new PinkNoise(seed).ScaleShift(0.5f, 0.5f);
					break;
				case MapGeneratorType.Billow:
					gen = new BillowNoise(seed).ScaleShift(0.5f, 0.5f);
					break;
				case MapGeneratorType.Custom:
					if (Graph == null)
						return;
					gen = Graph.GetEndGenerator();
					break;
				default:
					return;
			}

			_generator = gen;
		}

		/// <summary>
		/// Does this have a (non-null) generator?
		/// </summary>
		/// <returns>true if there is a generator</returns>
		public bool HasGenerator() {
			return Generator != null;
		}
	}

	[Serializable]
	public enum MapGeneratorType {
		Fractal,
		Perlin,
		Billow,
		Custom
	}
}
