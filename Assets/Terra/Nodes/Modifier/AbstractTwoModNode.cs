using Terra.CoherentNoise;
using Terra.Nodes.Generation;
using UNEB;

namespace Terra.Nodes.Modifier {
	public abstract class AbstractTwoModNode: AbstractGeneratorNode {
		public Generator Generator1 {
			get {
				AbstractGeneratorNode g = InputGen1.GetValue<AbstractGeneratorNode>();
				return g == null ? null : g.GetGenerator();
			}
		}
		public Generator Generator2 {
			get {
				AbstractGeneratorNode g = InputGen2.GetValue<AbstractGeneratorNode>();
				return g == null ? null : g.GetGenerator();
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