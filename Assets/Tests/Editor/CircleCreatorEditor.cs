using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CircleCreator))]
public class CircleCreatorEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		CircleCreator cc = (CircleCreator)target;

		if (GUILayout.Button("Recalculate Circle")) {
			cc.CalculateGridPointsFloat();
		}
	}
}
