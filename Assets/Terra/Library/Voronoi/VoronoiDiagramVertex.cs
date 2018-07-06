// Copyright 2016 afuzzyllama. All Rights Reserved.
using UnityEngine;
using PixelsForGlory.Extensions;

namespace PixelsForGlory.ComputationalSystem
{
    /// <summary>
    /// Represents a vertex in the Voronoi Diagram. 
    /// </summary>
    public class VoronoiDiagramVertex<T> where T : new()
    {
        /// <summary>
        /// Index of the vertex
        /// </summary>
        public int Index;

        /// <summary>
        /// Point of the vertex
        /// </summary>
        public Vector2 Coordinate;

        /// <summary>
        /// Creates a vertex at the intersection of two edges.
        /// </summary>
        /// <param name="halfEdgeA">The first edge</param>
        /// <param name="halfEdgeB">The second edge</param>
        /// <returns>The vertex of the intersection. null if the two edges do not intersect</returns>
        public static VoronoiDiagramVertex<T> Intersect(VoronoiDiagramHalfEdge<T> halfEdgeA, VoronoiDiagramHalfEdge<T> halfEdgeB)
        {
            VoronoiDiagramEdge<T> edgeA, edgeB, edge;
            VoronoiDiagramHalfEdge<T> halfEdge;
            float determinant, intersectionX, intersectionY;
            bool isRightOfSite;

            edgeA = halfEdgeA.Edge;
            edgeB = halfEdgeB.Edge;

            if(edgeA == null || edgeB == null)
            {
                return null;
            }

            if(edgeA.RightSite == edgeB.RightSite)
            {
                return null;
            }

            determinant = (edgeA.A * edgeB.B) - (edgeA.B * edgeB.A);

            if(determinant.IsAlmostZero())
            {
                // The edges are parallel
                return null;
            }

            intersectionX = (edgeA.C * edgeB.B - edgeB.C * edgeA.B) / determinant;
            intersectionY = (edgeB.C * edgeA.A - edgeA.C * edgeB.A) / determinant;

            if(
                edgeA.RightSite.Coordinate.y < edgeB.RightSite.Coordinate.y ||
                (
                    edgeA.RightSite.Coordinate.y.IsAlmostEqualTo(edgeB.RightSite.Coordinate.y) &&
                    edgeA.RightSite.Coordinate.x < edgeB.RightSite.Coordinate.x
                    )
                )
            {
                halfEdge = halfEdgeA;
                edge = edgeA;
            }
            else
            {
                halfEdge = halfEdgeB;
                edge = edgeB;
            }

            isRightOfSite = intersectionX >= edge.RightSite.Coordinate.x;
            if(
                (isRightOfSite && halfEdge.EdgeType == VoronoiDiagramEdgeType.Left) ||
                (!isRightOfSite && halfEdge.EdgeType == VoronoiDiagramEdgeType.Right)
                )
            {
                return null;
            }

            return new VoronoiDiagramVertex<T>(-1, new Vector2(intersectionX, intersectionY));
        }

        /// <summary>
        /// Create a new vertex
        /// </summary>
        /// <param name="index">Index of the new vertex</param>
        /// <param name="coordinate">Coordinate of the new vertex</param>
        public VoronoiDiagramVertex(int index, Vector2 coordinate)
        {
            Index = index;
            Coordinate = coordinate;

            // float != float == NAN
            if(float.IsNaN(Coordinate.x) || float.IsNaN(Coordinate.y))
            {
                // This probably should not happen, but it will alert in the logs if it does
                Debug.LogError("Contains NaN");
            }
        }
    }
}
