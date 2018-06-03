using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Threading;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.CoherentNoise.Generation.Combination;
using Terra.CoherentNoise;
using Terra.Terrain.Util;
using System.Collections.Generic;

public class GenerationTests {
	[Test]
	public void MultithreadedGeneratorEquality() {
		float[] threadHeights = new float[126 * 126];
		float[] mainHeights = new float[126 * 126];

		Thread t = new Thread(() => {
			Generator gen = GetTestGenerator();

			//Simulate 126 x 126 terrain
			for (int x = 0; x < 126; x++) {
				for (int y = 0; y < 126; y++) {
					threadHeights[x + y * 126] = gen.GetValue(x, y, 0);
				}
			}
		});
		t.Start();

		Generator gen2 = GetTestGenerator();

		//Simulate 126 x 126 terrain
		for (int x = 0; x < 126; x++) {
			for (int y = 0; y < 126; y++) {
				mainHeights[x + y * 126] = gen2.GetValue(x, y, 0);
			}
		}

		//Wait for t to finish
		t.Join();

		float avgDifference = 0f;
		for (int i = 0; i < threadHeights.Length; i++) {
			float v1 = threadHeights[i];
			float v2 = mainHeights[i];
			avgDifference += Mathf.Abs(v1 - v2);
		}
		avgDifference /= 126 * 126;

		Assert.Less(avgDifference, 0.001f);
	}

	[Test]
	public void NewellTest() {
		//Generate mesh verts and tris
		const int res = 126;
		Vector3[] vertices = new Vector3[res * res];
		Generator generator = GetTestGenerator();

		for (int x = 0; x < res; x++) {
			for (int z = 0; z < res; z++) {	
				vertices[x + z * res] = new Vector3(x, generator.GetValue(x, z, 0), z);
			}
		}

		Vector2[] uvs = new Vector2[vertices.Length];
		for (int v = 0; v < res; v++) {
			for (int u = 0; u < res; u++) {
				uvs[u + v * res] = new Vector2((float)u / (res - 1), (float)v / (res - 1));
			}
		}

		int nbFaces = (res - 1) * (res - 1);
		int[] triangles = new int[nbFaces * 6];
		int t = 0;
		for (int face = 0; face < nbFaces; face++) {
			int i = face % (res - 1) + (face / (res - 1) * res);

			triangles[t++] = i + res;
			triangles[t++] = i + 1;
			triangles[t++] = i;

			triangles[t++] = i + res;
			triangles[t++] = i + res + 1;
			triangles[t++] = i + 1;
		}

		//Calculate normals using newell method
		Vector3[] normals = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) {
			Vector3 normal = Vector3.zero;

			for (int j = 0; j < 3; j++) {
				Vector3 currVec = vertices[triangles[(i * 3) + j]];
				Vector3 nextVec = vertices[triangles[(i * 3) + ((j + 1) % 3)]];

				normal.x = normal.x + ((currVec.y - nextVec.y) * (currVec.z + nextVec.z));
				normal.y = normal.y + ((currVec.z - nextVec.z) * (currVec.x + nextVec.x));
				normal.z = normal.z + ((currVec.x - nextVec.x) * (currVec.y + nextVec.y));
			}

			normals[i] = normal.normalized;
		}

		Debug.Log("Newell " + normals[0]);
		Debug.Log("Newell " + normals[1]);

		//Calculate using cross

		//Calculate using mesh
		Mesh m = new Mesh();
		m.vertices = vertices;
		m.uv = uvs;
		m.triangles = triangles;
		m.RecalculateNormals();

		Debug.Log("Mesh " + m.normals[0].normalized);
		Debug.Log("Mesh " + m.normals[1].normalized);
	}

	[Test]
	public void PoissonGridTest() {
		Debug.Log("Calculate time of 500 grid samples");

		const int sampleCount = 50;

		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start();
		for (int i = 0; i < sampleCount; i++) {
			PoissonDiscSampler pds = new PoissonDiscSampler(20, 20, 1);
			List<Vector2> total = new List<Vector2>();

			foreach (Vector2 sample in pds.Samples()) {
				total.Add(sample);
			}

			Debug.Log(total.Count + " grid verts");
		}

		sw.Stop();
		long time = sw.ElapsedMilliseconds;

		Debug.Log("Elapsed time: " + time + " ms. " + time / 1000 + "s.");
		Debug.Log("Approx " + ((float)(time / 1000)) / sampleCount + " seconds");
	}

	private Generator GetTestGenerator() {
		RidgeNoise rn = new RidgeNoise(1337);
		rn.Frequency = 2;
		PinkNoise n = new PinkNoise(234);

		Add added = new Add(rn, n);

		BillowNoise bn = new BillowNoise(534);
		return bn - added;
	}
}
