using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Assets.Code.Bon;
using Assets.Code.Bon.Socket;
using Assets.Editor.Bon;
using System.Linq;
using System.IO;

/// <summary>
/// This class contains the logic of the editor window. It contains canvases that
/// are containing graphs. It uses the BonLauncher to load, save and close Graphs.
/// </summary>
public class BonWindow: EditorWindow {
	private const string Name = "Noise Editor"; //TODO: Change to editor name
	public const int TopOffset = 30;
	public const int BottomOffset = 22;
	public const int TopMenuHeight = 24;

	private const int WindowTitleHeight = 21;
	private const float CanvasZoomMin = 0.1f;
	private const float CanvasZoomMax = 1.0f;

	private Rect SaveButtonRect = new Rect(8, 8, 80, TopMenuHeight);
	private Rect FileNameLabelRect = new Rect(94, 8, 500, TopMenuHeight);

	private Vector2 NextTranlationPosition;

	private BonCanvas CurrentCanvas;
	private Rect CanvasRegion = new Rect();

	private AbstractSocket DragSourceSocket = null;
	private Vector2 LastMousePosition;

	private GenericMenu Menu;
	private Dictionary<string, Type> MenuEntryToNodeType;

	private Rect TmpRect = new Rect();
	
	static void OnCreateWindow() {
		BonWindow window = GetWindow<BonWindow>();
		// BonWindow window = CreateInstance<BonWindow>(); // to create a new window
		window.Show();
	}

	public void OnEnable() {
		Init();
	}

	public void Init() {
		EditorApplication.playmodeStateChanged = OnPlaymodeStateChanged;
		// create GameObject and the Component if it is not added to the scene

		titleContent = new GUIContent(Name);
		wantsMouseMove = true;
		EventManager.TriggerOnWindowOpen();
		MenuEntryToNodeType = CreateMenuEntries();
		Menu = CreateGenericMenu();

		CurrentCanvas = null;

		//if (GetLauncher().Graphs.Count > 0) LoadCanvas(GetLauncher().Graph);
		//else LoadCanvas(GetLauncher().LoadGraph(BonConfig.DefaultGraphName));

		LoadCanvas(GetLauncher().Graph);
		UpdateGraphs();
		Repaint();
	}

	private void OnPlaymodeStateChanged() {
		UpdateGraphs();
		Repaint();
	}

	private void UpdateGraphs() {
		GetLauncher().Graph.ForceUpdateNodes();
	}

	private void LoadCanvas(List<Graph> graphs) {
		foreach (var graph in graphs) LoadCanvas(graph);
	}

	private void LoadCanvas(Graph graph) {
		CurrentCanvas = new BonCanvas(graph);
	}

