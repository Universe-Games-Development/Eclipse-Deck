using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {
    [SerializeField] private BoardSettings boardConfig;

    public override void InstallBindings() {
        Container.BindInstance(boardConfig).AsSingle();
        Container.Bind<OpponentManager>().AsSingle();

        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<GridManager>().AsSingle();
        Container.Bind<CommandManager>().AsTransient();

        Container.Bind<Player>().AsSingle().NonLazy();
        Container.Bind<Enemy>().AsTransient().NonLazy();
    }
}
