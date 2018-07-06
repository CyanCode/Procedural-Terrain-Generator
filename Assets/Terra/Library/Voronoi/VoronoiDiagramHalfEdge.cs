// Copyright 2016 afuzzyllama. All Rights Reserved.
using UnityEngine;
using PixelsForGlory.Extensions;

namespace PixelsForGlory.ComputationalSystem
{
    /// <summary>
    /// Half edge representation (left or right) for a Voronoi Diagram.  
    /// </summary>
    public class VoronoiDiagramHalfEdge<T> where T : new()
    {
        public VoronoiDiagramEdge<T> Edge;
        public VoronoiDiagramEdgeType EdgeType;
        public VoronoiDiagramVertex<T> Vertex;
        public float StarY;

        public VoronoiDiagramHalfEdge<T> EdgeListLeft;
        public VoronoiDiagramHalfEdge<T> EdgeListRight;
        public VoronoiDiagramHalfEdge<T> NextInPriorityQueue;

        /// <summary>
        /// Creates a new half edge 
        /// </summary>
        /// <param name="edge">The edge of the new half edge</param>
        /// <param name="edgeType">The edge type that this half edge represents (left or right).  The only half edges that should be "None" are the buckets in the priority queue.</param>
        public VoronoiDiagramHalfEdge(VoronoiDiagramEdge<T> edge, VoronoiDiagramEdgeType edgeType)
        {
            Edge = edge;
            EdgeType = edgeType;
            Vertex = null;
            StarY = 0f;
            EdgeListLeft = null;
            EdgeListRight = null;
            NextInPriorityQueue = null;
        }

        /// <summary>
        /// Is the edge left of the passed in point?
        /// </summary>
        /// <param name="point">Point to check against</param>
        /// <returns>True if left, false if right</returns>
        public bool IsLeftOf(Vector2 point)
        {
            VoronoiDiagramSite<T> topSite = Edge.RightSite;
            bool isRightOfSite = point.x > topSite.Coordinate.x;
            bool isAbove;

            if(isRightOfSite && EdgeType == VoronoiDiagramEdgeType.Left)
            {
                return true;
            }

            if(!isRightOfSite && EdgeType == VoronoiDiagramEdgeType.Right)
            {
                return false;
            }

            if(Edge.A.IsAlmostEqualTo(1f))
            {
                var dyp = point.y - topSite.Coordinate.y;
                var dxp = point.x - topSite.Coordinate.x;

                var isFast = false;
                if((!isRightOfSite && Edge.B < 0.0f) ||
                   (isRightOfSite && Edge.B >= 0.0f))
                {
                    isAbove = dyp >= Edge.B * dxp;
                    isFast = isAbove;
                }
                else
                {
                    isAbove = point.x + point.y * Edge.B > Edge.C;
                    if(Edge.B < 0.0f)
                    {
                        isAbove = !isAbove;
                    }
                    if(!isAbove)
                    {
                        isFast = true;
                    }
                }

                if(!isFast)
                {
                    var dxs = topSite.Coordinate.x - Edge.LeftSite.Coordinate.x;
                    isAbove = Edge.B * (dxp * dxp - dyp * dyp) < dxs * dyp * (1f + 2f * dxp / dxs + Edge.B * Edge.B);
                    if(Edge.B < 0f)
                    {
                        isAbove = !isAbove;
                    }
                }
            }
            else // edge.b == 1.0
            {
                float t1, t2, t3, yl;
                yl = Edge.C - Edge.A * point.x;
                t1 = point.y - yl;
                t2 = point.x - topSite.Coordinate.x;
                t3 = yl - topSite.Coordinate.y;
                isAbove = t1 * t1 > t2 * t2 + t3 * t3;
            }
            return EdgeType == VoronoiDiagramEdgeType.Left ? isAbove : !isAbove;
        }

        /// <summary>
        /// Is the edge right of the passed in point?
        /// </summary>
        /// <param name="point">Point to check against</param>
        /// <returns>True if right, false if left</returns>
        public bool IsRightOf(Vector2 point)
        {
            return !IsLeftOf(point);
        }

        /// <summary>
        /// Are there any references to this half edge?  This is useful to know if a half edge is still being used in the edge list or priority queue.  It might not be necessary due to the functionality already supplied by
        /// </summary>
        /// <returns>True if yes, false otherwise.</returns>
        public bool HasReferences()
        {
            return EdgeListLeft != null || EdgeListRight != null || NextInPriorityQueue != null;
        }
    }
}
