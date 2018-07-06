// Copyright 2016 afuzzyllama. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using PixelsForGlory.Extensions;
using UnityEngine;

namespace PixelsForGlory.ComputationalSystem
{
    public class VoronoiColorDiagram : VoronoiDiagram<Color>
    {
        public VoronoiColorDiagram()
        {
        }

        public VoronoiColorDiagram(Rect bounds) : base(bounds)
        {
        }
    }

    /// <summary>
    /// Generates a voronoi diagram
    /// </summary>
    public class VoronoiDiagram<T> where T : new()
    {
        // Bounds of the Voronoi Diagram
        public Rect Bounds;

        // Generated sites.  Filled after GenerateSites() is called
        public Dictionary<int, VoronoiDiagramGeneratedSite<T>> GeneratedSites;

        private readonly List<VoronoiDiagramSite<T>> _originalSites;

        // Stored added points as Sites that are currently being processed.  Ordered lexigraphically by y and then x
        private readonly List<VoronoiDiagramSite<T>> _sites;

        // Stores the bottom most site when running GenerateEdges
        private VoronoiDiagramSite<T> _bottomMostSite;

        // Stores the current site index when running GenerateEdges
        private int _currentSiteIndex;

        // Stores the minimum values of the points in site array.
        private Vector2 _minValues;

        // Stores the delta values of the minimum and maximum values.
        private Vector2 _deltaValues;

        public VoronoiDiagram()
        {
            Bounds = new Rect();
            GeneratedSites = new Dictionary<int, VoronoiDiagramGeneratedSite<T>>();
            _originalSites = new List<VoronoiDiagramSite<T>>();
            _sites = new List<VoronoiDiagramSite<T>>();
        }

        public VoronoiDiagram(Rect bounds)
        {
            Bounds = bounds;
            GeneratedSites = new Dictionary<int, VoronoiDiagramGeneratedSite<T>>();
            _originalSites = new List<VoronoiDiagramSite<T>>();
            _sites = new List<VoronoiDiagramSite<T>>();
        }

        /// <summary>
        /// Adds points to the diagram to be used at generation time
        /// </summary>
        /// <param name="points">List of points to be added.</param>
        /// <returns>True if added successful, false otherwise.  If false, no points are added.</returns>
        public bool AddSites(List<VoronoiDiagramSite<T>> points)
        {
            foreach(VoronoiDiagramSite<T> point in points)
            {
                if(!Bounds.Contains(point.Coordinate))
                {
                    Debug.LogError(string.Format("point ({0}, {1}) out of diagram bounds ({2}, {3})", point.Coordinate.x,
                        point.Coordinate.y, Bounds.width, Bounds.height));
                    return false;
                }
            }

            foreach(VoronoiDiagramSite<T> point in points)
            {
                _originalSites.Add(point);
            }

            return true;
        }

