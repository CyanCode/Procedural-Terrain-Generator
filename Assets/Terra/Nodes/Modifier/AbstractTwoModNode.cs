using Terra.CoherentNoise;
using Terra.Nodes.Generation;
using UNEB;

namespace Terra.Nodes.Modifier {
	public abstract class AbstractTwoModNode: AbstractGeneratorNode {
		public Generator Generator1 {
			get {
				if (!(InputGen1.ParentNode is AbstractGeneratorNode)) {
					return null;
				}

				Generator g = (InputGen1.ParentNode as AbstractGeneratorNode).GetGenerator();
				return g == null ? null : g;
			}
		}
		public Generator Generator2 {
			get {
				if (!(InputGen2.ParentNode is AbstractGeneratorNode)) {
					return null;
				}

				Generator g = (InputGen2.ParentNode as AbstractGeneratorNode).GetGenerator();
				return g == null ? null : g;
			}
		}

		private NodeInput InputGen1;
		private NodeInput InputGen2;

		public override void Init() {
			base.Init();

			InputGen1 = AddInput("Generator");
			InputGen2 = AddInput("Generator");
			FitKnobs();
		}
	}
}