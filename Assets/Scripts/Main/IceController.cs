using System.Collections.Generic;
using System.Linq;
using Geometry;
using Penguins;
using UnityEngine;
using Zenject;

namespace Main
{
    public class IceController : MonoBehaviour
    {
        [SerializeField] private Transform floeRoot;
        
        private Floe.Factory _floeFactory;
        private Penguin.Factory _penguinFactory;
        private Polygon _polygon;
        private Floe _currentFloe;
        
        [Inject]
        public void Construct(Floe.Factory floeFactory, Penguin.Factory penguinFactory)
        {
            _floeFactory = floeFactory;
            _penguinFactory = penguinFactory;
        }
        
        private void Awake()
        {
            var vertices = new List<Vector2>() { 
                new(0, 0), new(0, 1), new(1, 1), 
                new(1, 2), new(2, 2), new(2, -2)
            };

            vertices = vertices.Select(v => 2 * v).ToList();

            _polygon = new Polygon(vertices);
            
            _currentFloe = _floeFactory.Create();
            
            _currentFloe.SetPolygon(_polygon);

            var penguin = _penguinFactory.Create();
            
            penguin.Polygon = _polygon;
            penguin.Position = new Vector2(1, 1);
            penguin.RandomizeDirection();
        }

        public void TrySlice(List<Vector2> slicePoints)
        {
            if (_polygon.TrySlice(slicePoints, out var parts))
            {
                parts = parts.OrderBy(p => -p.Area()).ToList();
                _polygon = parts[0];
                _currentFloe.SetPolygon(_polygon);
                for (var i = 1; i < parts.Count; i++)
                {
                    var a = _floeFactory.Create();
                    a.SetPolygon(parts[i]);
                    a.Hide();
                }
            }
        }
    }
}