using Terra;
using Terra.Structures;
using Terra.Terrain;
using UnityEngine;

public class TerraStartTest : MonoBehaviour {
	public TerraConfig Config;
	public GameObject FPSController;

	bool Started = false;
	Vector3 InitPos;

	void Start() {
		InitPos = FPSController.transform.position;
	}

	void Update() {
		if (!Started) {
			FPSController.transform.position = InitPos;
		}

		if (Input.GetMouseButtonDown(0)) {
			Started = true;

			if (!Config.Generator.GenerateOnStart)
				Config.Generate();
		}
	}
}
