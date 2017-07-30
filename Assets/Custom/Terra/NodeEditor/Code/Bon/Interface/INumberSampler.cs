using Terra.GraphEditor.Sockets;

namespace Terra.GraphEditor.Nodes {
	public interface INumberSampler
	{
		float GetNumber(OutputSocket outSocket, float x, float y, float z, float seed);
	}
}
