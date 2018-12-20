using XNode;

namespace Terra.Graph.Value {
	[CreateNodeMenu(MENU_PARENT_NAME + "Int")]
	class IntNode: ValueNode {
		[Output] public int Output;
		[Input] public int Value;

		public override object GetValue(NodePort port) {
			return base.GetValue(port);
		}
	}
}
