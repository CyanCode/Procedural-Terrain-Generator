using System.Collections.Generic;
using XNodeEditor;

namespace Terra.Graph {
	public abstract class PreviewableNodeEditor: NodeEditor {
		public abstract void ShouldShowPreviewGenerator();

		public void OnBodyGUI(bool showPreview) {
			OnBodyGUI(showPreview, new string[0]);
		}

		public void OnBodyGUI(bool showPreview, string[] excludes) {
			List<string> allExclude = new List<string>();
			allExclude.AddRange(excludes);
			allExclude.AddRange(PreviewableNode.FieldNames);

			OnBodyGUI(allExclude.ToArray());
			
			if (showPreview) {
				ShouldShowPreviewGenerator();
			}
		}
	}
}