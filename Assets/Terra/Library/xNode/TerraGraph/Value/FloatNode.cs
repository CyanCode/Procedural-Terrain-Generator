namespace Terra.Graph.Value {
	[CreateNodeMenu(MENU_PARENT_NAME + "Float")]
	class FloatNode: ValueNode {
		[Output] public float Output;
		[Input] public float Value;
	}
}
