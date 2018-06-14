using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Terra.Terrain.Detail;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Terra.Terrain {
	/// <summary>
	/// This class acts as a helped class for displaying object 
	/// placement previews in the editor
	/// </summary>
	class ObjectPlacerPreview {
		private TerraSettings Settings;
		private Mesh PreviewMesh;
		private GameObject Parent;
		private ObjectRenderer Placer;

		public static readonly string OBJ_PREVIEW_NAME = "OBJECT_PREVIEW";

		/// <summary>
		/// Creates an ObjectPlacerPreview instance with the passed 
		/// TerraSettings (for sampling different placement settings) and 
		/// a Mesh to place the objects on.
		/// </summary>
		/// <param name="settings">TerraSettings</param>
		/// <param name="m">A preview mesh to draw on top of</param>
		public ObjectPlacerPreview(TerraSettings settings, Mesh m) {
			Settings = settings;
			PreviewMesh = m;
			Placer = new ObjectRenderer();
			CreateParentGO();
		}

		public void PreviewAllObjects() {
			//Clear existing objects if any
			ClearExistingObjects();

			foreach (ObjectPlacementType type in Settings.ObjectPlacementSettings) {
				List<Vector3> positions = Placer.GetFilteredGrid(PreviewMesh, type);
				
				//Don't exceed max objects count
				int count = positions.Count > type.MaxObjects ? type.MaxObjects : positions.Count;

				for (int i = 0; i < count; i++) { 
					if (type.Prefab != null) {
						#if UNITY_EDITOR
						GameObject obj = (GameObject) PrefabUtility.InstantiatePrefab(type.Prefab);

						//Calculate correct positioning
						obj.transform.parent = Parent.transform;
						type.TransformGameObject(obj, positions[i], Settings.Length, Vector3.zero);
						#endif
					}
				}
			}
		}

		/// <summary>
		/// Creates the parent gameobject (if it doesn't already exist)
		/// that will contain all instantiated objects.
		/// </summary>
		void CreateParentGO() {
			if (Parent == null) {
				var comps = Settings.GetComponentsInChildren<Transform>().Where(t => t.gameObject.name == OBJ_PREVIEW_NAME);

				if (comps.Count() > 0) {
					Parent = comps.ToArray()[0].gameObject;
				} else {
					GameObject go = new GameObject(OBJ_PREVIEW_NAME);
					go.transform.parent = Settings.gameObject.transform;
					Parent = go;
				}
			}
		}

		/// <summary>
		/// Clears all existing gameobjects that are children of 
		/// the parent gameobject container.
		/// </summary>
		void ClearExistingObjects() {
			Component[] children = Parent.GetComponentsInChildren<Transform>();

			for (int i = 0; i < children.Length; i++) {
				if (children[i] != null && children[i].gameObject != Parent) {
					GameObject.DestroyImmediate(children[i].gameObject);
				}
			}
		}
	}
}
