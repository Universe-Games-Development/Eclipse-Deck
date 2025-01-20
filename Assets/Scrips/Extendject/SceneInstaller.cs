using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {
    

    public override void InstallBindings() {
        Container.Bind<OpponentManager>().AsSingle();

        
        Container.Bind<GameboardController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TableController>().FromComponentInHierarchy().AsSingle();

        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<GridManager>().AsSingle();
        Container.Bind<CommandManager>().AsTransient();

        Container.Bind<Player>().AsSingle().NonLazy();
        Container.Bind<Enemy>().AsTransient().NonLazy();
    }
}
