using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terra.Structure;
using UnityEngine;

namespace Terra.Terrain {
	[Serializable]
	public class TilePaint {
		/// <summary>
		/// List of control texture maps. 1 control map holds placement 
		/// information for up to 4 splat maps. This is a cached result 
		/// from calling <see cref="GatherTextures"/>
		/// </summary>
		public Texture2D[] Controls;

		/// <summary>
		/// List of SplatData that are placed (splatted) onto the terrain.
		/// This is a cached result from calling <see cref="GatherTextures"/>
		/// </summary>
		public SplatData[] Splats;

		/// <summary>
		/// Standard unity terrain representation of an alphamap. Call <see cref="CalculateAlphaMap"/> 
		/// to fill.
		/// See <see cref="https://docs.unity3d.com/ScriptReference/TerrainData.SetAlphamaps.html"/>
		/// </summary>
		public float[,,] Alphamap { get; private set; }

		public readonly WeightedBiomeMap BiomeMap;

		private UnityEngine.Terrain _terrain {
			get {
				return _tile.GetComponent<UnityEngine.Terrain>();
			}
		}

		[SerializeField]
		private readonly Tile _tile;
		
		public TilePaint(Tile tile) {
			_tile = tile;
			BiomeMap = new WeightedBiomeMap(tile);
		}

		/// <summary>
		/// TODO Summary
		/// </summary>
		public void Paint() {
			BiomeMap.CreateMap();

			if (BiomeMap == null) {
				Debug.LogWarning("CalculateBiomeMap() failed to produce a non-null BiomeMap");
				return;
			}

			if (TerraConfig.TerraDebug.WRITE_BIOME_DEBUG_TEXTURE) {
				WriteDebugBiomeMap();
			}

			SetSplatPrototypes();
			CalculateAlphaMap();
			SetAlphamap();
			//ApplyControlMapsToShaders();
		}

		/// <summary>
		/// Calculates the alphamap for this <see cref="Tile"/>. 
		/// </summary>
		public void CalculateAlphaMap() { //todo implement for terrain
			//Initialize Alphamap structure
			int resolution = _tile.LodLevel.SplatmapResolution;
			Alphamap = new float[resolution, resolution, Splats.Length];

			//Sample weights and fill in textures
			UnityEngine.Terrain t = _terrain;

			for (int x = 0; x < resolution; x++) {
				for (int z = 0; z < resolution; z++) {
					float normx = (float)x / resolution;
					float normz = (float)z / resolution;

					BiomeData[] biomesAt = BiomeMap.BiomesAt(x, z);
					float[] weightsAt = BiomeMap.WeightsAt(x, z);
					float height = t.terrainData.GetHeight(x, z);
					float angle = t.terrainData.GetSteepness(normz, normx);

					float[] weights = GetTextureWeights(biomesAt, weightsAt, height, angle);
					for (var i = 0; i < weights.Length; i++) {
						Alphamap[x, z, i] = weights[i];
					}
				}
			}
		}

		/// <summary>
		/// Applies the calculated alphamap to the Terrain.
		/// </summary>
		public void SetAlphamap() {
			_terrain.terrainData.SetAlphamaps(0, 0, Alphamap);
		}
		
		public void SetSplatPrototypes() {
			SetSplats();	

			SplatPrototype[] prototypes = new SplatPrototype[Splats.Length];
			
			for (int i = 0; i < Splats.Length; i++) {
				SplatData sd = Splats[i];
				SplatPrototype sp = new SplatPrototype();
				
				sp.metallic = sd.Metallic;
				sp.smoothness = sd.Smoothness;

				sp.tileOffset = sd.Offset;
				sp.tileSize = sd.Tiling;

				sp.normalMap = sd.Normal;
				sp.texture = sd.Diffuse;

				prototypes[i] = sp;
			}

			_terrain.terrainData.splatPrototypes = prototypes;
		}

