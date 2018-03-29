using UnityEditor;

namespace Terra.Nodes.Generation {
	public abstract class AbstractVoronoiNoiseNode: AbstractGeneratorNode {
		public float Frequency;
		public float Period;

		public override void Init() {
			base.Init();

			bodyRect.height += 30f;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();

			Frequency = EditorGUILayout.FloatField("Frequency", Frequency);
			Period = EditorGUILayout.FloatField("Period", Period);
		}
	}
}