using System.IO;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TerrainPaint {
	public class MeshSample {
		readonly public float Height;
		readonly public float Angle;

		public MeshSample(float height = 0f, float angle = 0f) {
			Height = height;
			Angle = angle;
		}
	}
	[Serializable]
	public class SplatSetting {
		public Texture MainTexture;
		public Texture NormalTexture;
		public Vector2 Tiling = new Vector2(1, 1);
		public Vector2 Offset;
		public float Smoothness;
		public float Metallic;

		public PlacementType PlacementType;

		public int Angle;

		public float MinRange;
		public float MaxRange;

		public float Impact = 1f;
		public float Precision = 0.9f;
	}
	public enum PlacementType {
		ElevationRange,
		Angle,
	}

	public int AlphaMapResolution = 128 * 2; //128

	GameObject TerrainObject;
	Material TerrainMaterial;
	Mesh Mesh;
	Vector3[] Vertices;
	Vector3[] Normals;
	int MeshResolution;

	float MaxHeight = float.NegativeInfinity;
	float MinHeight = float.PositiveInfinity;

	/// <summary>
	/// Create a TerrainPaint object that paints the passed gameobject
	/// </summary>
	/// <param name="gameobject">Gameobject to paint</param>
	public TerrainPaint(GameObject gameobject) {
		TerrainObject = gameobject;

		const string path = "Nature/Terrain/Standard";
		TerrainMaterial = TerrainObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find(path));

		Mesh = TerrainObject.GetComponent<MeshFilter>().sharedMesh;
		Vertices = Mesh.vertices;
		Normals = Mesh.normals;
		MeshResolution = (int) Math.Sqrt(Mesh.vertexCount);
	}

	/// <summary>
	/// Displays options for materials tab. Can only 
	/// be called from the OnGUI method
	/// </summary>
	/// <param name="settings">Settings instance for modifying values</param>
	public static void DisplayGUI(TerrainSettings settings) {
		EditorGUILayout.Space();

		if (settings.SplatSettings != null) {
			for (int i = 0; i < settings.SplatSettings.Count; i++) {
				SplatSetting splat = settings.SplatSettings[i];

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("X", GUILayout.Height(16), GUILayout.Width(18))) {
					settings.SplatSettings.RemoveAt(i);
					i--;
					continue;
				}

				EditorGUILayout.BeginVertical();
				splat.MainTexture = (Texture)EditorGUILayout.ObjectField("Main Texture", splat.MainTexture, typeof(Texture), true, GUILayout.Height(16));
				splat.NormalTexture = (Texture)EditorGUILayout.ObjectField("Normal Texture", splat.NormalTexture, typeof(Texture), true, GUILayout.Height(16));
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				splat.Metallic = EditorGUILayout.Slider("Metallic", splat.Metallic, 0f, 1f);
				splat.Smoothness = EditorGUILayout.Slider("Smoothness", splat.Smoothness, 0f, 1f);
				splat.Tiling = EditorGUILayout.Vector2Field("Tiling", splat.Tiling);
				splat.Offset = EditorGUILayout.Vector2Field("Offset", splat.Offset);
				
				splat.PlacementType = (PlacementType)EditorGUILayout.EnumPopup("Placement Type", splat.PlacementType);
				switch (splat.PlacementType) {
					case PlacementType.Angle:
						splat.Angle = EditorGUILayout.IntSlider("Angle", splat.Angle, 0, 90);
						break;
					case PlacementType.ElevationRange:
						splat.MaxRange = EditorGUILayout.FloatField("Max Height", splat.MaxRange);
						splat.MinRange = EditorGUILayout.FloatField("Min Height", splat.MinRange);

						if (splat.MinRange > splat.MaxRange) splat.MinRange = splat.MaxRange;
						break;
				}

				splat.Impact = EditorGUILayout.FloatField("Impact", splat.Impact);

				EditorGUILayout.Separator();
			}
		}

		if (GUILayout.Button("Add Material")) {
			if (settings.SplatSettings == null)
				settings.SplatSettings = new List<SplatSetting>();

			settings.SplatSettings.Add(new SplatSetting());
		}
	}

	public void CreateAlphaMap(List<SplatSetting> settings) {
		Texture2D Tex = new Texture2D(AlphaMapResolution, AlphaMapResolution);
		Color[] Colors = new Color[AlphaMapResolution * AlphaMapResolution];

		int colorIdx = 0;
		for (int x = 0; x < AlphaMapResolution; x++) {
			for (int y = 0; y < AlphaMapResolution; y++) {
				MeshSample sample = SampleAt((float) y / (float)AlphaMapResolution, (float)x / (float)AlphaMapResolution);
				float height = sample.Height;
				float angle = sample.Angle;
				float[] weights = new float[settings.Count];
				var blend = 20f; //TODO: Actually put in settings

				for (int i = 0; i < settings.Count; i++) {
					SplatSetting splat = settings[i];
					
					switch (splat.PlacementType) {
						case PlacementType.Angle:
							if (Math.Abs(angle - splat.Angle) < splat.Precision)
								weights[i] = 0f; //TODO: Fix
							break;
						case PlacementType.ElevationRange: 
							if (height > splat.MinRange && height < splat.MaxRange)
								weights[i] = Mathf.Clamp01((blend - (splat.MinRange - height)) / blend);
							break;
					}
				}

				var sum = weights.Sum();
				var l = weights.Length;
				for (int i = 0; i < l; i++) {
					weights[i] /= sum;
				}

				Colors[colorIdx] = new Color(l > 0 ? weights[0] : 0f,
					l > 1 ? weights[1] : 0f,
					l > 2 ? weights[2] : 0f,
					l > 3 ? weights[3] : 0f);
				colorIdx++;
			}
		}

		Tex.SetPixels(Colors);
		Tex.Apply();
		TerrainMaterial.SetTexture("_Control", Tex);

		var len = settings.Count;
		if (len > 0) SetMaterialForSplatIndex(0, settings[0]);
		if (len > 1) SetMaterialForSplatIndex(1, settings[1]);
		if (len > 2) SetMaterialForSplatIndex(2, settings[2]);
		if (len > 3) SetMaterialForSplatIndex(3, settings[3]);
		
		bool test = false;
		if (test) {
			byte[] bytes = Tex.EncodeToPNG();
			File.WriteAllBytes(Application.dataPath + "/Splat.png", bytes);
		}
	}

	//public void X() {

	//	terrainData.splatPrototypes = Textures.Select(s => new SplatPrototype { texture = s.Texture }).ToArray();                   //Get all the textures and assign it to the terrain's spaltprototypes
	//	terrainData.RefreshPrototypes();                                                                                            //gotta refresh my terraindata's prototypes after its manipulated

	//	int splatLengths = terrainData.splatPrototypes.Length;
	//	int alphaMapResolution = terrainData.alphamapResolution;
	//	int alphaMapHeight = terrainData.alphamapResolution;
	//	int alphaMapWidth = terrainData.alphamapResolution;

	//	var splatMap = new float[alphaMapResolution, alphaMapResolution, splatLengths];       //create a new splatmap array equal to our map's, we will store our new splat weights in here, then assight it to the map later
	//	var heights = terrainData.GetHeights(0, 0, alphaMapWidth, alphaMapHeight);                                 //get all the height points for the terrain... this will be where ware are going paint our textures on

	//	for (var zRes = 0; zRes < alphaMapHeight; zRes++) {
	//		for (var xRes = 0; xRes < alphaMapWidth; xRes++) {
	//			var splatWeights = new float[splatLengths];                                             //create a temp array to store all our 'none-normalised weights'
	//			var normalizedX = (float)xRes / (alphaMapWidth - 1);                        //gets the normalised X position based on the map resolution                     
	//			var normalizedZ = (float)zRes / (alphaMapHeight - 1);                       //gets the normalised Y position based on the map resolution 
	//			var randomBlendNoise = ReMap(Mathf.PerlinNoise(xRes * .8f, zRes * .5f), 0, 1, .8f, 1);  //Get a random perlin value

	//			float angle = terrainData.GetSteepness(normalizedX, normalizedZ);                       //Get the ANGLE/STEEPNESS at this point: returns the angle between 0 and 90
	//			Vector3 direction = terrainData.GetInterpolatedNormal(xRes, zRes);                      //Get the DIRECTION at this point: returns the direction of the normal as a Vector3
	//			float elevation = heights[zRes, xRes];                                                  //Get the HEIGHT at this point: return between 0 and 1 (0=lowest trough, .5f=Water level. 1f=highest peak)
	//			float perlinElevation = heights[zRes, xRes] * randomBlendNoise;                         //Get a semi random height based on perlin noise, this is to give a more random blend, rather than straight horizontal lines.

	//			for (var i = 0; i < Textures.Length; i++)                                               //Loop through all our trextures and apply them accoding to the rules defined
	//			{
	//				var weighting = 0f;                                                                 //set the default weighting to 0, this means that if the image does not meet any of the criteria, then it will have no impact
	//				var textureSetting = Textures[i];                                                   //get the setting instance based on index
	//				var calculatedHeight = textureSetting.RandomBlend ? perlinElevation : elevation;    //create a new height variable, and make it the actual height, unless the user selected to add a bit of randomness                                      

	//				switch (textureSetting.PlacementType) {
	//					case PlacementType.Angle:
	//						if (Math.Abs(angle - textureSetting.Angle) < textureSetting.Precisision)                        //check if the specified angle is the same as the current angle (allow a variance based on the precision)
	//							weighting = textureSetting.Impact;
	//						break;
	//					case PlacementType.Direction:
	//						if (Vector3.SqrMagnitude(direction - textureSetting.Direction) < textureSetting.Precisision)    //check if the specified direction is the same as the current direction (allow a variance based on the precision)
	//							weighting = textureSetting.Impact;
	//						break;
	//					case PlacementType.Elevation:
	//						if (Math.Abs(textureSetting.Elevation = calculatedHeight) < textureSetting.Precisision)         //check if the specified elevation is the same as the current elevation (allow a variance based on the precision)
	//							weighting = textureSetting.Impact;
	//						break;
	//					case PlacementType.ElevationRange:
	//						if (calculatedHeight > textureSetting.MinRange && calculatedHeight < textureSetting.MaxRange)    //check if the current height is between the specified min and max heights
	//							weighting = textureSetting.Impact;
	//						break;
	//				}

	//				splatWeights[i] = weighting;
	//			}

	//			#region normalize
	//			//we need to make sure that the sum of our weights is not greater than 1, so lets normalise it
	//			var totalWeight = splatWeights.Sum();                               //sum all the splat weights,
	//			for (int i = 0; i < splatLengths; i++)        //Loop through each splatWeights
	//			{
	//				splatWeights[i] /= totalWeight;                                 //Normalize so that sum of all texture weights = 1
	//				splatMap[zRes, xRes, i] = splatWeights[i];                      //Assign this point to the splatmap array
	//			}
	//			#endregion
	//		}
	//	}

	//	terrainData.SetAlphamaps(0, 0, splatMap);
	//}

	/// <summary>
	/// Gets the highest vertex point y on the mesh and caches result.
	/// </summary>
	/// <returns>Highest vertex height</returns>
	float GetMaxHeight() {
		if (float.IsNegativeInfinity(MaxHeight)) {
			float y = float.NegativeInfinity;

			foreach (Vector3 pos in Mesh.vertices) {
				if (pos.y > y)
					y = pos.y;
			}

			MaxHeight = y;
		}

		return MaxHeight;
	}

	/// <summary>
	/// Gets the lowest vertex point y on the mesh and caches result.
	/// </summary>
	/// <returns>Lowest vertex height</returns>
	float GetMinHeight() {
		if (float.IsInfinity(MinHeight)) {
			float y = float.PositiveInfinity;

			foreach (Vector3 pos in Mesh.vertices) {
				if (pos.y < y)
					y = pos.y;
			}

			MinHeight = y;
		}

		return MinHeight;
	}

	/// <summary>
	/// Resets both min and max height samples to their 
	/// respective defaults allowing GetMinHeight and 
	/// GetMaxHeight to be recalculated.
	/// </summary>
	void ResetCachedHeightSamples() {
		MaxHeight = float.NegativeInfinity;
		MinHeight = float.PositiveInfinity;
	}

	/// <summary>
	/// Finds the height of the passed x and z values on the mesh 
	/// by raycasting. x and z should be normalized.
	/// </summary>
	/// <param name="x">Normalized x position to sample</param>
	/// <param name="z">Normalized z position to sample</param>
	/// <returns>Height if found, float.Nan otherwise</returns>
	MeshSample SampleAt(float x, float z) {
		float res = MeshResolution;
		int sampleLoc = Mathf.RoundToInt(Mathf.Clamp(x * res, 0f, res - 1)) + 
			Mathf.RoundToInt(Mathf.Clamp(z * res, 0f, res - 1)) * MeshResolution;
		float height = Vertices[sampleLoc].y;
		float angle = Normals[sampleLoc].y;
		
		return new MeshSample(height, angle);
	}

	/// <summary>
	/// Sets the terrain splat texture at the passed index to the same 
	/// information provided in the passed material.
	/// </summary>
	/// <param name="index">Splat index to apply material to (0 - 3)</param>
	/// <param name="mat">Material to apply</param>
	void SetMaterialForSplatIndex(int index, SplatSetting splat) {
		//Main Texture
		TerrainMaterial.SetTexture("_Splat" + index, splat.MainTexture);
		TerrainMaterial.SetTextureScale("_Splat" + index, splat.Tiling);
		TerrainMaterial.SetTextureOffset("_Splat" + index, splat.Offset);

		//Normal Texture
		TerrainMaterial.SetTexture("_Normal" + index, splat.NormalTexture);
		TerrainMaterial.SetTextureScale("_Normal" + index, splat.Tiling);
		TerrainMaterial.SetTextureOffset("_Normal" + index, splat.Offset);

		//Metallic / Smoothness information
		TerrainMaterial.SetFloat("_Metallic" + index, splat.Metallic);
		TerrainMaterial.SetFloat("_Smoothness" + index, splat.Smoothness);
	}
}
