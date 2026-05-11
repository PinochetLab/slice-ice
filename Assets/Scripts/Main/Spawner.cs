using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Main
{
    public class Spawner<T> where T : MonoBehaviour, IRespawnable
    {
        private readonly GameObject _prefab;

        private readonly List<T> _ts = new ();
        
        public Spawner(GameObject prefab, Transform root = null)
        {
            _prefab = prefab;
        }

        public T Spawn()
        {
            T t;
            if (_ts.Any(t => t.Unused))
            {
                t = _ts.First(t => t.Unused);
            }
            else
            {
                t = Object.Instantiate(_prefab).GetComponent<T>();
                _ts.Add(t);
            }
            t.Init();
            t.Unused = false;
            t.gameObject.SetActive(true);
            return t;
        }
    }
}