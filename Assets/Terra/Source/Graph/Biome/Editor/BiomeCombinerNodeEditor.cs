using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(BiomeCombinerNode))]
	public class BiomeCombinerNodeEditor: NodeEditor {
		private BiomeCombinerNode Bcn {
			get {
				return (BiomeCombinerNode)target;
			}
		}

		public override void OnBodyGUI() {
			//Output 
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Output"));

			//Draw mix method enum
			SerializedProperty mixType = serializedObject.FindProperty("Mix");
			NodeEditorGUILayout.PropertyField(mixType, new GUIContent("Mix Type"));

			//Draw Instance Ports with colors
			if (!Bcn.DidAddPort) {
				NodePort[] ports = Bcn.GetInstanceInputs();

				for (var i = 0; i < ports.Length; i++) {
					NodePort p = ports[i];
					EditorGUILayout.BeginHorizontal();
					NodeEditorGUILayout.PortField(p, GUILayout.ExpandWidth(false));

					BiomeNode node = p.GetInputValue<BiomeNode>();
					if (node != null) {
#if UNITY_2018_1_OR_NEWER
                        EditorGUILayout.ColorField(GUIContent.none, node.PreviewColor, false, false, false,
							GUILayout.MaxWidth(32f));
#else
                        EditorGUILayout.ColorField(GUIContent.none, node.PreviewColor, 
                            false, false, false, null, GUILayout.MaxWidth(32f));
#endif
                    }

					EditorGUILayout.EndHorizontal();
				}
			} else {
				Bcn.DidAddPort = false;
			}
		

			//Show Preview
			PreviewField.Show(Bcn);
		}

		public override Color GetTint() {
			return EditorColors.TintBiome;
		}

		public override string GetTitle() {
			return "Biome Combiner";
		}
	}
}