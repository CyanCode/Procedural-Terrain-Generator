using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEngine;
using UnityEditor;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(ConstraintNode))]
	public class ConstraintNodeEditor: NodeEditor {
		private const string TITLE = "Constraint";

		private ConstraintNode Constraint {
			get {
				return (ConstraintNode)target;
			}
		}
		
		public override void OnBodyGUI() {
			//Output / Mask Input
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Output"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Mask"));

		    EditorGUI.BeginChangeCheck();


            if (Constraint.GetMaskValue() != null) {
                Constraint.Tolerance = EditorGUIExtension.MinMaxFloatField("Tolerance", Constraint.Tolerance, 0f, 1f);
            }

            //Height Constraint
            ConstraintNode cons = Constraint;
            cons.ConstrainHeight = EditorGUILayout.Toggle("Height", cons.ConstrainHeight);

            if (cons.ConstrainHeight) {
                EditorGUI.indentLevel = 1;

                cons.HeightConstraint = EditorGUIExtension.DrawConstraintRange("Height", cons.HeightConstraint, 0, 1);

                EditorGUILayout.BeginHorizontal();
                cons.HeightProbCurve = EditorGUILayout.CurveField("Probability", cons.HeightProbCurve, Color.green, new Rect(0, 0, 1, 1));
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

            //Angle Constraint
            cons.ConstrainAngle = EditorGUILayout.Toggle("Angle", cons.ConstrainAngle);

            if (cons.ConstrainAngle) {
                EditorGUI.indentLevel = 1;
                
                cons.AngleConstraint = EditorGUIExtension.DrawConstraintRange("Angle", cons.AngleConstraint, 0, 1, 90);

                EditorGUILayout.BeginHorizontal();
                cons.AngleProbCurve = EditorGUILayout.CurveField("Probability", cons.AngleProbCurve, Color.green, new Rect(0, 0, 1, 1));
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

		    Constraint.PlacementProbability = EditorGUIExtension.MinMaxIntField("Place Prob.", Constraint.PlacementProbability, 0, 100);

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
		}

		public override string GetTitle() {
			return TITLE;
		}

		public override Color GetTint() {
			return EditorColors.TintBiome;
		}

		/// <summary>
		/// Shows a map in this node with the passed information
		/// </summary>
		/// <param name="mapProperty">Name of the serialized map generator property</param>
		/// <param name="minMaxMaskProperty">Name of the serialized float min max mask property</param>
		/// <param name="useMapProperty">Name of the serialized bool use map property</param>
		/// <param name="displayName">Display name of this map (heightmap, temperature, etc)</param>
		private void ShowMapField(string mapProperty, string minMaxMaskProperty, string useMapProperty, string displayName) {
			SerializedProperty mapProp = serializedObject.FindProperty(mapProperty);
			SerializedProperty minMaxProp = serializedObject.FindProperty(minMaxMaskProperty);
			SerializedProperty useMapProp = serializedObject.FindProperty(useMapProperty);

			EditorGUILayout.PropertyField(useMapProp, new GUIContent(displayName));
			
			//Use this map as a mask
			if (useMapProp.boolValue) {
				EditorGUI.indentLevel++;

				NodeEditorGUILayout.PropertyField(mapProp, new GUIContent("Generator"));

				//Min / Max Slider
				Rect ctrl = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
				EditorGUI.BeginProperty(ctrl, GUIContent.none, minMaxProp);

				EditorGUI.BeginChangeCheck();

				Vector2 minMaxVal = minMaxProp.vector2Value;
				EditorGUI.MinMaxSlider(ctrl, ref minMaxVal.x, ref minMaxVal.y, 0f, 1f);

				//Modify serialized value if changed
				if (EditorGUI.EndChangeCheck()) {
					minMaxProp.vector2Value = minMaxVal;
				}

				EditorGUI.EndProperty();
				EditorGUI.indentLevel--;
			}
		}
	}
}