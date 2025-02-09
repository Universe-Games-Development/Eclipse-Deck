using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TestingBoard : MonoBehaviour, IEventListener
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

    private BattleManager _battleManager;
    IEventQueue eventqueue;

    [Inject]
    public void Construct(BattleManager battleManager, IEventQueue eventQueue) {
        _battleManager = battleManager;
        this.eventqueue = eventQueue;
        eventQueue.RegisterListener(this, EventType.ON_TURN_END);
    }

    private void Start() {
        // DEBUG
        enemy = enemy_c.enemy;
        player = player_c.player;
        _battleManager.RegisterOpponentForBattle(player);
        _battleManager.RegisterOpponentForBattle(enemy);

        if (!TesOn) return;
        DebugLogic().Forget();
    }


    private async UniTask DebugLogic() {

        await gridManager.UpdateGrid(boardConfig);

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

    public object OnEventReceived(object data) {
        return data switch {
            TurnEndEventData turnEndData => GetPlayCardCommand(turnEndData),
            _ => new EmptyCommand(),
        };
    }

    private ICommand GetPlayCardCommand(TurnEndEventData turnEndData) {
        ICommand command = new EmptyCommand();
        if (turnEndData != null) {
            if (turnEndData.endTurnOpponent == player) {

                Card playerCard = player.hand.GetRandomCard();
                Card enemyCard = enemy.hand.GetRandomCard();

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

    private void OnDestroy() {
        boardConfig.ResetSize();
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
