using UnityEngine;
using XNode;

namespace Terra.Graph.Value {
	[CreateNodeMenu(AbsValueNode.MENU_PARENT_NAME + "Vector 3")]
	class Vector3Node: XNode.Node {
		[Output] public Vector3 Output;
		[Input] public Vector3 Value;

		public override object GetValue(NodePort port) {
			return base.GetValue(port);
		}
	}
}
