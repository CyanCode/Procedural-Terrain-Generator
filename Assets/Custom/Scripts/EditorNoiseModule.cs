using UnityEditor;
using UnityEngine;

public class EditorNoiseModule {
	private string Name;
	private EditorNoiseModule ChildModule1;
	private EditorNoiseModule ChildModule2;

	private bool FoldoutStatus;

	public EditorNoiseModule(string name = "Module") {
		Name = name;
	}

	/// <summary>
	/// Renders this noise module's texture and its children.
	/// </summary>
	public void RenderNoiseTextures() {
		if (ChildModule1 != null) ChildModule1.RenderNoiseTextures();
		if (ChildModule2 != null) ChildModule2.RenderNoiseTextures();

		//Actually render here
	}

	/// <summary>
	/// Adds a child module if one can be added (no more than 1 already exists)
	/// </summary>
	public void AddChildModule() {
		Debug.Log("Adding child module");

		if (ChildModule1 == null) {
			ChildModule1 = new EditorNoiseModule();
		} else if (ChildModule2 == null) {
			ChildModule2 = new EditorNoiseModule();
		}
	}

	public void Draw() {
		FoldoutStatus = EditorGUILayout.Foldout(FoldoutStatus, Name);

		if (FoldoutStatus) {
			//Dropdown options
			EditorGUI.indentLevel++;

			if (ChildModule1 != null && ChildModule2 != null) {
				EditorGUILayout.Popup(0, new string[] { "Add", "Subtract", "Multiply", "Divide" });
			} if (ChildModule1 != null) {
				ChildModule1.Draw();
			} if (ChildModule2 != null) {
				ChildModule2.Draw();
			}

			//Add noise and combiner modules
			if ((ChildModule1 == null || ChildModule2 == null) && GUILayout.Button("Add Noise Module")) {
				AddChildModule();
			} if ((ChildModule1 == null || ChildModule2 == null) && GUILayout.Button("Add Combiner Module")) {
				AddChildModule();
			}
		}
	}
}