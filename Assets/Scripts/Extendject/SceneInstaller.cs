using Zenject;

public class CoreGameInstaller : MonoInstaller<CoreGameInstaller> {
    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<BattleRegistrator>().AsSingle().NonLazy();
        Container.Bind<BoardGame>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TravelManager>().FromComponentInHierarchy().AsSingle();
    }
}
