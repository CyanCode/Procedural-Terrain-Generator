using System;
using UnityEngine;
using System.Collections.Generic;
using Terra;
using Terra.Structures;
using Terra.Util;

[Serializable]
public class DetailSampler {
    [SerializeField]
    private DetailData _placeable;
    [SerializeField]
    private int _seed;

    /// <summary>
    /// Initialize a new DetailSampler
    /// </summary>
    /// <param name="detailData">Placeable object to reference</param>
    /// <param name="seed">Optional seed to use. Default is seed in TerraConfig</param>
    public DetailSampler(DetailData detailData, int? seed = null) {
        _placeable = detailData;
        _seed = seed == null ? TerraConfig.Instance.Seed : seed.Value;
    }

    /// <summary>
    /// Creates a list of positions in the range of [0, 1] by 
    /// running the poisson disc sampling algorithm.
    /// </summary>
    /// <param name="density">Density of the placement of objects</param>
    /// <param name="gridSize">Size of the grid to sample</param>
    public Vector2[] GetPoissonGridSamples(float density, int gridSize) {
        PoissonDiscSampler pds = new PoissonDiscSampler(gridSize, gridSize, density, _seed);
        List<Vector2> total = new List<Vector2>();

        foreach (Vector2 sample in pds.Samples()) {
            total.Add(sample / gridSize);
        }

        return total.ToArray();
    }

    /// <inheritdoc cref="GetPoissonGridSamples(float,int)"/>
    public Vector2[] GetPoissonGridSamples(int gridSize = 100) {
        return GetPoissonGridSamples(_placeable.Spread, gridSize);
    }
}
