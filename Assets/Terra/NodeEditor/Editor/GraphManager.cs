using Terra.CoherentNoise;
using System.IO;
using Terra.GraphEditor;
using Terra.Nodes;
using Terra.Terrain;
using UnityEditor;
using UnityEngine;

namespace Assets.Terra.NodeEditor.Editor {
	public abstract class GraphManager {
		protected TerraSettings Settings;
		protected BonWindow ActiveWindow;
		protected Graph.GraphType GType;

		public GraphManager(TerraSettings settings, Graph.GraphType graphType) {
			Settings = settings;
			GType = graphType;

			//Register event handler
			EventManager.OnCloseGraph += OnClose;
		}

		/// <summary>
		/// Handler for OnClose graph delegate action. Used for saving.
		/// </summary>
		/// <param name="graph">Graph that will close</param>
		public void OnClose(Graph graph) {
			if (graph.Path == null || graph.Path == "") {
				//Open save dialog. If there's no active BonWindow, we
				//have no idea what this user is doing, ignore.
				if (ActiveWindow != null)
					ActiveWindow.OpenSaveDialog();
			} else {
				Settings.Launcher.SaveGraph(graph, graph.Path);
			}
		}

		/// <summary>
		/// Opens a graph at the path specified in settings
		/// </summary>
		/// <param name="settings">Settings to persist created Graph instance</param>
		public void Open() {
			Settings.LoadedNoiseGraph = Settings.Launcher.LoadGraph(Settings.SelectedNoiseFile, GType);
			CreateGraphWindow(Settings);
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
		/// Displays GUI options for a successful graph deserialization. 
		/// Allows the user to open the deserialized graph and edit it.
		/// </summary>
		/// <param name="settings">Associated terrain Settings</param>
		public abstract void OptionGraphOpenSuccess();

		/// <summary>
		/// Displays the default GUI options that appear at the bottom of 
		/// any option.
		/// </summary>
		/// <param name="settings">Associated terrain Settings</param>
		protected void OptionDefault() {
			if (GUILayout.Button("Create New Graph")) {
				Settings.SelectedNoiseFile = EditorUtility.SaveFilePanelInProject("Save Graph",
					"TerrainGraph", "json", "Choose a location to save the graph file.");

				if (Settings.SelectedNoiseFile != "") {
					File.WriteAllText(Settings.SelectedNoiseFile, "");
					OpenNew(Settings.SelectedNoiseFile);
				}
			}

			if (GUILayout.Button("Select Existing Graph")) {
				Settings.SelectedNoiseFile = EditorUtility.OpenFilePanel("Select a JSON graph file", Application.dataPath, "json");
				Open();
			}
		}

		protected void CreateGraphWindow(TerraSettings Settings) {
			ActiveWindow = BonWindow.Init(GType);
			ActiveWindow.CreateCanvas(Settings.SelectedNoiseFile, GType);
		}
	}
}