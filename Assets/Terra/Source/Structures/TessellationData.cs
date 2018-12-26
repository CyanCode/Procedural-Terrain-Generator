using System;

namespace Terra.Structures {
	[Serializable]
	public class TessellationData {
		public float TessellationAmount = 4f;
		public float TessellationMinDistance = 5f;
		public float TessellationMaxDistance = 30f;
		public bool UseTessellation = true;
	}
}

