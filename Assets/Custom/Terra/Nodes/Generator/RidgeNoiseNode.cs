using UnityEngine;
using CoherentNoise;
using System;
using CoherentNoise.Generation.Fractal;
using Terra.GraphEditor.Sockets;
using Terra.GraphEditor;
using Terra.GraphEditor.Nodes;

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise", "Ridge")]
	public class RidgeNoiseNode: AbstractFractalNoiseNode {
		[NonSerialized]
		private Rect LabelExponent;
		[NonSerialized]
		private Rect LabelOffset;
		[NonSerialized]
		private Rect LabelGain;

		[NonSerialized]
		private InputSocket InputSocketExponent;
		[NonSerialized]
		private InputSocket InputSocketOffset;
		[NonSerialized]
		private InputSocket InputSocketGain;

		public RidgeNoiseNode(int id, Graph parent) : base(id, parent) {
			LabelExponent = new Rect(6, 60, 90, BonConfig.SocketSize);
			LabelOffset = new Rect(6, 80, 90, BonConfig.SocketSize);
			LabelGain = new Rect(6, 100, 90, BonConfig.SocketSize);

			InputSocketExponent = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketOffset = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketGain = new InputSocket(this, typeof(AbstractNumberNode));

			InputSocketExponent.SetDirectInputNumber(1f, false);
			InputSocketOffset.SetDirectInputNumber(1f, false);
			InputSocketGain.SetDirectInputNumber(2f, false);

			Sockets.Add(InputSocketExponent);
			Sockets.Add(InputSocketOffset);
			Sockets.Add(InputSocketGain);

			Height = 140;
		}

		public override Generator GetGenerator() {
			RidgeNoise noise = new RidgeNoise(12); //TODO: Implement static seed
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;

			noise.Exponent = AbstractNumberNode.GetInputNumber(InputSocketExponent);
			noise.Offset = AbstractNumberNode.GetInputNumber(InputSocketOffset);
			noise.Gain = AbstractNumberNode.GetInputNumber(InputSocketGain);

			return noise;
		}

		public override void OnGUI() {
			base.OnGUI();

			GUI.Label(LabelExponent, "Exponent");
			GUI.Label(LabelOffset, "Offset");
			GUI.Label(LabelGain, "Gain");
		}
	}
}
