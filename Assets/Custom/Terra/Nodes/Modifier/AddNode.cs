using Terra.CoherentNoise;
using System;
using Terra.GraphEditor;

namespace Terra.Nodes.Modifier {
	[Serializable]
	[GraphContextMenuItem("Modifier", "Add")]
	public class AddNode: AbstractTwoModNode {
		public AddNode(int id, Graph parent) : base(id, parent) { }

		public override Generator GetGenerator() {
			return Generator1 + Generator2;
		}
	}
}