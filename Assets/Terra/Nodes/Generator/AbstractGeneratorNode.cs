using Terra.CoherentNoise;
using System;
using UnityEngine;
using UNEB;

namespace Terra.Nodes.Generation {
	public abstract class AbstractGeneratorNode: Node {
		public override void Init() {
			base.Init();

			AddOutput("Output", true);
			FitKnobs();

			name = GetName();
			bodyRect.height += 20f;
		}

		public override void OnNewInputConnection(NodeInput addedInput) {
			base.OnNewInputConnection(addedInput);
			
			if (addedInput.ParentNode != null && addedInput.ParentNode is NoisePreviewNode) {
				((NoisePreviewNode) addedInput.ParentNode).TextureNeedsUpdating = true;
			}
		}

		/// <summary>
		/// Get the generator associated with this node. Is 
		/// Used to compute the final generator passed to the end 
		/// node and preview nodes.
		/// </summary>
		/// <returns>Generator</returns>
		public abstract Generator GetGenerator();

		/// <summary>
		/// Get the name of this node. Will be displayed in the header 
		/// section of the node in the graph editor.
		/// </summary>
		/// <returns>Name</returns>
		public abstract string GetName();

		//[NonSerialized]
		//protected OutputSocket OutSocket;
		//[NonSerialized]
		//protected Texture2D PreviewTexture;
		//[NonSerialized]
		//protected bool TextureNeedsUpdating = true;
		//[NonSerialized]
		//protected Generator Generator;

		//[NonSerialized]
		//private Rect ErrorMessageLabel;

		//protected AbstractGeneratorNode(int id, Graph parent) : base(id, parent) {
		//	OutSocket = new OutputSocket(this, typeof(AbstractGeneratorNode));
		//	Sockets.Add(OutSocket);
		//}


		//public static Generator GetInputGenerator(InputSocket socket) {
		//	if (socket.Type == typeof(AbstractGeneratorNode)) {
		//		if (!socket.IsConnected()) return null;

		//		return ((AbstractGeneratorNode)socket.GetConnectedSocket().Parent).GetGenerator();
		//	} else {
		//		Debug.LogError("InputSocket is not of type AbstractGeneratorNode");
		//		return null;
		//	}
		//}
	}
}