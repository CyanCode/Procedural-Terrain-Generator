using System.Collections.Generic;
using UnityEngine;
using Terra.CoherentNoise;
using Assets.Terra.Terrain.Util;

namespace Terra.Terrain {
	public class TerrainPreview : ScriptableObject {
		TerraSettings Settings;

		TerrainPaint Paint;
		List<Texture2D> Splats;
		
		void OnEnable() {
			this.Settings = TerraSettings.Instance; 
		}

		/// <summary>
		/// Checks whether a terrain preview can be created
		/// </summary>
		public bool CanPreview() {
			return Settings.EditorState.DisplayPreview && Settings.Generator.Graph != null &&
				Settings.Generator.Graph.GetEndGenerator() != null && Settings.Preview != null;
		}

		/// <summary>
		/// Creates a preview of the entire mesh including the Mesh, 
		/// Materials, and object placement. Only performs update if
		/// <c>Settings.DisplayPreview</c> is true
		/// </summary>
		public void TriggerPreviewUpdate() {
			if (CanPreview() && Settings.EditorState.DisplayPreview) {
				RemoveComponents();
				AddComponents();
			}
		}

		/// <summary>
		/// Updates only the materials preview. Only performs update if
		/// <c>Settings.DisplayPreview</c> is true
		/// </summary>
		public void TriggerMaterialsUpdate() {
			if (Settings.EditorState.DisplayPreview) {
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
			if (Settings.EditorState.DisplayPreview && HasMesh()) {
				ObjectPlacerPreview preview = new ObjectPlacerPreview(Settings, Settings.GetComponent<MeshFilter>().sharedMesh);
				preview.PreviewAllObjects();
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
		/// Checks to see if TerraSettings has an attached end Generator.
		/// </summary>
		/// <returns>true if an end generator is attached</returns>
		public bool HasEndGenerator() {
			return Settings.Generator.Graph.GetEndGenerator() != null;
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
		/// Can be called at the beginning of Play mode. 
		/// Removes all added components and objects used 
		/// for previewing.
		/// </summary>
		public void Cleanup() {
			if (Settings.GetComponent<MeshRenderer>() != null) Object.Destroy(Settings.GetComponent<MeshRenderer>());
			if (Settings.GetComponent<MeshFilter>() != null) Object.Destroy(Settings.GetComponent<MeshFilter>());

			foreach (Transform c in Settings.transform) {
				if (c.name == ObjectPlacerPreview.OBJ_PREVIEW_NAME) {
					Object.Destroy(c.gameObject);
					break;
				}
			}
		}

		/// <summary>
		/// Creates a mesh with noise applied if a generator has been created
		/// </summary>
		/// <returns>Filled Mesh, null if no end generator is provided.</returns>
		private Mesh CreateMesh() {
			if (Settings.Generator.Graph != null) {
				Generator end = Settings.Generator.Graph.GetEndGenerator();

				if (end != null) {
					return Tile.GetPreviewMesh(Settings, Settings.Generator.Graph.GetEndGenerator());
				}
			}

			return null;
		}

		/// <summary>
		/// Creates splatmaps for the currently active terrain gameobject. Does not apply 
		/// the splats.
		/// </summary>
		/// <returns>List of splat textures if SplatSettings is not null and 
		/// more than 0 splats were generated. Null otherwise.</returns>
		private List<Texture2D> CreateSplats() {
			if (Settings.SplatData != null) {
				if (Paint == null)
					Paint = new TerrainPaint(Settings.gameObject);

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
			TriggerMeshUpdate();
			TriggerMaterialsUpdate();
			TriggerObjectPlacementUpdate();
			TriggerHideInInspectorUpdate();
		}

		private void AddMaterialComponent() {
			if (Settings.GetComponent<MeshRenderer>() == null) {
				MeshRenderer rend = Settings.gameObject.AddComponent<MeshRenderer>();

				if (false && Settings.CustomMaterial != null) {
					//Apply custom material to terrain
					rend.sharedMaterial = Settings.CustomMaterial;
				} else {
					//Get cached or generated splatmaps
					if (Splats != null && Splats.Count > 0) {
						Paint.ApplySplatmapsToShaders(Splats);
					} else if (Settings.SplatData != null) {
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
				}
			}
		}

		/// <summary>
		/// Adds a mesh component if one doesn't already exist AND 
		/// there is an attached end generator.
		/// </summary>
		private void TriggerMeshUpdate() {
			if (Settings.GetComponent<MeshFilter>() == null && HasEndGenerator()) {
				MeshFilter filter = Settings.gameObject.AddComponent<MeshFilter>();

				//Generate mesh
				Mesh m = CreateMesh();
				if (m != null) {
					filter.sharedMesh = CreateMesh();
				}				
			}
		}

		/// <summary>
		/// Hides or shows components in inspector based on 
		/// set TerraDebug setting.
		/// </summary>
		private void TriggerHideInInspectorUpdate() {
			MeshFilter mf = Settings.GetComponent<MeshFilter>();
			MeshRenderer mr = Settings.GetComponent<MeshRenderer>();
			HideFlags isHidden = TerraSettings.TerraDebug.HIDE_IN_INSPECTOR ? HideFlags.HideInInspector : HideFlags.None;

			//If hiding in inspector, only hide specific components
			if (isHidden == HideFlags.HideInInspector) {
				if (mf != null) {
					mf.hideFlags = isHidden;
				}
				if (mr != null) {
					mr.hideFlags = isHidden;

					if (mr.sharedMaterial != null) {
						mr.sharedMaterial.hideFlags = isHidden;
					}
				}
			} else { //Otherwise show everything
				foreach (Component c in Settings.GetComponents<Component>()) {
					c.hideFlags = HideFlags.None;
				}
			}
		}
	}
}