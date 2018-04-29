using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace Terra.Terrain {
	public class TerrainPreview {
		TerraSettings Settings;

		TerrainPaint Paint;
		List<Texture2D> Splats;
		bool HideInInspector;
		
		/// <summary>
		/// Creates a new TerrainPreview instance
		/// </summary>
		/// <param name="settings">Active settings component</param>
		/// <param name="hideInInspector">Optionally hide created components in the inspector</param>
		public TerrainPreview(TerraSettings settings, bool hideInInspector = true) {
			this.Settings = settings;
			this.HideInInspector = hideInInspector;
		}

		/// <summary>
		/// Creates a preview of the entire mesh including the Mesh, 
		/// Materials, and object placement. Only performs update if
		/// <c>Settings.DisplayPreview</c> is true
		/// </summary>
		public void TriggerPreviewUpdate() {
			if (Settings.DisplayPreview) {
				RemoveComponents();
				AddComponents();
			}
		}

		/// <summary>
		/// Updates only the materials preview. Only performs update if
		/// <c>Settings.DisplayPreview</c> is true
		/// </summary>
		public void TriggerMaterialsUpdate() {
			if (Settings.DisplayPreview) {
				//Remove old splats
				Splats = null;
				Paint = null;

				if (Settings.GetComponent<MeshRenderer>() != null) {
					Object.DestroyImmediate(Settings.GetComponent<MeshRenderer>());
				}

				AddMaterialComponent();
			}
		}

		/// <summary>
		/// Updates only the procedural object placement. 
		/// </summary>
		public void TriggerObjectPlacementUpdate() {
			if (Settings.DisplayPreview && HasMesh()) {
				List<Vector2> objs = Settings.Placer.GetPoissonGrid(1);

				Bounds meshBounds = Settings.GetComponent<MeshFilter>().sharedMesh.bounds;
				float maxY = meshBounds.max.y;
				float minY = meshBounds.min.y;
				float length = meshBounds.size.x;

				//Only draw Gizmos in editor mode
				foreach (Vector2 pos in objs) {
					//Get 3D Y location on Mesh via Raycasting
					float x = (length / 2)

					UnityEditor.EditorUtility.InstantiatePrefab()
				}
			}
		}

		/// <summary>
		/// Checks to see if this gameobject has an attached mesh
		/// </summary>
		/// <returns>true if has MeshFilter component</returns>
		public bool HasMesh() {
			return Settings.GetComponent<MeshFilter>() != null && 
			Settings.GetComponent<MeshFilter>().sharedMesh != null;
		}

		/// <summary>
		/// Removes components associated with this preview, 
		/// does not remove cached data
		/// </summary>
		public void RemoveComponents() {
			if (Settings.GetComponent<MeshRenderer>() != null) {
				Object.DestroyImmediate(Settings.GetComponent<MeshRenderer>());
			}

			if (Settings.GetComponent<MeshFilter>() != null) {
				Object.DestroyImmediate(Settings.GetComponent<MeshFilter>());
			}
		}

		/// <summary>
		/// Creates a mesh with noise applied if a generator has been created
		/// </summary>
		/// <returns>Filled Mesh</returns>
		private Mesh CreateMesh() {
			return TerrainTile.GetPreviewMesh(Settings, Settings.Manager.GetEndGenerator());
		}

		/// <summary>
		/// Creates splatmaps for the currently active terrain gameobject. Does not apply 
		/// the splats.
		/// </summary>
		/// <returns>List of splat textures if SplatSettings is not null and 
		/// more than 0 splats were generated. Null otherwise.</returns>
		private List<Texture2D> CreateSplats() {
			if (Settings.SplatSettings != null) {
				if (Paint == null)
					Paint = new TerrainPaint(Settings.gameObject, Settings.SplatSettings);

				List<Texture2D> splats = Paint.GenerateSplatmaps(false);
				return splats.Count > 0 ? splats : null;
			}

			return null;
		}

		/// <summary>
		/// Adds mesh and material components associated with this preview, 
		/// fills the component data with available information 
		/// either cached or generated.
		/// </summary>
		private void AddComponents() {
			AddMeshComponent();
			TriggerMaterialsUpdate();
			TriggerObjectPlacementUpdate();
		}

		private void AddMaterialComponent() {
			if (Settings.GetComponent<MeshRenderer>() == null) {
				MeshRenderer rend = Settings.gameObject.AddComponent<MeshRenderer>();

				//Get cached or generated splatmaps
				if (Splats != null && Splats.Count > 0) {
					Paint.ApplySplatmapsToShaders(Splats);
				} else if (Settings.SplatSettings != null) {
					Splats = CreateSplats();

					if (Splats != null) {
						Paint.ApplySplatmapsToShaders(Splats);
					}
				}

				//Apply default material
				if (Splats == null || Splats.Count == 0) {
					const string path = "Nature/Terrain/Standard";
					rend.material = new Material(Shader.Find(path));
				}

				//Optionally hide renderer & material in inspector
				if (HideInInspector) {
					rend.hideFlags = HideFlags.HideInInspector;

					if (rend.sharedMaterial != null)
						rend.sharedMaterial.hideFlags = HideFlags.HideInInspector;
				}
			}
		}

		private void AddMeshComponent() {
			if (Settings.GetComponent<MeshFilter>() == null) {
				MeshFilter filter = Settings.gameObject.AddComponent<MeshFilter>();
				if (HideInInspector)
					filter.hideFlags = HideFlags.HideInInspector;

				//Generate mesh
				filter.sharedMesh = CreateMesh();
			}
		}
	}
}