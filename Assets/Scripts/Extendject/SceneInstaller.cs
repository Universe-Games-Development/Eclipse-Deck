using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {
    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<BattleManager>().AsSingle();
        Container.Bind<BatttleActionManager>().AsSingle().NonLazy();
        
        Container.Bind<DungeonMapUIController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<RoomPresenter>().FromComponentInHierarchy().AsSingle();
        Container.Bind<EnemyPresenter>().FromComponentInHierarchy().AsSingle();
        
        Container.Bind<PlayerPresenter>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<TravelManager>().AsSingle().NonLazy();

        Container.Bind<IActionFiller>().To<ActionInputSystem>().FromComponentInHierarchy().AsSingle().WhenInjectedInto<Player>();

        Container.Bind<IActionFiller>().To<EnemyInputSystem>().AsTransient().WhenInjectedInto<Enemy>();

        

        Container.Bind<IDungeonGenerator>().To<DungeonGenerator>().AsSingle();

        Container.Bind<DungeonVisualizer>().FromComponentInHierarchy().AsCached().Lazy();

        

        // Gameboard
        //Container.Bind<GameBoardPresenter>().FromComponentInHierarchy().AsSingle();
        //Container.Bind<CardPlayService>().AsSingle();
        Container.Bind<GameboardBuilder>().AsSingle();
        Container.Bind<BoardAssigner>().AsSingle();

        // Creature Movement
        Container.Bind<CreatureNavigator>().AsSingle();
        Container.Bind<CreatureBehaviour>().AsTransient();

        Container.Bind<OpponentRegistrator>().AsSingle().NonLazy();
        //Container.BindInterfacesAndSelfTo<GameInitializer>().AsSingle();

        Container.Bind<DialogueSystem>().FromComponentInHierarchy().AsSingle();
    }
}
