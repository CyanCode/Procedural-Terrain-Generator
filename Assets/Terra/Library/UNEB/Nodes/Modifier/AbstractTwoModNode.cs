using System;
using Terra.CoherentNoise;
using Terra.Nodes.Generation;
using UNEB;
using UnityEngine;

namespace Terra.Nodes.Modifier {
	[Serializable]
	public abstract class AbstractTwoModNode: AbstractGeneratorNode {
		public Generator Generator1 {
			get {
				if (InputGen1 == null || !InputGen1.HasOutputConnected() ||
					!(InputGen1.GetOutput(0).ParentNode is AbstractGeneratorNode)) {
					return null;
				}

				return (InputGen1.GetOutput(0).ParentNode as AbstractGeneratorNode).GetGenerator();
			}
		}
		public Generator Generator2 {
			get {
				if (InputGen2 == null || !InputGen2.HasOutputConnected() ||
					!(InputGen2.GetOutput(0).ParentNode is AbstractGeneratorNode)) {
					return null;
				}

				return (InputGen2.GetOutput(0).ParentNode as AbstractGeneratorNode).GetGenerator();
			}
		}

		[SerializeField]
		private NodeInput InputGen1;
		[SerializeField]
		private NodeInput InputGen2;

		public override void Init() {
			base.Init();

			InputGen1 = AddInput("Generator");
			InputGen2 = AddInput("Generator");
			FitKnobs();
		}
	}
}