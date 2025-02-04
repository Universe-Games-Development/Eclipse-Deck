using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class GameboardController : MonoBehaviour {
    //DEBUG
    [SerializeField] private HealthCellController playerCell;
    [SerializeField] private HealthCellController enemyCell;
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
    [Inject] ResourceManager resManager;
    [Inject] GridVisual tableController;

    private Enemy enemy;
    private Player player;

    [Inject] OpponentRegistrator OpponentRegistrator;

    public void Construct(OpponentRegistrator opponentRegistrator) {
        OpponentRegistrator = opponentRegistrator;
        OpponentRegistrator.OnOpponentRegistered += AssignHPCellToOpponent;
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

        Card playerCard = player.GetTestCard();
        Card enemyCard = enemy.GetTestCard();

        CreatureSO data = resManager.GetRandomResource<CreatureSO>(ResourceType.CREATURE);

        Creature playerCreature = new(playerCard, data);
        Creature enemyCreature = new(enemyCard, data);

        if (gridManager.GridBoard != null) {
            Field fieldToPlace = gridManager.GridBoard.GetFieldAt(-1, -1);
            gameBoard.SummonCreature(player, fieldToPlace, playerCreature);
        }

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
            int action = Random.Range(0, 6); // Випадковий вибір одного з шести методів

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


    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }

    public Field GetFieldByWorldPosition(Vector3? mouseWorldPosition) {
        if (!mouseWorldPosition.HasValue) {
            return null;
        }
        Vector2Int? indexes = tableController.GetGridIndex(mouseWorldPosition.Value);
        if (!indexes.HasValue) {
            return null;
        }

        Field field = gridManager.GridBoard.GetFieldAt(indexes.Value.x, indexes.Value.y);

        if (!gameBoard.IsValidFieldSelected(field)) {
            if (field != null) {
                Debug.LogWarning($"Can`t select field : {field.GetTextCoordinates()}");
            }
        }

        if (field != null) {
            Debug.Log($"Selected : {field.GetTextCoordinates()}");
        }

        return field;
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


    public bool PlayCard(Opponent opponent, Card selectedCard, Field field) {
        // Determine is that creature or spell

        // if creature summon it
        Creature creature = null;
        return gameBoard.SummonCreature(opponent, field, creature);
    }

    private void OnDisable() {
        boardConfig.ResetSettings();
    }
}