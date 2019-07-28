using Terra.CoherentNoise;
using UnityEngine;
using UnityEditor;

namespace Terra.Graph.Generators.Modifiers {
	[CreateNodeMenu(MENU_PARENT_NAME + "Round")]
	public class InvertNode: AbsModNode {
		public override Generator GetGenerator() {
			throw new System.NotImplementedException();
		}

		public override string GetTitle() {
			return "Invert";
		}
	}
}