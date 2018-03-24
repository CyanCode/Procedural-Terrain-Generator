using UnityEngine;
using NUnit.Framework;
using Terra.Terrain;
using System.Collections.Generic;

public class ObjectPlacementTest {
	private TerraSettings Settings;

	[SetUp]
	public void Setup() {
		Settings = Object.FindObjectOfType<TerraSettings>();
		Assert.IsNotNull(Settings);
	}

	[Test]
	public void ObjectPlacementTestSimplePasses() {
		ObjectPlacer op = Settings.Placer;
		List<Vector2> grid = op.GetPoissonGrid(Settings.ObjectPlacementSettings[0]);
		Debug.Log(grid.Count);
	}
}
