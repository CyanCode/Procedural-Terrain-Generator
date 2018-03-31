#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Terra.Nodes.Generation {
	public abstract class AbstractFractalNoiseNode: AbstractGeneratorNode {
		public float Frequency = 1f;
		public float Lacunarity = 2.17f;
		public int OctaveCount = 6;

		public override void Init() {
			base.Init();

			bodyRect.height += 50;
		}

		#if UNITY_EDITOR
		public override void OnBodyGUI() {
			base.OnBodyGUI();

			Frequency = EditorGUILayout.FloatField("Frequency", Frequency);
			Lacunarity = EditorGUILayout.FloatField("Lacunarity", Lacunarity);
			OctaveCount = EditorGUILayout.IntField("Octave Count", OctaveCount);
		}
		#endif
	}
}