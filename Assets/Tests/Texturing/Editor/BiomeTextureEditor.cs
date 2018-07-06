using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PixelsForGlory.ComputationalSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BiomeTexture))]
public class BiomeTextureEditor: Editor {
	public override void OnInspectorGUI() {
		var target = (BiomeTexture)base.target;

		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		//target.Seed = EditorGUILayout.IntField("Seed", target.Seed);
		//target.BiomeCount = EditorGUILayout.IntField("Biome Count", target.BiomeCount);
		if (EditorGUI.EndChangeCheck()) {
			//CalculateTexture();
			CalculateVoronoi();
		}

		var ctr = EditorGUILayout.GetControlRect(false, target.ImageSize);
		ctr.width = target.ImageSize;
		if (target.Texture) {
			EditorGUI.DrawPreviewTexture(ctr, target.Texture);
			ctr.x += ctr.width;
			EditorGUI.DrawPreviewTexture(ctr, target.NoiseTexture);
		}
	}

	private void CalculateTexture() {
		var target = (BiomeTexture)base.target;
		int s = target.ImageSize;

		if (target.BiomeCount < 1) return;
		float dist = 1 / (float)target.BiomeCount;

		//Assign colors
		Random.InitState(target.Seed);
		Color[] colors = new Color[target.BiomeCount];
		for (int i = 0; i < colors.Length; i++) {
			colors[i] = Random.ColorHSV();
		}

		Texture2D t = new Texture2D(s, s);
		Texture2D nt = new Texture2D(s, s);

		List<VoronoiDiagramSite<Color>> points = new List<VoronoiDiagramSite<Color>>();
		VoronoiDiagram<Color> vd = new VoronoiDiagram<Color>(new Rect(0, 0, s, s));
		while (points.Count < target.PointCount) {
			int randX = Random.Range(0, s - 1);
			int randY = Random.Range(0, s - 1);

			var point = new Vector2(randX, randY);
			if (points.All(item => item.Coordinate != point)) {
				points.Add(new VoronoiDiagramSite<Color>(point, new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))));
			}
		}

		vd.AddSites(points);
		vd.GenerateSites(2);
		Color[,] vals = vd.Get2DSampleArray();


		for (int i = 0; i < s; i++) {
			for (int j = 0; j < s; j++) {
				t.SetPixel(i, j, vals[i, j]);

				//Choose biome from cutoff
				int bc = 0;
				//for (float b = 0; b < 1f; b += dist) {
				//	//float perlin = Mathf.PerlinNoise(i * target.Frequency * target.Displacement, j * target.Frequency * target.Displacement);
				//	//float noise = fn.GetCellular(i * target.Frequency, j * target.Frequency);
				//	//float noise = fn.GetValueFractal(i * target.Frequency, j * target.Frequency);
				//	//float noise = Mathf.PerlinNoise(i * target.Frequency, j * target.Frequency);


				//	//normalize values [-1, 1] -> [0, 1]
				//	//noise = (noise + 1) / 2;

				//	//Value falls within biome range, color
				//	if (b < noise && noise < b + dist) {
				//		t.SetPixel(i, j, MixColorsLinear(colors, bc, noise));
				//		nt.SetPixel(i, j, ColorBW(noise));
				//	}
				//	bc++;
				//}
			}
		}

		t.Apply();
		nt.Apply();

		target.Texture = t;
		target.NoiseTexture = nt;
	}

	private void CalculateVoronoi() {
		var target = (BiomeTexture)base.target;
		int s = target.ImageSize;

		if (target.BiomeCount < 1) return;
		float dist = 1 / (float)target.BiomeCount;

		Texture2D t = new Texture2D(s, s);
		List<VoronoiDiagramSite<Color>> points = new List<VoronoiDiagramSite<Color>>();
		VoronoiDiagram<Color> vd = new VoronoiDiagram<Color>(new Rect(0, 0, s, s));

		while (points.Count < target.PointCount) {
			int randX = Random.Range(0, s - 1);
			int randY = Random.Range(0, s - 1);

			var point = new Vector2(randX, randY);
			if (points.All(item => item.Coordinate != point)) {
				points.Add(new VoronoiDiagramSite<Color>(point, new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))));
			}
		}

		vd.AddSites(points);
		vd.GenerateSites(2);

		//		foreach (var vert in vd.GeneratedSites[0].Vertices) {
		//			Debug.Log(vert);
		//		}

		Color[] colors = vd.Get1DSampleArrayBlended(target.Displacement);
		t.SetPixels(colors);

		t.Apply();
		target.Texture = t;
	}

	private Color MixColorsLinear(Color[] colors, int index, float noiseVal) {
		float mixAmt = ((BiomeTexture)target).BlendSize;

		//Convert range which contains all biomes to 
		//range which only contains the biome at index.
		//[--b1--b2--b3--] -> [--b2--]
		float dist = 1 / (float)colors.Length;
		float min = dist * index; float max = min + dist;
		float v = (noiseVal - min) / (max - min);

		//Can mix left or right if v falls within 
		//triangle on extents of number line
		bool mixLeft = v <= mixAmt && index > 0;
		bool mixRight = v >= 1 - mixAmt && index < colors.Length - 1;
		if (!mixRight && !mixLeft) {
			return colors[index];
		}

		//If mixing left, invert v to compute positive 
		//linear equation
		v = mixLeft ? 1 - v : v;

		//Convert range again to fit within linear equation
		min = 1 - mixAmt; max = 1;
		float vp = (v - min) / max - min;

		//Linear equation: y = mx where m = (b/a)
		const float b = 0.5f;
		float m = b / mixAmt;
		float height = m * vp;

		Color neighbor = mixLeft ? colors[index - 1] : colors[index + 1];
		return (neighbor * (1 - height)) + (colors[index] * height);
	}

	/// <summary>
	/// Accepts a noise value from 0 to 1
	/// </summary>
	private Color ColorBW(float n) {
		return new Color(n, n, n, 1f);
	}
}

