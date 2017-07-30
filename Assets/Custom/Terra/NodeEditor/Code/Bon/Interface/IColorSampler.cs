using Terra.GraphEditor.Sockets;
using UnityEngine;

namespace Terra.GraphEditor.Nodes {
	public interface IColorSampler
	{
		Color GetColor(OutputSocket socket, float i);
	}
}
