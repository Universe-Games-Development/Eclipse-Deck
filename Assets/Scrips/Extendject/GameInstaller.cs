using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller {

    [SerializeField] private GameObject eventManagerPrefab;
    [SerializeField] private GameObject interactionManagerPrefab;
    [SerializeField] private GameObject resourseManagerPrefab;
    public override void InstallBindings() {
        Container.Bind<IEventManager>().To<EventManager>().FromComponentInNewPrefab(eventManagerPrefab).AsSingle().NonLazy();
        Container.Bind<InteractionManager>().FromComponentInNewPrefab(interactionManagerPrefab).AsSingle().NonLazy();
        Container.Bind<ResourceManager>().FromComponentInNewPrefab(resourseManagerPrefab).AsSingle().NonLazy();

        Container.Bind<UIInfo>().FromComponentInHierarchy().AsSingle();
    }
}
