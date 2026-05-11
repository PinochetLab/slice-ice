using System;
using System.Collections.Generic;
using System.Linq;
using Penguins;
using UnityEngine;

namespace Geometry
{
    public class Polygon
    {
        public List<Vector2> Vertices { get; }

        public Polygon(List<Vector2> vertices)
        {
            Vertices = new List<Vector2>(vertices);
            if (!IsClockwise())
            {
                Vertices.Reverse();
            }
        }
        
        private int Last(int i) => (Vertices.Count + i - 1) % Vertices.Count;
        private int Next(int i) => (i + 1) % Vertices.Count;

        public bool TrySlice(List<Vector2> slicePoints, out List<Polygon> parts)
        {
            parts = null;
            
            if (Vertices.Count < 3)
                return false;
            
            if (slicePoints.Count < 2)
                return false;
            
            if (IsInside(slicePoints[0]))
                return false;

            if (!GetIntersections(slicePoints, out var intersections, out var points))
                return false;

            var first = intersections[0];
            var second = intersections[1];

            parts = Divide(points, first, second);
            return true;
        }

        public Ricochet GetRicochet(Vector2 position, Vector2 direction)
        {
            var end = position + direction * 1000f;
            var segment =  new Segment(position, end);

            /*Debug.Log("--------");
            Debug.Log($"position: {position}");
            Debug.Log($"direction: {direction}");*/
            
            var intersections = GetIntersections(segment)
                .Where(x => x.SegmentX > 0.0001f)
                .OrderBy(x => x.SegmentX).ToList();

            if (intersections.Count == 0)
            {
                Debug.Log("--------");
                Debug.Log($"position: ({position.x}, {position.y})");
                Debug.Log($"direction: {direction}");
                throw new Exception("No intersection found.");
            }
            
            var intersection = intersections.First();
            
            /*Debug.Log($"intersection.Point: {intersection.Point}");
            Debug.Log($"intersection.Index: {intersection.Index}");
            Debug.Log($"intersection.SegmentX: {intersection.SegmentX}");
            Debug.Log($"intersection.EdgeX: {intersection.EdgeX}");*/

            const float dx = 0.5f;
            
            var distance = intersection.SegmentX;
            var newPosition = intersection.Point;
            var edgeSegment = GetEdge(intersection.Index);
            var e = (edgeSegment.B - edgeSegment.A).normalized;
            var p = new Vector2(e.y, -e.x);
            var newDirection = direction - 2 * Vector2.Dot(direction, p) * p;
            return new Ricochet(distance, newPosition, newDirection);
        }

        private List<Polygon> Divide(List<Vector2> points, Intersection first, Intersection second)
        {
            var points1 = points.ToList();
            var points2 = points.ToList();
            
            if (first.Index == second.Index)
            {
                var n = second.Index;
                var m = Next(n);
                
                var endLater = second.EdgeX > first.EdgeX;
                
                var start = endLater ? m : n;
                var end = endLater ? n : m;
                
                for (var i = start;; i = endLater ? Next(i) : Last(i))
                {
                    points2.Add(Vertices[i]);
                    if (i == end)
                        break;
                }
            }
            else
            {
                for (var i = Next(second.Index);; i = Next(i))
                {
                    points1.Add(Vertices[i]);
                    if (i == first.Index)
                        break;
                }
                
                for (var i = second.Index;; i = Last(i))
                {
                    points2.Add(Vertices[i]);
                    if (i == Next(first.Index))
                        break;
                }
            }
            
            var polygon1 = new Polygon(points1);
            var polygon2 = new Polygon(points2);
            
            return new List<Polygon> {polygon1, polygon2};
        }

        private bool GetIntersections(List<Vector2> line, out List<Intersection> intersections, out List<Vector2> points)
        {
            intersections = new List<Intersection>();
            points = new List<Vector2>();
            var firstFound = false;
            
            for (var i = 0; i < line.Count - 1; i++)
            {
                var a = line[i];
                var b = line[i + 1];
                var segment = new Segment(a, b);
                if (firstFound)
                    points.Add(a);

                if (!IsEdgeIntersectedBySegment(segment, out var segmentIntersections)) continue;

                firstFound = true;
                
                foreach (var intersection in segmentIntersections)
                {
                    intersections.Add(intersection);
                    points.Add(intersection.Point);

                    if (intersections.Count == 2)
                        return true;
                }
            }
            
            return false;
        }

        private Segment GetEdge(int i)
        {
            var next = (i + 1) % Vertices.Count;
            var p1 = Vertices[i];
            var p2 = Vertices[next];
            return new Segment(p1, p2);
        }

        private IEnumerable<Intersection> GetIntersections(Segment segment)
        {
            var vertexCount = Vertices.Count;

            for (var i = 0; i < vertexCount; i++)
            {
                var edge = GetEdge(i);
                if (!Segment.AreIntersected(segment, edge, out var intersectionPoint))
                    continue;
                var edgeX = Vector2.Distance(Vertices[i], intersectionPoint);
                var segmentX = Vector2.Distance(segment.A, intersectionPoint);
                //var enter = Vector3.Cross(edge.B - edge.A, segment.A - edge.A).z > 0;
                yield return new Intersection(edgeX, segmentX, intersectionPoint, i);
            }
        }

        private bool IsEdgeIntersectedBySegment(Segment segment, out List<Intersection> intersections)
        {
            intersections = GetIntersections(segment).OrderBy(inter => inter.SegmentX).ToList();
            return intersections.Count > 0;
        }

        public bool IsInside(Vector2 point)
        {
            var inside = false;
            var vertexCount = Vertices.Count;

            for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
            {
                var vi = Vertices[i];
                var vj = Vertices[j];

                var intersect = vi.y > point.y != vj.y > point.y &&
                                point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x;

                if (intersect)
                    inside = !inside;
            }

            return inside;
        }
        
        public float Area()
        {
            if (Vertices == null || Vertices.Count < 3)
                return 0f;
    
            var area = 0f;
            var count = Vertices.Count;
    
            for (var i = 0; i < count; i++)
            {
                var current = Vertices[i];
                var next = Vertices[(i + 1) % count];
        
                area += current.x * next.y - next.x * current.y;
            }
    
            return Mathf.Abs(area) * 0.5f;
        }
        
        private bool IsClockwise()
        {
            var sum = 0f;
            var count = Vertices.Count;
    
            for (var i = 0; i < count; i++)
            {
                var current = Vertices[i];
                var next = Vertices[(i + 1) % count];
                sum += (next.x - current.x) * (next.y + current.y);
            }
    
            return sum > 0;
        }
    }
}