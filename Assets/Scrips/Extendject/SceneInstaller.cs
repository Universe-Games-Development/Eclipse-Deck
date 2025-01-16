using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller> {
    [SerializeField] private BoardSettings boardConfig;

    public override void InstallBindings() {
        // ����'����� BoardSettings �� Singleton
        Container.BindInstance(boardConfig).AsSingle();
        // ����'����� BoardOverseer �� Singleton, ������� �� ���'������ � ���������� ������ ���
        Container.Bind<OpponentManager>().AsSingle();

        // ����'����� GameBoard �� Transient, ��� ������� ���� ���������� ���� ����
        Container.Bind<GameBoard>().AsTransient();
        Container.Bind<GridManager>().AsSingle();
    }
}
