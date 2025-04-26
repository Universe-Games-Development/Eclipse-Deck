using Zenject;

public class CoreGameInstaller : MonoInstaller<CoreGameInstaller> {
    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<PlayerHeroFactory>().AsSingle().NonLazy();
        Container.Bind<OpponentRegistrator>().AsSingle().NonLazy();
        Container.Bind<BoardGame>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TravelManager>().FromComponentInHierarchy().AsSingle();
        
    }
}
