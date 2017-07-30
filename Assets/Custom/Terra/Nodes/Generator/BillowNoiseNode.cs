using UnityEngine;
using CoherentNoise;
using System;
using CoherentNoise.Generation.Fractal;
using Terra.GraphEditor;
using Terra.GraphEditor.Sockets;
using Terra.GraphEditor.Nodes;

namespace Terra.Nodes.Generation {
	[Serializable]
	[GraphContextMenuItem("Noise", "Billow")]
	public class BillowNoiseNode: AbstractFractalNoiseNode {
		[NonSerialized]
		private Rect LabelPersistence;
		[NonSerialized]
		private Rect LabelOffset;
		[NonSerialized]
		private Rect LabelGain;

		[NonSerialized]
		private InputSocket InputSocketPersistence;
		[NonSerialized]
		private InputSocket InputSocketOffset;
		[NonSerialized]
		private InputSocket InputSocketGain;

		public BillowNoiseNode(int id, Graph parent) : base(id, parent) {
			LabelPersistence = new Rect(6, 60, 90, BonConfig.SocketSize);
			InputSocketPersistence = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketPersistence.SetDirectInputNumber(1f, false);
			Sockets.Add(InputSocketPersistence);

			Height = 100;
		}

		public override Generator GetGenerator() {
			BillowNoise noise = new BillowNoise(12); //TODO: Implement static seed
			noise.Frequency = Frequency;
			noise.Lacunarity = Lacunarity;
			noise.OctaveCount = OctaveCount;
			noise.Persistence = AbstractNumberNode.GetInputNumber(InputSocketPersistence);

			return noise;
		}

		public override void OnGUI() {
			base.OnGUI();

			GUI.Label(LabelPersistence, "Persistence");
		}
	}
}