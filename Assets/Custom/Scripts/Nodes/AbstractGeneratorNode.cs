using Assets.Code.Bon;
using Assets.Code.Bon.Socket;
using CoherentNoise;
using CoherentNoise.Texturing;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Bon.Nodes {
	public abstract class AbstractGeneratorNode: Node {
		[NonSerialized] protected OutputSocket OutSocket;
		[NonSerialized] protected Texture PreviewTexture;
		[NonSerialized] private Rect ErrorMessageLabel;

		protected AbstractGeneratorNode(int id, Graph parent) : base(id, parent) {
			OutSocket = new OutputSocket(this, typeof(AbstractGeneratorNode));
			Sockets.Add(OutSocket);
		}

		protected void DrawTexture() {
			PreviewTexture = TextureMaker.MonochromeTexture(88, 88, GetGenerator(OutSocket));
			GUI.DrawTexture(new Rect(6, 0, 88, 88), PreviewTexture);
		}

		//protected bool IsUpdatingTexture() {
		//	foreach (GUIThreadedTexture t in Textures)
		//		if (t.IsUpdating) return true;

		//	return false;
		//}

		public abstract Generator GetGenerator(OutputSocket socket);
	}
}