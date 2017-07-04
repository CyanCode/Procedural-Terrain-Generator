using System.Collections.Generic;
using UnityEngine;
using Assets.Code.Bon.Socket;
using Assets.Code.Bon;

/// <summary>
/// A class to controll the creation of Graphs. It contains loaded Grpahs.
/// (A gameobject with this script is created by the editor if it is not in the scene)
/// </summary>
[ExecuteInEditMode]
[System.Serializable]
public class BonLauncher: MonoBehaviour {
	private StandardGraphController _controller;
	public Graph Graph;

	/// <summary>
	/// Loads a graph by its path, adds it to the internal list
	/// and returns it.
	/// (Also used by the editor to open Graphs)
	/// </summary>
	public Graph LoadGraph(string path) {
		Graph g;
		if (path.Equals(BonConfig.DefaultGraphName)) g = CreateDefaultGraph();
		else g = Graph.Load(path);
		g.Name = path;
		Graph = g;

		CreateGraphController(g);
		g.UpdateNodes();
		return g;
	}

	/// <summary>
	/// Saves a graph by its path.
	/// (Also used by the editor to save Graphs)
	/// </summary>
	public void SaveGraph(Graph g, string path) {
		Graph.Save(path, g);
	}

	/// <summary>
	/// Saves a graph by its path.
	/// (Also used by the editor to save Graphs)
	/// </summary>
	public void SaveGraph(string path) {
		Graph.Save(path, Graph);
	}

	/// <summary>
	/// Create a controller for the assigned Graph.
	/// </summary>
	private void CreateGraphController(Graph graph) {
		// in this case we create one controller for all graphs
		// you could also create different controllers for different graphs
		//if (_controller == null) _controller = new StandardGraphController();
	}

	public void OnEnable() {
		if (_controller == null) _controller = new StandardGraphController();
		_controller.Register();

		Graph.ResetVisitCount();
	}

	/// <summary>
	/// Creates a default Graph.
	/// (see: BonConfig.DefaultGraphName)
	/// </summary>
	public Graph CreateDefaultGraph() {
		Graph graph = new Graph();

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

		// == test serialization an deserialization ==
		var serializedJSON = graph.ToJson();
		var deserializedGraph = Graph.FromJson(serializedJSON);
		// =====

		return deserializedGraph;
	}
}
