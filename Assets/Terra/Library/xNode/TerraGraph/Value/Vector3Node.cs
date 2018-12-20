using UnityEngine;
using XNode;

namespace Terra.Graph.Value {
	[CreateNodeMenu(MENU_PARENT_NAME + "Vector 3")]
	class Vector3Node: ValueNode {
		[Output] public Vector3 Output;
		[Input] public Vector3 Value;

		public override object GetValue(NodePort port) {
			return base.GetValue(port);
		}
	}
}
