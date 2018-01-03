using UnityEngine;

[ExecuteInEditMode]
public class ShaderTest : MonoBehaviour {
	public Texture2D Tex0;
	public Texture2D Tex1;
	public Texture2D Tex2;
	public Texture2D Tex3;
	public Texture2D Control;

	void Awake() {

	}

	void Update() {
		Material mat = GetComponent<MeshRenderer>().material;

		mat.SetTexture("_Splat0", Tex0);
		mat.SetTexture("_Splat1", Tex1);
		mat.SetTexture("_Splat2", Tex2);
		mat.SetTexture("_Splat3", Tex3);
		mat.SetTexture("_Control", Control);

		for (int i = 0; i < mat.passCount; i++) {
			Debug.Log("Pass " + i + ": " + mat.GetPassName(i) + " enabled? " + mat.GetShaderPassEnabled(mat.GetPassName(i)));
		}

		foreach (string keyword in mat.shaderKeywords) {
			Debug.Log("Keyword: " + keyword);
		}
	}
}
