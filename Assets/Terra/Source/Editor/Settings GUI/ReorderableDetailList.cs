using System.Collections.Generic;
using Terra.Data;
using Terra.ReorderableList;
using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	public class ReorderableDetailList : GenericListAdaptor<BiomeData.DetailData> {
		private TerraSettings _settings;
		private Dictionary<int, Rect> _positions; //Cached positions at last repaint

		public ReorderableDetailList(TerraSettings settings) : base(settings.Details, null, 200) {
			_settings = settings;
			_positions = new Dictionary<int, Rect>();
		}

		public override void DrawItem(Rect position, int index) {
			//bool isRepaint = Event.current.type == EventType.Repaint;
			//if (isRepaint) {
			//	_positions[index] = position;
			//}

			////Resize area to fit within list gui
			//var areaPos = _positions.ContainsKey(index) ? _positions[index] : new Rect();
			//areaPos.x += 6;
			//areaPos.y += 8;
			//areaPos.width -= 6;

			//var detail = this[index];
			//if (detail == null)
			//	return;

			//GUILayout.BeginArea(areaPos);

			//string name = string.IsNullOrEmpty(biome.Name) ? "Biome " + (index + 1) : biome.Name;
			//biome.Name = EditorGUILayout.TextField("Name", name, GUILayout.ExpandWidth(true));

			//GUILayout.EndArea();
		}
	}
}
