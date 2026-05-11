using Main;
using Penguins;
using UnityEngine;
using Zenject;

namespace UIs
{
    public class IceCubeSpawner : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private GameObject iceCubePrefab;
        [SerializeField] private Transform iceCubeRoot;

        private IceCube.Factory _iceCubeFactory;
        
        [Inject]
        public void Construct(IceCube.Factory iceCubeFactory)
        {
            _iceCubeFactory = iceCubeFactory;
        }

        public void Spawn(Vector2 screenPosition, float score)
        {
            var iceCube = _iceCubeFactory.Create();
            iceCube.Drop(target, screenPosition, score);
        }
    }
}