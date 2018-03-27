using Terra.CoherentNoise;
using System.IO;
using Terra.GraphEditor;
using Terra.Nodes;
using Terra.Terrain;
using UnityEditor;
using UnityEngine;

namespace Assets.Terra.NodeEditor.Editor {
	public class NoiseGraphManager: GraphManager {
		public Graph noiseGraph {
			get {
				return Settings.Launcher.GetGraphOfType(Graph.GraphType.Noise);
			}
		}

		public NoiseGraphManager(TerraSettings settings) : base(settings, Graph.GraphType.Noise) { }

		/// <summary>
		/// Returns whether or not the graph has an EndNode that connected to a generator
		/// </summary>
		/// <returns>true if found and connected, false otherwise</returns>
		public bool HasValidEndNode() {
			GraphLauncher launcher = Settings.Launcher;
			if (launcher != null && noiseGraph != null) {
				EndNode endNode = noiseGraph.GetNode<EndNode>();
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
				EndNode endNode = noiseGraph.GetNode<EndNode>();

				if (endNode != null)
					return endNode.GetFinalGenerator();
			}

			return null;
		}

		/// <summary>
		/// Checks whether file at path can be deserialized into a Graph
		/// </summary>
		/// <param name="path">Path to check</param>
		/// <returns>true if successful, false otherwise</returns>
		public bool GraphFileCanBeRead(string path) {
			if (path != null && path != "") {
				return Settings.Launcher.LoadGraph(path) != null;
			}

			return false;
		}
		
		/// <summary>
		/// Opens a new graph from the specified path
		/// </summary>
		/// <param name="path">file path to load from</param>
		public void OpenNew(string path) {
			GraphLauncher launcher = Settings.Launcher;
			Settings.LoadedNoiseGraph = launcher.LoadGraph(BonConfig.DefaultGraphName, GType);
			launcher.SaveGraph(Settings.LoadedNoiseGraph, path);
			CreateGraphWindow(Settings);
		}

		/// <summary>
		/// Displays GUI options for a successful graph deserialization. 
		/// Allows the user to open the deserialized graph and edit it.
		/// </summary>
		/// <param name="settings">Associated terrain Settings</param>
		public override void OptionGraphOpenSuccess() {
			string msg = "The node graph for this terrain is ready for use.";
			EditorGUILayout.HelpBox(msg, MessageType.Info);
			EditorGUILayout.LabelField("Selected File: " + Path.GetFileNameWithoutExtension(Settings.SelectedNoiseFile));

			if (GUILayout.Button("Edit Selected Graph")) {
				Open();
			}

			OptionDefault();

			EditorGUILayout.Space();

			Settings.Spread = EditorGUILayout.FloatField("Spread", Settings.Spread);
			Settings.Amplitude = EditorGUILayout.FloatField("Amplitude", Settings.Amplitude);

			EditorGUILayout.Space();
			if (Application.isEditor && Settings.DisplayPreview) {
				if (GUILayout.Button("Update Preview")) {
					Generator gen = GetGraphGenerator();

					if (gen != null) {
						//Settings.PreviewMesh = TerrainTile.GetPreviewMesh(Settings, gen);
						Settings.Preview.TriggerPreviewUpdate();
					}
				}
			}
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
	}
}