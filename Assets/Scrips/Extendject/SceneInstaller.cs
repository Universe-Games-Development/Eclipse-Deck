using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {


    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();

        // Player
        Container.Bind<Player>().AsSingle();
        Container.Bind<ICardsInputFiller>().To<CardInputUI>().FromComponentInHierarchy().AsSingle().WhenInjectedInto<Player>();

        // Enemy
        Container.Bind<Enemy>().AsTransient();
        Container.Bind<ICardsInputFiller>().To<EnemyCommandFiller>().AsTransient().WhenInjectedInto<Enemy>();
        
        Container.Bind<IInputRequirementRegistry>()
            .To<InputRequirementRegistry>()
            .AsSingle();

        // Gameboard
        Container.Bind<GameboardController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<BoardUpdater>().AsSingle();
        Container.Bind<BoardAssigner>().AsSingle();

        Container.Bind<GridVisual>().FromComponentInHierarchy().AsSingle();

        // Creature Movement
        Container.Bind<CreatureNavigator>().AsSingle();

        Container.Bind<BattleManager>().AsSingle().NonLazy();
        Container.Bind<OpponentRegistrator>().AsSingle().NonLazy();
    }

}
