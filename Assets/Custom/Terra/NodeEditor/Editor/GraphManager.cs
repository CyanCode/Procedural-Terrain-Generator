using Terra.CoherentNoise;
using System.IO;
using Terra.GraphEditor;
using Terra.Nodes;
using Terra.Terrain;
using UnityEditor;
using UnityEngine;

public class GraphManager {
	private TerraSettings Settings;

	public GraphManager(TerraSettings settings) {
		Settings = settings;
	}

	/// <summary>
	/// Opens a graph at the path specified in settings
	/// </summary>
	/// <param name="settings">Settings to persist created Graph instance</param>
	public void Open() {
		Settings.LoadedGraph = Settings.Launcher.LoadGraph(Settings.SelectedFile);
		CreateGraphWindow(Settings);
	}

	public void OpenNew(string path) {
		GraphLauncher launcher = Settings.Launcher;
		Settings.LoadedGraph = launcher.LoadGraph(BonConfig.DefaultGraphName);
		launcher.SaveGraph(Settings.LoadedGraph, path);
		CreateGraphWindow(Settings);
	}

	/// <summary>
	/// Checks whether file at path can be deserialized into a Graph
	/// </summary>
	/// <param name="path">Path to check</param>
	/// <returns>true if successful, false otherwise</returns>
	public bool GraphFileCanBeRead(string path) {
		if (path != null && path != "") {
			return Graph.Load(path) != null;
		}

		return false;
	}

	/// <summary>
	/// Displays GUI options for a successful graph deserialization. 
	/// Allows the user to open the deserialized graph and edit it.
	/// </summary>
	/// <param name="settings">Associated terrain Settings</param>
	public void OptionGraphOpenSuccess() {
		string msg = "The node graph for this terrain is ready for use.";
		EditorGUILayout.HelpBox(msg, MessageType.Info);
		EditorGUILayout.LabelField("Selected File: " + Path.GetFileNameWithoutExtension(Settings.SelectedFile));

		if (GUILayout.Button("Edit Selected Graph")) {
			Open();
		}

		OptionDefault();

		EditorGUILayout.Space();

		Settings.Spread = EditorGUILayout.FloatField("Spread", Settings.Spread);
		Settings.Amplitude = EditorGUILayout.FloatField("Amplitude", Settings.Amplitude);

		if (Application.isEditor) {
			if (GUILayout.Button("Update Preview")) {
				Generator gen = GetGraphGenerator();

				if (gen != null) {
					Settings.PreviewMesh = TerrainTile.GetPreviewMesh(Settings, gen);
				}
			}

			Settings.IsWireframePreview = GUILayout.Toggle(Settings.IsWireframePreview, "Wireframe Preview");
		}
	}

	/// <summary>
	/// Displays GUI options for a failed graph deserialization. 
	/// Allows the user to select another file.
	/// </summary>
	/// <param name="settings">Associated terrain Settings</param>
	public void OptionGraphOpenError() {
		string msg = "The JSON file you selected failed to load. " +
					"Make sure the selected file is a valid graph file";
		EditorGUILayout.HelpBox(msg, MessageType.Error);

		OptionDefault();
	}

	/// <summary>
	/// Displays GUI options for an incorrect file selection. 
	/// Allows the user to select another file.
	/// </summary>
	/// <param name="settings">Associated terrain Settings</param>
	public void OptionIncorrectFileSelection() {
		string msg = "There is no node graph associated with this terrain. " +
				"Either create a new graph or select an existing one from the file system.";
		EditorGUILayout.HelpBox(msg, MessageType.Warning);

		OptionDefault();
	}

	/// <summary>
	/// Returns whether or not the graph has an EndNode that connected to a generator
	/// </summary>
	/// <returns>true if found and connected, false otherwise</returns>
	public bool HasValidEndNode() {
		GraphLauncher launcher = Settings.Launcher;
		if (launcher != null && launcher.Graph != null) {
			EndNode endNode = launcher.Graph.GetNode<EndNode>();
			return endNode != null && endNode.GetFinalGenerator() != null;
		}

		return false;
	}

	/// <summary>
	/// Gets the generator associated with the EndNode in the Graph
	/// </summary>
	/// <returns>The end generator if it exists</returns>
	public Generator GetGraphGenerator() {
		if (HasValidEndNode()) {
			GraphLauncher launcher = Settings.Launcher;
			EndNode endNode = launcher.Graph.GetNode<EndNode>();

			return endNode.GetFinalGenerator();
		}

		return null;
	}

	/// <summary>
	/// Displays an alert when no EndNode is found in the graph
	/// </summary>
	public void MessageNoEndNode() {
		EditorGUILayout.HelpBox("A node graph exists but cannot be used for generation as there is no connected End node.", MessageType.Warning);
		if (GUILayout.Button("Edit Selected Graph")) {
			Open();
		}

		OptionDefault();
	}

	/// <summary>
	/// Displays the default GUI options that appear at the bottom of 
	/// any option.
	/// </summary>
	/// <param name="settings">Associated terrain Settings</param>
	private void OptionDefault() {
		if (GUILayout.Button("Create New Graph")) {
			Settings.SelectedFile = EditorUtility.SaveFilePanelInProject("Save Graph",
				"TerrainGraph", "json", "Choose a location to save the graph file.");

			if (Settings.SelectedFile != "") {
				File.WriteAllText(Settings.SelectedFile, "");
				OpenNew(Settings.SelectedFile);
			}
		}

		if (GUILayout.Button("Select Existing Graph")) {
			Settings.SelectedFile = EditorUtility.OpenFilePanel("Select a JSON graph file", Application.dataPath, "json");
			Open();
		}
	}

	private void CreateGraphWindow(TerraSettings Settings) {
		BonWindow window = EditorWindow.GetWindow<BonWindow>();
		window.CreateCanvas(Settings.SelectedFile);
	}
}