		/// <summary>
		/// Calculates the weights of each splat texture within this 
		/// <see cref="BiomeMap"/>. 
		/// </summary>
		/// <param name="biomes">List of biomes at some point on the Tile</param>
		/// <param name="biomeWeights">List of weights associated with the passed biomes</param>
		/// <param name="height">Height at some point on the Tile</param>
		/// <param name="angle">Angle at some point on the Tile</param>
		/// <returns>Weight of each texture in the order they appear in <see cref="BiomeMap"/></returns>
		float[] GetTextureWeights(BiomeData[] biomes, float[] biomeWeights, float height, float angle) {
			int totalSplatCount = Splats.Length;
			float[] weights = new float[totalSplatCount];
			
			height = Mathf.Clamp01(height);

			//TODO most likely causing issues with terrain painting
			//this is the only different code between here and WeightedBiomeMap.GetTexturePreview()
			for (int bi = 0; bi < biomes.Length; bi++) {
				BiomeData b = biomes[bi];
				float biomeWeight = biomeWeights[bi];

				//Length of SplatData in previous biomes
				int prevBiomeSplatCount = 0;
				for (int i = 0; i < bi; i++) {
					prevBiomeSplatCount += biomes[i].Details.SplatsData.Count;
				}
				
				//Calculate individual "splat weight"
				for (int si = 0; si < b.Details.SplatsData.Count; si++) {
					int weightIndex = prevBiomeSplatCount + si;
					SplatData splat = b.Details.SplatsData[si];

					//Check whether this SplatData fits required height and angle constraints
					bool passHeight = splat.ConstrainHeight && splat.HeightConstraint.Fits(height) || !splat.ConstrainHeight;
					bool passAngle = splat.ConstrainAngle && splat.AngleConstraint.Fits(angle) || !splat.ConstrainAngle;

					if (!passHeight && !passAngle) {
						continue;
					}

					//If it passes height or angle constraints what are the texturing weights
					float weight = 0;
					int count = 0;
					if (passHeight) {
						weight += splat.HeightConstraint.Weight(height, splat.Blend);
						count++;
					}
					if (passAngle) {
						weight += splat.AngleConstraint.Weight(angle, splat.Blend);
						count++;
					}

					weight /= count;
					weight *= biomeWeight;

					//Blend the bottom with the top
					if (weightIndex > 0) {
						weights[weightIndex - 1] = 1 - weight;
						weights[weightIndex] = weight;
					} else {
						weights[weightIndex] = 1f;
					}

					
				}
			}

			//Normalize weights
			float sum = weights.Sum();
			for (var i = 0; i < weights.Length; i++) {
				weights[i] /= sum;
			}

			return weights;
		}

		/// <summary>
		/// Fills <see cref="Splats"/> with the splat data it needs to render 
		/// terrain. Assumes <see cref="BiomeMap"/> has been calculated
		/// first.
		/// </summary>
		private void SetSplats() {
			if (BiomeMap == null)
				return;

			Splats = BiomeMap.WeightedBiomeSet.SelectMany(b => b.Details.SplatsData).ToArray();
		}

		/// <summary>
		/// Writes debug textures to the file system if 
		/// <see cref="TerraConfig.TerraDebug.WRITE_SPLAT_TEXTURES"/> is 
		/// true.
		/// </summary>
		private void WriteDebugTextures() {
#pragma warning disable CS0162 // Unreachable code detected
			if (TerraConfig.TerraDebug.WRITE_SPLAT_TEXTURES) {
				string tileName = _tile != null ?
					"Tile[" + _tile.GridPosition.X + "_" + _tile.GridPosition.Z + "]" :
					"Tile[0_0]";
				string folderPath = Application.dataPath + "/SplatImages/";
				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				int max = TerraConfig.TerraDebug.MAX_TEXTURE_WRITE_COUNT;
				int length = Controls.Length > max ? max : Controls.Length;
				for (var i = 0; i < length; i++) {
					byte[] bytes = Controls[i].EncodeToPNG();
					string name = "Splat" + i + "_" + tileName + ".png";
					File.WriteAllBytes(folderPath + name, bytes);
				}
			}
#pragma warning restore CS0162 // Unreachable code detected
		}

		/// <summary>
		/// Writes a texture representing the biome map to the file system
		/// </summary>
		private void WriteDebugBiomeMap() {
			string tileName = _tile != null ?
				"Tile[" + _tile.GridPosition.X + "_" + _tile.GridPosition.Z + "]" :
				"Tile[0_0]";
			string folderPath = Application.dataPath + "/BiomeImages/";
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			Texture2D preview = BiomeMap.GetPreviewTexture();
			byte[] bytes = preview.EncodeToPNG();
			string name = "Biome_" + tileName + ".png";
			File.WriteAllBytes(folderPath + name, bytes);
		}
	}
}