        /// <summary>
        /// Runs Fortune's Algorithm to generate sites with edges for the diagram
        /// </summary>
        /// <param name="relaxationCycles">Number of relaxation cycles to run</param>
        public void GenerateSites(int relaxationCycles)
        {
            if(_originalSites.Count == 0)
            {
                Debug.LogError("No points added to the diagram.  Sites cannot be generated");
                return;
            }

            _sites.Clear();
            foreach(VoronoiDiagramSite<T> site in _originalSites)
            {
                _sites.Add(new VoronoiDiagramSite<T>(_sites.Count, site));
            }
            SortSitesAndSetValues();

            // Cycles related to Lloyd's algorithm
            for(int cycles = 0; cycles < relaxationCycles; cycles++)
            {
                // Fortune's Algorithm
                int numGeneratedEdges = 0;
                int numGeneratedVertices = 0;
                _currentSiteIndex = 0;

                var priorityQueue = new VoronoiDiagramPriorityQueue<T>(_sites.Count, _minValues, _deltaValues);
                var edgeList = new VoronoiDiagramEdgeList<T>(_sites.Count, _minValues, _deltaValues);

                Vector2 currentIntersectionStar = Vector2.zero;
                VoronoiDiagramSite<T> currentSite;

                var generatedEdges = new List<VoronoiDiagramEdge<T>>();

                bool done = false;
                _bottomMostSite = GetNextSite();
                currentSite = GetNextSite();
                while(!done)
                {
                    if(!priorityQueue.IsEmpty())
                    {
                        currentIntersectionStar = priorityQueue.GetMinimumBucketFirstPoint();
                    }

                    VoronoiDiagramSite<T> bottomSite;
                    VoronoiDiagramHalfEdge<T> bisector;
                    VoronoiDiagramHalfEdge<T> rightBound;
                    VoronoiDiagramHalfEdge<T> leftBound;
                    VoronoiDiagramVertex<T> vertex;
                    VoronoiDiagramEdge<T> edge;
                    if(
                        currentSite != null &&
                        (
                            priorityQueue.IsEmpty() ||
                            currentSite.Coordinate.y < currentIntersectionStar.y ||
                            (
                                currentSite.Coordinate.y.IsAlmostEqualTo(currentIntersectionStar.y) &&
                                currentSite.Coordinate.x < currentIntersectionStar.x
                            )
                        )
                    )
                    {
                        // Current processed site is the smallest
                        leftBound = edgeList.GetLeftBoundFrom(currentSite.Coordinate);
                        rightBound = leftBound.EdgeListRight;
                        bottomSite = GetRightRegion(leftBound);

                        edge = VoronoiDiagramEdge<T>.Bisect(bottomSite, currentSite);
                        edge.Index = numGeneratedEdges;
                        numGeneratedEdges++;

                        generatedEdges.Add(edge);

                        bisector = new VoronoiDiagramHalfEdge<T>(edge, VoronoiDiagramEdgeType.Left);
                        edgeList.Insert(leftBound, bisector);

                        vertex = VoronoiDiagramVertex<T>.Intersect(leftBound, bisector);
                        if(vertex != null)
                        {
                            priorityQueue.Delete(leftBound);

                            leftBound.Vertex = vertex;
                            leftBound.StarY = vertex.Coordinate.y + currentSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(leftBound);
                        }

                        leftBound = bisector;
                        bisector = new VoronoiDiagramHalfEdge<T>(edge, VoronoiDiagramEdgeType.Right);

                        edgeList.Insert(leftBound, bisector);

                        vertex = VoronoiDiagramVertex<T>.Intersect(bisector, rightBound);
                        if(vertex != null)
                        {
                            bisector.Vertex = vertex;
                            bisector.StarY = vertex.Coordinate.y + currentSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(bisector);
                        }

                        currentSite = GetNextSite();
                    }
                    else if(priorityQueue.IsEmpty() == false)
                    {
                        // Current intersection is the smallest
                        leftBound = priorityQueue.RemoveAndReturnMinimum();
                        VoronoiDiagramHalfEdge<T> leftLeftBound = leftBound.EdgeListLeft;
                        rightBound = leftBound.EdgeListRight;
                        VoronoiDiagramHalfEdge<T> rightRightBound = rightBound.EdgeListRight;
                        bottomSite = GetLeftRegion(leftBound);
                        VoronoiDiagramSite<T> topSite = GetRightRegion(rightBound);

                        // These three sites define a Delaunay triangle
                        // Bottom, Top, EdgeList.GetRightRegion(rightBound);
                        // Debug.Log(string.Format("Delaunay triagnle: ({0}, {1}), ({2}, {3}), ({4}, {5})"),
                        //      bottomSite.Coordinate.x, bottomSite.Coordinate.y,
                        //      topSite.Coordinate.x, topSite.Coordinate.y,
                        //      edgeList.GetRightRegion(leftBound).Coordinate.x,
                        //      edgeList.GetRightRegion(leftBound).Coordinate.y);

                        var v = leftBound.Vertex;
                        v.Index = numGeneratedVertices;
                        numGeneratedVertices++;

                        leftBound.Edge.SetEndpoint(v, leftBound.EdgeType);
                        rightBound.Edge.SetEndpoint(v, rightBound.EdgeType);

                        edgeList.Delete(leftBound);

                        priorityQueue.Delete(rightBound);
                        edgeList.Delete(rightBound);

                        var edgeType = VoronoiDiagramEdgeType.Left;
                        if(bottomSite.Coordinate.y > topSite.Coordinate.y)
                        {
                            var tempSite = bottomSite;
                            bottomSite = topSite;
                            topSite = tempSite;
                            edgeType = VoronoiDiagramEdgeType.Right;
                        }

                        edge = VoronoiDiagramEdge<T>.Bisect(bottomSite, topSite);
                        edge.Index = numGeneratedEdges;
                        numGeneratedEdges++;

                        generatedEdges.Add(edge);

                        bisector = new VoronoiDiagramHalfEdge<T>(edge, edgeType);
                        edgeList.Insert(leftLeftBound, bisector);

                        edge.SetEndpoint(v,
                            edgeType == VoronoiDiagramEdgeType.Left
                                ? VoronoiDiagramEdgeType.Right
                                : VoronoiDiagramEdgeType.Left);

                        vertex = VoronoiDiagramVertex<T>.Intersect(leftLeftBound, bisector);
                        if(vertex != null)
                        {
                            priorityQueue.Delete(leftLeftBound);

                            leftLeftBound.Vertex = vertex;
                            leftLeftBound.StarY = vertex.Coordinate.y + bottomSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(leftLeftBound);
                        }

                        vertex = VoronoiDiagramVertex<T>.Intersect(bisector, rightRightBound);
                        if(vertex != null)
                        {
                            bisector.Vertex = vertex;
                            bisector.StarY = vertex.Coordinate.y + bottomSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(bisector);
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }

                GeneratedSites.Clear();
                // Bound the edges of the diagram
                foreach(VoronoiDiagramEdge<T> currentGeneratedEdge in generatedEdges)
                {
                    currentGeneratedEdge.GenerateClippedEndPoints(Bounds);
                }

                foreach(VoronoiDiagramSite<T> site in _sites)
                {
                    try
                    {
                        site.GenerateCentroid(Bounds);
                    }
                    catch(Exception)
                    {
                        Debug.Log("Coordinate");
                        Debug.Log(site.Coordinate);
                        Debug.Log("End points:");
                        foreach(var edge in site.Edges)
                        {
                            Debug.Log(edge.LeftClippedEndPoint + " , " +  edge.RightClippedEndPoint);
                        }
                        throw;
                    }
                    
                }

                foreach(VoronoiDiagramSite<T> site in _sites)
                {
                    var generatedSite = new VoronoiDiagramGeneratedSite<T>(site.Index, site.Coordinate, site.Centroid, new T(), site.IsCorner, site.IsEdge);
                    generatedSite.Vertices.AddRange(site.Vertices);
                    generatedSite.SiteData = site.SiteData;

                    foreach(VoronoiDiagramEdge<T> siteEdge in site.Edges)
                    {
                        // Only add edges that are visible
                        // Don't need to check the Right because they will both be float.MinValue
                        if(siteEdge.LeftClippedEndPoint == new Vector2(float.MinValue, float.MinValue))
                        {
                            continue;
                        }

                        generatedSite.Edges.Add(new VoronoiDiagramGeneratedEdge(siteEdge.Index,
                            siteEdge.LeftClippedEndPoint, siteEdge.RightClippedEndPoint));

                        if(siteEdge.LeftSite != null && !generatedSite.NeighborSites.Contains(siteEdge.LeftSite.Index))
                        {
                            generatedSite.NeighborSites.Add(siteEdge.LeftSite.Index);
                        }

                        if(siteEdge.RightSite != null && !generatedSite.NeighborSites.Contains(siteEdge.RightSite.Index))
                        {
                            generatedSite.NeighborSites.Add(siteEdge.RightSite.Index);
                        }
                    }
                    
                    GeneratedSites.Add(generatedSite.Index, generatedSite);

                    // Finished with the edges, remove the references so they can be removed at the end of the method
                    site.Edges.Clear();
                }

                // Clean up
                _bottomMostSite = null;
                _sites.Clear();

                // Lloyd's Algorithm
                foreach(KeyValuePair<int, VoronoiDiagramGeneratedSite<T>> generatedSite in GeneratedSites)
                {
                    var centroidPoint = 
                        new Vector2(
                            Mathf.Clamp(generatedSite.Value.Centroid.x, 0, Bounds.width), 
                            Mathf.Clamp(generatedSite.Value.Centroid.y, 0, Bounds.height));
                    var newSite = new VoronoiDiagramSite<T>(new Vector2(centroidPoint.x, centroidPoint.y), generatedSite.Value.SiteData);
					
                    if(!_sites.Any(item => item.Coordinate.x.IsAlmostEqualTo(newSite.Coordinate.x) && item.Coordinate.y.IsAlmostEqualTo(newSite.Coordinate.y)))
                    {
                        _sites.Add(new VoronoiDiagramSite<T>(_sites.Count, newSite));
                    }
                }
                SortSitesAndSetValues();
            }
        }

        /// <summary>
        /// Creates a 1d array of the Voronoi Diagram. Assumes that diagram has been run through GenerateDiagram
        /// </summary>
        /// <returns>1d array of T</returns>
        public T[] Get1DSampleArray()
        {
            var returnData = new T[(int)Bounds.width * (int)Bounds.height];

            for (int i = 0; i < returnData.Length; i++)
            {
                returnData[i] = default(T);
            }

            foreach (KeyValuePair<int, VoronoiDiagramGeneratedSite<T>> site in GeneratedSites)
            {
                if (site.Value.Vertices.Count == 0)
                {
                    continue;
                }

                Vector2 minimumVertex = site.Value.Vertices[0];
                Vector2 maximumVertex = site.Value.Vertices[0];

                for (int i = 1; i < site.Value.Vertices.Count; i++)
                {
                    if (site.Value.Vertices[i].x < minimumVertex.x)
                    {
                        minimumVertex.x = site.Value.Vertices[i].x;
                    }

                    if (site.Value.Vertices[i].y < minimumVertex.y)
                    {
                        minimumVertex.y = site.Value.Vertices[i].y;
                    }

                    if (site.Value.Vertices[i].x > maximumVertex.x)
                    {
                        maximumVertex.x = site.Value.Vertices[i].x;
                    }

                    if (site.Value.Vertices[i].y > maximumVertex.y)
                    {
                        maximumVertex.y = site.Value.Vertices[i].y;
                    }
                }

                if (minimumVertex.x < 0.0f)
                {
                    minimumVertex.x = 0.0f;
                }

                if (minimumVertex.y < 0.0f)
                {
                    minimumVertex.y = 0.0f;
                }

                if (maximumVertex.x > Bounds.width)
                {
                    maximumVertex.x = Bounds.width;
                }

                if (maximumVertex.y > Bounds.height)
                {
                    maximumVertex.y = Bounds.height;
                }

                for (int x = (int)minimumVertex.x; x <= maximumVertex.x; x++)
                {
                    for (int y = (int)minimumVertex.y; y <= maximumVertex.y; y++)
                    {
                        if (PointInVertices(new Vector2(x, y), site.Value.Vertices))
                        {
                            if(Bounds.Contains(new Vector2(x, y)))
                            {
                                int index = x + y * (int)Bounds.width;
                                returnData[index] = site.Value.SiteData;
                            }
                        }
                    }
                }
            }

            return returnData;
        }

        /// <summary>
        /// Creates a 2d array of the Voronoi Diagram. Assumes that diagram has been run through GenerateDiagram
        /// </summary>
        /// <returns>2d array of T</returns>
        public T[,] Get2DSampleArray()
        {
            var returnData = new T[(int)Bounds.width, (int)Bounds.height];

            for (int x = 0; x < (int)Bounds.width; x++)
            {
                for(int y = 0; y < (int)Bounds.height; y++)
                {
                    returnData[x, y] = default(T);
                }
            }

            foreach (KeyValuePair<int, VoronoiDiagramGeneratedSite<T>> site in GeneratedSites)
            {
                if (site.Value.Vertices.Count == 0)
                {
                    continue;
                }

                Vector2 minimumVertex = site.Value.Vertices[0];
                Vector2 maximumVertex = site.Value.Vertices[0];

                for (int i = 1; i < site.Value.Vertices.Count; i++)
                {
                    if (site.Value.Vertices[i].x < minimumVertex.x)
                    {
                        minimumVertex.x = site.Value.Vertices[i].x;
                    }

                    if (site.Value.Vertices[i].y < minimumVertex.y)
                    {
                        minimumVertex.y = site.Value.Vertices[i].y;
                    }

                    if (site.Value.Vertices[i].x > maximumVertex.x)
                    {
                        maximumVertex.x = site.Value.Vertices[i].x;
                    }

                    if (site.Value.Vertices[i].y > maximumVertex.y)
                    {
                        maximumVertex.y = site.Value.Vertices[i].y;
                    }
                }

                if (minimumVertex.x < 0.0f)
                {
                    minimumVertex.x = 0.0f;
                }

                if (minimumVertex.y < 0.0f)
                {
                    minimumVertex.y = 0.0f;
                }

                if (maximumVertex.x > Bounds.width)
                {
                    maximumVertex.x = Bounds.width;
                }

                if (maximumVertex.y > Bounds.height)
                {
                    maximumVertex.y = Bounds.height;
                }

                for (int x = (int)minimumVertex.x; x <= maximumVertex.x; x++)
                {
                    for (int y = (int)minimumVertex.y; y <= maximumVertex.y; y++)
                    {
                        if (PointInVertices(new Vector2(x, y), site.Value.Vertices))
                        {
                            if (Bounds.Contains(new Vector2(x, y)))
                            {
                                returnData[x, y] = site.Value.SiteData;
                            }
                        }
                    }
                }
            }

            return returnData;
        }

        /// <summary>
        /// Sorts sites and calculates _minValues and _deltaValues
        /// </summary>
        private void SortSitesAndSetValues()
        {
            _sites.Sort(
                delegate(VoronoiDiagramSite<T> siteA, VoronoiDiagramSite<T> siteB)
                {
                    if(Mathf.RoundToInt(siteA.Coordinate.y) < Mathf.RoundToInt(siteB.Coordinate.y))
                    {
                        return -1;
                    }

                    if(Mathf.RoundToInt(siteA.Coordinate.y) > Mathf.RoundToInt(siteB.Coordinate.y))
                    {
                        return 1;
                    }

                    if(Mathf.RoundToInt(siteA.Coordinate.x) < Mathf.RoundToInt(siteB.Coordinate.x))
                    {
                        return -1;
                    }

                    if(Mathf.RoundToInt(siteA.Coordinate.x) > Mathf.RoundToInt(siteB.Coordinate.x))
                    {
                        return 1;
                    }

                    return 0;
                });

            var currentMin = new Vector2(float.MaxValue, float.MaxValue);
            var currentMax = new Vector2(float.MinValue, float.MinValue);
            foreach(VoronoiDiagramSite<T> site in _sites)
            {
                if(site.Coordinate.x < currentMin.x)
                {
                    currentMin.x = site.Coordinate.x;
                }

                if(site.Coordinate.x > currentMax.x)
                {
                    currentMax.x = site.Coordinate.x;
                }

                if(site.Coordinate.y < currentMin.y)
                {
                    currentMin.y = site.Coordinate.y;
                }

                if(site.Coordinate.y > currentMax.y)
                {
                    currentMax.y = site.Coordinate.y;
                }
            }

            _minValues = currentMin;
            _deltaValues = new Vector2(currentMax.x - currentMin.x, currentMax.y - currentMin.y);

        }

        /// <summary>
        /// Returns the next site and increments _currentSiteIndex
        /// </summary>
        /// <returns>The next site</returns>
        private VoronoiDiagramSite<T> GetNextSite()
        {
            if(_currentSiteIndex < _sites.Count)
            {
                VoronoiDiagramSite<T> nextSite = _sites[_currentSiteIndex];
                _currentSiteIndex++;
                return nextSite;
            }

            return null;
        }

        /// <summary>
        /// Returns the left region in relation to a half edge
        /// </summary>
        /// <param name="halfEdge">The half edge to calculate from</param>
        /// <returns>The left region</returns>
        private VoronoiDiagramSite<T> GetLeftRegion(VoronoiDiagramHalfEdge<T> halfEdge)
        {
            if(halfEdge.Edge == null)
            {
                return _bottomMostSite;
            }

            if(halfEdge.EdgeType == VoronoiDiagramEdgeType.Left)
            {
                return halfEdge.Edge.LeftSite;
            }
            else
            {
                return halfEdge.Edge.RightSite;
            }
        }

        /// <summary>
        /// Returns the right region in relation to a half edge
        /// </summary>
        /// <param name="halfEdge">The half edge to calculate from</param>
        /// <returns>The right region</returns>
        private VoronoiDiagramSite<T> GetRightRegion(VoronoiDiagramHalfEdge<T> halfEdge)
        {
            if(halfEdge.Edge == null)
            {
                return _bottomMostSite;
            }

            if(halfEdge.EdgeType == VoronoiDiagramEdgeType.Left)
            {
                return halfEdge.Edge.RightSite;
            }
            else
            {
                return halfEdge.Edge.LeftSite;
            }
        }

        /// <summary>
        /// Does the passed in point lie inside of the vertices passed in.  The vertices are assumed to be sorted.
        /// </summary>
        /// <param name="point">Point that lies inside the vertices?</param>
        /// <param name="vertices">Sorted vertices</param>
        /// <returns></returns>
        public static bool PointInVertices(Vector2 point, List<Vector2> vertices)
        {
            int i;
            int j = vertices.Count - 1;

            bool oddNodes = false;

            for (i = 0; i < vertices.Count; ++i)
            {
                if (
                    (vertices[i].y < point.y && vertices[j].y >= point.y ||
                     vertices[j].y < point.y && vertices[i].y >= point.y) &&
                    (vertices[i].x <= point.x || vertices[j].x <= point.x)
                    )
                {
                    if (vertices[i].x +
                       (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) * (vertices[j].x - vertices[i].x) <
                       point.x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }
    }
}
