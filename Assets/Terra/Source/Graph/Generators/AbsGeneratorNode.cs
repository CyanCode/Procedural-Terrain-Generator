using Terra.CoherentNoise;
using Terra.Structures;
using Terra.Terrain;
using UnityEngine;
using XNode;

namespace Terra.Graph.Generators {
	/// <summary>
	/// This class represents a node that outputs a Generator
	/// </summary>
	public abstract class AbsGeneratorNode: PreviewableNode {
		[Output] public AbsGeneratorNode Output;

		public override object GetValue(NodePort port) {
			return this;
		}

		public override Texture2D DidRequestTextureUpdate() {
			int s = PreviewTextureSize;
			Texture2D tex = new Texture2D(s, s);

			Generator g = GetGenerator();
			if (g == null) {
				return tex;
			}

			GeneratorSampler sampler = new GeneratorSampler(g);

			//Retrieve values
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;
			float[,] values = new float[s, s];

			for (int x = 0; x < s; x++) {
				for (int y = 0; y < s; y++) {
					float val = sampler.GetValue(x, y, GridPosition.Zero, s, s, 1);

					if (val < min) {
						min = val;
					}
					if (val > max) {
						max = val;
					}

					values[x, y] = val;
				}
			}

			//Normalize values and set texture
			for (int x = 0; x < s; x++) {
				for (int y = 0; y < s; y++) {
					float val = values[x, y];
					float norm = (val - min) / (max - min);

					tex.SetPixel(x, y, new Color(norm, norm, norm, 1f));
				}
			}

			tex.Apply();
			return tex;
		}

		/// <summary>
		/// Called when a value has changed in the graph
		/// </summary>
		public void OnValueChange() {
			
		}

		/// <summary>
		/// Convenience method for checking whether all of the 
		/// provided generator nodes have an accessible generator 
		/// attached to them.
		/// </summary>
		/// <param name="g1">Array of generator nodes to check</param>
		/// <returns>true if generators can be accessed in all nodes</returns>
		internal static bool HasAllGenerators(params AbsGeneratorNode[] gens) {
			foreach (AbsGeneratorNode agn in gens) {
				if (agn == null || agn.GetGenerator() == null) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Get the generator associated with this node. Is 
		/// Used to compute the final generator passed to the end 
		/// node and preview nodes.
		/// </summary>
		/// <returns>Generator</returns>
		public abstract Generator GetGenerator();

		/// <summary>
		/// Gets the name of this node to display in the 
		/// title.
		/// </summary>
		/// <returns>Title in string form</returns>
		public abstract string GetTitle();
	}
}
 