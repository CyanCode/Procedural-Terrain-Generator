﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace JBooth.MegaSplat.NodeEditorFramework 
{
	/// <summary>
	/// Provides methods for loading resources both at runtime and in the editor; 
	/// Though, to load at runtime, they have to be in a resources folder
	/// </summary>
	public static class ResourceManager 
	{
		#region Common Resource Loading


		/// <summary>
		/// Loads a resource in the resources folder in both the editor and at runtime. 
		/// Path can be global, relative to the assets folder or, if used at runtime only, any subfolder, but has to be in a Resource folder to be loaded at runtime
		/// </summary>
		public static T[] LoadResources<T> (string path) where T : UnityEngine.Object
		{
         #if UNITY_EDITOR
         if (!path.StartsWith("Resources/"))
         {
            return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().ToArray();
         }
         #endif
         if (path.Contains ("Resources"))
            path = path.Substring (path.LastIndexOf ("Resources") + 10);
         path = path.Substring (0, path.LastIndexOf ('.'));
         return Resources.LoadAll<T>(path);
		}

		/// <summary>
		/// Loads a resource in the resources folder in both the editor and at runtime
		/// Path can be global, relative to the assets folder or, if used at runtime only, any subfolder, but has to be in a Resource folder to be loaded at runtime
		/// </summary>
		public static T LoadResource<T> (string path) where T : UnityEngine.Object
		{
         #if UNITY_EDITOR
         path = path.Replace("\\", "/");
         if (path.Contains("/Assets/"))
         {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path.Substring(path.IndexOf("/Assets/") + 1));
         }
         if (!path.StartsWith("Resources/"))
         {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
         }

         #endif
         if (path.Contains ("Resources"))
            path = path.Substring (path.LastIndexOf ("Resources") + 10);
        // path = path.Substring (0, path.LastIndexOf ('.'));
         return Resources.Load<T>(path);
		}

		#endregion

		#region Texture Management

		private static List<MemoryTexture> loadedTextures = new List<MemoryTexture> ();

		/// <summary>
		/// Loads a texture in the resources folder in both the editor and at runtime and manages it in a memory for later use.
		/// If you don't wan't to optimise memory, just use LoadResource instead
		/// It's adviced to prepare the texPath using the function before to create a uniform 'path format', because textures are compared through their paths
		/// </summary>
		public static Texture2D LoadTexture (string texPath)
		{
			if (String.IsNullOrEmpty (texPath))
				return null;
			int existingInd = loadedTextures.FindIndex ((MemoryTexture memTex) => memTex.path == texPath);
			if (existingInd != -1) 
			{ // If we have this texture in memory already, return it
				if (loadedTextures[existingInd].texture == null)
					loadedTextures.RemoveAt (existingInd);
				else
					return loadedTextures[existingInd].texture;
			}
			// Else, load up the texture and store it in memory
			Texture2D tex = LoadResource<Texture2D> (texPath);
			AddTextureToMemory (texPath, tex);
			return tex;
		}

		/// <summary>
		/// Loads up a texture tinted with col, and manages it in a memory for later use.
		/// It's adviced to prepare the texPath using the function before to create a uniform 'path format', because textures are compared through their paths
		/// </summary>
		public static Texture2D GetTintedTexture (string texPath, Color col) 
		{
			string texMod = "Tint:" + col.ToString ();
			Texture2D tintedTexture = GetTexture (texPath, texMod);
			if (tintedTexture == null)
			{ // We have to create a tinted version, perhaps even load the default texture if not yet in memory, and store it
				tintedTexture = LoadTexture (texPath);
				AddTextureToMemory (texPath, tintedTexture); // Register default texture for re-use
				tintedTexture = JBooth.MegaSplat.NodeEditorFramework.RTEditorGUI.Tint (tintedTexture, col);
				AddTextureToMemory (texPath, tintedTexture, texMod); // Register texture for re-use
			}
			return tintedTexture;
		}
		
		/// <summary>
		/// Records an additional texture for the manager memory with optional modifications
		/// It's adviced to prepare the texPath using the function before to create a uniform 'path format', because textures are compared through their paths
		/// </summary>
		public static void AddTextureToMemory (string texturePath, Texture2D texture, params string[] modifications)
		{
			if (texture == null) return;
			loadedTextures.Add (new MemoryTexture (texturePath, texture, modifications));
		}
		
		/// <summary>
		/// Returns whether the manager memory contains the texture
		/// It's adviced to prepare the texPath using the function before to create a uniform 'path format', because textures are compared through their paths
		/// </summary>
		public static MemoryTexture FindInMemory (Texture2D tex)
		{
			int existingInd = loadedTextures.FindIndex ((MemoryTexture memTex) => memTex.texture == tex);
			return existingInd != -1? loadedTextures[existingInd] : null;
		}
		
		/// <summary>
		/// Whether the manager memory contains a texture with optional modifications
		/// It's adviced to prepare the texPath using the function before to create a uniform 'path format', because textures are compared through their paths
		/// </summary>
		public static bool HasInMemory (string texturePath, params string[] modifications)
		{
			int existingInd = loadedTextures.FindIndex ((MemoryTexture memTex) => memTex.path == texturePath);
			return existingInd != -1 && EqualModifications (loadedTextures[existingInd].modifications, modifications);
		}
		
		/// <summary>
		/// Gets a texture already in manager memory with specified modifications (check with contains before!)
		/// It's adviced to prepare the texPath using the function before to create a uniform 'path format', because textures are compared through their paths
		/// </summary>
		public static MemoryTexture GetMemoryTexture (string texturePath, params string[] modifications)
		{
			List<MemoryTexture> textures = loadedTextures.FindAll ((MemoryTexture memTex) => memTex.path == texturePath);
			if (textures == null || textures.Count == 0)
				return null;
			foreach (MemoryTexture tex in textures)
				if (EqualModifications (tex.modifications, modifications))
					return tex;
			return null;
		}
		
		/// <summary>
		/// Gets a texture already in manager memory with specified modifications (check with 'HasInMemory' before!)
		/// It's adviced to prepare the texPath using the function before to create a uniform 'path format', because textures are compared through their paths
		/// </summary>
		public static Texture2D GetTexture (string texturePath, params string[] modifications)
		{
			MemoryTexture memTex = GetMemoryTexture (texturePath, modifications);
			return memTex == null? null : memTex.texture;
		}
		
		private static bool EqualModifications (string[] modsA, string[] modsB) 
		{
			return modsA.Length == modsB.Length && Array.TrueForAll (modsA, mod => modsB.Count (oMod => mod == oMod) == modsA.Count (oMod => mod == oMod));
		}
		
		public class MemoryTexture 
		{
			public string path;
			public Texture2D texture;
			public string[] modifications;
			
			public MemoryTexture (string texPath, Texture2D tex, params string[] mods) 
			{
				path = texPath;
				texture = tex;
				modifications = mods;
			}
		}
		
		#endregion
	}

}