using Terra.CoherentNoise;
using System;
using UnityEngine;
using UNEB;
using System.Collections.Generic;

namespace Terra.Nodes.Generation {
	public abstract class AbstractGeneratorNode: Node {
		public override void Init() {
			base.Init();

			NodeOutput output = AddOutput("Output", true);
			output.getValue = GetGenerator;
			FitKnobs();

			name = GetName();
			bodyRect.height += 20f;
		}

		public override void OnNewInputConnection(NodeInput addedInput) {
			base.OnNewInputConnection(addedInput);
			
			if (addedInput.ParentNode != null && addedInput.ParentNode is NoisePreviewNode) {
				((NoisePreviewNode) addedInput.ParentNode).TextureNeedsUpdating = true;
			}
		}

		/// <summary>
		/// This can be called whenever a value changes in 
		/// one of the nodes. By default this will make any 
		/// NoisePreviewNode spanning off of <c>this</c> Node 
		/// update its texture.
		/// </summary>
		public void NotifyValueChange() {
			/**
			 * Perform a depth first search for any NoisePreviewNode
			 * that spans off of this Node
			 */
			var dfs = new Stack<Node>();

			dfs.Push(this);

			while (dfs.Count != 0) {

				var node = dfs.Pop();

				// Search neighbors
				foreach (var output in node.Outputs) {
					foreach (var input in output.Inputs) {
						dfs.Push(input.ParentNode);
					}
				}

				var outputNode = node as NoisePreviewNode;
				if (outputNode != null) {
					outputNode.TextureNeedsUpdating = true;
				}
			}
		}

		/// <summary>
		/// Get the generator associated with this node. Is 
		/// Used to compute the final generator passed to the end 
		/// node and preview nodes.
		/// </summary>
		/// <returns>Generator</returns>
		public abstract Generator GetGenerator();
	}
}
 