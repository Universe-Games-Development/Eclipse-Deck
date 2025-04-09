using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TestingBoard : MonoBehaviour
{
    [SerializeField] private BoardGame gameboardController;

    [SerializeField] private PlayerPresenter playerPresenter;
    [SerializeField] private EnemyPresenter enemyPresenter;

    [Header("Test")]
    [SerializeField] private BoardSettingsData boardConfig;
    [SerializeField] private int randomTaskAmount;
    [Range(0, 10)]
    [SerializeField] private int randomSizeOffset;
    [SerializeField] private bool TesOn;


    // GAme context
    [Inject] CommandManager commandManager;

    private Enemy enemy;
    private Player player;

    private BattleRegistrator _registrator;
    GameEventBus eventBus;

    [Inject]
    public void Construct(BattleRegistrator registrator, GameEventBus eventBus) {
        _registrator = registrator;
        this.eventBus = eventBus;
    }


    private void SetRandomSize() {
        for (int i = 0; i < randomSizeOffset; i++) {
            int action = Random.Range(0, 6); // Випадковий вибір одного з шести методів

            switch (action) {
                case 0:
                    boardConfig.RemoveRow(Direction.South);
                    break;
                case 1:
                    boardConfig.AddRow(Direction.North);
                    break;
                case 2:
                    boardConfig.AddRow(Direction.South);
                    break;
                case 3:
                    boardConfig.RemoveRow(Direction.North);
                    break;
                case 4:
                    boardConfig.AddColumn(Direction.East);
                    break;
                case 5:
                    boardConfig.AddColumn(Direction.West);
                    break;
                case 6:
                    boardConfig.RemoveColumn(Direction.West);
                    break;
                case 7:
                    boardConfig.RemoveColumn(Direction.West);
                    break;
            }
        }
    }

    public List<int> GenerateRandomList(int minSize, int maxSize) {
        List<int> randomList = new();

        // Генеруємо випадковий розмір списку в межах заданого діапазону
        int listSize = Random.Range(minSize, maxSize + 1);

        // Заповнюємо список випадковими значеннями 0 або 1
        for (int i = 0; i < listSize; i++) {
            randomList.Add(Random.Range(0, 2));
        }

        return randomList;
    }

    private void OnDestroy() {
        boardConfig.ResetSize();
    }
}