	/// <summary>
	/// Creates a dictonary that maps a menu entry string to a node type using reflection.
	/// </summary>
	/// <returns>
	/// Dictonary that maps a menu entry string to a node type
	/// </returns>
	public Dictionary<string, Type> CreateMenuEntries() {
		Dictionary<string, Type> menuEntries = new Dictionary<string, Type>();

		IEnumerable<Type> classesExtendingNode = Assembly.GetAssembly(typeof(Node)).GetTypes()
			.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Node)));

		foreach (Type type in classesExtendingNode) menuEntries.Add(GetItemMenuName(type), type);

		menuEntries.OrderBy(x => x.Key);
		return menuEntries;
	}

	private string GetItemMenuName(Type type) {
		string path = Node.GetNodePath(type);
		if (path != null) return path + "/" + Node.GetNodeName(type);
		return Node.GetNodeName(type);
	}


	private BonLauncher GetLauncher() {
		if (FindObjectOfType<TerrainSettings>() == null) {
			Debug.LogError("Cannot launch graph editor without a TerrainSettings component.");
			return null;
		}

		BonLauncher launcher = null;
		if ((launcher = FindObjectOfType<BonLauncher>()) == null) {
			launcher = FindObjectOfType<TerrainSettings>().gameObject.AddComponent<BonLauncher>();
			Log.Info("Added BonLauncher Component '" + BonConfig.GameObjectName + "' to TerrainSettings gameobject");
		}

		return launcher;
	}

	/// <summary>Draws the UI</summary>
	void OnGUI() {
		HandleCanvasTranslation();
		HandleDragAndDrop();

		if (Event.current.type == EventType.ContextClick) {
			Menu.ShowAsContext();
			Event.current.Use();
		}

		HandleMenuButtons();

		if (GetLauncher() == null) return;
		if (CurrentCanvas != null) {
			float infoPanelY = Screen.height - TopOffset - 6;
			TmpRect.Set(5, infoPanelY, 70, 20);
			GUI.Label(TmpRect, "zoom: " + Math.Round(CurrentCanvas.Zoom, 1));
			TmpRect.Set(60, infoPanelY, 70, 20);
			GUI.Label(TmpRect, "x: " + Math.Round(CurrentCanvas.Position.x));
			TmpRect.Set(130, infoPanelY, 70, 20);
			GUI.Label(TmpRect, "y: " + Math.Round(CurrentCanvas.Position.y));
			TmpRect.Set(200, infoPanelY, 70, 20);
			GUI.Label(TmpRect, "nodes: " + CurrentCanvas.Graph.GetNodeCount());
		}

		if (CurrentCanvas != null) {
			CanvasRegion.Set(0, TopOffset, Screen.width, Screen.height - 2 * TopOffset - BottomOffset);
			CurrentCanvas.Draw(this, CanvasRegion, DragSourceSocket);
		}
		LastMousePosition = Event.current.mousePosition;

		Repaint();
	}

	private void SetCurrentCanvas(BonCanvas canvas) {
		UpdateGraphs();
		Repaint();
		if (canvas != null) EventManager.TriggerOnFocusGraph(canvas.Graph);
		CurrentCanvas = canvas;
	}

	private GenericMenu CreateGenericMenu() {
		GenericMenu m = new GenericMenu();
		foreach (KeyValuePair<string, Type> entry in MenuEntryToNodeType)
			m.AddItem(new GUIContent(entry.Key), false, OnGenericMenuClick, entry.Value);
		return m;
	}

	private void OnGenericMenuClick(object item) {
		if (CurrentCanvas != null) {
			CurrentCanvas.CreateNode((Type)item, LastMousePosition);
		}
	}

	public void CreateCanvas(string path) {
		BonCanvas canvas;
		if (path != null) canvas = new BonCanvas(GetLauncher().LoadGraph(path));
		else canvas = new BonCanvas(GetLauncher().LoadGraph(BonConfig.DefaultGraphName));
		canvas.FilePath = path;
		SetCurrentCanvas(canvas);
	}

	private void OpenSaveDialog() {
		string path = CurrentCanvas.FilePath != "" ?
			CurrentCanvas.FilePath : EditorUtility.SaveFilePanelInProject("Save Graph",
				"TerrainGraph", "json", "Choose a location to save the graph file.");
		
		GetLauncher().SaveGraph(CurrentCanvas.Graph, path);
		CurrentCanvas.FilePath = path;
		EditorUtility.DisplayDialog("Graph Saved", "Graph has been successfully saved to: " + path, "Close");
	}

	private void HandleMenuButtons() {
		if (CurrentCanvas.FilePath != null && GUI.Button(SaveButtonRect, "Save")) OpenSaveDialog();

		string name = CurrentCanvas.FilePath == "" || CurrentCanvas.FilePath == null ? 
			"[No File Opened]" : Path.GetFileNameWithoutExtension(CurrentCanvas.FilePath);

		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		GUI.Label(FileNameLabelRect, "Opened File: " + name); //TODO: SHow open file
	}


	private void HandleCanvasTranslation() {
		if (CurrentCanvas == null) return;

		// Zoom
		if (Event.current.type == EventType.ScrollWheel) {
			Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(Event.current.mousePosition);
			float zoomDelta = -Event.current.delta.y / 150.0f;
			float oldZoom = CurrentCanvas.Zoom;
			CurrentCanvas.Zoom = Mathf.Clamp(CurrentCanvas.Zoom + zoomDelta, CanvasZoomMin, CanvasZoomMax);

			NextTranlationPosition = CurrentCanvas.Position + (zoomCoordsMousePos - CurrentCanvas.Position) -
				(oldZoom / CurrentCanvas.Zoom) * (zoomCoordsMousePos - CurrentCanvas.Position);

			if (NextTranlationPosition.x >= 0) NextTranlationPosition.x = 0;
			if (NextTranlationPosition.y >= 0) NextTranlationPosition.y = 0;
			CurrentCanvas.Position = NextTranlationPosition;
			Event.current.Use();
			return;
		}

		// Translate
		if (Event.current.type == EventType.MouseDrag &&
			(Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
			Event.current.button == 2) {
			Vector2 delta = Event.current.delta;
			delta /= CurrentCanvas.Zoom;

			NextTranlationPosition = CurrentCanvas.Position + delta;
			if (NextTranlationPosition.x >= 0) NextTranlationPosition.x = 0;
			if (NextTranlationPosition.y >= 0) NextTranlationPosition.y = 0;

			CurrentCanvas.Position = NextTranlationPosition;
			Event.current.Use();
		}
	}

	private void HandleSocketDrag(AbstractSocket dragSource) {
		if (dragSource != null) {
			if (dragSource.IsInput() && dragSource.IsConnected()) {
				DragSourceSocket = ((InputSocket)dragSource).Edge.GetOtherSocket(dragSource);
				CurrentCanvas.Graph.UnLink((InputSocket)dragSource, (OutputSocket)DragSourceSocket);
			}
			if (dragSource.IsOutput()) DragSourceSocket = dragSource;
			Event.current.Use();
		}
		Repaint();
	}

	private void HandleSocketDrop(AbstractSocket dropTarget) {
		if (dropTarget != null && dropTarget.GetType() != DragSourceSocket.GetType()) {
			if (dropTarget.IsInput()) {
				CurrentCanvas.Graph.Link((InputSocket)dropTarget, (OutputSocket)DragSourceSocket);
			}
			Event.current.Use();
		}
		DragSourceSocket = null;
		Repaint();
	}

	private void HandleDragAndDrop() {
		if (CurrentCanvas == null) return;

		if (Event.current.type == EventType.MouseDown) {
			HandleSocketDrag(CurrentCanvas.GetSocketAt(Event.current.mousePosition));
		}

		if (Event.current.type == EventType.MouseUp && DragSourceSocket != null) {
			HandleSocketDrop(CurrentCanvas.GetSocketAt(Event.current.mousePosition));
		}

		if (Event.current.type == EventType.MouseDrag) {
			if (DragSourceSocket != null) Event.current.Use();
		}
	}

	private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords) {
		return (screenCoords - CanvasRegion.TopLeft()) / CurrentCanvas.Zoom + CurrentCanvas.Position;
	}
}
