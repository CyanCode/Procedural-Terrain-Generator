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

		public override string GetName() {
			return "Pink Noise";
		}

#if UNITY_EDITOR
		public override void OnBodyGUI() {
			base.OnBodyGUI();

			Persistence = EditorGUILayout.FloatField("Persistence", Persistence);
		}
#endif
	}
}