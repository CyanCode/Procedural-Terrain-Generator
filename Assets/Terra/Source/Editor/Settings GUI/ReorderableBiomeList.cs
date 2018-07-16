using System.Collections.Generic;
using Terra.ReorderableList;
using Terra.Terrain;
using UnityEditor;
using UnityEngine;

public class ReorderableBiomeList: GenericListAdaptor<TerraSettings.BiomeData> {
	private const float MAX_HEIGHT = 200f;

	private TerraSettings _settings;
	private Dictionary<int, Rect> _positions; //Cached positions at last repaint

	public ReorderableBiomeList(TerraSettings settings) : base(settings.BiomesData, null, MAX_HEIGHT) {
		_settings = settings;
		_positions = new Dictionary<int, Rect>();
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

		GUILayout.BeginArea(areaPos);

		string name = string.IsNullOrEmpty(biome.Name) ? "Biome " + (index + 1) : biome.Name;
		biome.Name = EditorGUILayout.TextField("Name", name, GUILayout.ExpandWidth(true));

		if (biome.Color == default(Color))
			biome.Color = Random.ColorHSV();
		biome.Color = EditorGUILayout.ColorField("Preview Color", biome.Color);

		//Constraints
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Constraints", EditorGUIExtension.TerraStyle.TextBold);
		GUILayout.Space(-2);

		//angle
		biome.IsAngleConstrained = EditorGUILayout.Toggle("Angle", biome.IsAngleConstrained);
		if (biome.IsAngleConstrained) {
			EditorGUI.indentLevel++;
			biome.AngleConstraint = DrawConstraintRange("Min/Max", biome.AngleConstraint, 0f, 90f);
			EditorGUI.indentLevel--;
		}

		//height
		biome.IsHeightConstrained = EditorGUILayout.Toggle("Height", biome.IsHeightConstrained);
		if (biome.IsHeightConstrained) {
			EditorGUI.indentLevel++;
			biome.HeightConstraint = DrawConstraintRange("Min/Max", biome.HeightConstraint, 0f, 1f);
			EditorGUI.indentLevel--;
		}

		//temperature
		biome.IsTemperatureConstrained = EditorGUILayout.Toggle("Temperature", biome.IsTemperatureConstrained);
		if (biome.IsTemperatureConstrained) {
			EditorGUI.indentLevel++;
			biome.TemperatureConstraint = DrawConstraintRange("Min/Max", biome.TemperatureConstraint, 0f, 1f);
			EditorGUI.indentLevel--;
		}

		//moisture
		biome.IsMoistureConstrained = EditorGUILayout.Toggle("Moisture", biome.IsMoistureConstrained);
		if (biome.IsMoistureConstrained) {
			EditorGUI.indentLevel++;
			biome.MoistureConstraint = DrawConstraintRange("Min/Max", biome.MoistureConstraint, 0f, 1f);
			EditorGUI.indentLevel--;
		}

		GUILayout.EndArea();
	}
	
	public override float GetItemHeight(int index) {
		var biome = this[index];
		int controlCount = 0;

		//Space, Name, Color, Constraints label, Angle, Height, Temperature, Moisture
		controlCount += 10;

		if (biome.IsAngleConstrained) controlCount += 2;
		if (biome.IsHeightConstrained) controlCount += 2;
		if (biome.IsTemperatureConstrained) controlCount += 2;
		if (biome.IsMoistureConstrained) controlCount += 2;

		return EditorGUIUtility.singleLineHeight * controlCount;
	}

	private void DrawColorBox(TerraSettings.BiomeData biome) {
		if (biome.Color == default(Color)) {
			biome.Color = Random.ColorHSV();
		}

		var tex = new Texture2D(1, 1);
		tex.SetPixel(1, 1, biome.Color);
		tex.Apply();

		var style = new GUIStyle();
		style.normal.background = tex;
		style.margin.top = 2;

		GUILayout.Box("", style, GUILayout.MinWidth(15), GUILayout.MaxHeight(15));
	}

	private TerraSettings.BiomeData.Constraint DrawConstraint(string text, TerraSettings.BiomeData.Constraint constraint) {
		Vector2 result = new Vector2(constraint.Min, constraint.Max);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Min/Max", GUILayout.MaxWidth(EditorGUIUtility.labelWidth), GUILayout.MinWidth(60));
		
		EditorGUILayout.BeginVertical();
		result.x = EditorGUILayout.FloatField(result.x, GUILayout.ExpandWidth(true));
		result.y = EditorGUILayout.FloatField(result.y, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		return new TerraSettings.BiomeData.Constraint { Min = result.x, Max = result.y };
	}

	/// <summary>
	/// Draws the UI for a constraint using a range slider between the 
	/// passed min and max values
	/// </summary>
	private TerraSettings.BiomeData.Constraint DrawConstraintRange(string text, TerraSettings.BiomeData.Constraint constraint, float min, float max) {
		float minConst = constraint.Min;
		float maxConst = constraint.Max;
		EditorGUILayout.MinMaxSlider(text, ref minConst, ref maxConst, min, max, GUILayout.ExpandWidth(true));
		
		GUIStyle style = new GUIStyle {alignment = TextAnchor.MiddleRight};
		EditorGUILayout.LabelField("[" + constraint.Min.ToString("F1") + "," + constraint.Max.ToString("F1") + "]", style);

		return new TerraSettings.BiomeData.Constraint(minConst, maxConst);
	}
}