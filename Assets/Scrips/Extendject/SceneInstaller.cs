using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {
    [SerializeField] private BoardSettings boardConfig;

    public override void InstallBindings() {
        // Прив'язуємо BoardSettings як Singleton
        Container.BindInstance(boardConfig).AsSingle();
        // Прив'язуємо BoardOverseer як Singleton, оскільки він пов'язаний з конкретним станом гри
        Container.Bind<OpponentManager>().AsSingle();

        // Прив'язуємо GameBoard як Transient, щоб кожного разу створювати нову копію
        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<GridManager>().AsSingle();
    }
}
