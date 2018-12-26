using Terra;
using Terra.Structures;
using UnityEngine;

[ExecuteInEditMode]
public class LandMatEditor : MonoBehaviour {
	public float Height0;
	public float Height1;
	public float Height4;

	//Material to modify
	Material m;

	// Use this for initialization
	void Start () {
		TerraConfig config = FindObjectOfType<TerraConfig>();
		//m = settings.CustomMaterial;
	}
	
	// Update is called once per frame
	void Update () {
		if (m != null) {
			m.SetFloat("_Height0", Height0);
			m.SetFloat("_Height1", Height1);
			m.SetFloat("_Height4", Height4);
		}
	}
}
