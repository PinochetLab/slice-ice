using System;
using System.Collections.Generic;
using System.Linq;
using FishAlive;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fishes
{
    public class FishSystem : MonoBehaviour
    {
        [SerializeField] private Transform fishRoot;
        [SerializeField] private Transform pointRoot;
        
        private readonly List<FishMotion> _fishes = new();
        private List<float> _times = new();
        private List<float> _durations = new();
        private List<Transform> _targets = new();
        private readonly List<Transform> _freeTargets = new();
        private readonly List<Transform> _busyTargets = new();
        
        private const float MinTime = 5f;
        private const float MaxTime = 60f;

        private void Awake()
        {
            foreach (Transform child in fishRoot)
            {
                var fish = child.GetComponent<FishMotion>();
                _fishes.Add(fish);
            }
            
            _targets = Enumerable.Repeat<Transform>(null, _fishes.Count).ToList();
            _times = Enumerable.Repeat(0f, _fishes.Count).ToList();
            _durations = Enumerable.Repeat(0f, _fishes.Count).ToList();
            
            foreach (Transform child in pointRoot)
            {
                child.localPosition += Vector3.up * Random.Range(-0.05f, 0.05f);
                _freeTargets.Add(child);
            }
            
            Enumerable.Range(0, _fishes.Count).ToList().ForEach(ProcessTime);
        }

        private Transform GetRandomPoint()
        {
            var point = _freeTargets[Random.Range(0, _freeTargets.Count)];
            return point;
        }

        private void ProcessTime(int i)
        {
            _times[i] = 0;
            _durations[i] = Random.Range(MinTime, MaxTime);
        }

        private void FreeTarget(int i)
        {
            var target = _targets[i];
            if (!target)
            {
                return;
            }
            _busyTargets.Remove(target);
            _freeTargets.Add(target);
        }

        private void BusyTarget(int i)
        {
            var target = _targets[i];
            _freeTargets.Remove(target);
            _busyTargets.Add(target);
        }

        private void SetTarget(int i)
        {
            var target = GetRandomPoint();
            FreeTarget(i);
            _fishes[i].target = target.gameObject;
            BusyTarget(i);
        }

        private void Update()
        {
            for (var i = 0; i < _fishes.Count; i++)
            {
                _times[i] += Time.deltaTime;
                if (_times[i] > _durations[i])
                {
                    ProcessTime(i);
                    SetTarget(i);
                }
            }
        }
    }
}