using XNode;

namespace Terra.Graph.Value {
	[CreateNodeMenu(AbsValueNode.MENU_PARENT_NAME + "Float")]
	class FloatNode: XNode.Node {
		[Output] public float Output;
		[Input] public float Value;

		public override object GetValue(NodePort port) {
			return base.GetValue(port);
		}
	}
}
