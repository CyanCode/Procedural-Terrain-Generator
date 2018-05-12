using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Terra.Terrain.Cave {
	[CustomEditor(typeof(CaveGenerator))]
	public class CaveEditor: Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			if (GUILayout.Button("Update Cave")) {
				
			}
		}
	}
}
