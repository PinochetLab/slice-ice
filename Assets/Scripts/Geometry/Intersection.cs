using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class Intersection
    {
        public float EdgeX { get; }
        public float SegmentX { get; }
        public Vector2 Point { get; }
        public int Index { get; }

        public Intersection(float edgeX, float segmentX, Vector2 point, int index)
        {
            EdgeX = edgeX;
            SegmentX = segmentX;
            Point = point;
            Index = index;
        }
    }
}