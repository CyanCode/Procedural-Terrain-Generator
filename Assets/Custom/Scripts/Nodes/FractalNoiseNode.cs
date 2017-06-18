using Assets.Code.Bon.Nodes;
using Assets.Code.Bon.Socket;
using System;
using UnityEngine;
using UnityEditor;
using CoherentNoise;
using CoherentNoise.Generation.Fractal;

namespace Assets.Code.Bon.Graph.Custom {
	[Serializable]
	[GraphContextMenuItem("Noise", "Fractal")]
	public class FractalNoiseNode: AbstractGeneratorNode {
		[Serializable]
		private enum FractalNoises {
			Billow,
			Pink,
			Ridge
		}

		[SerializeField] private FractalNoises NoiseSelection = FractalNoises.Billow;

		[NonSerialized] private Rect LabelSeed;
		[NonSerialized] private Rect LabelLacunarity;
		[NonSerialized] private Rect LabelOctaveCount;
		[NonSerialized] private Rect LabelFrequency;

		[NonSerialized] private InputSocket InputSocketSeed;
		[NonSerialized] private InputSocket InputSocketLacunarity;
		[NonSerialized] private InputSocket InputSocketOctaveCount;
		[NonSerialized] private InputSocket InputSocketFrequency;

		public FractalNoiseNode(int id, Graph parent) : base(id, parent) {
			LabelSeed = new Rect(6, 120, 90, BonConfig.SocketSize);
			LabelLacunarity = new Rect(6, 140, 90, BonConfig.SocketSize);
			LabelOctaveCount = new Rect(6, 160, 90, BonConfig.SocketSize);
			LabelFrequency = new Rect(6, 180, 90, BonConfig.SocketSize);

			InputSocketSeed = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketLacunarity = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketOctaveCount = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketFrequency = new InputSocket(this, typeof(AbstractNumberNode));

			InputSocketLacunarity.SetDirectInputNumber(2.17f, false);
			InputSocketOctaveCount.SetDirectInputNumber(6, false);
			InputSocketFrequency.SetDirectInputNumber(1, false);

			Sockets.Add(InputSocketSeed);
			Sockets.Add(InputSocketLacunarity);
			Sockets.Add(InputSocketOctaveCount);
			Sockets.Add(InputSocketFrequency);

			Height = 220;
		}
		
		public override void OnGUI() {
			//if (!Textures[0].DoneInitialUpdate)
			//	Update();
			//Textures[0].X = 40;

			DrawTexture();
			NoiseSelection = (FractalNoises)EditorGUI.EnumPopup(new Rect(6, 100, 90, BonConfig.SocketSize), NoiseSelection);
			
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
			GUI.Label(LabelSeed, "Seed");
			GUI.Label(LabelLacunarity, "Lacunarity");
			GUI.Label(LabelOctaveCount, "Octave Count");
			GUI.Label(LabelFrequency, "Frequency");

			SocketTopOffsetInput = 120;
		}

		public override Generator GetGenerator(OutputSocket socket) {
			float seed = 1f;
			FractalNoiseBase fractal = null;

			switch (NoiseSelection) {
				case FractalNoises.Billow:
					fractal = new BillowNoise(seed);
					break;
				case FractalNoises.Pink:
					fractal = new PinkNoise(seed);
					break;
				case FractalNoises.Ridge:
					fractal = new RidgeNoise(seed);
					break;
			}

			return fractal;
		}

		public override void Update() {
			
		}
	}
}