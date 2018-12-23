using System.Collections.Generic;
using Terra.Structure;
using Terra.ReorderableList;
using UnityEditor;
using UnityEditor.Terra;
using UnityEngine;
using Terra;

public class ReorderableBiomeList: GenericListAdaptor<BiomeData> {
	private const float MAX_HEIGHT = 200f;

	private List<KeyValuePair<BiomeData, ReorderableMaterialList>> _materialList;
	private List<KeyValuePair<BiomeData, ReorderableObjectList>> _objectList;

	private TerraConfig _config;
	private Dictionary<int, Rect> _positions; //Cached positions at last repaint

	public ReorderableBiomeList(TerraConfig config) : base(config.BiomesData, null, MAX_HEIGHT) {
		_config = config;

		_positions = new Dictionary<int, Rect>();
		_materialList = new List<KeyValuePair<BiomeData, ReorderableMaterialList>>();
		_objectList = new List<KeyValuePair<BiomeData, ReorderableObjectList>>();
	}

	/// <summary>
	/// Gets the material list associated with the passed biome
	/// </summary>
	/// <returns>Material list if found, null otherwise</returns>
	public ReorderableMaterialList GetMaterialList(BiomeData biome) {
		foreach (var kv in _materialList) {
			if (kv.Key == biome)
				return kv.Value;
		}

		return null;
	}

	/// <summary>
	/// Gets the object list associated with the passed biome
	/// </summary>
	/// <returns>Object list if found, null otherwise</returns>
	public ReorderableObjectList GetObjectList(BiomeData biome) {
		foreach (var kv in _objectList) {
			if (kv.Key == biome)
				return kv.Value;
		}

		return null;
	}

	public override void DrawItem(Rect position, int index) {
		bool isRepaint = Event.current.type == EventType.Repaint;
		if (isRepaint) {
			_positions[index] = position;
		}

		//Resize area to fit within list gui
		var areaPos = _positions.ContainsKey(index) ? _positions[index] : new Rect();
		areaPos.x += 6;
		areaPos.y += 8;
		areaPos.width -= 6;

		//Cache biome instance
		var biome = this[index];
		if (biome == null) 
			return;

		//Init sublists for biome if they don't exist yet
		if (GetMaterialList(biome) == null)
			_materialList.Add(new KeyValuePair<BiomeData, ReorderableMaterialList>(biome, new ReorderableMaterialList(_config, biome.Details)));
		if (GetObjectList(biome) == null)
			_objectList.Add(new KeyValuePair<BiomeData, ReorderableObjectList>(biome, new ReorderableObjectList(_config, biome.Details)));

		GUILayout.BeginArea(areaPos);

		string name = string.IsNullOrEmpty(biome.Name) ? "Biome " + (index + 1) : biome.Name;
		biome.Name = EditorGUILayout.TextField("Name", name, GUILayout.ExpandWidth(true));

		if (biome.Color == default(Color))
			biome.Color = Random.ColorHSV();
		biome.Color = EditorGUILayout.ColorField(new GUIContent("Preview Color"), biome.Color, false, false, false, new ColorPickerHDRConfig(0, 1, 0, 1));

		//Constraints
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Constraints", EditorGUIExtension.TerraStyle.TextBold);
		GUILayout.Space(-2);

		biome.MixMethod = (ConstraintMixMethod)EditorGUIExtension.EnumToolbar(biome.MixMethod);

		//height
		biome.IsHeightConstrained = EditorGUILayout.Toggle("Height", biome.IsHeightConstrained);
		if (biome.IsHeightConstrained) {
			EditorGUI.indentLevel++;
			biome.HeightConstraint = EditorGUIExtension.DrawConstraintRange("Min/Max", biome.HeightConstraint, 0f, 1f);
			EditorGUI.indentLevel--;
		}

		//temperature
		biome.IsTemperatureConstrained = EditorGUILayout.Toggle("Temperature", biome.IsTemperatureConstrained);
		if (biome.IsTemperatureConstrained) {
			EditorGUI.indentLevel++;
			biome.TemperatureConstraint = EditorGUIExtension.DrawConstraintRange("Min/Max", biome.TemperatureConstraint, 0f, 1f);
			EditorGUI.indentLevel--;
		}

		//moisture
		biome.IsMoistureConstrained = EditorGUILayout.Toggle("Moisture", biome.IsMoistureConstrained);
		if (biome.IsMoistureConstrained) {
			EditorGUI.indentLevel++;
			biome.MoistureConstraint = EditorGUIExtension.DrawConstraintRange("Min/Max", biome.MoistureConstraint, 0f, 1f);
			EditorGUI.indentLevel--;
		}

		GUILayout.EndArea();
	}
	
	public override float GetItemHeight(int index) {
		var biome = this[index];
		int controlCount = 0;

		//Space, Name, Color, Constraints label, Height, Temperature, Moisture
		controlCount += 10;

		if (biome.IsHeightConstrained) controlCount += 2;
		if (biome.IsTemperatureConstrained) controlCount += 2;
		if (biome.IsMoistureConstrained) controlCount += 2;

		return EditorGUIUtility.singleLineHeight * controlCount;
	}

	public override void Remove(int index) {
		//Remove from material and object lists first
		if (GetMaterialList(this[index]) != null)
			_materialList.RemoveAt(index);
		if (GetObjectList(this[index]) != null)
			_objectList.RemoveAt(index);

		base.Remove(index);
	}
}