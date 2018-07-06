using Terra.CoherentNoise;
using UnityEngine;

public class NoiseTexturePreviewTest: MonoBehaviour {
	[HideInInspector]
	public Texture Texture;

	public float Cutoff;
	public int Seed;

	[HideInInspector]
	public Generator Generator;
}