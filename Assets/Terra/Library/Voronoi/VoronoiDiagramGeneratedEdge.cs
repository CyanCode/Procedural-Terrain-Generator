// Copyright 2016 afuzzyllama. All Rights Reserved.
using UnityEngine;

namespace PixelsForGlory.ComputationalSystem
{
    /// <summary>
    /// Stores final information about an edge that is returned in GenerateEdges
    /// </summary>
    public class VoronoiDiagramGeneratedEdge
    {
        public int Index;
        public Vector2 LeftEndPoint;
        public Vector2 RightEndPoint;

        public VoronoiDiagramGeneratedEdge(int index, Vector2 leftEndPoint, Vector2 rightEndPoint)
        {
            Index = index;
            LeftEndPoint = leftEndPoint;
            RightEndPoint = rightEndPoint;
        }
    }
}
