using Terra.CoherentNoise.Texturing;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseTexturePreviewTest))]
public class NoiseTexturePreviewEditor: Editor {
	public override void OnInspectorGUI()
	{
		var target = (NoiseTexturePreviewTest)base.target;

		EditorGUI.BeginChangeCheck();
		target.Seed = EditorGUILayout.IntField("Seed", target.Seed);
		target.Cutoff = EditorGUILayout.Slider("Cutoff", target.Cutoff, 0.0f, 0.1f);
		if (EditorGUI.EndChangeCheck()) {
			target.Texture = GetTexture();
		}

		var ctr = EditorGUILayout.GetControlRect(false, 100);
		ctr.width = 100;
		if (target.Texture) {
			EditorGUI.DrawPreviewTexture(ctr, target.Texture);
		}
	}

	private Texture2D GetTexture() {
		var target = (NoiseTexturePreviewTest)base.target;
		var seed = target.Seed;
		int s = 100;

		Texture2D t  = new Texture2D(s, s);
		FastNoise fn = new FastNoise(seed);
		for (int i = 0; i < s; i++) {
			for (int j = 0; j < s; j++) {
				float nv = fn.GetWhiteNoiseInt(i, j);

				//normalize values [-1, 1] -> [0, 1]
				nv = (nv + 1) / 2;

				//Find dark pixels
				nv = nv < target.Cutoff ? 1f : 0f;

				t.SetPixel(i, j, new Color(nv, nv, nv, 1));
			}
		}

		t.Apply();
		return t;
	}
}

