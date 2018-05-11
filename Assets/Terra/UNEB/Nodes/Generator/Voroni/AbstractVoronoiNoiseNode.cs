#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Terra.Nodes.Generation {
	public abstract class AbstractVoronoiNoiseNode: AbstractGeneratorNode {
		public float Frequency;
		public float Period;

		public override void Init() {
			base.Init();

			bodyRect.height += 30f;
		}

#if UNITY_EDITOR
		public override void OnBodyGUI() {
			base.OnBodyGUI();

			EditorGUI.BeginChangeCheck();
			Frequency = EditorGUILayout.FloatField("Frequency", Frequency);
			Period = EditorGUILayout.FloatField("Period", Period);
			if (EditorGUI.EndChangeCheck()) {
				NotifyValueChange();
			}
		}
#endif
	}
}