// Copyright 2016 afuzzyllama. All Rights Reserved.
using UnityEngine;
using System.Collections.Generic;
using PixelsForGlory.Extensions;

namespace PixelsForGlory.ComputationalSystem
{
    /// <summary>
    /// Represents a Voronoi Diagram Site.  Must be instantiated as a pointer.
    /// </summary>
    public class VoronoiDiagramSite<T> where T : new()
    {
        public int Index;
        public Vector2 Coordinate;
        public Vector2 Centroid;
        public List<Vector2> Vertices;
        public bool IsCorner;
        public bool IsEdge;
        public List<VoronoiDiagramEdge<T>> Edges;
        public T SiteData;

        /// <summary>
        /// Creates a site
        /// </summary>
        /// <param name="coordinate">Coordinate of the new site</param>
        public VoronoiDiagramSite(Vector2 coordinate)
        {
            Index = -1;
            Coordinate = coordinate;
            IsCorner = false;
            IsEdge = false;
            Vertices = new List<Vector2>();
            Edges = new List<VoronoiDiagramEdge<T>>();
            SiteData = new T();
        }

        /// <summary>
        /// Creates a site
        /// </summary>
        /// <param name="coordinate">Coordinate of the new site</param>
        /// <param name="siteData">Site data to be passed along with the site</param>
        public VoronoiDiagramSite(Vector2 coordinate, T siteData)
        {
            Index = -1;
            Coordinate = coordinate;
            IsCorner = false;
            IsEdge = false;
            Vertices = new List<Vector2>();
            Edges = new List<VoronoiDiagramEdge<T>>();
            SiteData = siteData;
        }

        /// <summary>
        /// Creates a site
        /// </summary>
        /// <param name="index">Index of the new site</param>
        /// <param name="site">new site</param>
        internal VoronoiDiagramSite(int index, VoronoiDiagramSite<T> site)
        {
            Index = index;
            Coordinate = site.Coordinate;
            IsCorner = false;
            IsEdge = false;
            Vertices = new List<Vector2>();
            Edges = new List<VoronoiDiagramEdge<T>>();
            SiteData = site.SiteData;
        }

        /// <summary>
        /// Gets the distance between the site and the passed in Voronoi Diagram vertex
        /// </summary>
        /// <param name="vertex">Vertex to calculation distance from</param>
        /// <returns></returns>
        public float GetDistanceFrom(VoronoiDiagramVertex<T> vertex)
        {
            float dx, dy;

            dx = Coordinate.x - vertex.Coordinate.x;
            dy = Coordinate.y - vertex.Coordinate.y;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }


