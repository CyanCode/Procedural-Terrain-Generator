using UnityEngine;
using System.Collections.Generic;
using Terra.Structures;
using Terra.Util;

public class ObjectSampler {
    private PlaceableObject _placeable;

    public ObjectSampler(PlaceableObject placeablePlaceable) {
        _placeable = placeablePlaceable;
    }

    public Vector2[] GetPoissonGridSamples(float density, int gridSize) {
        PoissonDiscSampler pds = new PoissonDiscSampler(gridSize, gridSize, density);
        List<Vector2> total = new List<Vector2>();

        foreach (Vector2 sample in pds.Samples()) {
            total.Add(sample);
        }

        return total.ToArray();
    }

    public Vector2[] GetPoissonGridSamples(int gridSize = 100) {
        return GetPoissonGridSamples(_placeable.Spread, gridSize);
    }
}
