using UnityEngine;

namespace Terra.Graph {
	internal static class EditorColors {
		public static Color TintBiome = C(214, 255, 255);    //green
		public static Color TintNoise = C(207, 208, 254);    //purple
		public static Color TintModifier = C(255, 208, 242); //pink
		public static Color TintValue = C(254, 173, 206);    //red
		public static Color TintEnd = C(254, 255, 213);      //yellow

		private static Color C(int r, int g, int b, int a = 1) {
			return new Color(r / 255f, g / 255f, b / 255f, a);
		}
	}
}