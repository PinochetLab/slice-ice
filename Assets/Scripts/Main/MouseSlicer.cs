using System.Collections;
using System.Collections.Generic;
using Levels;
using UIs;
using UnityEngine;
using Zenject;

namespace Main
{
    public class MouseSlicer : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private LineRenderer incorrectLine;
        [SerializeField] private GameObject snowEffect;
        [SerializeField] private float snowDistance = 1f;
        [SerializeField] private int iceCubeCount = 10;
        
        [Inject] private IceController _iceController;
        [Inject] private ViewMaster _viewMaster;
        [Inject] private IceCubeSpawner _iceCubeSpawner;
        
        private bool _isSlicing;
        private readonly List<Vector2> _slicePoints = new ();
        private readonly List<Vector2> _screenPoints = new ();
        private readonly List<Vector3> _worldPositions = new ();
        private float _distance;

        private void Awake()
        {
            lineRenderer.enabled = false;
        }

        private void ApplyPoints(List<Vector3> positions)
        {
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }

        private List<Vector2> Shrink(List<Vector2> points, int count)
        {
            if (points == null || points.Count < 2)
                return points;
    
            if (count < 2)
                count = 2;
    
            if (count >= points.Count)
                return new List<Vector2>(points);
    
            List<Vector2> result = new List<Vector2>();
    
            // Calculate total path length
            float totalLength = 0;
            List<float> segmentLengths = new List<float>();
    
            for (int i = 0; i < points.Count - 1; i++)
            {
                float length = Vector2.Distance(points[i], points[i + 1]);
                segmentLengths.Add(length);
                totalLength += length;
            }
    
            // Always include first and last points
            result.Add(points[0]);
    
            // Calculate step length between samples
            float stepLength = totalLength / (count - 1);
            float currentLength = 0;
            int segmentIndex = 0;
    
            for (int i = 1; i < count - 1; i++)
            {
                float targetLength = stepLength * i;
        
                // Find which segment contains the target length
                while (segmentIndex < segmentLengths.Count && 
                       currentLength + segmentLengths[segmentIndex] < targetLength)
                {
                    currentLength += segmentLengths[segmentIndex];
                    segmentIndex++;
                }
        
                // Calculate position within the current segment
                float remainingLength = targetLength - currentLength;
                float t = remainingLength / segmentLengths[segmentIndex];
        
                Vector2 newPoint = Vector2.Lerp(points[segmentIndex], 
                    points[segmentIndex + 1], 
                    t);
                result.Add(newPoint);
            }
    
            // Add last point
            result.Add(points[points.Count - 1]);
    
            return result;
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
                var point = _viewMaster.ScreenToXZ(screenPos);
                var worldPos = _viewMaster.ScreenToWorld(screenPos, 10);

                var d = 0f;
                if (_slicePoints.Count > 0)
                {
                    d = Vector3.Distance(point, _slicePoints[^1]);
                }
        
                if (_slicePoints.Count == 0 || d > 0.1f)
                {
                    _distance += d;
                    if (_distance > snowDistance)
                    {
                        _distance = 0;
                        SpawnEffect();
                    }
                    _slicePoints.Add(point);
                    _screenPoints.Add(screenPos);
                    _worldPositions.Add(worldPos);
                    ApplyPoints(_worldPositions);
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

        private void SpawnEffect()
        {
            var screenPos = Input.mousePosition;
            var worldPos = _viewMaster.ScreenToWorld(screenPos, 10);
            Instantiate(snowEffect, worldPos, Quaternion.identity);
        }

        private void StopSlice()
        {
            if (HasIntersections(_slicePoints))
            {
                Shake();
            }
            else
            {
                var b = _iceController.TrySlice(_slicePoints, out var percentArea);
                if (!b)
                {
                    Shake();
                }
                else
                {
                    var screenPoints = Shrink(_screenPoints, iceCubeCount);
                    foreach (var screenPoint in screenPoints)
                    {
                        _iceCubeSpawner.Spawn(screenPoint, percentArea / iceCubeCount);
                    }
                }
            }
            SpawnEffect();
            _distance = 0;
            _screenPoints.Clear();
            _slicePoints.Clear();
            _worldPositions.Clear();
            _isSlicing = false;
            lineRenderer.enabled = false;
        }

        private void Shake()
        {
            StartCoroutine(ShakeCoroutine(0.3f, 0.05f));
        }
        
        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            incorrectLine.positionCount = lineRenderer.positionCount;
            incorrectLine.SetPositions(_worldPositions.ToArray());
            incorrectLine.enabled = true;
            
            var t = incorrectLine.transform;
            var originalPosition = t.position;
            var elapsed = 0f;
        
            while (elapsed < duration)
            {
                var x = Random.Range(-1f, 1f) * magnitude;
                var y = Random.Range(-1f, 1f) * magnitude;
            
                t.position = originalPosition + new Vector3(x, y, 0);
            
                elapsed += 0.05f;
                yield return new WaitForSeconds(0.05f);
            }
        
            t.position = originalPosition;
            incorrectLine.enabled = false;
        }
    }
}