using UnityEngine;
using Terra.CoherentNoise;
using System;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.GraphEditor;
using Terra.GraphEditor.Sockets;
using Terra.GraphEditor.Nodes;
using Terra.Terrain;
using UnityEditor;

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise", "Billow")]
	public class BillowNoiseNode: AbstractFractalNoiseNode {
		float Persistence = 1f;

		public override void OnBodyGUI() {
			base.OnBodyGUI();

			Persistence = EditorGUILayout.FloatField("Persistence", Persistence);
		}

		public override Generator GetGenerator() {
			BillowNoise noise = new BillowNoise(TerraSettings.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;
			noise.Persistence = Persistence;
			
			return noise;
		}
	}
}