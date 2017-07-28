using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class TerrainSplat: MonoBehaviour {
	public TextureSetting[] Textures;

	void Start() {
		if (this.GetComponent<Terrain>() == null)   // if this game obeject does not have a terrain attached, well then fuckit dont run
		{
			return;
		}

		var terrain = this.gameObject.GetComponent<Terrain>();                                                              //create a neat reference to our terrain
		var terrainData = terrain.terrainData;                                                                              //create a neat reference to our terrain data

		terrainData.splatPrototypes = Textures.Select(s => new SplatPrototype { texture = s.Texture }).ToArray();                   //Get all the textures and assign it to the terrain's spaltprototypes
		terrainData.RefreshPrototypes();                                                                                            //gotta refresh my terraindata's prototypes after its manipulated

		int splatLengths = terrainData.splatPrototypes.Length;
		int alphaMapResolution = terrainData.alphamapResolution;
		int alphaMapHeight = terrainData.alphamapResolution;
		int alphaMapWidth = terrainData.alphamapResolution;

		var splatMap = new float[alphaMapResolution, alphaMapResolution, splatLengths];       //create a new splatmap array equal to our map's, we will store our new splat weights in here, then assight it to the map later
		var heights = terrainData.GetHeights(0, 0, alphaMapWidth, alphaMapHeight);                                 //get all the height points for the terrain... this will be where ware are going paint our textures on

		for (var zRes = 0; zRes < alphaMapHeight; zRes++) {
			for (var xRes = 0; xRes < alphaMapWidth; xRes++) {
				var splatWeights = new float[splatLengths];                                             //create a temp array to store all our 'none-normalised weights'
				var normalizedX = (float)xRes / (alphaMapWidth - 1);                        //gets the normalised X position based on the map resolution                     
				var normalizedZ = (float)zRes / (alphaMapHeight - 1);                       //gets the normalised Y position based on the map resolution 
				var randomBlendNoise = ReMap(Mathf.PerlinNoise(xRes * .8f, zRes * .5f), 0, 1, .8f, 1);  //Get a random perlin value

				float angle = terrainData.GetSteepness(normalizedX, normalizedZ);                       //Get the ANGLE/STEEPNESS at this point: returns the angle between 0 and 90
				Vector3 direction = terrainData.GetInterpolatedNormal(xRes, zRes);                      //Get the DIRECTION at this point: returns the direction of the normal as a Vector3
				float elevation = heights[zRes, xRes];                                                  //Get the HEIGHT at this point: return between 0 and 1 (0=lowest trough, .5f=Water level. 1f=highest peak)
				float perlinElevation = heights[zRes, xRes] * randomBlendNoise;                         //Get a semi random height based on perlin noise, this is to give a more random blend, rather than straight horizontal lines.

				for (var i = 0; i < Textures.Length; i++)                                               //Loop through all our trextures and apply them accoding to the rules defined
				{
					var weighting = 0f;                                                                 //set the default weighting to 0, this means that if the image does not meet any of the criteria, then it will have no impact
					var textureSetting = Textures[i];                                                   //get the setting instance based on index
					var calculatedHeight = textureSetting.RandomBlend ? perlinElevation : elevation;    //create a new height variable, and make it the actual height, unless the user selected to add a bit of randomness                                      

					switch (textureSetting.PlacementType) {
						case PlacementType.Angle:
							if (Math.Abs(angle - textureSetting.Angle) < textureSetting.Precisision)                        //check if the specified angle is the same as the current angle (allow a variance based on the precision)
								weighting = textureSetting.Impact;
							break;
						case PlacementType.Direction:
							if (Vector3.SqrMagnitude(direction - textureSetting.Direction) < textureSetting.Precisision)    //check if the specified direction is the same as the current direction (allow a variance based on the precision)
								weighting = textureSetting.Impact;
							break;
						case PlacementType.Elevation:
							if (Math.Abs(textureSetting.Elevation = calculatedHeight) < textureSetting.Precisision)         //check if the specified elevation is the same as the current elevation (allow a variance based on the precision)
								weighting = textureSetting.Impact;
							break;
						case PlacementType.ElevationRange:
							if (calculatedHeight > textureSetting.MinRange && calculatedHeight < textureSetting.MaxRange)    //check if the current height is between the specified min and max heights
								weighting = textureSetting.Impact;
							break;
					}

					splatWeights[i] = weighting;
				}

				#region normalize
				//we need to make sure that the sum of our weights is not greater than 1, so lets normalise it
				var totalWeight = splatWeights.Sum();                               //sum all the splat weights,
				for (int i = 0; i < splatLengths; i++)        //Loop through each splatWeights
				{
					splatWeights[i] /= totalWeight;                                 //Normalize so that sum of all texture weights = 1
					splatMap[zRes, xRes, i] = splatWeights[i];                      //Assign this point to the splatmap array
				}
				#endregion
			}
		}

		terrainData.SetAlphamaps(0, 0, splatMap);
	}

	//Get a random periln value within acceptable range
	public float ReMap(float value, float sMin, float sMax, float mMin, float mMax) {
		return (value - sMin) * (mMax - mMin) / (sMax - sMin) + mMin;
	}
}

[Serializable]
public class TextureSetting {
	[Tooltip("The texture you want to be placed")]
	public Texture2D Texture;
	[Tooltip("The type of placement")]
	public PlacementType PlacementType;

	[Tooltip("The exact height you want this texture to be displayed (.5 will be the middle of the hieght of the map)")]
	[Range(0, 1)]
	public float Elevation;

	[Tooltip("The angle you want this texture to be displayed at (0-19 deggrees)")]
	[Range(0, 90)]
	public float Angle;

	[Tooltip("The min and the max height you want this texture to be displayed (.5 will be the middle of the hieght of the map)")]
	public Vector3 Direction;


	public float MinRange;
	public float MaxRange;

	[Tooltip("Add some random variations to height based placement, this will give a smoother blend based on height")]
	public bool RandomBlend;

	[Tooltip("Comparing floats gives us a chance of losing floating point values. How precisly do you want your values to be interperetted (0.0001f beeing EXTREMELY precise, 0.9f being irrelevent almost)")]
	public float Precisision;

	public int Impact;
}

public enum PlacementType {
	Elevation,
	ElevationRange,
	Angle,
	Direction,
}