using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Terrain.Util {
	public static class CircleUtil {
		/// <summary>
		/// Calculates a set of Vector2 positions that form a 
		/// grid of points inside a circle.
		/// </summary>
		/// <param name="origin">Where the origin of the circle is</param>
		/// <param name="radius">Radius of the circle</param>
		/// <param name="stepSize">How spread out the points are from each other [0, 2]</param>
		/// <returns></returns>
		public static List<Vector2> GetPointsInside(Vector2 origin, float radius, float stepSize) {
			List<Vector2> points = new List<Vector2>();

			//Start at max circle and decrease inwards
			for (float i = radius; i > 0f; i -= stepSize) {
				var perimeter = GetPointsAround(origin, i, stepSize);
				points.AddRange(perimeter);
			}

			return points;
		}

		public static List<Vector2> GetPointsAround(Vector2 origin, float radius, float stepSize) {
			//Travel distance decreases as radius increases
			float dist = stepSize / radius;
			const float fullRotation = 2 * Mathf.PI;
			List<Vector2> points = new List<Vector2>();

			for (float a = 0; a < fullRotation; a += dist) {
				//x = h + r*cos(t)
				//y = k + r*sin(t)
				float x = origin.x + radius * Mathf.Cos(a);
				float y = origin.y + radius * Mathf.Sin(a);

				points.Add(new Vector2(x, y));
			}

			return points;
		}

		public static List<Vector2> GetPointsFromGrid(Vector2 origin, float radius, float stepSize) {
			List<Vector2> points = new List<Vector2>();
			float xCenter = origin.x;
			float yCenter = origin.y;

			for (float x = xCenter - radius; x <= xCenter; x += stepSize) {
				for (float y = yCenter - radius; y <= yCenter; y += stepSize) {
					if ((x - xCenter) * (x - xCenter) + (y - yCenter) * (y - yCenter) <= radius * radius) {
						float xSym = xCenter - (x - xCenter);
						float ySym = yCenter - (y - yCenter);

						bool onXAxis = x - xCenter == 0;
						bool onYAxis = y - yCenter == 0;
						
						if (onXAxis || onYAxis) {
							if (onXAxis && onYAxis) {
								//No symmetry on both X and Y
								points.Add(new Vector2(x, y));
								continue;
							}
							if (onXAxis) {
								//No symmetry along X axis
								points.Add(new Vector2(x, y));
								points.Add(new Vector2(x, ySym));
							}
							if (onYAxis) {
								//No symmetry along Y axis
								points.Add(new Vector2(x, y));
								points.Add(new Vector2(xSym, y));
							}
						} else {
							// (x, y), (x, ySym), (xSym , y), (xSym, ySym) are in the circle
							points.Add(new Vector2(x, y));
							points.Add(new Vector2(x, ySym));
							points.Add(new Vector2(xSym, y));
							points.Add(new Vector2(xSym, ySym));
						}
					}
				}
			}

			return points;
		}
	}
}