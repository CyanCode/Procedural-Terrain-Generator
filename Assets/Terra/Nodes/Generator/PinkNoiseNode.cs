using Terra.CoherentNoise;
using System;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Terrain;
using UnityEditor;
using Assets.Terra.UNEB.Utility;

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise", "Pink")]
	public class PinkNoiseNode: AbstractFractalNoiseNode {
		float Persistence = 1f;

		public override void Init() {
			base.Init();

			bodyRect.height += 10;
		}

		public override Generator GetGenerator() {
			PinkNoise noise = new PinkNoise(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;
			noise.Persistence = Persistence;

			return noise;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();

			Persistence = EditorGUILayout.FloatField("Persistence", Persistence);
		}

		public override string GetName() {
			return "Pink Noise";
		}
	}
}