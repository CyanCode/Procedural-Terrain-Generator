using UnityEngine;

namespace Terra.Graph.Values {
	[CreateNodeMenu(MENU_PARENT_NAME + "Vector 3")]
	class Vector3Node: ValueNode {
		[Output] public Vector3 Output;
		[Input] public Vector3 Value;
	}
}
