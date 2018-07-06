// Copyright 2016 afuzzyllama. All Rights Reserved.
using System;
using PixelsForGlory.Extensions;
using UnityEngine;

namespace PixelsForGlory.ComputationalSystem
{
    public enum VoronoiDiagramEdgeType
    {
        None,
        Left,
        Right
    }

    /// <summary>
    /// Represents a Voronoi Diagram edge.
    /// </summary>
    public class VoronoiDiagramEdge<T> where T : new()
    {
        public int Index;

        // The equation of the edge: ax + by = c
        public float A;
        public float B;
        public float C;

        public VoronoiDiagramVertex<T> LeftEndPoint;
        public VoronoiDiagramVertex<T> RightEndPoint;

        public Vector2 LeftClippedEndPoint;
        public Vector2 RightClippedEndPoint;

        public VoronoiDiagramSite<T> LeftSite;
        public VoronoiDiagramSite<T> RightSite;

        /// <summary>
        /// Represents a deleted edge.  Any edge that needs to present a deleted edge will point to this.
        /// </summary>
        public static VoronoiDiagramEdge<T> Deleted = new VoronoiDiagramEdge<T>();

        /// <summary>
        /// Return an edge that is the bisection between two sites
        /// </summary>
        /// <param name="siteA">The first site</param>
        /// <param name="siteB">The second site</param>
        /// <returns>The new edge</returns>
        public static VoronoiDiagramEdge<T> Bisect(VoronoiDiagramSite<T> siteA, VoronoiDiagramSite<T> siteB)
        {
            float dx, dy;
            var newEdge = new VoronoiDiagramEdge<T>
            {
                LeftSite = siteA,
                RightSite = siteB,
                LeftEndPoint = null,
                RightEndPoint = null
            };

            dx = siteB.Coordinate.x - siteA.Coordinate.x;
            dy = siteB.Coordinate.y - siteA.Coordinate.y;

            newEdge.C = siteA.Coordinate.x * dx + siteA.Coordinate.y * dy + (dx * dx + dy * dy) * 0.5f;
            if(Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                newEdge.A = 1f;
                newEdge.B = dy / dx;
                newEdge.C /= dx;
            }
            else
            {
                newEdge.B = 1f;
                newEdge.A = dx / dy;
                newEdge.C /= dy;
            }

            siteA.Edges.Add(newEdge);
            siteB.Edges.Add(newEdge);

            return newEdge;
        }

        /// <summary>
        /// Sets and end point for the edge
        /// </summary>
        /// <param name="vertex">The vertex that represents the end point</param>
        /// <param name="edgeType">The edge type of this vertex is (left or right) </param>
        public void SetEndpoint(VoronoiDiagramVertex<T> vertex, VoronoiDiagramEdgeType edgeType)
        {
            if(edgeType == VoronoiDiagramEdgeType.None)
            {
                throw new Exception("edgeType == VoronoiDiagramEdgeType.None");
            }

            if(edgeType == VoronoiDiagramEdgeType.Left)
            {
                LeftEndPoint = vertex;
            }
            else
            {
                RightEndPoint = vertex;
            }
        }

