using Zenject;

public class CoreGameInstaller : MonoInstaller<CoreGameInstaller> {
    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<BattleManager>().AsSingle();
        Container.Bind<BatttleActionManager>().AsSingle().NonLazy();
        Container.Bind<TravelManager>().AsSingle().NonLazy();
        Container.Bind<OpponentRegistrator>().AsSingle().NonLazy();
        // Container.BindInterfacesAndSelfTo<GameInitializer>().AsSingle(); // пъљю К
    }
}
