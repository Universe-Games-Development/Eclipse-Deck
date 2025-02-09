using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {


    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();

        // Player
        Container.Bind<Player>().AsSingle().NonLazy();
        Container.Bind<ICommandFiller>().To<PlayerCommandFiller>().AsSingle().WhenInjectedInto<Player>();
        Container.Bind<PlayCardUI>().FromComponentInHierarchy().AsSingle();

        // Enemy
        Container.Bind<Enemy>().AsTransient();
        Container.Bind<ICommandFiller>().To<EnemyCommandFiller>().AsTransient().WhenInjectedInto<Enemy>();

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
