using UnityEngine;
using Zenject;

public class ManagerInstaller : MonoInstaller {
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject mapManagerPrefab;
    [SerializeField] private GameObject levelManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;

    public override void InstallBindings() {
        Container.Bind<AudioManager>().FromComponentInNewPrefab(audioManagerPrefab).AsSingle().NonLazy();
        Container.Bind<MapManager>().FromComponentInNewPrefab(mapManagerPrefab).AsSingle().NonLazy();
        Container.Bind<LevelManager>().FromComponentInNewPrefab(levelManagerPrefab).AsSingle().NonLazy();
        Container.Bind<UIManager>().FromComponentInNewPrefab(uiManagerPrefab).AsSingle().NonLazy();

        Container.Bind<ResourceManager>().AsSingle().NonLazy();
        Container.Bind<IEventManager>().To<EventManager>().AsSingle().NonLazy();
    }
}
