using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeTexture: MonoBehaviour {
	public int Seed = 1337;
	public int BiomeCount = 2;
	public float Lacunarity = 1f;
	public float Displacement = 1f;
	public int ImageSize = 200;
	public float Offset = 1f;
	public int PointCount = 1000;

	[Range(0, 10f)]
	public float Frequency = 1f;

	[Range(0, 0.5f)]
	public float BlendSize = 0.1f;

	[HideInInspector]
	public Texture2D Texture;

	[HideInInspector]
	public Texture2D NoiseTexture;
}
