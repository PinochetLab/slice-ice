using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;
using Zenject;

namespace Main
{
    public class Floe : MonoBehaviour
    {
        [SerializeField] private MeshFilter floeMeshFilter;
        [SerializeField] private MeshFilter wallMeshFilter;
        [SerializeField] private PolygonCollider2D polygonCollider2D;
        
        [Inject] private SettingsProvider _settings;

        private Polygon _polygon;
        private float _y;
        private bool _isHiding;

        public void SetPolygon(Polygon polygon)
        {
            _polygon = polygon;
            UpdateMesh();
            GenerateWall();
        }

        private void UpdateMesh()
        {
            var vertices = _polygon.Vertices;
            
            if (vertices.Count < 3)
            {
                Debug.LogError("Polygon must have at least 3 vertices");
                return;
            }
            
            polygonCollider2D.enabled = true;
            
            polygonCollider2D.SetPath(0, vertices);
            var mesh = polygonCollider2D.CreateMesh(false, false);

            vertices = mesh.vertices.Select(v => (Vector2)v).ToList();
            
            mesh.vertices = vertices.Select(v => new Vector3(v.x, 0f, v.y)).ToArray();
            
            mesh.uv = CalculateUVs(vertices).ToArray();
            
            //mesh.RecalculateNormals();
            //mesh.RecalculateBounds();

            polygonCollider2D.enabled = false;
            
            floeMeshFilter.mesh = mesh;
        }

        private static List<Vector2> CalculateUVs(List<Vector2> vertices)
        {
            const float w = 15f;
            const float h = 10f;
            
            return vertices.Select(v =>
                new Vector2((v.x + w / 2) / w, (v.y + h / 2) / h)
            ).ToList();
        }
        
        private class TriangulationResult
        {
            public List<Vector2> Vertices = new();
            public List<int> Triangles = new();
        }
        
        
        
        

        private (List<Vector2> points, List<int> triangles) Trianglulate(List<Vector2> polygon)
        {
            List<Vector2> points = new List<Vector2>(polygon);
            List<int> triangles = new List<int>();

            if (points.Count < 3)
                return (points, triangles);

            List<int> indices = new List<int>();
            for (int i = 0; i < points.Count; i++)
                indices.Add(i);

            int guard = 0; // защита от бесконечного цикла

            while (indices.Count > 3 && guard < 10000)
            {
                guard++;
                bool earFound = false;

                for (int i = 0; i < indices.Count; i++)
                {
                    int i0 = indices[(i - 1 + indices.Count) % indices.Count];
                    int i1 = indices[i];
                    int i2 = indices[(i + 1) % indices.Count];

                    Vector2 a = points[i0];
                    Vector2 b = points[i1];
                    Vector2 c = points[i2];

                    if (!IsConvex(a, b, c))
                        continue;

                    if (ContainsPoint(points, indices, a, b, c))
                        continue;

                    // нашли "ухо"
                    triangles.Add(i0);
                    triangles.Add(i1);
                    triangles.Add(i2);

                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }

                if (!earFound)
                    break; // что-то пошло не так (например, самопересечение)
            }

            // последний треугольник
            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }

            return (points, triangles);
        }

        private bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
        {
            return Vector3.Cross(b - a, c - b).z > 0f;
        }

        private bool ContainsPoint(List<Vector2> points, List<int> indices, Vector2 a, Vector2 b, Vector2 c)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                Vector2 p = points[indices[i]];

                if (p == a || p == b || p == c)
                    continue;

