using System.Collections.Generic;
using Terra;
using Terra.Structure;
using Terra.ReorderableList;
using Terra.Terrain;
using UnityEditor;
using UnityEngine;

public class ReorderableObjectList: GenericListAdaptor<ObjectPlacementData> {
	private const float MAX_HEIGHT = 200f;

	private Dictionary<int, Rect> _positions; //Cached positions at last repaint

	public ReorderableObjectList(TerraConfig config, DetailData detail) : base(detail.ObjectData, null, MAX_HEIGHT) {
		_positions = new Dictionary<int, Rect>();
	}

	public override void DrawItem(Rect position, int index) {
		bool isRepaint = Event.current.type == EventType.Repaint;
		if (isRepaint) {
			_positions[index] = position;
		}

		//Resize area to fit within list gui
		var areaPos = _positions.ContainsKey(index) ? _positions[index] : new Rect();
		areaPos.x += 6;
		areaPos.y += 8;
		areaPos.width -= 6;

		//Cache object instance
		var obj = this[index];
		if (obj == null)
			return;

		int startIndent = EditorGUI.indentLevel;
		GUILayout.BeginArea(areaPos);

		//General
		obj.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", obj.Prefab, typeof(GameObject), false);
		obj.AllowsIntersection = EditorGUILayout.Toggle("Can Intersect", obj.AllowsIntersection);
		obj.PlacementProbability = EditorGUILayout.IntSlider("Place Probability", obj.PlacementProbability, 0, 100);
		obj.Spread = EditorGUILayout.Slider("Object Spread", obj.Spread, 5f, 50f);
		obj.MaxObjects = EditorGUILayout.IntField("Max Objects", obj.MaxObjects);
		if (obj.MaxObjects < 1) obj.MaxObjects = 1;

		//Height
		obj.ConstrainHeight = EditorGUILayout.Toggle("Constrain Height", obj.ConstrainHeight);
		if (obj.ConstrainHeight) {
			EditorGUI.indentLevel = 1;

			obj.HeightConstraint = EditorGUIExtension.DrawConstraintRange("Height", obj.HeightConstraint, 0, 1);

			EditorGUILayout.BeginHorizontal();
			obj.HeightProbCurve = EditorGUILayout.CurveField("Probability", obj.HeightProbCurve, Color.green, new Rect(0, 0, 1, 1));
			if (GUILayout.Button("?", GUILayout.Width(25))) {
				const string msg = "This is the height probability curve. The X axis represents the " +
									"min to max height and the Y axis represents the probability an " +
									"object will spawn. By default, the curve is set to a 100% probability " +
									"meaning all objects will spawn.";
				EditorUtility.DisplayDialog("Help - Height Probability", msg, "Close");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel = 0;
		}

		//Angle
		obj.ConstrainAngle = EditorGUILayout.Toggle("Constrain Angle", obj.ConstrainAngle);
		if (obj.ConstrainAngle) {
			EditorGUI.indentLevel = 1;

			obj.AngleConstraint = EditorGUIExtension.DrawConstraintRange("Angle", obj.AngleConstraint, 0, 90);

			EditorGUILayout.BeginHorizontal();
			obj.AngleProbCurve = EditorGUILayout.CurveField("Probability", obj.AngleProbCurve, Color.green, new Rect(0, 0, 90, 1));
			if (GUILayout.Button("?", GUILayout.Width(25))) {
				const string msg = "This is the angle probability curve. The X axis represents " +
									"0 to 90 degrees and the Y axis represents the probability an " +
									"object will spawn. By default, the curve is set to a 100% probability " +
									"meaning all objects will spawn.";
				EditorUtility.DisplayDialog("Help - Angle Probability", msg, "Close");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel = 0;
		}

		//Translate
		EditorGUI.indentLevel = 1;
		obj.ShowTranslateFoldout = EditorGUILayout.Foldout(obj.ShowTranslateFoldout, "Translate");
		if (obj.ShowTranslateFoldout) {
			obj.TranslationAmount = EditorGUILayout.Vector3Field("Translate", obj.TranslationAmount);

			EditorGUILayout.BeginHorizontal();
			obj.IsRandomTranslate = EditorGUILayout.Toggle("Random", obj.IsRandomTranslate);
			if (GUILayout.Button("?", GUILayout.Width(25))) {
				const string msg = "Optionally randomly translate the placed object. " +
									"Max and min extents for the random number generator can " +
									"be set.";
				EditorUtility.DisplayDialog("Help - Random Translate", msg, "Close");
			}
			EditorGUILayout.EndHorizontal();

			if (obj.IsRandomTranslate) {
				EditorGUI.indentLevel = 2;

				obj.RandomTranslateExtents.Min = EditorGUILayout.Vector3Field("Min", obj.RandomTranslateExtents.Min);
				obj.RandomTranslateExtents.Max = EditorGUILayout.Vector3Field("Max", obj.RandomTranslateExtents.Max);

				FitMinMax(ref obj.RandomTranslateExtents.Min, ref obj.RandomTranslateExtents.Max);
				EditorGUI.indentLevel = 1;
			}
		}

		//Rotate
		obj.ShowRotateFoldout = EditorGUILayout.Foldout(obj.ShowRotateFoldout, "Rotate");
		if (obj.ShowRotateFoldout) {
			obj.RotationAmount = EditorGUILayout.Vector3Field("Rotation", obj.RotationAmount);

			EditorGUILayout.BeginHorizontal();
			obj.IsRandomRotation = EditorGUILayout.Toggle("Random", obj.IsRandomRotation);
			if (GUILayout.Button("?", GUILayout.Width(25))) {
				const string msg = "Optionally randomly rotate the placed object. " +
									"Max and min extents for the random number generator can " +
									"be set.";
				EditorUtility.DisplayDialog("Help - Random Rotate", msg, "Close");
			}
			EditorGUILayout.EndHorizontal();

			if (obj.IsRandomRotation) {
				EditorGUI.indentLevel = 2;

				obj.RandomRotationExtents.Min = EditorGUILayout.Vector3Field("Min", obj.RandomRotationExtents.Min);
				obj.RandomRotationExtents.Max = EditorGUILayout.Vector3Field("Max", obj.RandomRotationExtents.Max);

				FitMinMax(ref obj.RandomRotationExtents.Min, ref obj.RandomRotationExtents.Max);
				EditorGUI.indentLevel = 1;
			}
		}

		//Scale
		obj.ShowScaleFoldout = EditorGUILayout.Foldout(obj.ShowScaleFoldout, "Scale");
		if (obj.ShowScaleFoldout) {
			obj.ScaleAmount = EditorGUILayout.Vector3Field("Scale", obj.ScaleAmount);

			EditorGUILayout.BeginHorizontal();
			obj.IsRandomScale = EditorGUILayout.Toggle("Random", obj.IsRandomScale);
			if (GUILayout.Button("?", GUILayout.Width(25))) {
				const string msg = "Optionally randomly scale the placed object. " +
									"Max and min extents for the random number generator can " +
									"be set.";
				EditorUtility.DisplayDialog("Help - Random Scale", msg, "Close");
			}
			EditorGUILayout.EndHorizontal();

			if (obj.IsRandomScale) {
				obj.IsUniformScale = EditorGUILayout.Toggle("Scale Uniformly", obj.IsUniformScale);

				EditorGUI.indentLevel = 2;

				if (obj.IsUniformScale) {
					obj.UniformScaleMin = EditorGUILayout.FloatField("Min", obj.UniformScaleMin);
					obj.UniformScaleMax = EditorGUILayout.FloatField("Max", obj.UniformScaleMax);
				} else {
					obj.RandomScaleExtents.Min = EditorGUILayout.Vector3Field("Min", obj.RandomScaleExtents.Min);
					obj.RandomScaleExtents.Max = EditorGUILayout.Vector3Field("Max", obj.RandomScaleExtents.Max);

					FitMinMax(ref obj.RandomScaleExtents.Min, ref obj.RandomScaleExtents.Max);
				}
				EditorGUI.indentLevel = 1;
			}
		}

		GUILayout.EndArea();

		EditorGUI.indentLevel = startIndent;
	}

	public override float GetItemHeight(int index) {
		const float foldoutHeight = 0.3f;
		float vec3Height = EditorGUIUtility.wideMode ? 1 : 2.5f;

		var obj = this[index];
		float controlCount = 0;

		//Space, Name, Color, Constraints label, Angle, Height, Temperature, Moisture
		controlCount += 12;

		if (obj.ConstrainHeight) controlCount += foldoutHeight + 3;
		if (obj.ConstrainAngle) controlCount += foldoutHeight + 3;

		if (obj.ShowTranslateFoldout) controlCount += foldoutHeight + 2;
		if (obj.ShowRotateFoldout) controlCount += foldoutHeight + 2;
		if (obj.ShowScaleFoldout) controlCount += foldoutHeight + 2;

		if (obj.ShowTranslateFoldout && obj.IsRandomTranslate) controlCount += 2 * vec3Height;
		if (obj.ShowRotateFoldout && obj.IsRandomRotation) controlCount += 2 * vec3Height;
		if (obj.ShowScaleFoldout && obj.IsRandomScale) {
			controlCount += 2 * vec3Height;
			controlCount += 2 * (obj.IsUniformScale ? 1 : vec3Height);
		}

		return EditorGUIUtility.singleLineHeight * controlCount;
	}

	public override void Add() {
		List.Add(new ObjectPlacementData(TerraConfig.GenerationSeed));
	}

	/// <summary>
	/// Fits the min and max values so that the min is never 
	/// greater than the max. 
	/// 
	/// If min > max
	///   min = max
	/// </summary>
	/// <param name="min">Minimum value</param>
	/// <param name="max">Maximum value</param>
	private static void FitMinMax(ref float min, ref float max) {
		min = min > max ? max : min;
	}

	/// <summary>
	/// Fits the min and max values so that the min Vector3's 
	/// components never exceed the max's.
	/// 
	/// If min > max
	///   min = max
	/// </summary>
	/// <param name="min">Minimum vector</param>
	/// <param name="max">Maximum vector</param>
	private static void FitMinMax(ref Vector3 min, ref Vector3 max) {
		if (min.x > max.x || min.y > max.y || min.z > max.z) {
			min = new Vector3(min.x > max.x ? max.x : min.x,
				min.y > max.y ? max.y : min.y,
				min.z > max.z ? max.z : min.z);
		}
	}
}