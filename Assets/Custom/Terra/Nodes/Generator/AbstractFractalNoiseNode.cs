using System;
using Terra.GraphEditor;
using Terra.GraphEditor.Nodes;
using Terra.GraphEditor.Sockets;
using UnityEngine;

namespace Terra.Nodes.Generation {
	public abstract class AbstractFractalNoiseNode: AbstractGeneratorNode {
		public float Frequency {
			get {
				return AbstractNumberNode.GetInputNumber(InputSocketFrequency);
			}
		}
		public float Lacunarity {
			get {
				return AbstractNumberNode.GetInputNumber(InputSocketLacunarity);
			}
		}
		public int OctaveCount {
			get {
				return (int)AbstractNumberNode.GetInputNumber(InputSocketOctaveCount);
			}
		}

		[NonSerialized]
		private Rect LabelFrequency;
		[NonSerialized]
		private Rect LabelLacunarity;
		[NonSerialized]
		private Rect LabelOctaveCount;

		[NonSerialized]
		private InputSocket InputSocketFrequency;
		[NonSerialized]
		private InputSocket InputSocketLacunarity;
		[NonSerialized]
		private InputSocket InputSocketOctaveCount;

		public AbstractFractalNoiseNode(int id, Graph parent) : base(id, parent) {
			LabelFrequency = new Rect(6, 0, 90, BonConfig.SocketSize);
			LabelLacunarity = new Rect(6, 20, 90, BonConfig.SocketSize);
			LabelOctaveCount = new Rect(6, 40, 90, BonConfig.SocketSize);

			InputSocketFrequency = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketLacunarity = new InputSocket(this, typeof(AbstractNumberNode));
			InputSocketOctaveCount = new InputSocket(this, typeof(AbstractNumberNode));

			InputSocketFrequency.SetDirectInputNumber(1, false);
			InputSocketLacunarity.SetDirectInputNumber(2.17f, false);
			InputSocketOctaveCount.SetDirectInputNumber(6, false);

			Sockets.Add(InputSocketFrequency);
			Sockets.Add(InputSocketLacunarity);
			Sockets.Add(InputSocketOctaveCount);
		}

		/// <summary>
		/// Creates GUI elements that all fractal noise nodes share:
		/// <list type="">
		/// <item>Frequency</item>
		/// <item>Lacunarity</item>
		/// <item>Octave Count</item>
		/// The last label ends at y position 40. 60 Should be used for the next element.
		/// </list>
		/// </summary>
		public override void OnGUI() {
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;

			GUI.Label(LabelFrequency, "Frequency");
			GUI.Label(LabelLacunarity, "Lacunarity");
			GUI.Label(LabelOctaveCount, "Octave Count");
		}

		public override void Update() { }
	}
}