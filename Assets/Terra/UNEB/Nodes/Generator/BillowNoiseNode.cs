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
	[GraphContextMenuItem("Noise", "Billow")]
	public class BillowNoiseNode: AbstractFractalNoiseNode {
		float Persistence = 1f;

		public override void Init() {
			base.Init();

			bodyRect.height += 10;
		}

		public override Generator GetGenerator() {
			BillowNoise noise = new BillowNoise(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;
			noise.Persistence = Persistence;
			
			return noise;
		}

		public override string GetName() {
			return "Billow Noise";
		}

#if UNITY_EDITOR
		public override void OnBodyGUI() {
			base.OnBodyGUI();

			EditorGUI.BeginChangeCheck();
			Persistence = EditorGUILayout.FloatField("Persistence", Persistence);
			if (EditorGUI.EndChangeCheck()) {
				NotifyValueChange();
			}
		}
		#endif
	}
}