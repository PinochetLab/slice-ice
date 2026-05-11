using Levels;
using Main;
using UIs;
using UnityEngine;
using Zenject;

namespace DI
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private IceController iceController;
        [SerializeField] private SmartSpawner smartSpawner;
        [SerializeField] private ViewMaster viewMaster;
        [SerializeField] private IceCubeSpawner iceCubeSpawner;
        [SerializeField] private IceCube iceCubePrefab;
        [SerializeField] private Transform iceCubeRoot;
        [SerializeField] private LevelTarget levelTarget;
        
        public override void InstallBindings()
        {
            Container.Bind<IceController>().FromInstance(iceController).AsSingle();
            
            Container.Bind<SmartSpawner>().FromInstance(smartSpawner).AsSingle();
            
            Container.Bind<ViewMaster>().FromInstance(viewMaster).AsSingle();
            
            Container.Bind<IceCubeSpawner>().FromInstance(iceCubeSpawner).AsSingle();
            
            Container.Bind<LevelTarget>().FromInstance(levelTarget).AsSingle();
            
            Container.BindFactory<IceCube, IceCube.Factory>()
                .FromComponentInNewPrefab(iceCubePrefab);
            Container.Bind<Transform>().WithId("IceCubeRoot").FromInstance(iceCubeRoot).AsCached();
        }
    }
}