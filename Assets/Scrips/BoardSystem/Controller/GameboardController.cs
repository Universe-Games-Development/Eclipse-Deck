using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Zenject.SpaceFighter;

public class GameboardController : MonoBehaviour {
    //DEBUG
    [SerializeField] private HealthCellController playerCell;
    [SerializeField] private HealthCellController enemyCell;
    [SerializeField] private PlayerController player_c;
    [SerializeField] private EnemyController enemy_c;

    [SerializeField] private BoardSettingsSO boardConfig;
    [SerializeField] private int taskDelay = 250;

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
        
        boardConfig.ResetSettings();
        
        await UniTask.Delay(taskDelay);
        boardConfig.SetEastColumns(new List<int> { 0, 1, 1, 0 });
        await gridManager.UpdateGrid(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.SetEastColumns(new List<int> { 0, 1, 1, 1 });
        await gridManager.UpdateGrid(boardConfig);
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

        Debug.Log($"Selected : {field.GetTextCoordinates()}");
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

    private void OnDestroy() {
        if (boardConfig != null) {
            boardConfig.ResetSettings();
        }
    }

    public bool PlayCard(Opponent opponent, Card selectedCard, Field field) {
        // Determine is that creature or spell

        // if creature summon it
        Creature creature = null;
        return gameBoard.SummonCreature(opponent, field, creature);
    }
}