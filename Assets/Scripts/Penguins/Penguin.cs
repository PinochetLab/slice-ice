using Geometry;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Penguins
{
    public class Penguin : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        
        public Polygon Polygon { get; set; }
        
        public Vector2 Position { get; set; }
        
        public Vector2 Direction { get; set; }

        private float _currentDistance;
        
        private Ricochet _currentRicochet;

        public void RandomizeDirection()
        {
            Direction = Random.insideUnitSphere.normalized;
            Apply();
        }

        private void UpdatePosition()
        {
            transform.localPosition = Position;
        }
        
        private void UpdateRotation()
        {
            transform.localRotation = Quaternion.FromToRotation(Vector2.right, Direction);
        }

        private void Apply()
        {
            UpdatePosition();
            UpdateRotation();
            
            _currentDistance = 0;
            _currentRicochet = Polygon.GetRicochet(Position, Direction);
        }

        private void Update()
        {
            var deltaPos = speed * Time.deltaTime;
            _currentDistance += deltaPos;
            Position += Direction * deltaPos;
            UpdatePosition();

            if (_currentDistance > _currentRicochet.Distance)
            {
                Position = _currentRicochet.NewPosition;
                Direction = _currentRicochet.NewDirection;
            }
        }
        
        public class Factory : PlaceholderFactory<Penguin>
        {
            [Inject(Id = "PenguinRoot")] private readonly Transform _penguinRoot;
    
            public override Penguin Create()
            {
                var penguin = base.Create();
                penguin.transform.SetParent(_penguinRoot, false);
                return penguin;
            }
        }
    }
}