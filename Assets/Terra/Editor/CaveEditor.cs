using UnityEditor;
using UnityEngine;

namespace Assets.Terra.Terrain.Cave {
	[CustomEditor(typeof(CaveGenerator))]
	public class CaveEditor: Editor {
		private CaveGenerator CaveGen {
			get {
				return ((CaveGenerator)target);
			}
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			if (GUILayout.Button("Update Cave")) {
				CaveGen.ClearCave();
				CaveGen.DrawCave();
			}
		}
	}
}
