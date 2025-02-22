using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {


    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<BattleManager>().AsSingle();
        Container.Bind<BatttleActionManager>().AsSingle().NonLazy();
        

        // Player
        Container.Bind<Player>().AsSingle();
        Container.Bind<IActionFiller>().To<AbilityInputSystem>().FromComponentInHierarchy().AsSingle().WhenInjectedInto<Player>();

        // Enemy
        Container.Bind<Enemy>().AsTransient();
        Container.Bind<IActionFiller>().To<EnemyInputSystem>().AsTransient().WhenInjectedInto<Enemy>();
        Container.Bind<CardPlayService>().AsSingle();
        Container.Bind<PlayManagerRegistrator>().AsSingle().NonLazy();
        

        // Gameboard
        Container.Bind<GameBoardController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<GameBoardManager>().AsSingle();
        Container.Bind<BoardAssigner>().AsSingle();

        Container.Bind<GridVisual>().FromComponentInHierarchy().AsSingle();

        // Creature Movement
        Container.Bind<CreatureNavigator>().AsSingle();
        Container.Bind<MovementStrategyFactory>().AsSingle();

        Container.Bind<OpponentRegistrator>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GameInitializer>().AsSingle();
    }

}
