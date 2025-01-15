using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller {
    [SerializeField] private BoardSettings _boardSettings;

    public override void InstallBindings() {
        Container.BindInstance(_boardSettings).AsSingle();
        Container.Bind<GameBoard>().AsSingle();
    }
}
