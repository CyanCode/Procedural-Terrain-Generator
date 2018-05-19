﻿using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace JBooth.MegaSplat.NodeEditorFramework
{
	public class NodeEditorUserCache
	{
		public NodeCanvas nodeCanvas;
		public NodeEditorState editorState;
		public void AssureCanvas () { if (nodeCanvas == null) NewNodeCanvas (); if (editorState == null) NewEditorState (); }

		private string cachePath;
		private bool useCache;
		private const string MainEditorStateIdentifier = "MainEditorState";

		private string lastSessionPath { get { return cachePath + "/LastSession.asset"; } }

		public string openedCanvasPath = "";

		public NodeEditorUserCache (NodeCanvas loadedCanvas)
		{
			useCache = false;
			SetCanvas (loadedCanvas);
		}

		public NodeEditorUserCache ()
		{
			useCache = false;
		}

		#if UNITY_EDITOR
		public NodeEditorUserCache (string CachePath, NodeCanvas loadedCanvas)
		{
			useCache = true;
			cachePath = CachePath;
			SetCanvas (loadedCanvas);
		}

		public NodeEditorUserCache (string CachePath)
		{
			useCache = true;
			cachePath = CachePath;
		}
		#endif

		#if UNITY_EDITOR

		#region Cache

		public void SetupCacheEvents () 
		{ 
			if (!useCache)
				return;

			// Load the cache after the NodeEditor was cleared
			EditorLoadingControl.lateEnteredPlayMode -= LoadCache;
			EditorLoadingControl.lateEnteredPlayMode += LoadCache;
			EditorLoadingControl.justOpenedNewScene -= LoadCache;
			EditorLoadingControl.justOpenedNewScene += LoadCache;

			// Add new objects to the cache save file
			NodeEditorCallbacks.OnAddNode -= SaveNewNode;
			NodeEditorCallbacks.OnAddNode += SaveNewNode;
			NodeEditorCallbacks.OnAddNodeKnob -= SaveNewNodeKnob;
			NodeEditorCallbacks.OnAddNodeKnob += SaveNewNodeKnob;

			LoadCache ();
		}

		public void ClearCacheEvents () 
		{
			EditorLoadingControl.lateEnteredPlayMode -= LoadCache;
			EditorLoadingControl.justLeftPlayMode -= LoadCache;
			EditorLoadingControl.justOpenedNewScene -= LoadCache;
			NodeEditorCallbacks.OnAddNode -= SaveNewNode;
			NodeEditorCallbacks.OnAddNodeKnob -= SaveNewNodeKnob;
		}

		private void SaveNewNode (Node node) 
		{
			if (!useCache)
				return;
			if (nodeCanvas.livesInScene)
			{
				DeleteCache ();
				return;
			}
			if (!nodeCanvas.nodes.Contains (node))
				return;

			CheckCurrentCache ();

			NodeEditorSaveManager.AddSubAsset (node, lastSessionPath);
			foreach (ScriptableObject so in node.GetScriptableObjects ())
				NodeEditorSaveManager.AddSubAsset (so, node);

			foreach (NodeKnob knob in node.nodeKnobs)
			{
				NodeEditorSaveManager.AddSubAsset (knob, node);
				foreach (ScriptableObject so in knob.GetScriptableObjects ())
					NodeEditorSaveManager.AddSubAsset (so, knob);
			}

			UpdateCacheFile ();
		}

		private void SaveNewNodeKnob (NodeKnob knob) 
		{
			if (!useCache)
				return;
			if (nodeCanvas.livesInScene) 
			{
				DeleteCache ();
				return;
			}
			if (!nodeCanvas.nodes.Contains (knob.body))
				return;

			CheckCurrentCache ();

			NodeEditorSaveManager.AddSubAsset (knob, knob.body);
			foreach (ScriptableObject so in knob.GetScriptableObjects ())
				NodeEditorSaveManager.AddSubAsset (so, knob);

			UpdateCacheFile ();
		}

		/// <summary>
		/// Creates a new cache save file for the currently loaded canvas 
		/// Only called when a new canvas is created or loaded
		/// </summary>
		private void SaveCache () 
		{
			if (!useCache)
				return;
			if (nodeCanvas.livesInScene)
			{
				DeleteCache ();
				return;
			}

			nodeCanvas.editorStates = new NodeEditorState[] { editorState };
			NodeEditorSaveManager.SaveNodeCanvas (lastSessionPath, nodeCanvas, false);

			CheckCurrentCache ();
		}

		/// <summary>
		/// Loads the canvas from the cache save file
		/// Called whenever a reload was made
		/// </summary>
		private void LoadCache () 
		{
			if (!useCache)
			{
				NewNodeCanvas ();
				return;
			}
			// Try to load the NodeCanvas
			if (!File.Exists (lastSessionPath) || (nodeCanvas = NodeEditorSaveManager.LoadNodeCanvas (lastSessionPath, false)) == null)
			{
				NewNodeCanvas ();
				return;
			}

			// Fetch the associated MainEditorState
			editorState = NodeEditorSaveManager.ExtractEditorState (nodeCanvas, MainEditorStateIdentifier);
			if (!UnityEditor.AssetDatabase.Contains (editorState))
				NodeEditorSaveManager.AddSubAsset (editorState, lastSessionPath);

			CheckCurrentCache ();

			NodeEditor.RepaintClients ();
		}

		private void CheckCurrentCache () 
		{
			if (!nodeCanvas.livesInScene && UnityEditor.AssetDatabase.GetAssetPath (nodeCanvas) != lastSessionPath)
				throw new UnityException ("Cache system error: Current Canvas is not saved as the temporary cache!");
		}

		private void DeleteCache () 
		{
			UnityEditor.AssetDatabase.DeleteAsset (lastSessionPath);
			UnityEditor.AssetDatabase.Refresh ();
			//UnityEditor.EditorPrefs.DeleteKey ("NodeEditorLastSession");
		}

		private void UpdateCacheFile () 
		{
			UnityEditor.EditorUtility.SetDirty (nodeCanvas);
			//UnityEditor.AssetDatabase.SaveAssets ();
			//UnityEditor.AssetDatabase.Refresh ();
		}

		#endregion

		#endif

		#region Save/Load

		public void SetCanvas (NodeCanvas canvas)
		{
			nodeCanvas = canvas;
			editorState = NodeEditorSaveManager.ExtractEditorState (nodeCanvas, MainEditorStateIdentifier);
			NodeEditor.RepaintClients ();
		}

		/// <summary>
		/// Saves the mainNodeCanvas and it's associated mainEditorState as an asset at path
		/// </summary>
		public void SaveSceneNodeCanvas (string path) 
		{
			nodeCanvas.editorStates = new NodeEditorState[] { editorState };
			NodeEditorSaveManager.SaveSceneNodeCanvas (path, ref nodeCanvas, true);
			editorState = NodeEditorSaveManager.ExtractEditorState (nodeCanvas, MainEditorStateIdentifier);
		#if UNITY_EDITOR
			if (useCache)
				DeleteCache ();
		#endif
			NodeEditor.RepaintClients ();
		}

		/// <summary>
		/// Loads the mainNodeCanvas and it's associated mainEditorState from an asset at path
		/// </summary>
		public void LoadSceneNodeCanvas (string path) 
		{
		#if UNITY_EDITOR
			if (useCache)
				DeleteCache ();
		#endif
			// Try to load the NodeCanvas
			if ((nodeCanvas = NodeEditorSaveManager.LoadSceneNodeCanvas (path, true)) == null)
			{
				NewNodeCanvas ();
				return;
			}
			editorState = NodeEditorSaveManager.ExtractEditorState (nodeCanvas, MainEditorStateIdentifier);

			openedCanvasPath = path;
			NodeEditor.RepaintClients ();
		}

		/// <summary>
		/// Saves the mainNodeCanvas and it's associated mainEditorState as an asset at path
		/// </summary>
		public void SaveNodeCanvas (string path) 
		{
			nodeCanvas.editorStates = new NodeEditorState[] { editorState };
			NodeEditorSaveManager.SaveNodeCanvas (path, nodeCanvas, true);
			NodeEditor.RepaintClients ();
		}

		/// <summary>
		/// Loads the mainNodeCanvas and it's associated mainEditorState from an asset at path
		/// </summary>
		public void LoadNodeCanvas (string path) 
		{
			// Try to load the NodeCanvas
			if (!File.Exists (path) || (nodeCanvas = NodeEditorSaveManager.LoadNodeCanvas (path, true)) == null)
			{
				NewNodeCanvas ();
				return;
			}
			editorState = NodeEditorSaveManager.ExtractEditorState (nodeCanvas, MainEditorStateIdentifier);

			openedCanvasPath = path;
		#if UNITY_EDITOR
			if (useCache)
				SaveCache ();
		#endif

			NodeEditor.RepaintClients ();
		}

		/// <summary>
		/// Creates and loads a new NodeCanvas
		/// </summary>
		public void NewNodeCanvas (Type canvasType = null) 
		{
			if (canvasType != null && canvasType.IsSubclassOf (typeof(NodeCanvas)))
				nodeCanvas = ScriptableObject.CreateInstance(canvasType) as NodeCanvas;
			else
				nodeCanvas = ScriptableObject.CreateInstance<NodeCanvas>();
			nodeCanvas.name = "New " + nodeCanvas.canvasName;

			//EditorPrefs.SetString ("NodeEditorLastSession", "New Canvas");
			NewEditorState ();
			openedCanvasPath = "";
         NodeMegaSplatOutput root = (NodeMegaSplatOutput)NodeMegaSplatOutput.Create(NodeMegaSplatOutput.ID, Vector2.zero);
         nodeCanvas.nodes.Add(root);
		#if UNITY_EDITOR
			if (useCache)
				SaveCache ();
		#endif
		}

		/// <summary>
		/// Creates a new EditorState for the current NodeCanvas
		/// </summary>
		public void NewEditorState () 
		{
			editorState = ScriptableObject.CreateInstance<NodeEditorState> ();
			editorState.canvas = nodeCanvas;
			editorState.name = MainEditorStateIdentifier;
			nodeCanvas.editorStates = new NodeEditorState[] { editorState };
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty (nodeCanvas);
			#endif
		}

		#endregion
	}

}