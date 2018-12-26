namespace Terra.Graph.Values {
	[CreateNodeMenu(MENU_PARENT_NAME + "Int")]
	class IntNode: ValueNode {
		[Output] public int Output;
		[Input] public int Value;
	}
}
