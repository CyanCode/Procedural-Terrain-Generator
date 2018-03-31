using Terra.CoherentNoise;
using System;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Terrain;
using Assets.Terra.UNEB.Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise", "Ridge")]
	public class RidgeNoiseNode: AbstractFractalNoiseNode {
		float Exponent = 1f;
		float Offset = 1f;
		float Gain = 2f;

		public override void Init() {
			base.Init();

			bodyRect.height += 50;
		}
		
		public override Generator GetGenerator() {
			RidgeNoise noise = new RidgeNoise(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;

			noise.Exponent = Exponent;
			noise.Offset = Offset;
			noise.Gain = Gain;

			return noise;
		}

		public override string GetName() {
			return "Ridge Noise";
		}

#if UNITY_EDITOR
		public override void OnBodyGUI() {
			base.OnBodyGUI();

			Exponent = EditorGUILayout.FloatField("Exponent", Exponent);
			Offset = EditorGUILayout.FloatField("Offset", Offset);
			Gain = EditorGUILayout.FloatField("Gain", Gain);
		}
#endif
	}
}
