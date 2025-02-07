using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {


    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();


        Container.Bind<Player>().AsSingle().NonLazy();
        Container.Bind<Enemy>().AsTransient();

        // Gameboard
        Container.Bind<GameboardController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<BoardUpdater>().AsSingle();
        Container.Bind<BoardAssigner>().AsSingle();
        

        Container.Bind<GridVisual>().FromComponentInHierarchy().AsSingle();
        Container.Bind<FieldPool>().FromComponentsInHierarchy().AsSingle();

        // Creature Movement
        Container.Bind<CreatureNavigator>().AsSingle();

        Container.Bind<BattleManager>().AsSingle().NonLazy();
        Container.Bind<OpponentRegistrator>().AsSingle().NonLazy();
    }
}
