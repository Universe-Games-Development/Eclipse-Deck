using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Testing : MonoBehaviour, IEventListener
{
    [SerializeField] private GameboardController gameboardController;

    [SerializeField] private PlayerController player_c;
    [SerializeField] private EnemyController enemy_c;

    [Header("Test")]
    [SerializeField] private BoardSettingsSO boardConfig;
    [SerializeField] private int taskDelay = 250;
    [SerializeField] private int randomTaskAmount;
    [Range(0, 10)]
    [SerializeField] private int randomSizeOffset;
    [SerializeField] private bool TesOn;

    // Scene Context
    [Inject] private GameBoard gameBoard;
    [Inject] private BoardUpdater gridManager;

    // GAme context
    [Inject] AddressablesResourceManager resManager;

    private Enemy enemy;
    private Player player;

    private EventQueue eventQueue;
    private OpponentRegistrator OpponentRegistrator;

    [Inject]
    public void Construct(OpponentRegistrator opponentRegistrator, EventQueue eventQueue) {
        OpponentRegistrator = opponentRegistrator;
        this.eventQueue = eventQueue;
    }

    private void Start() {
        // DEBUG
        enemy = enemy_c.enemy;
        player = player_c.player;
        OpponentRegistrator.RegisterOpponent(player);
        OpponentRegistrator.RegisterOpponent(enemy);

        DebugLogic().Forget();
    }


    private async UniTask DebugLogic() {

        await gridManager.UpdateGrid(boardConfig);

        if (!TesOn) return;
        await UniTask.Delay(taskDelay);
        boardConfig.RandomizeAllGrids();
        await gridManager.UpdateGrid(boardConfig);

        await RandomSizeSpawn();
    }

    private async UniTask RandomFieldSpawn() {
        for (int i = 0; i < randomTaskAmount; i++) {
            await UniTask.Delay(taskDelay);
            boardConfig.RandomizeAllGrids();
            await gridManager.UpdateGrid(boardConfig);
        }
    }

    private async UniTask RandomSizeSpawn() {
        while (randomTaskAmount > 0) {
            randomTaskAmount--;
            await UniTask.Delay(taskDelay);
            SetRandomSize();
            boardConfig.RandomizeAllGrids();
            await gridManager.UpdateGrid(boardConfig);
        }
    }

    private void SetRandomSize() {
        for (int i = 0; i < randomSizeOffset; i++) {
            int action = Random.Range(0, 6); // ���������� ���� ������ � ����� ������

            switch (action) {
                case 0:
                    boardConfig.AddColumn();
                    break;
                case 1:
                    boardConfig.AddNorthRow();
                    break;
                case 2:
                    boardConfig.AddSouthRow();
                    break;
                case 3:
                    boardConfig.RemoveColumn();
                    break;
                case 4:
                    boardConfig.RemoveNorthRow();
                    break;
                case 5:
                    boardConfig.RemoveSouthRow();
                    break;
            }
        }
    }

    public List<int> GenerateRandomList(int minSize, int maxSize) {
        List<int> randomList = new();

        // �������� ���������� ����� ������ � ����� �������� ��������
        int listSize = Random.Range(minSize, maxSize + 1);

        // ���������� ������ ����������� ���������� 0 ��� 1
        for (int i = 0; i < listSize; i++) {
            randomList.Add(Random.Range(0, 2));
        }

        return randomList;
    }

    public object OnEventReceived(object data) {
        ICommand command = new EmptyCommand();
        ;
        TurnEndEventData turnEndEvent = (TurnEndEventData)data;
        if (turnEndEvent != null) {
            if (turnEndEvent.activePlayer == player) {

                Card playerCard = player.GetTestCard();
                Card enemyCard = enemy.GetTestCard();

                if (playerCard != null || enemyCard != null) {
                    CreatureSO creatureData = resManager.GetRandomResource<CreatureSO>(ResourceType.CREATURE);

                    Creature playerCreature = new(playerCard, creatureData);
                    Creature enemyCreature = new(enemyCard, creatureData);

                    if (gridManager.GridBoard != null) {

                        Field fieldToPlace = gridManager.GridBoard.GetFieldAt(-1, -1);
                        command = new PlayCardCommand(player, playerCard, fieldToPlace);
                    }
                }
            }
        }
        return command;
    }
}


public class PlayCardCommand : ICommand {
    [Inject] GameboardController gameBoardController;
    private Player player;
    private Card playerCard;
    private Field fieldToPlace;

    public PlayCardCommand(Player player, Card playerCard, Field fieldToPlace) {
        this.player = player;
        this.playerCard = playerCard;
        this.fieldToPlace = fieldToPlace;
    }

    public async UniTask Execute() {
        Debug.Log("Play card command");
        gameBoardController.SummonCreature(player, playerCard, fieldToPlace);
        await UniTask.CompletedTask;
    }

    public async UniTask Undo() {
        await UniTask.CompletedTask;
    }
}
