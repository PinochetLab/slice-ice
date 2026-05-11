using System.Collections.Generic;
using System.Linq;
using Geometry;
using Penguins;
using UnityEngine;
using Zenject;

namespace Main
{
    public class Floe : MonoBehaviour, IRespawnable
    {
        [SerializeField] private MeshFilter floeMeshFilter;
        [SerializeField] private MeshFilter wallMeshFilter;
        [SerializeField] private PolygonCollider2D polygonCollider2D;

        [SerializeField] private float thickness = 0.7f;
        [SerializeField] private float hideDepth = 0.4f;
        [SerializeField] private float hideSpeed = 1f;
        
        public float HideSpeed => hideSpeed;

        private Polygon _polygon;
        private Vector3 _offset;
        private float _y;
        private bool _isSinking;
        private readonly List<Slipper> _slippers = new ();

        public void SetOffset(Vector3 offset)
        {
            _offset = offset;
        }

        public void SetPolygon(Polygon polygon)
        {
            _polygon = polygon;
            
            UpdateMesh();
            GenerateWall();
        }

        public void AddSlipper(Slipper slipper)
        {
            _slippers.Add(slipper);
            slipper.transform.parent = transform;
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
            vertices = mesh.vertices.Select(v => (Vector2)(v - _offset)).ToList();
            //vertices = mesh.vertices.Select(v => (Vector2)v).ToList();
            
            mesh.vertices = vertices.Select(v => new Vector3(v.x, 0f, v.y)).ToArray();
            mesh.uv = vertices.ToArray();
            
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            
            polygonCollider2D.enabled = false;
            floeMeshFilter.mesh = mesh;
        }

        private void GenerateWall()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            var uvX = 0f;
    
            var index = 0;
            var vertexCount = _polygon.Vertices.Count;
    
            for (var i = 0; i < vertexCount; i++)
            {
                var v0 = _polygon.Vertices[i];
                var v1 = _polygon.Vertices[(i + 1) % vertexCount];
                var distance = Vector2.Distance(v0, v1);
        
                vertices.Add(new Vector3(v0.x, 0, v0.y));
                vertices.Add(new Vector3(v1.x, 0, v1.y));
                vertices.Add(new Vector3(v1.x, -thickness, v1.y));
                vertices.Add(new Vector3(v0.x, -thickness, v0.y));

                var uv2 = uvX + distance;
                
                uvs.Add(new Vector2(uvX, 0));
                uvs.Add(new Vector2(uv2, 0));
                uvs.Add(new Vector2(uv2, thickness));
                uvs.Add(new Vector2(uvX, thickness));

                var indices = new List<int> { 0, 2, 1, 0, 3, 2 };
                
                indices.ForEach(t => triangles.Add(index + t));
        
                index += 4;
                uvX += distance;
            }
    
            var mesh = new Mesh
            {
                vertices = vertices.Select(v => v).ToArray(),
                triangles = triangles.ToArray(),
                uv = uvs.ToArray()
            };
    
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            wallMeshFilter.mesh = mesh;
        }
        
        private void Update()
        {
            Move();
        }

        private void Move()
        {
            if (!_isSinking)
            {
                transform.position = _offset;
                return;
            }
            _y -= Time.deltaTime * hideSpeed;
            transform.position = _offset + Vector3.up * _y;
            if (_y < -hideDepth)
            {
                Unused = true;
                foreach (var slipper in _slippers)
                {
                    slipper.Unused = true;
                }
                gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            _isSinking = true;
        }

        public bool Unused { get; set; }
        public void Init()
        {
            _isSinking = false;
            _y = 0;
            transform.position = Vector3.zero;
        }
    }
}