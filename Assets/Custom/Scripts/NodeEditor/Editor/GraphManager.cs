using Assets.Code.Bon;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GraphManager {
	/// <summary>
	/// Opens a graph at the specified path
	/// </summary>
	/// <param name="path">Path of graph to open</param>
	/// <param name="settings">Settings to persist created Graph instance</param>
	public static void Open(string path, TerrainSettings settings) {
		BonLauncher launcher = Object.FindObjectOfType<BonLauncher>();
		if (launcher == null) {
			Debug.LogError("There is no graph launcher in the scene. Make sure to not delete graph launcher component");
			return;
		}

		settings.LoadedGraph = launcher.LoadGraph(path);
		CreateGraphWindow(settings);
	}

	public static void OpenNew(string path, TerrainSettings settings) {
		BonLauncher launcher = Object.FindObjectOfType<BonLauncher>();
		if (launcher == null) {
			Debug.LogError("There is no graph launcher in the scene. Make sure to not delete graph launcher component");
			return;
		}

		settings.LoadedGraph = launcher.LoadGraph(BonConfig.DefaultGraphName);
		launcher.SaveGraph(settings.LoadedGraph, path);
		CreateGraphWindow(settings);
	}

	/// <summary>
	/// Checks whether file at path can be deserialized into a Graph
	/// </summary>
	/// <param name="path">Path to check</param>
	/// <returns>true if successful, false otherwise</returns>
	public static bool GraphFileCanBeRead(string path) {
		if (path != null && path != "") {
			return Graph.Load(path) != null;
		}

		return false;
	}

	/// <summary>
	/// Displays GUI options for a successful graph deserialization. 
	/// Allows the user to open the deserialized graph and edit it.
	/// </summary>
	/// <param name="settings">Associated terrain settings</param>
	public static void OptionGraphOpenSuccess(TerrainSettings settings) {
		string msg = "The node graph for this terrain is ready for use.";
		EditorGUILayout.HelpBox(msg, MessageType.Info);
		EditorGUILayout.LabelField("Selected File: " + Path.GetFileNameWithoutExtension(settings.SelectedFile));

		if (GUILayout.Button("Edit Node Graph")) {
			Open(settings.SelectedFile, settings);
		}

		OptionDefault(settings);
	}

	/// <summary>
	/// Displays GUI options for a failed graph deserialization. 
	/// Allows the user to select another file.
	/// </summary>
	/// <param name="settings">Associated terrain settings</param>
	public static void OptionGraphOpenError(TerrainSettings settings) {
		string msg = "The JSON file you selected failed to load. " +
					"Make sure the selected file is a valid graph file";
		EditorGUILayout.HelpBox(msg, MessageType.Error);

		OptionDefault(settings);
	}

	/// <summary>
	/// Displays GUI options for an incorrect file selection. 
	/// Allows the user to select another file.
	/// </summary>
	/// <param name="settings">Associated terrain settings</param>
	public static void OptionIncorrectFileSelection(TerrainSettings settings) {
		string msg = "There is no node graph associated with this terrain. " +
				"Either create a new graph or select an existing one from the file system.";
		EditorGUILayout.HelpBox(msg, MessageType.Warning);

		OptionDefault(settings);
	}

	/// <summary>
	/// Displays the default GUI options that appear at the bottom of 
	/// any option.
	/// </summary>
	/// <param name="settings">Associated terrain settings</param>
	private static void OptionDefault(TerrainSettings settings) {
		if (GUILayout.Button("New Node Graph")) {
			settings.SelectedFile = EditorUtility.SaveFilePanelInProject("Save Graph",
				"TerrainGraph", "json", "Choose a location to save the graph file.");

			if (settings.SelectedFile != "") {
				File.WriteAllText(settings.SelectedFile, "");
				OpenNew(settings.SelectedFile, settings);
			}
		}

		if (GUILayout.Button("Select Existing Graph")) {
			settings.SelectedFile = EditorUtility.OpenFilePanel("Select a JSON graph file", Application.dataPath, "json");
		}
	}

	private static void CreateGraphWindow(TerrainSettings settings) {
		EditorWindow.GetWindow<BonWindow>();
	}
}
