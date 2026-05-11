using UnityEngine;

namespace Levels
{
    public class ViewMaster : MonoBehaviour
    {
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
        
        public Vector2 ScreenToXZ(Vector2 position)
        {
            var ray = Camera.ScreenPointToRay(position);
            var t = -ray.origin.y / ray.direction.y;
            var v3 = ray.origin + ray.direction * t;
            return new Vector2(v3.x, v3.z);
        }
        
        public Vector2 WorldToXZ(Vector3 position)
        {
            var ray = Camera.ScreenPointToRay(Camera.WorldToScreenPoint(position));
            var t = -ray.origin.y / ray.direction.y;
            var v3 = ray.origin + ray.direction * t;
            return new Vector2(v3.x, v3.z);
        }

        public Vector3 ScreenToWorld(Vector2 position, float distance)
        {
            return Camera.ScreenToWorldPoint(new Vector3(position.x, position.y, distance));
        }
    }
}