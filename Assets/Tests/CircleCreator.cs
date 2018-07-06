using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleCreator: MonoBehaviour {
	public List<Vector3> Grid = null;

	public int MaxPoints = 100;
	public int Radius = 10;
	public float Factor = 1;
	public float VariationAmount = 0.3f;
	public bool RandomVariation = true;
	public int TaperStart = 5;
	public int TaperEnd = 10;

	void OnDrawGizmos() {
		UnityEngine.Random.InitState(1337);
		
		if (Grid != null) {
			int size = Grid.Count > MaxPoints ? MaxPoints : Grid.Count;
		
			for (int i = 0; i < size; i++) {
				float varX = transform.position.x +
					UnityEngine.Random.Range(-VariationAmount, VariationAmount);
				float varZ = transform.position.z +
					UnityEngine.Random.Range(-VariationAmount, VariationAmount);

				Gizmos.DrawSphere(Grid[i] + new Vector3(varX, 0, varZ), 0.5f);
			}
		}
	}

	public List<Vector3> CalculateGridPoints(int radius) {
		if (Grid == null)
			Grid = new List<Vector3>();
		Grid.Clear();

		float x0 = 0; 
		float z0 = 0;

		if (RandomVariation) {
			x0 += UnityEngine.Random.Range(-0.25f, 0.25f);
			z0 += UnityEngine.Random.Range(-0.25f, 0.25f);
		}

		int x = radius - 1;
		int y = 0;
		int dx = 1;
		int dy = 1;
		int err = dx - (radius << 1);

		while (x >= y) {
			Grid.Add(new Vector3(x0 + x, z0 + y));
			Grid.Add(new Vector3(x0 + y, z0 + x));
			Grid.Add(new Vector3(x0 - y, z0 + x));
			Grid.Add(new Vector3(x0 - x, z0 + y));
			Grid.Add(new Vector3(x0 - x, z0 - y));
			Grid.Add(new Vector3(x0 - y, z0 - x));
			Grid.Add(new Vector3(x0 + y, z0 - x));
			Grid.Add(new Vector3(x0 + x, z0 - y));

			if (err <= 0) {
				y++;
				err += dy;
				dy += 2;
			}

			if (err > 0) {
				x--;
				dx += 2;
				err += dx - (radius << 1);
			}
		}

		return Grid;
	}

	public List<Vector3> CalculateCircleFloat(float stepSize, float radius) {
		List<Vector3> positions = new List<Vector3>();

		float x, y;
		float angle = 0;
		stepSize = stepSize / (2 * Mathf.PI * radius);

		while (angle < (2 * Math.PI) - stepSize) {
			x = radius * Mathf.Cos(angle);
			y = radius * Mathf.Sin(angle);

			positions.Add(new Vector3(x, 0, y));
			angle += stepSize;
		}

		return positions;
	}

	public void CalculateGridPointsDecreasing() {
		List<Vector3> points = new List<Vector3>();

		//Fill in inner circle first
		for (int i = TaperStart; i > 1; i -= (int)Factor) {
			points.AddRange(CalculateGridPoints(i));
		}

		//Fill in outer tapering region
		const float spreadAmt = 1.2f;
		int distance = 1; 
		for (int i = TaperStart; i < TaperEnd - Factor; i += (int)Factor) {
			points.AddRange(CalculateGridPoints(i + distance));

			distance += (int) Mathf.Pow(distance, spreadAmt);
		}

		Grid = points;
	}

	/// <summary>
	/// Calculates grid points in a circle from a
	/// </summary>
	public void CalculateCircleFromSquare() {

	}

	public void CalculateGridPointsFloat() {
		var positions = new List<Vector3>();
	
		for (float i = 1; i < Radius; i += 2) {
			positions.AddRange(CalculateCircleFloat(Factor, i));
		}

		Grid = positions;
	}
}
