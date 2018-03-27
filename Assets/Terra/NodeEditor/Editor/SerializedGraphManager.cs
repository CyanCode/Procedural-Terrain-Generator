using System;
using Terra.GraphEditor;
using Terra.Terrain;
using UnityEngine;

namespace Assets.Terra.NodeEditor.Editor {
	/// <summary>
	/// Works the same as a normal GraphManager except 
	/// data from the stored Graph is serialized through 
	/// Unity rather than saving JSON files.
	/// </summary>
	public abstract class SerializedGraphManager: GraphManager {
		[SerializeField]
		public string graphJSON;

		public SerializedGraphManager(TerraSettings settings, Graph.GraphType graphType) : base(settings, graphType) { }

		public override abstract void OptionGraphOpenSuccess();
	}
}
