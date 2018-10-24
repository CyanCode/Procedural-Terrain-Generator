using System;
using System.Collections.Generic;

namespace Terra.Structure {
	[Serializable]
	public class DetailData {
		public bool ShowMaterialFoldout = false;
		public bool ShowObjectFoldout = false;
		public bool ShowGrassFoldout = false;

		public bool IsMaxHeightSelected = false;
		public bool IsMinHeightSelected = false;

		public List<SplatData> SplatsData;
		public List<ObjectPlacementData> ObjectData;

		public DetailData() {
			if (SplatsData == null)
				SplatsData = new List<SplatData>();
			if (ObjectData == null)
				ObjectData = new List<ObjectPlacementData>();
		}
	}
}
