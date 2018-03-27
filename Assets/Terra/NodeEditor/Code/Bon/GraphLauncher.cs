using UnityEngine;
using Terra.CoherentNoise;
using Terra.Nodes.Generation;
using Terra.Nodes;
using Terra.GraphEditor.Sockets;
using System.Collections.Generic;
using System.IO;
using System;

namespace Terra.GraphEditor {
	/// <summary>
	/// A class to controll the creation of Graphs. It contains loaded Grpahs.
	/// (A gameobject with this script is created by the editor if it is not in the scene)
	/// </summary>
	[ExecuteInEditMode]
	[System.Serializable]
	public class GraphLauncher {
		private StandardGraphController Controller;
		public List<Graph> Graphs;

		public GraphLauncher() {
			Graphs = new List<Graph>();

			if (Controller == null)
				Controller = new StandardGraphController();
			Controller.Register();
		}

		/// <summary>
		/// Loads a graph by its path, adds it to the internal list
		/// and returns it.
		/// (Also used by the editor to open Graphs)
		/// </summary>
		/// <returns>Graph if it was loaded successfully, null otherwise.</returns>
		public Graph LoadGraph(string path, Graph.GraphType graphType) {
			Graph graph = null;

			if (File.Exists(path) && path != BonConfig.DefaultGraphName) {
				StreamReader sr = File.OpenText(path);
				string json = sr.ReadToEnd();
				sr.Close();

				graph = Graph.FromJson(json);
				if (graph.Version != BonConfig.Version) {
					Debug.LogWarning("You loading a graph with a different version number: " + graph.Version +
						" the current version is " + BonConfig.Version);
				}
			} else if (path == BonConfig.DefaultGraphName) {
				graph = CreateDefaultGraph(graphType);
			} else { 
				path = path == "" ? "[No File Selected]" : path;
				Debug.LogWarning("Could not open the graph file: '" + path + "'. Make sure the noise tab has a selected graph file.");
			}

			if (graph != null) {
				graph.Path = path;
				Graphs.Add(graph);

				graph.UpdateNodes();
			}

			return graph;
		}

		/// <summary>
		/// Returns the graph in the currently active list 
		/// of graphs that matches the passed graph type
		/// </summary>
		/// <param name="gtype">graph type to check for</param>
		public Graph GetGraphOfType(Graph.GraphType gtype) {
			foreach (Graph g in Graphs) {
				if (g.GType == gtype) {
					return g;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the generator associated with the EndNode in
		/// the passed graph.
		/// </summary>
		/// <returns>The end generator if it exists</returns>
		public Generator GetEndNode(Graph g) {
			EndNode endNode = g.GetNode<EndNode>();

			return endNode.GetFinalGenerator();
		}

		/// <summary>
		/// Saves a graph by its passed filepath
		/// (Also used by the editor to save Graphs)
		/// </summary>
		/// <param name="g">Graph to save</param>
		/// <param name="path">File path to save to</param>
		/// <returns>True if graph saved successfully</returns>
		public bool SaveGraph(Graph g, string path) {
			try {
				var file = File.CreateText(path);
				file.Write(g.ToJson());
				file.Close();
			} catch (Exception e) {
				Debug.LogException(e);
				return false;
			}			

			return true;
		}

		/// <summary>
		/// Loads the passed json file as a graph and 
		/// adds it to the list of active graphs.
		/// </summary>
		/// <param name="fileName">File path to load</param>
		/// <returns></returns>
		public Graph LoadGraph(string fileName) {
			if (File.Exists(fileName)) {
				var file = File.OpenText(fileName);
				var json = file.ReadToEnd();
				file.Close();

				Graph deserializedGraph = Graph.FromJson(json);
				if (deserializedGraph.Version != BonConfig.Version) {
					Debug.LogWarning("You loading a graph with a different version number: " + deserializedGraph.Version +
						" the current version is " + BonConfig.Version);
				}

				return deserializedGraph;
			} else {
				fileName = fileName == "" ? "[No File Selected]" : fileName;
				Debug.LogWarning("Could not open the graph file: '" + fileName + "'. Make sure the noise tab has a selected graph file.");

				return null;
			}
		}

		/// <summary>
		/// Creates a default Graph and adds it to the 
		/// active list of graphs
		/// (see: BonConfig.DefaultGraphName)
		/// </summary>
		public Graph CreateDefaultGraph(Graph.GraphType graphType) {
			switch (graphType) {
				case Graph.GraphType.Noise:
					Graph graph = new Graph(graphType);

					//Pink Noise
					Node pink = graph.CreateNode<PinkNoiseNode>();
					pink.X = 100;
					pink.Y = 100;
					graph.AddNode(pink);

					//Preview Noise
					Node preview = graph.CreateNode<NoisePreviewNode>();
					preview.X = 300;
					preview.Y = 100;
					graph.AddNode(preview);

					graph.Link((InputSocket)preview.GetSocket(typeof(AbstractGeneratorNode), typeof(InputSocket), 0),
						(OutputSocket)pink.GetSocket(typeof(AbstractGeneratorNode), typeof(OutputSocket), 0));

					//End Node
					Node end = graph.CreateNode<EndNode>();
					end.X = 300;
					end.Y = 300;
					graph.AddNode(end);

					graph.Link((InputSocket)end.GetSocket(typeof(AbstractGeneratorNode), typeof(InputSocket), 0),
						(OutputSocket)pink.GetSocket(typeof(AbstractGeneratorNode), typeof(OutputSocket), 0));

					Graphs.Add(graph);
					return graph;
				default:
					return null; //TODO implement material and objectplacement types
			}			
		}
	}
}