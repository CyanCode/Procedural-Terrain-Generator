using System;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   public class MegaSplatTextureGraphWindow : EditorWindow
   {
      Bounds currentBounds;
      // Information about current instance
      private static MegaSplatTextureGraphWindow _editor;

      public static MegaSplatTextureGraphWindow editor
      {
         get
         {
            AssureEditor();
            return _editor;
         }
      }

      public static void AssureEditor()
      {
         if (_editor == null)
            OpenNodeEditor();
      }

      // Opened Canvas
      public static NodeEditorUserCache canvasCache;

      // GUI
      private Rect loadSceneUIPos;
      private Rect createCanvasUIPos;
      private int sideWindowWidth = 400;

      public Rect sideWindowRect { get { return new Rect(position.width - sideWindowWidth, 0, sideWindowWidth, position.height); } }

      public Rect canvasWindowRect { get { return new Rect(0, 0, position.width - sideWindowWidth, position.height); } }

      #region General

      /// <summary>
      /// Opens the Node Editor window and loads the last session
      /// </summary>
      [MenuItem("Window/MegaSplat/Texture Graph")]
      public static MegaSplatTextureGraphWindow OpenNodeEditor()
      {
         _editor = GetWindow<MegaSplatTextureGraphWindow>();
         _editor.minSize = new Vector2(800, 600);
         NodeEditor.ReInit(false);

         Texture iconTexture = ResourceManager.LoadTexture(EditorGUIUtility.isProSkin ? "Textures/Icon_Dark" : "Textures/Icon_Light");
         _editor.titleContent = new GUIContent("Texture Graph", iconTexture);

         return _editor;
      }

      [UnityEditor.Callbacks.OnOpenAsset(1)]
      private static bool AutoOpenCanvas(int instanceID, int line)
      {
         if (Selection.activeObject != null && Selection.activeObject is NodeCanvas)
         {
            string NodeCanvasPath = AssetDatabase.GetAssetPath(instanceID);
            MegaSplatTextureGraphWindow.OpenNodeEditor();
            canvasCache.LoadNodeCanvas(NodeCanvasPath);
            return true;
         }
         return false;
      }


      private void OnEnable()
      {            
         _editor = this;
         NodeEditor.checkInit(false);

         NodeEditor.ClientRepaints -= Repaint;
         NodeEditor.ClientRepaints += Repaint;

         EditorLoadingControl.justLeftPlayMode -= NormalReInit;
         EditorLoadingControl.justLeftPlayMode += NormalReInit;
         // Here, both justLeftPlayMode and justOpenedNewScene have to act because of timing
         EditorLoadingControl.justOpenedNewScene -= NormalReInit;
         EditorLoadingControl.justOpenedNewScene += NormalReInit;

         //EditorLoadingControl.beforeEnteringPlayMode -= EnterPlaymode;
         //EditorLoadingControl.beforeEnteringPlayMode += EnterPlaymode;
         SceneView.onSceneGUIDelegate -= OnSceneGUI;
         SceneView.onSceneGUIDelegate += OnSceneGUI;

         // Setup Cache
         canvasCache = new NodeEditorUserCache(Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))));
         canvasCache.SetupCacheEvents();
      }

      private void NormalReInit()
      {
         NodeEditor.ReInit(false);
      }

      private void OnDestroy()
      {
         EditorUtility.SetDirty(canvasCache.nodeCanvas);
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();

         NodeEditor.ClientRepaints -= Repaint;

         EditorLoadingControl.justLeftPlayMode -= NormalReInit;
         EditorLoadingControl.justOpenedNewScene -= NormalReInit;

         SceneView.onSceneGUIDelegate -= OnSceneGUI;

         // Clear Cache
         canvasCache.ClearCacheEvents();
      }

      #endregion

      #region GUI

      NodeMegaSplatOutput masterNode = null;
      private void OnSceneGUI(SceneView sceneview)
      {
         bool found = false;
         for (int i = 0; i < canvasCache.nodeCanvas.nodes.Count; ++i)
         {
            masterNode = canvasCache.nodeCanvas.nodes[i] as NodeMegaSplatOutput;
            if (masterNode != null)
            {
               found = true;
               // hack to make preview data available..
               NodeMegaSplatOutput.sConfig = masterNode.config;
               break;
            }
         }
         if (!found)
         {
            //masterNode = (NodeMegaSplatOutput)NodeMegaSplatOutput.Create(NodeMegaSplatOutput.ID, Vector2.zero);
            //canvasCache.nodeCanvas.nodes.Add(masterNode);
            return;
         }
         DrawSceneGUI();
      }

      private void DrawSceneGUI()
      {
         if (canvasCache.editorState.selectedNode != null)
            canvasCache.editorState.selectedNode.OnSceneGUI();
         SceneView.lastActiveSceneView.Repaint();
      }

      private void OnGUI()
      {            
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         // Initiation
         NodeEditor.checkInit(true);
         if (NodeEditor.InitiationError)
         {
            GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
            return;
         }
         AssureEditor();
         canvasCache.AssureCanvas();

         // Specify the Canvas rect in the EditorState
         canvasCache.editorState.canvasRect = canvasWindowRect;
         // If you want to use GetRect:
//			Rect canvasRect = GUILayoutUtility.GetRect (600, 600);
//			if (Event.current.type != EventType.Layout)
//				mainEditorState.canvasRect = canvasRect;
         NodeEditorGUI.StartNodeGUI();

         // Perform drawing with error-handling
         try
         {
            NodeEditor.DrawCanvas(canvasCache.nodeCanvas, canvasCache.editorState);
         }
         catch (UnityException e)
         { // on exceptions in drawing flush the canvas to avoid locking the ui.
            canvasCache.NewNodeCanvas();
            NodeEditor.ReInit(true);
            Debug.LogError("Unloaded Canvas due to an exception during the drawing phase!");
            Debug.LogException(e);
         }

         // Draw Side Window
         sideWindowWidth = Math.Min(600, Math.Max(200, (int)(position.width / 5)));
         GUILayout.BeginArea(sideWindowRect, GUI.skin.box);
         DrawSideWindow();
         GUILayout.EndArea();

         NodeEditorGUI.EndNodeGUI();
      }

      private void DrawSideWindow()
      {
         GUILayout.Label(new GUIContent("Node Editor (" + canvasCache.nodeCanvas.name + ")", "Opened Canvas path: " + canvasCache.openedCanvasPath), NodeEditorGUI.nodeLabelBold);

         if (GUILayout.Button(new GUIContent("New Canvas", "Loads an Specified Empty CanvasType")))
         {
            JBooth.MegaSplat.NodeEditorFramework.GenericMenu menu = new JBooth.MegaSplat.NodeEditorFramework.GenericMenu();
            NodeCanvasManager.FillCanvasTypeMenu(ref menu, canvasCache.NewNodeCanvas);
            menu.Show(createCanvasUIPos.position, createCanvasUIPos.width);
         }
         if (Event.current.type == EventType.Repaint)
         {
            Rect popupPos = GUILayoutUtility.GetLastRect();
            createCanvasUIPos = new Rect(popupPos.x + 2, popupPos.yMax + 2, popupPos.width - 4, 0);
         }

         GUILayout.Space(6);

         if (GUILayout.Button(new GUIContent("Save Canvas", "Saves the Canvas to a Canvas Save File in the Assets Folder")))
         {
            string path = EditorUtility.SaveFilePanelInProject("Save Node Canvas", "Node Canvas", "asset", "", Application.dataPath);
            if (!string.IsNullOrEmpty(path))
               canvasCache.SaveNodeCanvas(path);
         }

         if (GUILayout.Button(new GUIContent("Load Canvas", "Loads the Canvas from a Canvas Save File in the Assets Folder")))
         {
            string path = EditorUtility.OpenFilePanel("Load Node Canvas", Application.dataPath, "asset");
            if (!path.Contains(Application.dataPath))
            {
               if (!string.IsNullOrEmpty(path))
                  ShowNotification(new GUIContent("You should select an asset inside your project folder!"));
            }
            else
               canvasCache.LoadNodeCanvas(path);
         }

         GUILayout.Space(6);
         if (masterNode != null)
         {
            masterNode.MainGUI();
         }
         if (GUILayout.Button("Prepare Selection"))
         {
            CaptureObjects();
         }

         for (int i = 0; i < jobs.Count; ++i)
         {
            var j = jobs[i];
            if (j.stream != null)
            {
               EditorGUILayout.LabelField(j.stream.gameObject.name);
            }
            if (j.terrain != null)
            {
               EditorGUILayout.LabelField(j.terrain.gameObject.name);
            }
         }

         if (GUILayout.Button("Compile"))
         {
            Process();
         }



         GUILayout.Space(6);

         if (canvasCache.editorState.selectedNode != null && Event.current.type != EventType.Ignore)
            canvasCache.editorState.selectedNode.DrawNodePropertyEditor();
      }

      public void MaterialUpdate()
      {
         Process();
      }



      List<NodeMegaSplatOutput.Job> jobs = new List<NodeMegaSplatOutput.Job>();

      public void CaptureObjects()
      {
         UnityEngine.Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.OnlyUserModifiable | SelectionMode.Deep);

         foreach (var j in jobs)
         {
            j.Release();
         }
         jobs.Clear();
         Bounds b = new Bounds();
         bool found = false;
         for (int i = 0; i < objs.Length; ++i)
         {
            var go = (GameObject)objs[i];
            var vis = go.GetComponent<JBooth.VertexPainterPro.VertexInstanceStream>();
            var terrain = go.GetComponent<Terrain>();
            if (vis != null)
            {
               NodeMegaSplatOutput.Job j = new NodeMegaSplatOutput.Job();
               Bounds bound = j.Init(vis, masterNode);
               jobs.Add(j);
               if (found)
               {
                  b.Encapsulate(bound);
               }
               else
               {
                  found = true;
                  b = bound;
               }
            }
            if (terrain != null)
            {
               if (terrain.materialType == Terrain.MaterialType.Custom && terrain.materialTemplate != null)
               {
                  var mat = terrain.materialTemplate;
                  if (mat.IsKeywordEnabled("_TERRAIN") && mat.HasProperty("_SplatControl"))
                  {
                     NodeMegaSplatOutput.Job j = new NodeMegaSplatOutput.Job();
                     Bounds bound = j.Init(terrain, masterNode);
                     jobs.Add(j);
                     if (found)
                     {
                        b.Encapsulate(bound);
                     }
                     else
                     {
                        found = true;
                        b = bound;
                     }
                  }
               }
            }
         }
         currentBounds = b;
         foreach (var j in jobs)
         {
            j.bounds = currentBounds;
         }
      }

      public void Process()
      {
         NodeMegaSplatOutput outputNode = null;
         foreach (var node in canvasCache.nodeCanvas.nodes)
         {
            node.compiled = false;
            var mo = node as NodeMegaSplatOutput;
            if (mo != null)
            {
               outputNode = mo;
            }
         }
            

         EvalData ed = new EvalData();
         if (EvalData.curves == null)
         {
            EvalData.curves = new Texture2DArray(256, 1, 256, TextureFormat.Alpha8, false, true);
            EvalData.curves.wrapMode = TextureWrapMode.Clamp;
         }
         ed.bounds = currentBounds;
         EvalData.data = ed;

         outputNode.WriteShader(ed);

         outputNode.Process(jobs, currentBounds, ed);

         EvalData.data = null;
      
      }



      public void LoadSceneCanvasCallback(object canvas)
      {
         canvasCache.LoadSceneNodeCanvas((string)canvas);
      }
         
      void Update()
      {
         for (int i = 0; i < jobs.Count; ++i)
         {
            if (jobs[i].needsRender)
            {
               jobs[i].Render(NodeMegaSplatOutput.sRenderMat);
               break;
            }
            if (jobs[i].needsApply)
            {
               jobs[i].Apply();
               break;
            }
         }

      }

      #endregion
   }
}