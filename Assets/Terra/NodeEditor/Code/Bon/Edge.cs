using System;
using Terra.GraphEditor.Sockets;
using UnityEditor;
using UnityEngine;

namespace Terra.GraphEditor {
	public class Edge
	{
		public static Color HandleColor = Color.black;

		public InputSocket Input;
		public OutputSocket Output;

		// cached vectors for drawing
		private Vector2 _tmpStartPos;
		private Vector2 _tmpEndPos;
		private Vector2 _tmpTangent01;
		private Vector2 _tmpTangent02;

		public Edge(OutputSocket outputSocket, InputSocket inputSocket)
		{
			Input = inputSocket;
			Output = outputSocket;
		}

		public AbstractSocket GetOtherSocket(AbstractSocket socket)
		{
			if (socket == Input) return Output;
			return Input;
		}

		public void Draw()
		{
			if (Input != null && Output != null)
			{
				_tmpStartPos = GetEdgePosition(Output, _tmpStartPos);
				_tmpEndPos = GetEdgePosition(Input, _tmpEndPos);
				_tmpTangent01 = GetTangentPosition(Output, _tmpStartPos);
				_tmpTangent02 = GetTangentPosition(Input, _tmpEndPos);
				DrawEdge(_tmpStartPos, _tmpTangent01, _tmpEndPos, _tmpTangent02, Output.Type);

				Handles.color = Color.black;
				_tmpStartPos.Set(_tmpEndPos.x - 5, _tmpEndPos.y - 5);
				Handles.DrawLine(_tmpEndPos, _tmpStartPos);
				_tmpStartPos.Set(_tmpEndPos.x - 5, _tmpEndPos.y + 5);
				Handles.DrawLine(_tmpEndPos, _tmpStartPos);
			}
		}

		public static void DrawEdge(Vector2 position01, Vector2 tangent01, Vector2 position02, Vector2 tangent02, Type type)
		{
			Handles.DrawBezier(
				position01, position02,
				tangent01, tangent02, HandleColor, null, 6);

			Handles.DrawBezier(
				position01, position02,
				tangent01, tangent02, HandleColor, null, 3);
		}

		public static Vector2 GetEdgePosition(AbstractSocket socket, Vector2 position)
		{
			if (socket.Parent.Collapsed)
			{
				float width = BonConfig.SocketSize;
				if (socket.IsOutput()) width = 0;
				position.Set(socket.X + width, socket.Parent.WindowRect.y + 8);
			}
			else
			{
				float width = 0;
				if (socket.IsOutput()) width = BonConfig.SocketSize;
				position.Set(socket.X + width, socket.Y + BonConfig.SocketSize / 2f);
			}
			return position;
		}

		public static Vector2 GetTangentPosition(AbstractSocket socket, Vector2 position)
		{
			if (socket.IsInput()) return position + Vector2.left*BonConfig.EdgeTangent;
			return position + Vector2.right*BonConfig.EdgeTangent;
		}

		///<summary>Creates a serializable version of this edge.</summary>
		/// <returns>A serializable version of this edge.</returns>
		public SerializableEdge ToSerializedEgde()
		{
			SerializableEdge s = new SerializableEdge();
			s.InputNodeId = Input.Parent.Id;
			s.InputSocketIndex = Input.Parent.Sockets.IndexOf(Input);
			s.OutputNodeId = Output.Parent.Id;
			s.OutputSocketIndex = Output.Parent.Sockets.IndexOf(Output);
			return s;
		}
	}


	[Serializable] public class SerializableEdge
	{
		[SerializeField] public int OutputNodeId = -1;
		[SerializeField] public int OutputSocketIndex = -1;
		[SerializeField] public int InputNodeId = -1;
		[SerializeField] public int InputSocketIndex = -1;
	}
}


