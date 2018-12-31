using UnityEngine;
using UnityEditor;
using System;
using Terra.Structures;
using Terra.ReorderableList;
using Terra.ReorderableList.Internal;
using Terra.Terrain;

public static class EditorGUIExtension {
	public static GUIStyle StyleButtonToggled {
		get {
			BuildStyle();
			return _toggledStyle;
		}
	}

	public static GUIStyle StyleLabelText {
		get {
			BuildStyle();
			return _labelTextStyle;
		}
	}

	/// <summary>
	/// Creates a min/max element that doesn't allow the min 
	/// to be greater than the max OR the max to be smaller than 
	/// the min.
	/// </summary>
	/// <param name="name">Name of control</param>
	/// <param name="min">min value</param>
	/// <param name="max">max value</param>
	/// <returns>Created Vector2</returns>
	public static Vector2 MinMaxVector2(string name, float min, float max) {
		//Clamp
		min = min > max ? max : min;
		max = max < min ? min : max;

		Vector2 v = new Vector2(min, max);
		EditorGUILayout.Vector2Field(name, v);

		return v;
	}

	/// <summary>
	/// Creates an array foldout like in inspectors for SerializedProperty of array type.
	/// Counterpart for standard EditorGUILayout.PropertyField which doesn't support SerializedProperty of array type.
	/// </summary>
	public static void ArrayField(SerializedProperty property) {
		//EditorGUIUtility.LookLikeInspector();
		bool wasEnabled = GUI.enabled;
		int prevIdentLevel = EditorGUI.indentLevel;

		// Iterate over all child properties of array
		bool childrenAreExpanded = true;
		int propertyStartingDepth = property.depth;
		while (property.NextVisible(childrenAreExpanded) && propertyStartingDepth < property.depth) {
			childrenAreExpanded = EditorGUILayout.PropertyField(property);
		}

		EditorGUI.indentLevel = prevIdentLevel;
		GUI.enabled = wasEnabled;
	}

	/// <summary>
	/// Creates a filepath textfield with a browse button. Opens the open file panel.
	/// </summary>
	public static string FileLabel(string name, float labelWidth, string path, string extension) {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label(name, GUILayout.MaxWidth(labelWidth));
		string filepath = EditorGUILayout.TextField(path);
		if (GUILayout.Button("Browse")) {
			filepath = EditorUtility.OpenFilePanel(name, path, extension);
		}
		EditorGUILayout.EndHorizontal();
		return filepath;
	}

	/// <summary>
	/// Creates a folder path textfield with a browse button. Opens the save folder panel.
	/// </summary>
	public static string FolderLabel(string name, float labelWidth, string path) {
		EditorGUILayout.BeginHorizontal();
		string filepath = EditorGUILayout.TextField(name, path);
		if (GUILayout.Button("Browse", GUILayout.MaxWidth(60))) {
			filepath = EditorUtility.SaveFolderPanel(name, path, "Folder");
		}
		EditorGUILayout.EndHorizontal();
		return filepath;
	}

