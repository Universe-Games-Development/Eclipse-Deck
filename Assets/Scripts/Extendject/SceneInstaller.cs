using Zenject;

public class CoreGameInstaller : MonoInstaller<CoreGameInstaller> {
    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<TravelManager>().AsSingle().NonLazy();
        Container.Bind<BattleRegistrator>().AsSingle().NonLazy();
        Container.Bind<BoardGame>().FromComponentInHierarchy().AsSingle();
        
    }
}