                if (PointInTriangle(p, a, b, c))
                    return true;
            }

            return false;
        }

        private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
        
        
        

        /// <summary>
        /// Триангуляция многоугольника (выпуклого или вогнутого) методом отрезания "ушей"
        /// </summary>
        private TriangulationResult TriangulatePolygon(List<Vector2> polygon)
        {
            var result = new TriangulationResult();
            
            if (polygon.Count < 3) return result;
            
            // Создаем копию списка вершин для манипуляций
            var vertices = new List<Vector2>(polygon);
            var indices = Enumerable.Range(0, vertices.Count).ToList();
            
            var triangles = new List<int>();
            
            int vertexCount = vertices.Count;
            int iterations = 0;
            int maxIterations = vertexCount * 3; // Защита от бесконечного цикла
            
            while (indices.Count > 3 && iterations < maxIterations)
            {
                iterations++;
                bool earFound = false;
                
                for (int i = 0; i < indices.Count; i++)
                {
                    int prev = GetPreviousIndex(indices, i);
                    int curr = indices[i];
                    int next = GetNextIndex(indices, i);
                    
                    Vector2 a = vertices[prev];
                    Vector2 b = vertices[curr];
                    Vector2 c = vertices[next];
                    
                    // Проверяем, является ли угол выпуклым
                    if (IsConvexAngle(a, b, c, IsClockwise(vertices, indices)))
                    {
                        // Проверяем, находится ли какая-либо другая вершина внутри треугольника
                        bool hasPointInside = false;
                        
                        for (int j = 0; j < indices.Count; j++)
                        {
                            if (j == i || j == prev || j == next) continue;
                            
                            Vector2 point = vertices[indices[j]];
                            if (IsPointInTriangle(point, a, b, c))
                            {
                                hasPointInside = true;
                                break;
                            }
                        }
                        
                        if (!hasPointInside)
                        {
                            // Нашли "ухо" - добавляем треугольник
                            triangles.Add(prev);
                            triangles.Add(curr);
                            triangles.Add(next);
                            
                            // Удаляем текущую вершину
                            indices.RemoveAt(i);
                            earFound = true;
                            break;
                        }
                    }
                }
                
                if (!earFound)
                {
                    // Если не нашли ухо, используем простой метод для выпуклых многоугольников
                    return TriangulateConvex(polygon);
                }
            }
            
            // Добавляем последний треугольник
            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }
            
            result.Vertices = vertices;
            result.Triangles = triangles;
            return result;
        }
        
        /// <summary>
        /// Триангуляция выпуклого многоугольника (простой метод)
        /// </summary>
        private TriangulationResult TriangulateConvex(List<Vector2> polygon)
        {
            var result = new TriangulationResult();
            result.Vertices = polygon;
            
            var triangles = new List<int>();
            
            for (int i = 1; i < polygon.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
            
            result.Triangles = triangles;
            return result;
        }
        
        private int GetPreviousIndex(List<int> indices, int currentIndex)
        {
            return currentIndex == 0 ? indices[indices.Count - 1] : indices[currentIndex - 1];
        }
        
        private int GetNextIndex(List<int> indices, int currentIndex)
        {
            return currentIndex == indices.Count - 1 ? indices[0] : indices[currentIndex + 1];
        }
        
        private bool IsConvexAngle(Vector2 a, Vector2 b, Vector2 c, bool clockwise)
        {
            float cross = Cross(b - a, c - b);
            
            if (clockwise)
                return cross <= 0; // Для ориентации по часовой стрелке
            else
                return cross >= 0; // Для ориентации против часовой стрелки
        }
        
        private bool IsClockwise(List<Vector2> polygon, List<int> indices)
        {
            float sum = 0;
            for (int i = 0; i < indices.Count; i++)
            {
                Vector2 a = polygon[indices[i]];
                Vector2 b = polygon[indices[(i + 1) % indices.Count]];
                sum += (b.x - a.x) * (b.y + a.y);
            }
            return sum < 0;
        }
        
        private float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
        
        private bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float sign1 = Cross(b - a, p - a);
            float sign2 = Cross(c - b, p - b);
            float sign3 = Cross(a - c, p - c);
            
            bool hasNegative = (sign1 < 0) || (sign2 < 0) || (sign3 < 0);
            bool hasPositive = (sign1 > 0) || (sign2 > 0) || (sign3 > 0);
            
            return !(hasNegative && hasPositive);
        }
        
        private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            var inside = false;
            
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (polygon[i].y > point.y != polygon[j].y > point.y &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / 
                    (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }
            
            return inside;
        }

        private void GenerateWall()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
    
            var index = 0;
            var vertexCount = _polygon.Vertices.Count;
    
            for (var i = 0; i < vertexCount; i++)
            {
                var v0 = _polygon.Vertices[i];
                var v1 = _polygon.Vertices[(i + 1) % vertexCount];
        
                vertices.Add(new Vector3(v0.x, 0, v0.y));
                vertices.Add(new Vector3(v1.x, 0, v1.y));
                vertices.Add(new Vector3(v1.x, -_settings.Thickness, v1.y));
                vertices.Add(new Vector3(v0.x, -_settings.Thickness, v0.y));

                var indices = new List<int> { 0, 2, 1, 0, 3, 2 };
                
                indices.ForEach(t => triangles.Add(index + t));
        
                index += 4;
            }
    
            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
                uv = CalculateShadowUVs(vertexCount).ToArray()
            };
    
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            wallMeshFilter.mesh = mesh;
        }
        
        private static List<Vector2> CalculateShadowUVs(int vertexCount)
        {
            var uvs = new List<Vector2>();
            for (var i = 0; i < vertexCount; i++)
            {
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(0, 1));
            }
            return uvs;
        }

        private void Update()
        {
            if (!_isHiding)
                return;
            _y -= Time.deltaTime * _settings.HideSpeed;
            transform.localPosition = Vector3.up * _y;
            if (_y < -_settings.HideDepth)
            {
                Destroy(gameObject);
            }
        }

        public void Hide()
        {
            _isHiding = true;
        }
        
        public class Factory : PlaceholderFactory<Floe>
        {
            [Inject(Id = "FloeRoot")] private readonly Transform _floeRoot;
    
            public override Floe Create()
            {
                var floe = base.Create();
                floe.transform.SetParent(_floeRoot, false);
                return floe;
            }
        }
    }
}