        public void GenerateCentroid(Rect bounds)
        {
            var sortedVertices = new List<Vector2>();

            // Gather all vertices from the edges
            // Solve for corners

            bool hasXMin = false;
            bool hasXMax = false;
            bool hasMinY = false;
            bool hasMaxY = false;
            foreach(VoronoiDiagramEdge<T> edge in Edges)
            {
                // Don't add edge that is (0,0) -> (0,0).  Increment Index if no edge is removed, otherwise the remove should do this shifting for us
                if(
                    edge.LeftClippedEndPoint.x.IsAlmostEqualTo(float.MinValue) &&
                    edge.LeftClippedEndPoint.y.IsAlmostEqualTo(float.MinValue) &&
                    edge.RightClippedEndPoint.x.IsAlmostEqualTo(float.MinValue) &&
                    edge.RightClippedEndPoint.y.IsAlmostEqualTo(float.MinValue)
                    )
                {
                    continue;
                }

                Vector2 leftEndPoint = edge.LeftClippedEndPoint;
                Vector2 rightEndPoint = edge.RightClippedEndPoint;

                // (x, Min)
                if(leftEndPoint.y.IsAlmostZero(FloatExtensions.NotReallySmallishNumber) ||
                   rightEndPoint.y.IsAlmostZero(FloatExtensions.NotReallySmallishNumber))
                {
                    hasXMin = true;
                }

                // (x, Max)
                if(leftEndPoint.y.IsAlmostEqualTo(bounds.height, FloatExtensions.NotReallySmallishNumber) ||
                   rightEndPoint.y.IsAlmostEqualTo(bounds.height, FloatExtensions.NotReallySmallishNumber))
                {
                    hasXMax = true;
                }

                // (Min, y)
                if(leftEndPoint.x.IsAlmostZero(FloatExtensions.NotReallySmallishNumber) ||
                   rightEndPoint.x.IsAlmostZero(FloatExtensions.NotReallySmallishNumber))
                {
                    hasMinY = true;
                }

                // (Max, y)
                if(leftEndPoint.x.IsAlmostEqualTo(bounds.width, FloatExtensions.NotReallySmallishNumber) ||
                   rightEndPoint.x.IsAlmostEqualTo(bounds.width, FloatExtensions.NotReallySmallishNumber))
                {
                    hasMaxY = true;
                }

                sortedVertices.Add(leftEndPoint);
                sortedVertices.Add(rightEndPoint);
            }

            // Add corners if applicable
            // (x, Min) -> (Min, y)
            // Min, Min Corner
            if(hasXMin && hasMinY)
            {
                sortedVertices.Add(Vector2.zero);
                IsCorner = true;
            }

            // x, Min -> Max, y
            // Min, Max Corner
            if(hasXMin && hasMaxY)
            {
                sortedVertices.Add(new Vector2(bounds.width, 0f));
                IsCorner = true;
            }

            // x, Max -> Min, y
            // Max, Min Corner
            if(hasXMax && hasMinY)
            {
                sortedVertices.Add(new Vector2(0f, bounds.height));
                IsCorner = true;
            }

            // x, Max -> Max, y
            // Max, Max Corner
            if(hasXMax && hasMaxY)
            {
                sortedVertices.Add(new Vector2(bounds.width, bounds.height));
                IsCorner = true;
            }

            if(hasXMin || hasXMax || hasMinY || hasMaxY)
            {
                IsEdge = true;
            }

            // Monotone Chain
            // Sort the vertices lexigraphically by X and then Y
            sortedVertices.Sort(
                delegate(Vector2 vertexA, Vector2 vertexB)
                {
                    if(vertexA.x < vertexB.x)
                    {
                        return -1;
                    }

                    if(vertexA.x > vertexB.x)
                    {
                        return 1;
                    }

                    if(vertexA.y < vertexB.y)
                    {
                        return -1;
                    }

                    if(vertexA.y > vertexB.y)
                    {
                        return 1;
                    }

                    return 0;
                });

            var lowerHull = new List<Vector2>();
            for(int i = 0; i < sortedVertices.Count; i++)
            {
                while(lowerHull.Count >= 2 &&
                      (Cross(lowerHull[lowerHull.Count - 2], lowerHull[lowerHull.Count - 1], sortedVertices[i]) < 0.0f ||
                       Cross(lowerHull[lowerHull.Count - 2], lowerHull[lowerHull.Count - 1], sortedVertices[i]).IsAlmostZero()))
                {
                    lowerHull.RemoveAt(lowerHull.Count - 1);
                }
                lowerHull.Add(sortedVertices[i]);
            }

            var upperHull = new List<Vector2>();
            for(int i = sortedVertices.Count - 1; i >= 0; i--)
            {
                while(upperHull.Count >= 2 &&
                      (Cross(upperHull[upperHull.Count - 2], upperHull[upperHull.Count - 1], sortedVertices[i]) < 0.0f ||
                       Cross(upperHull[upperHull.Count - 2], upperHull[upperHull.Count - 1], sortedVertices[i]).IsAlmostZero()))
                {
                    upperHull.RemoveAt(upperHull.Count - 1);
                }
                upperHull.Add(sortedVertices[i]);
            }

            // Remove last vertex because they are represented in the other list
            upperHull.RemoveAt(upperHull.Count - 1);
            lowerHull.RemoveAt(lowerHull.Count - 1);

            sortedVertices.Clear();
            sortedVertices.AddRange(lowerHull);
            sortedVertices.AddRange(upperHull);

            // Calculate Centroid
            Centroid = Vector2.zero;
            Vertices.Clear();
            Vertices.AddRange(sortedVertices);

            Vector2 currentVertex;
            Vector2 nextVertex;
            float signedArea = 0.0f;
            float partialArea;

            // Use all vertices except the last one
            for(int index = 0; index < sortedVertices.Count - 1; index++)
            {
                currentVertex = sortedVertices[index];
                nextVertex = sortedVertices[index + 1];

                partialArea = currentVertex.x * nextVertex.y - nextVertex.x * currentVertex.y;
                signedArea += partialArea;

                Centroid = new Vector2(Centroid.x + (currentVertex.x + nextVertex.x) * partialArea,
                    Centroid.y + (currentVertex.y + nextVertex.y) * partialArea);
            }

            // Process last vertex
            currentVertex = sortedVertices[sortedVertices.Count - 1];
            nextVertex = sortedVertices[0];
            partialArea = (currentVertex.x * nextVertex.y - nextVertex.x * currentVertex.y);
            signedArea += partialArea;
            Centroid = 
                new Vector2(
                    Centroid.x + (currentVertex.x + nextVertex.x) * partialArea,
                    Centroid.y + (currentVertex.y + nextVertex.y) * partialArea
                );

            signedArea *= 0.5f;
            Centroid = new Vector2(Centroid.x / (6f * signedArea), Centroid.y / (6f * signedArea));
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b)
        {
            return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
        }
    }
}
