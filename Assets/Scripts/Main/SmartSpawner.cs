using Penguins;
using UnityEngine;
using Zenject;

namespace Main
{
    public class SmartSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject floePrefab;
        [SerializeField] private GameObject penguinPrefab;
        [SerializeField] private GameObject sealPrefab;

        private Spawner<Floe> _floeSpawner;
        private Spawner<Penguin> _penguinSpawner;
        private Spawner<Seal> _sealSpawner;

        [Inject]
        public void Construct()
        {
            _floeSpawner = new Spawner<Floe>(floePrefab);
            _penguinSpawner = new Spawner<Penguin>(penguinPrefab);
            _sealSpawner = new Spawner<Seal>(sealPrefab);
        }

        public Floe SpawnFloe()
        {
            return _floeSpawner.Spawn();
        }
        
        public Penguin SpawnPenguin()
        {
            return _penguinSpawner.Spawn();
        }
        
        public Seal SpawnSeal()
        {
            return _sealSpawner.Spawn();
        }
    }
}