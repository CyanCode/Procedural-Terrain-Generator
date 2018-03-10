using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	public class TerrainPreview {
		TerraSettings Settings;

		GameObject PreviewGO;
		TerrainPaint Paint;
		MeshFilter Filter;
		Mesh PrevMesh;

		public TerrainPreview(TerraSettings settings) {
			this.Settings = settings;
		}

		/// <summary>
		/// Creates a preview of the entire mesh including the Mesh, 
		/// Materials, and object placement
		/// </summary>
		public void TriggerPreviewUpdate() {
			PrevMesh = CreateMesh();

			//Generate a "clean" gameobject
			if (PreviewGO != null) {
				Object.Destroy(PreviewGO);
			}

			//TODO: Set object placement
			//Set materials if needed
			TriggerMaterialUpdate();
		}

		/// <summary>
		/// Updates only the material on this TerrainTile
		/// </summary>
		public void TriggerMaterialUpdate() {
			if (Settings.SplatSettings != null) {
				if (Paint == null)
					Paint = new TerrainPaint(PreviewGO, Settings.SplatSettings);

				Paint.GenerateSplatmaps();
			}
		}

		/// <summary>
		/// Sets this preview gameobject's visibility. When invisible, 
		/// the gameobject is removed from the scene until set visible again.
		/// </summary>
		/// <param name="isVisible"></param>
		public void SetVisible(bool isVisible) {
			if (isVisible) {
				if (PrevMesh != null)
					SetupBasicGO(PrevMesh);
				else
					SetupBasicGO(CreateMesh());
					
				TriggerMaterialUpdate();
			} else {
				if (PreviewGO != null)
					Object.DestroyImmediate(PreviewGO);
			}
		}

		/// <summary>
		/// Sets up the PreviewGO gameobject with the basic required
		/// components (MeshFilter, MeshRenderer, and the supplied Mesh)
		/// </summary>
		/// <param name="mesh">Mesh to apply</param>
		private void SetupBasicGO(Mesh mesh) {
			PreviewGO = new GameObject("Terrain Preview");
			PreviewGO.AddComponent<MeshRenderer>();
			Filter = PreviewGO.AddComponent<MeshFilter>();
			PrevMesh = Filter.mesh = mesh;
		}

		/// <summary>
		/// Creates a mesh with noise applied if a generator has been created
		/// </summary>
		/// <returns></returns>
		private Mesh CreateMesh() {
			return TerrainTile.GetPreviewMesh(Settings, Settings.Generator);
		}
	}
}