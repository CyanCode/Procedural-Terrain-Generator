using System.Collections.Generic;
using UnityEngine;

namespace Terra.Terrain {
	public class TerraEvent {
		public delegate void Action(GameObject go);
		public delegate void MeshAction(GameObject go, Mesh mesh);
		public delegate void SplatAction(GameObject go, Texture2D splat);

		public static event Action OnMeshWillForm;
		public static event MeshAction OnMeshDidForm;

		public static event Action OnSplatmapWillCalculate;
		public static event SplatAction OnSplatmapDidCalculate;

		public static void TriggerOnMeshWillForm(GameObject go) {
			if (OnMeshWillForm != null) OnMeshWillForm(go);
		}

		public static void TriggerOnMeshDidForm(GameObject go, Mesh mesh) {
			if (OnMeshDidForm != null) OnMeshDidForm(go, mesh);
		}

		public static void TriggerOnSplatmapWillCalculate(GameObject go) {
			if (OnSplatmapWillCalculate != null) OnSplatmapWillCalculate(go);
		}

		public static void TriggerOnSplatmapDidCalculate(GameObject go, Texture2D splat) {
			if (OnSplatmapDidCalculate != null) OnSplatmapDidCalculate(go, splat);
		}
	}
}