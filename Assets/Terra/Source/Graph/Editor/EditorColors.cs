using UnityEngine;

namespace Terra.Graph {
	internal static class EditorColors {
		public static Color TintBiome = ByteToFloatColor(0, 100, 0);        //green
		public static Color TintNoise = ByteToFloatColor(0, 98, 128);        //red
		public static Color TintModifier = ByteToFloatColor(128, 49, 176);  //purple
		public static Color TintValue = ByteToFloatColor(128, 0, 0);       //pink
		public static Color TintEnd = ByteToFloatColor(137, 51, 11);        //orange

		private static Color ByteToFloatColor(int r, int g, int b, int a = 1) {
			return new Color(r / 255f, g / 255f, b / 255f, a);
		}
	}
}