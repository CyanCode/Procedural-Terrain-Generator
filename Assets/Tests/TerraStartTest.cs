using Terra.Terrain;
using UnityEngine;

public class TerraStartTest : MonoBehaviour {
	public TerraSettings Settings;
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

			if (!Settings.EditorState.GenerateOnStart)
				Settings.Generate();
		}
	}
}
