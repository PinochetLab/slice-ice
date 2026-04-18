using UnityEngine;

namespace Geometry
{
    public class Segment
    {
        public Segment(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }
        
        public Vector2 A { get; }
        
        public Vector2 B { get; }
        
        public static bool AreIntersected(Segment e1, Segment e2, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            var a = e1.B - e1.A;
            var b = e2.B - e2.A;
            var c = e2.A - e1.A;

            var denominator = Vector3.Cross(a, b).z;

            if (denominator == 0)
                return false;

            var t = Vector3.Cross(c, b).z / denominator;
            var u = Vector3.Cross(c, a).z / denominator;

            if (t is < 0 or > 1 || u is < 0 or > 1) return false;
            
            t = Mathf.Clamp01(t);
            intersection = e1.A + t * a;
            return true;
        }
    }
}