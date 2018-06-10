using XNode;

namespace Terra.Graph.Value {
	[CreateNodeMenu(AbsValueNode.MENU_PARENT_NAME + "Int")]
	class IntNode: XNode.Node {
		[Output] public int Output;
		[Input] public int Value;

		public override object GetValue(NodePort port) {
			return base.GetValue(port);
		}
	}
}
