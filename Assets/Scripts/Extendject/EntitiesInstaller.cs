using Zenject;

public class EntitiesInstaller : MonoInstaller<EntitiesInstaller> {
    public override void InstallBindings() {
        Container.Bind<RoomPresenter>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerPresenter>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<EnemyPresenter>().FromComponentInHierarchy().AsSingle();
        Container.Bind<AnimationsDebugSettings>().FromComponentInHierarchy().AsSingle();
    }
}
