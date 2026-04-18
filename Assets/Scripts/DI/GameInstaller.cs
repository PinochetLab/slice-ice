using Main;
using Penguins;
using UnityEngine;
using Zenject;

namespace DI
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private IceController iceController;
        [SerializeField] private Floe floePrefab;
        [SerializeField] private Transform floeRoot;
        [SerializeField] private Penguin penguinPrefab;
        [SerializeField] private Transform penguinRoot;
        [SerializeField] private SettingsProvider settingsProvider;
        
        public override void InstallBindings()
        {
            Container.Bind<IceController>().FromInstance(iceController).AsSingle();
            
            Container.BindFactory<Floe, Floe.Factory>()
                .FromComponentInNewPrefab(floePrefab);
            Container.Bind<Transform>().WithId("FloeRoot").FromInstance(floeRoot).AsCached();
            
            Container.BindFactory<Penguin, Penguin.Factory>()
                .FromComponentInNewPrefab(penguinPrefab);
            Container.Bind<Transform>().WithId("PenguinRoot").FromInstance(penguinRoot).AsCached();
            
            Container.Bind<SettingsProvider>().FromInstance(settingsProvider).AsSingle();
        }
    }
}