using System;
using UnityEngine;

namespace Terra.Structure {
	[Serializable]
	public class GrassData {
		public bool PlaceGrass = false;
		public float GrassStepLength = 1.5f;
		public float GrassVariation = 0.8f;
		public float GrassHeight = 1.5f;
		public float BillboardDistance = 75f;
		public float ClipCutoff = 0.25f;
		public bool GrassConstrainHeight = false;
		public float GrassMinHeight = 0f;
		public float GrassMaxHeight = 200f;
		public bool GrassConstrainAngle = false;
		public float GrassAngleMin = 0f;
		public float GrassAngleMax = 25f;
		public Texture2D GrassTexture = null;
	}

}
