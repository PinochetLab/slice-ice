using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Main
{
    public class MouseSlicer : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        
        [Inject] private IceController _iceController;
        
        private bool _isSlicing;
        private readonly List<Vector2> _slicePoints = new ();
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
            lineRenderer.enabled = false;
        }

        private void ApplyPoints(List<Vector2> points)
        {
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.Select(x => (Vector3)x).ToArray());
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartSlice();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                StopSlice();
            }
            if (_isSlicing)
            {
                var screenPos = Input.mousePosition;
                var worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
    
                Vector2 point = _mainCamera.transform.InverseTransformPoint(worldPos);
        
                if (_slicePoints.Count == 0 || Vector3.Distance(point, _slicePoints[^1]) > 0.1f)
                {
                    _slicePoints.Add(point);
                    ApplyPoints(_slicePoints);
                }
            }
        }

        private void StartSlice()
        {
            _isSlicing = true;
            lineRenderer.enabled = true;
        }
        
        private bool LineSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            var a = a2 - a1;
            var b = b2 - b1;
            var c = b1 - a1;

            var denominator = Cross(a, b);

            if (denominator == 0)
                return false;

            var t = Cross(c, b) / denominator;
            var u = Cross(c, a) / denominator;

            return t is >= 0 and <= 1 && u is >= 0 and <= 1;
        }

        private float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        private bool HasIntersections(List<Vector2> points)
        {
            for (var i = 0; i < points.Count - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                
                for (var j = i + 2; j < points.Count - 1; j++)
                {
                    var c = points[j];
                    var d = points[j + 1];

                    if (LineSegmentsIntersect(a, b, c, d))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void StopSlice()
        {
            if (!HasIntersections(_slicePoints))
                _iceController.TrySlice(_slicePoints);
            _slicePoints.Clear();
            _isSlicing = false;
            lineRenderer.enabled = false;
        }
    }
}