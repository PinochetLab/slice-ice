using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Levels;
using Penguins;
using UIs;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Main
{
    public class IceController : MonoBehaviour
    {
        [SerializeField] private float minDistance = 0.05f;
        [SerializeField] private float offsetRadius = 0.05f;
        [SerializeField] private float offsetSpeed = 0.02f;

        [Inject] private SmartSpawner _smartSpawner;
        [Inject] private ViewMaster _viewMaster;
        [Inject] private LevelTarget _levelTarget;
        
        private Polygon _polygon;
        private Floe _currentFloe;
        private float _startArea;
        private readonly List<Floe> _floes = new ();
        private readonly List<Slipper> _penguins = new ();
        private readonly List<Slipper> _seals = new ();
        private Vector3 _offset;
        private Vector3 _targetOffset;

        private void SetOffset()
        {
            _targetOffset = Random.insideUnitSphere * offsetRadius;
        }

        private void Update()
        {
            //return;
            _offset = Vector3.MoveTowards(_offset, _targetOffset, offsetSpeed * Time.deltaTime);
            foreach (var floe in _floes)
            {
                floe.SetOffset(_offset);
            }
            if (_offset == _targetOffset)
            {
                SetOffset();
            }
        }

        public void ClearLevel()
        {
            foreach (var floe in _floes)
            {
                floe.Unused = true;
                floe.gameObject.SetActive(false);
            }
            
            foreach (var penguin in _penguins)
            {
                penguin.Unused = true;
                penguin.gameObject.SetActive(false);
            }
            
            foreach (var seal in _seals)
            {
                seal.Unused = true;
                seal.gameObject.SetActive(false);
            }
        }

        public void GenerateLevel(Level level)
        {
            var vertices = level.GetPoints();

            _polygon = new Polygon(vertices);
            
            _currentFloe = _smartSpawner.SpawnFloe();
            _floes.Add(_currentFloe);
            
            _currentFloe.SetPolygon(_polygon);

            _startArea = _polygon.Area();

            foreach (var position in level.GetPenguinsPositions())
            {
                var penguin = _smartSpawner.SpawnPenguin();
                penguin.Polygon = _polygon;
                penguin.Position = position;
                penguin.RandomizeDirection();
                penguin.transform.parent = _currentFloe.transform;
                _penguins.Add(penguin);
            }
            
            foreach (var position in level.GetSealsPositions())
            {
                var seal = _smartSpawner.SpawnSeal();
                seal.Polygon = _polygon;
                seal.Position = position;
                seal.RandomizeDirection();
                seal.transform.parent = _currentFloe.transform;
                _seals.Add(seal);
            }
            
            if (_seals.Count == 0)
            {
                _levelTarget.UnlockSeal();
            }
            
            SetOffset();
        }

        private bool TooCloseToPenguins(List<Vector2> line)
        {
            foreach (var penguin in _penguins)
            {
                var penguinPos = penguin.Position;
        
                foreach (var point in line)
                {
                    var distance = Vector2.Distance(point, penguinPos);
                    if (distance < minDistance)
                    {
                        return true;
                    }
                }
            }
    
            return false;
        }

        public bool TrySlice(List<Vector2> slicePoints, out float percentArea)
        {
            percentArea = 0;
            var slicePoints2 = slicePoints.Select(
                v => _viewMaster.WorldToXZ(new Vector3(v.x, 0, v.y) - _offset)).ToList();
            
            if (TooCloseToPenguins(slicePoints2))
            {
                return false;
            }
            
            if (!_polygon.TrySlice(slicePoints2, out var parts))
            {
                return false;
            }
            
            Polygon nextPart = null;
                
            foreach (var penguin in _penguins)
            {
                foreach (var t in parts)
                {
                    if (!t.IsInside(penguin.Position))
                    {
                        continue;
                    }
                    if (nextPart != null && t != nextPart)
                    {
                        return false;
                    }
                    nextPart = t;
                    break;
                }
            }

            if (nextPart == null)
            {
                return false;
            }

            _polygon = nextPart;
            parts.Remove(nextPart);
            _currentFloe.SetPolygon(_polygon);

            foreach (var penguin in _penguins)
            {
                penguin.Polygon = _polygon;
                penguin.Apply();
            }

            var floes = new List<Floe>();
                
            for (var i = 0; i < parts.Count; i++)
            {
                percentArea += parts[i].Area() / _startArea;
                var floe = _smartSpawner.SpawnFloe();
                _floes.Add(floe);
                floe.SetPolygon(parts[i]);
                floe.Hide();
                floes.Add(floe);
            }
            
            foreach (var seal in _seals.ToList())
            {
                if (!_polygon.IsInside(seal.Position))
                {
                    _seals.Remove(seal);

                    for (var i = 0; i < parts.Count; i++)
                    {
                        if (parts[i].IsInside(seal.Position))
                        {
                            floes[i].AddSlipper(seal);
                        }
                    }

                    seal.Die();
                }
                else
                {
                    seal.Polygon = _polygon;
                    seal.Apply();
                }
            }

            if (_seals.Count == 0)
            {
                _levelTarget.UnlockSeal();
            }

            return true;

        }
    }
}