using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Terra.Structures {
	[Serializable]
	public struct MinMaxResult {
		public float Min;
		public float Max;

		public MinMaxResult(float min, float max) {
			Min = min;
			Max = max;
		}

        public MinMaxResult Clamp(MinMaxResult to) {
            MinMaxResult from = new MinMaxResult(Min, Max);

            if (from.Min < to.Min) {
                from.Min = to.Min;
            }
            if (from.Max > to.Max) {
                from.Max = to.Max;
            }

            return from;
        }

        public Constraint ToConstraint() {
            return new Constraint(Min, Max);
        }

        public override string ToString() {
            return "Min: " + Min + " Max: " + Max;
        }
    }

    /// <summary>
    /// Used for tracking min/max values
    /// </summary>
    public class MinMaxRecorder {
        private float _min = float.PositiveInfinity;
        private float _max = float.NegativeInfinity;

        /// <summary>
        /// Sets the internal min and/or max values to the passed value 
        /// if the passed value &lt; internal min and/or the passed 
        /// value &gt; internal max.
        /// </summary>
        public void Register(float value) {
            if (value < _min) {
                _min = value;
            }
            if (value > _max) {
                _max = value;
            }
        }

        /// <returns>Returns the set min/max values after calls to <see cref="Register"/></returns>
        public MinMaxResult GetMinMax() {
            return new MinMaxResult(_min, _max);
        }

        /// <summary>
        /// Resets the internally tracked min/max values 
        /// </summary>
        public void Reset() {
            _min = float.PositiveInfinity;
            _max = float.NegativeInfinity;
        }
    }
    
	public static class MathUtil {
	    /// <summary>
	    /// Converts normalized coordinates to world coordinates
	    /// </summary>
	    /// <param path="gp">Current grid position of this Tile</param>
	    /// <param path="normal">Normalized coordinates</param>
	    /// <returns>World coordinates</returns>
	    public static Vector2 NormalToWorld(GridPosition gp, Vector2 normal) {
	        int length = TerraConfig.Instance.Generator.Length;

	        float wx = (normal.x * length) + (gp.X * length);
	        float wy = (normal.y * length) + (gp.Z * length);

	        return new Vector2(wx, wy);
	    }

        public static float Map(float value, float minOld, float maxOld, float minNew, float maxNew) {
			return minNew + (value - minOld) * (maxNew - minNew) / (maxOld - minOld);
		}

		public static float Map01(float value, float minOld, float maxOld) {
			return (value - minOld) / (maxOld - minOld);
		}

		public static MinMaxResult GetMinMax(float[] values) {
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			foreach (float val in values) {
				if (val < min) {
					min = val;
				}
				if (val > max) {
					max = val;
				}
			}

			return new MinMaxResult(min, max);
		}

		public static MinMaxResult MinMax(float[,] values) {
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			Loop(values, (x, y) => {
				float val = values[x, y];

				if (val < min) {
					min = val;
				}
				if (val > max) {
					max = val;
				}
			});

			return new MinMaxResult(min, max);
		}

		public static MinMaxResult MinMax(float[,,] values) {
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			Loop(values, (x, y, z) => {
				float val = values[x, y, z];

				if (val < min) {
					min = val;
				}
				if (val > max) {
					max = val;
				}
			});

			return new MinMaxResult(min, max);
		}

		public static float[,] Map(float[,] values, float minOld, float maxOld, float minNew, float maxNew) {
			Loop(values, (x, y) => {
				values[x, y] = Map(values[x, y], minOld, maxOld, minNew, maxNew);
			});

			return values;
		}

		public static float[,,] Map(float[,,] values, float minOld, float maxOld, float minNew, float maxNew) {
			Loop(values, (x, y, z) => {
				values[x, y, z] = Map(values[x, y, z], minOld, maxOld, minNew, maxNew);
			});

			return values;
		}

		public static float[,] Map01(float[,] values, float minOld, float maxOld) {
			Loop(values, (x, y) => {
				values[x, y] = Map01(values[x, y], minOld, maxOld);
			});

			return values;
		}

		public static float[,,] Map01(float[,,] values, float minOld, float maxOld) {
			Loop(values, (x, y, z) => {
				values[x, y, z] = Map01(values[x, y, z], minOld, maxOld);
			});

			return values;
		}

		public static void WriteDebugTexture(float[,] values, string path) {
			Texture2D tex = new Texture2D(values.GetLength(0), values.GetLength(1));

			Loop(values, (x, y) => {
				tex.SetPixel(x, y, Color.Lerp(Color.red, Color.green, values[x, y]));
			});

			WriteTexture(tex, path);
		}

		public static void WriteDebugTexture(float[,,] values, string path) {
			Texture2D tex = new Texture2D(values.GetLength(0), values.GetLength(1));
			int zlen = values.GetLength(2);

			for (int x = 0; x < values.GetLength(0); x++) {
				for (int y = 0; y < values.GetLength(1); y++) {
					Color pixel = new Color();	
					
					if (zlen == 1) {
						pixel.r = 1;
					} 
					if (zlen == 2) {
						pixel.r = values[x, y, 0];
						pixel.g = values[x, y, 1];
					}
					if (zlen == 3) {
						pixel.r = values[x, y, 0];
						pixel.g = values[x, y, 1];
						pixel.b = values[x, y, 2];
					}

					tex.SetPixel(x, y, pixel);
				}
			}

			WriteTexture(tex, path);
		}

		public static void WriteMap(float[,,] values, string path) {
			StringBuilder sb = new StringBuilder();
			for (int x = 0; x < values.GetLength(0); x++) {
				sb.Append("[" + x + "]");
				for (int y = 0; y < values.GetLength(1); y++) {
					sb.Append("[");
					for (int z = 0; z < values.GetLength(2); z++) {
						if (z == values.GetLength(2) - 1) {
							sb.Append(values[x, y, z]);
							continue;
						}
			
						sb.Append(values[x,y,z] + ", ");
					}
					sb.Append("]");
				}
			
				sb.Append("\r\n");
			}
			
			File.WriteAllText(path, sb.ToString());
		}

		private static void WriteTexture(Texture2D tex, string path) {
			byte[] encoded = tex.EncodeToJPG(100);
			File.WriteAllBytes(path, encoded);
		}

		private static void Loop(float[,] values, Action<int, int> operation) {
			for (int x = 0; x < values.GetLength(0); x++) {
				for (int y = 0; y < values.GetLength(1); y++) {
					operation(x, y);
				}
			}
		}

		private static void Loop(float[,,] values, Action<int, int, int> operation) {
			for (int x = 0; x < values.GetLength(0); x++) {
				for (int y = 0; y < values.GetLength(1); y++) {
					for (int z = 0; z < values.GetLength(2); z++) {
						operation(x, y, z);
					}
				}
			}
		}
	}
}
