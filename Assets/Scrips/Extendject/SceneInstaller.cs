using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {


    public override void InstallBindings() {
        Container.Bind<OpponentRegistrator>().AsSingle();
        Container.Bind<TurnManager>().AsSingle();


        Container.Bind<CommandManager>().AsTransient();


        Container.Bind<Player>().AsSingle().NonLazy();
        Container.Bind<Enemy>().AsTransient().NonLazy();

        // Gameboard
        Container.Bind<GameboardController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<BoardUpdater>().AsSingle();
        Container.Bind<BoardAssigner>().AsSingle();
        

        Container.Bind<GridVisual>().FromComponentInHierarchy().AsSingle();
        Container.Bind<FieldPool>().FromComponentsInHierarchy().AsSingle();
    }
}
