using System;
using UnityEngine;
using XNode;

namespace Terra.Graph {
	[Serializable]
	public abstract class PreviewableNode: Node {
		public Texture2D PreviewTexture;

		[SerializeField] public bool IsPreviewDropdown;

		public static string[] FieldNames {
			get {
				return new[] { "PreviewTexture", "PreviewTextureSize", "IsPreviewDropdown" };
			}
		}

		public abstract Texture2D DidRequestTextureUpdate(int size, float spread);
	}
}