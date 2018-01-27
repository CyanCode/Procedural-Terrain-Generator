using UnityEngine;

namespace Terra.GraphEditor {
	public static class Log {
		public static void Info(string info) {
			if (BonConfig.LogLevel > 0) {
				Debug.Log(info);
			}
		}
	}
}