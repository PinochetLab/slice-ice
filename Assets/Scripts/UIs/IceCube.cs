using System;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace UIs
{
    public class IceCube : MonoBehaviour
    {
        [SerializeField] private float height = 200f;
        [SerializeField] private float minSpeed = 180f;
        [SerializeField] private float maxSpeed = 220f;
        [SerializeField] private float spinSpeed = 500f;
        
        private RectTransform _rectTransform;
        
        [Inject] private LevelTarget _levelTarget;

        private float _score;
        private float _a, _b, _c;
        private float _endX;
        private float _x;
        private bool _move;
        private float _speed;

        private RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

        public void Drop(RectTransform target, Vector2 screenPosition, float score)
        {
            RectTransform.anchoredPosition = screenPosition;
            _score = score;
            
            var v1 = RectTransform.anchoredPosition;
            var v2 = target.anchoredPosition;
            
            var x1 = v1.x;
            var y1 = v1.y;
            
            var x2 = v2.x;
            var y2 = v2.y;

            var h = y1 + height;

            var r = Mathf.Sqrt((h - y1) / (h - y2));

            _b = (x1 + x2 * r) / (1 + r);

            _c = h;

            _a = (h - y1) / Mathf.Pow(x1 - _b, 2);

            _x = x1;
            _endX = x2;
            
            Debug.Log($"v1: {v1}, v2: {v2}, _a: {_a}, _b: {_b}, _c:{_c}");

            _move = true;
            
            _speed = Random.Range(minSpeed, maxSpeed);
        }

        private void Update()
        {
            if (!_move)
            {
                return;
            }
            var dx = _speed * Time.deltaTime / Mathf.Sqrt(1 + Mathf.Pow(2 * _a * _x, 2));
            _x = Mathf.MoveTowards(_x, _endX, dx);
            var y = -_a * Mathf.Pow(_x - _b, 2) + _c;
            _rectTransform.anchoredPosition = new Vector2(_x, y);
            transform.eulerAngles += Vector3.forward * (spinSpeed * Time.deltaTime);
            if (Mathf.Approximately(_x, _endX))
            {
                _levelTarget.Add(_score);
                Destroy(gameObject);
            }
        }
        
        public class Factory : PlaceholderFactory<IceCube>
        {
            [Inject(Id = "IceCubeRoot")] private readonly Transform _iceCubeRoot;
    
            public override IceCube Create()
            {
                var iceCube = base.Create();
                iceCube.transform.SetParent(_iceCubeRoot, false);
                return iceCube;
            }
        }
    }
}