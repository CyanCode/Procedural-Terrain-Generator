using Assets.Code.Bon;
using Assets.Code.Bon.Interface;
using Assets.Code.Bon.Socket;

public abstract class AbstractStringNode : Node, IStringSampler {

		protected AbstractStringNode(int id, Graph parent) : base(id, parent)
		{
		}

		public abstract string GetString(OutputSocket outSocket);
	}