        /// <summary>
        /// After a diagram is completely generated.  This will clean up any end points that are outside of the passed in bounds or that are still null.
        /// </summary>
        /// <param name="bounds">The bounds of the Voronoi Diagram</param>
        public void GenerateClippedEndPoints(Rect bounds)
        {
            var minimumValues = Vector2.zero;
            var maximumValues = new Vector2(bounds.width, bounds.height);

            VoronoiDiagramVertex<T> vertexA, vertexB;
            Vector2 pointA, pointB;

            if(A.IsAlmostEqualTo(1f) && B >= 0f)
            {
                vertexA = RightEndPoint;
                vertexB = LeftEndPoint;
            }
            else
            {
                vertexA = LeftEndPoint;
                vertexB = RightEndPoint;
            }

            if(A.IsAlmostEqualTo(1f))
            {
                pointA.y = minimumValues.y;
                if(vertexA != null && vertexA.Coordinate.y > minimumValues.y)
                {
                    pointA.y = vertexA.Coordinate.y;
                }
                if(pointA.y > maximumValues.y)
                {
                    LeftClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    RightClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    return;
                }
                pointA.x = C - B * pointA.y;

                pointB.y = maximumValues.y;
                if(vertexB != null && vertexB.Coordinate.y < maximumValues.y)
                {
                    pointB.y = vertexB.Coordinate.y;
                }
                if(pointB.y < minimumValues.y)
                {
                    LeftClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    RightClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    return;
                }
                pointB.x = C - B * pointB.y;

                if(
                    (pointA.x > maximumValues.x && pointB.x > maximumValues.x) ||
                    (pointA.x < minimumValues.x && pointB.x < minimumValues.x))
                {
                    LeftClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    RightClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    return;
                }

                if(pointA.x > maximumValues.x)
                {
                    pointA.x = maximumValues.x;
                    pointA.y = (C - pointA.x) / B;
                }
                else if(pointA.x < minimumValues.x)
                {
                    pointA.x = minimumValues.x;
                    pointA.y = (C - pointA.x) / B;
                }

                if(pointB.x > maximumValues.x)
                {
                    pointB.x = maximumValues.x;
                    pointB.y = (C - pointB.x) / B;
                }
                else if(pointB.x < minimumValues.x)
                {
                    pointB.x = minimumValues.x;
                    pointB.y = (C - pointB.x) / B;
                }
            }
            else
            {
                pointA.x = minimumValues.x;
                if(vertexA != null && vertexA.Coordinate.x > minimumValues.x)
                {
                    pointA.x = vertexA.Coordinate.x;
                }
                if(pointA.x > maximumValues.x)
                {
                    LeftClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    RightClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    return;
                }
                pointA.y = C - A * pointA.x;

                pointB.x = maximumValues.x;
                if(vertexB != null && vertexB.Coordinate.x < maximumValues.x)
                {
                    pointB.x = vertexB.Coordinate.x;
                }
                if(pointB.x < minimumValues.x)
                {
                    LeftClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    RightClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    return;
                }
                pointB.y = C - A * pointB.x;

                if(
                    (pointA.y > maximumValues.y && pointB.y > maximumValues.y) ||
                    (pointA.y < minimumValues.y && pointB.y < minimumValues.y))
                {
                    LeftClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    RightClippedEndPoint = new Vector2(float.MinValue, float.MinValue);
                    return;
                }

                if(pointA.y > maximumValues.y)
                {
                    pointA.y = maximumValues.y;
                    pointA.x = (C - pointA.y) / A;
                }
                else if(pointA.y < minimumValues.y)
                {
                    pointA.y = minimumValues.y;
                    pointA.x = (C - pointA.y) / A;
                }

                if(pointB.y > maximumValues.y)
                {
                    pointB.y = maximumValues.y;
                    pointB.x = (C - pointB.y) / A;
                }
                else if(pointB.y < minimumValues.y)
                {
                    pointB.y = minimumValues.y;
                    pointB.x = (C - pointB.y) / A;
                }
            }

            if(vertexA == LeftEndPoint)
            {
                LeftClippedEndPoint = new Vector2(pointA.x, pointA.y);
                RightClippedEndPoint = new Vector2(pointB.x, pointB.y);
            }
            else
            {
                RightClippedEndPoint = new Vector2(pointA.x, pointA.y);
                LeftClippedEndPoint = new Vector2(pointB.x, pointB.y);
            }
        }

        private VoronoiDiagramEdge()
        {
            Index = -1;
            A = 0f;
            B = 0f;
            C = 0f;
            LeftEndPoint = null;
            RightEndPoint = null;
            LeftSite = null;
            RightSite = null;
        }
    }
}
