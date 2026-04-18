using UnityEngine;

namespace Geometry
{
    public class Triangle
    {
        private const float Epsilon = 0.00001f;
        
        public Vector2 A, B, C;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Segment[] Edges => new Segment[]
        {
            new (A, B),
            new (B, C),
            new (C, A)
        };

        public bool HasVertex(Vector2 point)
        {
            return Vector2.Distance(A, point) < Epsilon ||
                   Vector2.Distance(B, point) < Epsilon ||
                   Vector2.Distance(C, point) < Epsilon;
        }

        public bool HasEdge(Segment edge)
        {
            return (Vector2.Distance(A, edge.A) < Epsilon && Vector2.Distance(B, edge.B) < Epsilon) ||
                   (Vector2.Distance(B, edge.A) < Epsilon && Vector2.Distance(C, edge.B) < Epsilon) ||
                   (Vector2.Distance(C, edge.A) < Epsilon && Vector2.Distance(A, edge.B) < Epsilon) ||
                   (Vector2.Distance(A, edge.B) < Epsilon && Vector2.Distance(B, edge.A) < Epsilon) ||
                   (Vector2.Distance(B, edge.B) < Epsilon && Vector2.Distance(C, edge.A) < Epsilon) ||
                   (Vector2.Distance(C, edge.B) < Epsilon && Vector2.Distance(A, edge.A) < Epsilon);
        }

        public bool IsPointInCircumcircle(Vector2 point)
        {
            float ax = A.x - point.x;
            float ay = A.y - point.y;
            float bx = B.x - point.x;
            float by = B.y - point.y;
            float cx = C.x - point.x;
            float cy = C.y - point.y;

            float det = (ax * ax + ay * ay) * (bx * cy - by * cx) -
                        (bx * bx + by * by) * (ax * cy - ay * cx) +
                        (cx * cx + cy * cy) * (ax * by - ay * bx);

            return det > Epsilon;
        }
    }
}