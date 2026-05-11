using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Levels
{
    public class Level : MonoBehaviour
    {
        [SerializeField] private Transform floePointsRoot;
        [SerializeField] private Transform penguinPointsRoot;
        [SerializeField] private Transform sealsPointsRoot;

        [Inject] private ViewMaster _viewMaster;

        private Camera _camera;

        private Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                }

                return _camera;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (floePointsRoot.childCount < 2) return;
            
            Gizmos.color = Color.cyan;
            
            for (var i = 0; i < floePointsRoot.childCount; i++)
            {
                var a = floePointsRoot.GetChild(i).position;
                var b = floePointsRoot.GetChild((i + 1) % floePointsRoot.childCount).position;
                Gizmos.DrawLine(a, b);
            }
            
            Gizmos.color = Color.green;

            foreach (Transform child in penguinPointsRoot)
            {
                Gizmos.DrawSphere(child.position, 20f);
            }
            
            Gizmos.color = Color.red;
            
            foreach (Transform child in sealsPointsRoot)
            {
                Gizmos.DrawSphere(child.position, 20f);
            }
        }

        public List<Vector2> GetPoints()
        {
            var points = new List<Vector2>();
            
            foreach (Transform child in floePointsRoot)
            {
                var rt = child.GetComponent<RectTransform>();
                var position = rt.anchoredPosition + new Vector2(Screen.width, Screen.height) / 2;
                points.Add(_viewMaster.ScreenToXZ(position));
            }

            return points;
        }

        public List<Vector2> GetPenguinsPositions()
        {
            var positions = new List<Vector2>();
            
            foreach (Transform child in penguinPointsRoot)
            {
                var rt = child.GetComponent<RectTransform>();
                var position = rt.anchoredPosition + new Vector2(Screen.width, Screen.height) / 2;
                positions.Add(_viewMaster.ScreenToXZ(position));
            }

            return positions;
        }
        
        public List<Vector2> GetSealsPositions()
        {
            var positions = new List<Vector2>();
            
            foreach (Transform child in sealsPointsRoot)
            {
                var rt = child.GetComponent<RectTransform>();
                var position = rt.anchoredPosition + new Vector2(Screen.width, Screen.height) / 2;
                positions.Add(_viewMaster.ScreenToXZ(position));
            }

            return positions;
        }
    }
}