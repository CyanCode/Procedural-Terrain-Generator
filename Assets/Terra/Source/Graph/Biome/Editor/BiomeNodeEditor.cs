using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEngine;
using UnityEditor;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(BiomeNode))]
	public class BiomeNodeEditor: NodeEditor {
		private const string TITLE = "Biome";

		private BiomeNode Bn {
			get {
				return (BiomeNode)target;
			}
		}
		
		public override void OnBodyGUI() {
			//Output
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Output"));

			//Biome Name
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Name"), new GUIContent("Biome Name"));
             
			//Preview Color
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewColor"), new GUIContent("Preview Color"));
            
			//Blend
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Blend"));

			//Maps
			EditorGUILayout.Space(); 
			EditorGUILayout.LabelField("Masks");

			ShowMapField("HeightmapGenerator", "HeightmapMinMaxMask", "UseHeightmap", "Heightmap");
			ShowMapField("TemperatureGenerator", "TemperatureMinMaxMask", "UseTemperature", "Temperature");
			ShowMapField("MoistureGenerator", "MoistureMinMaxMask", "UseMoisture", "Moisture");

			//Splats
			EditorGUILayout.Space(); 
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("SplatDetails"), new GUIContent("Splat"));

            //Trees
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Trees"), new GUIContent("Trees"));

            //Grass
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Grass"), new GUIContent("Grass"));

            //Objects
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Objects"));

			//Show Preview
			PreviewField.Show(Bn);

			serializedObject.ApplyModifiedProperties();
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