public static class VoronoiDiagramExtensions {
	/// <summary>
	/// Works similarly to <see cref="VoronoiDiagram{T}.Get1DSampleArray"/> except 
	/// values close to edges are blended with neighboring cell values
	/// </summary>
	public static Color[] Get1DSampleArrayBlended(this VoronoiDiagram<Color> vd, float transitionDistance) {
		var returnData = new Color[(int)vd.Bounds.width * (int)vd.Bounds.height];

		for (int i = 0; i < returnData.Length; i++) {
			returnData[i] = default(Color);
		}

		foreach (KeyValuePair<int, VoronoiDiagramGeneratedSite<Color>> site in vd.GeneratedSites) {
			if (site.Value.Vertices.Count == 0) {
				continue;
			}

			Vector2 minimumVertex = site.Value.Vertices[0];
			Vector2 maximumVertex = site.Value.Vertices[0];

			for (int i = 1; i < site.Value.Vertices.Count; i++) {
				if (site.Value.Vertices[i].x < minimumVertex.x) {
					minimumVertex.x = site.Value.Vertices[i].x;
				}

				if (site.Value.Vertices[i].y < minimumVertex.y) {
					minimumVertex.y = site.Value.Vertices[i].y;
				}

				if (site.Value.Vertices[i].x > maximumVertex.x) {
					maximumVertex.x = site.Value.Vertices[i].x;
				}

				if (site.Value.Vertices[i].y > maximumVertex.y) {
					maximumVertex.y = site.Value.Vertices[i].y;
				}
			}

			if (minimumVertex.x < 0.0f) {
				minimumVertex.x = 0.0f;
			}

			if (minimumVertex.y < 0.0f) {
				minimumVertex.y = 0.0f;
			}

			if (maximumVertex.x > vd.Bounds.width) {
				maximumVertex.x = vd.Bounds.width;
			}

			if (maximumVertex.y > vd.Bounds.height) {
				maximumVertex.y = vd.Bounds.height;
			}

			for (int x = (int)minimumVertex.x; x <= maximumVertex.x; x++) {
				for (int y = (int)minimumVertex.y; y <= maximumVertex.y; y++) {
					Vector2 vertex = new Vector2(x, y);

					if (VoronoiDiagram<Color>.PointInVertices(vertex, site.Value.Vertices)) {
						if (vd.Bounds.Contains(vertex)) {
							//Find closest neighbor via edge checking
							Color neighborColor = default(Color);
							Vector2 nCenter = default(Vector2);
							float minDist = float.MaxValue;

							foreach (var edge in site.Value.Edges) {
								float distance = GetPerpendicularLength(vertex, edge);

								if (distance < minDist) {
									minDist = distance;
									
									//Check which neighbor shares this edge
									foreach (int neighborIdx in site.Value.NeighborSites) {
										var neighbor = vd.GeneratedSites[neighborIdx];

										//If this neighbor has an edge with a matching index
										if (neighbor != site.Value && neighbor.Edges.Exists(vde => vde.Index == edge.Index)) {
											neighborColor = neighbor.SiteData;
											nCenter = neighbor.Centroid;
											break;
										}
									}
								}
							}

							Debug.Log("Site color " + site.Value.SiteData + 
										" | neighbor color " + neighborColor + 
										" | Site XY " + x + " " + y + 
										" | Neighbor Center XY " + nCenter);

							//Mix colors from distance
							Color c = site.Value.SiteData;
							if (minDist < transitionDistance && neighborColor != default(Color)) {
								c = Color.Lerp(neighborColor, site.Value.SiteData, minDist / transitionDistance);
							}

							int index = x + y * (int)vd.Bounds.width;
							returnData[index] = c;
						}
					}
				}
			}
		}

		return returnData;
	}

	/// <summary>
	/// Finds the formed perpendicular line from the passed point 
	/// to the passed diagram edge and returns its length.
	/// </summary>
	/// <returns></returns>
	private static float GetPerpendicularLength(Vector2 position, VoronoiDiagramGeneratedEdge edge) {
		var a = edge.LeftEndPoint;
		var b = edge.RightEndPoint;
		var c = position;

		float t = ((c.x - a.x) * (b.x - a.x) + (c.y - a.y) * (b.y - a.y)) /
					(Mathf.Pow(b.x - a.x, 2f) + Mathf.Pow(b.y - a.y, 2f));
		var dist = new Vector2(a.x + (t * (b.x - a.x)), a.y + (t * (b.y - a.y)));

		return dist.magnitude;
	}
}