using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller {

    [SerializeField] private GameObject eventManagerPrefab;
    [SerializeField] private GameObject interactionManagerPrefab;
    [SerializeField] private GameObject resourseManagerPrefab;

    public override void InstallBindings() {
        var eventManager = Container.InstantiatePrefabForComponent<EventManager>(eventManagerPrefab);
        DontDestroyOnLoad(eventManager.gameObject);
        Container.Bind<IEventManager>().FromInstance(eventManager).AsSingle().NonLazy();

        var interactionManager = Container.InstantiatePrefabForComponent<InteractionManager>(interactionManagerPrefab);
        DontDestroyOnLoad(interactionManager.gameObject);
        Container.Bind<InteractionManager>().FromInstance(interactionManager).AsSingle().NonLazy();

        var resourceManager = Container.InstantiatePrefabForComponent<ResourceManager>(resourseManagerPrefab);
        DontDestroyOnLoad(resourceManager.gameObject);
        Container.Bind<ResourceManager>().FromInstance(resourceManager).AsSingle().NonLazy();
    }
}
