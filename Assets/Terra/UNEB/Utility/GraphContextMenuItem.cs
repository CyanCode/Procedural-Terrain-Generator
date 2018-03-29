using System;
using UnityEditor;

namespace Assets.Terra.UNEB.Utility {

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public sealed class GraphContextMenuItem: Attribute {
		private string _menuPath;
		public string Path { get { return _menuPath; } }

		private string _itemName;
		public string Name { get { return _itemName; } }

		public GraphContextMenuItem(string menuPath) : this(menuPath, null) { }

		public GraphContextMenuItem(string menuPath, string itemName) {
			_menuPath = menuPath;
			_itemName = itemName;
		}

		/// <summary> Gets the menu entry name of this node </summary>
		public static string GetNodeName(Type nodeType) {
			object[] attrs = nodeType.GetCustomAttributes(typeof(GraphContextMenuItem), true);
			if (attrs.Length == 0) {
				return ObjectNames.NicifyVariableName(nodeType.Name);
			}

			GraphContextMenuItem attr = (GraphContextMenuItem)attrs[0];
			return string.IsNullOrEmpty(attr.Name) ? nodeType.Name : attr.Name;
		}

		/// <summary> Gets the menu entry path of this node </summary>
		public static string GetNodePath(Type nodeType) {
			object[] attrs = nodeType.GetCustomAttributes(typeof(GraphContextMenuItem), true);
			if (attrs.Length == 0) {
				return null;
			}

			GraphContextMenuItem attr = (GraphContextMenuItem)attrs[0];
			return string.IsNullOrEmpty(attr.Path) ? null : attr.Path;
		}

		public static string GetItemMenuName(Type type) {
			string path = GetNodePath(type);
			if (path != null) return path + "/" + GetNodeName(type);
			return GetNodeName(type);
		}
	}
}