	/// <summary>
	/// Creates an array foldout like in inspectors. Hand editable ftw!
	/// </summary>
	public static string[] ArrayFoldout(string label, string[] array, ref bool foldout) {
		EditorGUILayout.BeginVertical();
		//EditorGUIUtility.LookLikeInspector();
		foldout = EditorGUILayout.Foldout(foldout, label);
		string[] newArray = array;
		if (foldout) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
			int arraySize = EditorGUILayout.IntField("Size", array.Length);
			if (arraySize != array.Length)
				newArray = new string[arraySize];
			for (int i = 0; i < arraySize; i++) {
				string entry = "";
				if (i < array.Length)
					entry = array[i];
				newArray[i] = EditorGUILayout.TextField("Element " + i, entry);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		return newArray;
	}

	/// <summary>
	/// Creates a toolbar that is filled in from an Enum. Useful for setting tool modes.
	/// </summary>
	public static Enum EnumToolbar(Enum selected) {
		string[] toolbar = Enum.GetNames(selected.GetType());
		Array values = Enum.GetValues(selected.GetType());

		for (int i = 0; i < toolbar.Length; i++) {
			string toolname = toolbar[i];
			toolname = toolname.Replace("_", " ");
			toolbar[i] = toolname;
		}

		int selected_index = 0;
		while (selected_index < values.Length) {
			if (selected.ToString() == values.GetValue(selected_index).ToString()) {
				break;
			}
			selected_index++;
		}
		selected_index = GUILayout.Toolbar(selected_index, toolbar);
		return (Enum)values.GetValue(selected_index);
	}

	/// <summary>
	/// Creates a toolbar that is filled in from an Enum. Useful for setting tool modes.
	/// Toolbar displays images rather than text.
	/// </summary>
	public static Enum EnumToolbar(Enum selected, Texture[] images) {
		Array values = Enum.GetValues(selected.GetType());

		int selected_index = 0;
		while (selected_index < values.Length) {
			if (selected.ToString() == values.GetValue(selected_index).ToString()) {
				break;
			}

			selected_index++;
		}

		selected_index = GUILayout.Toolbar(selected_index, images, GUILayout.Height(30));
		return (Enum)values.GetValue(selected_index);
	}

	/// <summary>
	/// Creates a button that can be toggled. Looks nice than GUI.toggle
	/// </summary>
	/// <returns>
	/// Toggle state
	/// </returns>
	/// <param name='state'>
	/// If set to <c>true</c> state.
	/// </param>
	/// <param name='label'>
	/// If set to <c>true</c> label.
	/// </param>
	public static bool ToggleButton(bool state, string label) {
		BuildStyle();

		bool out_bool = false;

		if (state)
			out_bool = GUILayout.Button(label, _toggledStyle);
		else
			out_bool = GUILayout.Button(label);

		if (out_bool)
			return !state;
		
		return state;
	}

	private static readonly Color BlockAreaColor = new Color(0.83f, 0.83f, 0.83f);

	public static void BeginBlockArea() {
		Texture2D bg = new Texture2D(1, 1);
		bg.SetPixel(1, 1, BlockAreaColor);

		GUIStyle style = ReorderableListStyles.Container;
		style.margin = new RectOffset(5, 5, 0, 0);
		style.padding = new RectOffset(8, 8, 2, 2);

		EditorGUILayout.BeginVertical(style);
		EditorGUILayout.Space();
	}

	public static void AddBlockAreaSeperator() {
		EditorGUILayout.Space();
		GUIHelper.Separator(EditorGUILayout.GetControlRect(false, 1f), ReorderableListStyles.HorizontalLineColor);
		EditorGUILayout.Space();
	}

	public static void EndBlockArea() {
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

	/// <summary>
	/// Draws the UI for a constraint using a range slider between the 
	/// passed min and max values
	/// </summary>
	public static Constraint DrawConstraintRange(string text, Constraint constraint, float min, float max) {
		float minConst = constraint.Min;
		float maxConst = constraint.Max;
		EditorGUILayout.MinMaxSlider(text, ref minConst, ref maxConst, min, max, GUILayout.ExpandWidth(true));

		GUIStyle style = new GUIStyle { alignment = TextAnchor.MiddleRight };
		EditorGUILayout.LabelField("[" + constraint.Min.ToString("F1") + "," + constraint.Max.ToString("F1") + "]", style);

		return new Constraint(minConst, maxConst);
	}

	/// <summary>
	/// Draws the UI for a constraint using a range slider between the 
	/// passed min and max values
	/// </summary>
	public static Constraint DrawConstraintRange(Rect pos, string text, Constraint constraint, float min, float max) {
		float minConst = constraint.Min;
		float maxConst = constraint.Max;
		EditorGUI.MinMaxSlider(pos, text, ref minConst, ref maxConst, min, max);

		EditorGUI.indentLevel++; 
		Rect indented = EditorGUI.IndentedRect(pos);
		indented.y += EditorGUIUtility.singleLineHeight;
		EditorGUI.indentLevel--;

		GUIStyle style = new GUIStyle { alignment = TextAnchor.MiddleRight };
		EditorGUI.LabelField(indented, "[" + constraint.Min.ToString("F1") + "," + constraint.Max.ToString("F1") + "]", style);

		return new Constraint(minConst, maxConst);
	}

	public static Vector3 StackedVector3(string name, Vector3 vec) {
		float lineHeight = EditorGUIUtility.singleLineHeight;
		float ctrlHeight = lineHeight * 3;
		Rect ctrl = EditorGUILayout.GetControlRect(false, ctrlHeight);

		//Label
		Rect labelCtrl = ctrl;
		labelCtrl.width = EditorGUIUtility.labelWidth;
		EditorGUI.LabelField(labelCtrl, new GUIContent(name));

		//Axis Label
		const int axisLabelWidth = 15;
		Rect axisCtrl = ctrl;
		axisCtrl.x += EditorGUIUtility.labelWidth;
		axisCtrl.width = axisLabelWidth;

		EditorGUI.LabelField(axisCtrl, new GUIContent("X"));

		//X Axis
		Rect fieldCtrl = ctrl;
		fieldCtrl.x += EditorGUIUtility.labelWidth + axisLabelWidth;
		fieldCtrl.width = EditorGUIUtility.fieldWidth - axisLabelWidth;

		vec.x = EditorGUI.FloatField(fieldCtrl, GUIContent.none, vec.x);

		return vec;
	}

	public static Texture2D BackgroundColor(Color c) {
		Texture2D tex = new Texture2D(1, 1);
		tex.SetPixel(1, 1, c);

		return tex;
	}

	public struct TerraStyle {
		public const int TITLE_FONT_SIZE = 12;
		public const int NODE_LABEL_WIDTH = 120;

		public static GUIStyle TextBold = new GUIStyle { fontStyle = FontStyle.Bold };
		public static GUIStyle TextTitle = new GUIStyle { fontStyle = FontStyle.Bold, fontSize = TITLE_FONT_SIZE };
	}

	public class ModalPopupWindow: EditorWindow {
		public event Action<bool> OnChosen;
		string popText = "";
		string trueText = "Yes";
		string falseText = "No";

		public void SetValue(string text, string accept, string no) {
			this.popText = text;
			this.trueText = accept;
			this.falseText = no;
		}

		void OnGUI() {
			GUILayout.BeginVertical();
			GUILayout.Label(popText);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(trueText)) {
				if (OnChosen != null)
					OnChosen(true);
				this.Close();
			}
			if (GUILayout.Button(falseText)) {
				if (OnChosen != null)
					OnChosen(false);
				this.Close();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}

	static GUIStyle _toggledStyle;

	static GUIStyle _labelTextStyle;

	private static void BuildStyle() {
		if (_toggledStyle == null) {
			_toggledStyle = new GUIStyle(GUI.skin.button);
			_toggledStyle.normal.background = _toggledStyle.onActive.background;
			_toggledStyle.normal.textColor = _toggledStyle.onActive.textColor;
		}
		if (_labelTextStyle == null) {
			_labelTextStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).textField);
			_labelTextStyle.normal = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).button.onNormal;
		}
	}
}

