
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    /// <summary>
    /// Represents a graph of nodes.
    /// </summary>
    public class NodeGraph : ScriptableObject
    {
        /// <summary>
        /// The types of nodes accepted by the graph.
        /// </summary>
        public static HashSet<Type> nodeTypes = new HashSet<Type>();

        [HideInInspector]
        public List<Node> nodes = new List<Node>();

        /// <summary>
        /// Add a node to the graph.
        /// It is recommended that the save manager adds the nodes.
        /// </summary>
        /// <param name="n"></param>
        public void Add(Node n)
        {
            nodes.Add(n);
			NodeGraphEvent.TriggerOnAddedNode(this, n);
        }

        /// <summary>
        /// Removes a node from the graph but it is not destroyed.
        /// </summary>
        /// <param name="node"></param>
        public void Remove(Node node)
        {
            nodes.Remove(node);
			NodeGraphEvent.TriggerOnNodeRemoved(this, node);
        }

        /// <summary>
        /// Put the node at the end of the node list.
        /// </summary>
        /// <param name="node"></param>
        public void PushToEnd(Node node)
        {
            if (nodes.Remove(node)) {
                nodes.Add(node);
            }
        }

        /// <summary>
        /// Gets called right before the graph is saved.
        /// Can be used to setup things before saving like sorting nodes.
        /// </summary>
        public virtual void OnSave() { }
    }

	/// <summary>
	/// Contains events that can be assigned as delegates 
	/// for responding to nodes being added and removed.
	/// </summary>
	public static class NodeGraphEvent {
		public delegate void NodeAction(NodeGraph graph, Node node);

		public static event NodeAction OnAddedNode;
		public static event NodeAction OnNodeRemoved;

		public static void TriggerOnAddedNode(NodeGraph graph, Node node) {
			if (OnAddedNode != null) OnAddedNode(graph, node);
		}

		public static void TriggerOnNodeRemoved(NodeGraph graph, Node node) {
			if (OnNodeRemoved != null) OnNodeRemoved(graph, node);
		}
	}
}