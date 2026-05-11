using Geometry;
using Main;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Penguins
{
    public abstract class Slipper : MonoBehaviour, IRespawnable
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float spinSpeed = 1000f;
        [SerializeField] private float dieSpinSpeed = 500f;
        [SerializeField] private Animator animator;
        
        public Polygon Polygon { get; set; }

        public Vector2 Position { get; set; }

        private Vector2 Direction { get; set; }

        private float _currentDistance;
        
        private Quaternion _targetRotation;
        
        private Ricochet _currentRicochet;

        private bool _isDead;

        public void RandomizeDirection()
        {
            var angle = Random.Range(0, 2 * Mathf.PI);
            Direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            transform.localRotation = Quaternion.FromToRotation(Vector2.right, Direction);
            Apply();
            transform.localRotation = _targetRotation;
        }

        private void UpdatePosition()
        {
            transform.localPosition = new Vector3(Position.x, 0, Position.y);
        }
        
        private void UpdateRotation()
        {
            var forward = new Vector3(Direction.x, 0, Direction.y);
            _targetRotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        public void Apply()
        {
            UpdatePosition();
            UpdateRotation();
            
            animator.Play("Shrink");
            
            _currentDistance = 0;
            _currentRicochet = Polygon.GetRicochet(Position, Direction);
        }

        private void Update()
        {
            if (!_isDead)
            {
                var deltaPos = speed * Time.deltaTime;
                _currentDistance += deltaPos;
                Position += Direction * deltaPos;
                transform.localRotation = Quaternion.RotateTowards(
                    transform.localRotation,
                    _targetRotation, 
                    spinSpeed * Time.deltaTime);
                UpdatePosition();

                if (_currentDistance > _currentRicochet.Distance)
                {
                    Position = _currentRicochet.NewPosition;
                    Direction = _currentRicochet.NewDirection;
                    Apply();
                }
            }
            else
            {
                transform.Rotate(Vector3.up, Time.deltaTime * dieSpinSpeed);
            }
        }

        public void Die()
        {
            _isDead = true;
        }

        public bool Unused { get; set; }
        public void Init()
        {
            _isDead = false;
            _currentDistance = 0;
        }
    }
}