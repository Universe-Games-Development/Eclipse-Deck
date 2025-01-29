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

    [SerializeField] private GridSettings boardConfig;
    [SerializeField] private int taskDelay = 250;

    // Scene Context
    [Inject] private GameBoard gameBoard;
    [Inject] private GridManager gridManager;
    [Inject] private OpponentManager opponentManager;
    // GAme context
    [Inject] ResourceManager resManager;
    [Inject] GridVisual tableController;

    //[SerializeField] Transform debugObject;

    private void Awake() {
        gameBoard.SetBoardSettings(boardConfig);
    }

    private void Start() {
        DebugLogic().Forget();
    }

    private async UniTaskVoid DebugLogic() {
        //Ініціалізація

        Enemy enemy = enemy_c.enemy;
        Player player = player_c.player;
        gameBoard.OpponentManager.RegisterOpponent(player);
        gameBoard.OpponentManager.RegisterOpponent(enemy);
        /*
        await gameBoard.StartGame();
        gameBoard.SetCurrentPlayer(player);

        Card playerCard = player.GetTestCard();
        Card enemyCard = enemy.GetTestCard();

        CreatureSO data = resManager.GetRandomResource<CreatureSO>(ResourceType.CREATURE);

        Creature playerCreature = new(playerCard, data);
        Creature enemyCreature = new(enemyCard, data);

        Field fieldToPlace = gridManager.GridBoard.GetField(-1, -1);
        await gameBoard.SummonCreature(player, fieldToPlace, playerCreature);
        */

        
        boardConfig.ResetSettings();
        
        await UniTask.Delay(taskDelay);
        boardConfig.SetEastColumns(new List<int> { 0, 1, 1, 0 });
        gameBoard.SetBoardSettings(boardConfig);

        
        // Тестування змін конфігурації дошки
        // 1. Базове тестування колонок
        await UniTask.Delay(taskDelay);
        boardConfig.AddSouthRow(FieldType.Support); // Видалення першого рядка
        gameBoard.SetBoardSettings(boardConfig);
        
        
        await UniTask.Delay(taskDelay);
        boardConfig.SetEastColumns(GenerateRandomList(2, 6));// Спроба встановити кількість колонок нижче мінімальної
        gameBoard.SetBoardSettings(boardConfig);
        
        await UniTask.Delay(taskDelay);
        boardConfig.SetWestColumns(new List<int> { 0, 1, 1, 1 });// Спроба встановити кількість колонок нижче мінімальної
        gameBoard.SetBoardSettings(boardConfig);
        
        // 2. Базове тестування рядків
        await UniTask.Delay(taskDelay);
        boardConfig.AddSouthRow(FieldType.Support); // Додавання рядка Support
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.RemoveSouthRowAt(1); // Видалення останнього рядка
        gameBoard.SetBoardSettings(boardConfig);
        // */


        // 4. Тестування мінімальної та максимальної кількості рядків і колонок
        await UniTask.Delay(taskDelay);
        boardConfig.SetWestColumns(GenerateRandomList(25, 56));// Спроба встановити кількість колонок нижче мінімальної
        gameBoard.SetBoardSettings(boardConfig);
        //*/

        
        await UniTask.Delay(taskDelay);
        boardConfig.SetWestColumns(GenerateRandomList(0, 1));// Спроба встановити кількість колонок нижче мінімальної // Тестування мінімальної кількості колонок
        gameBoard.SetBoardSettings(boardConfig);


        await UniTask.Delay(taskDelay);
        boardConfig.AddSouthRow(FieldType.Support); // Додавання нового рядка
        boardConfig.AddNorthRow(FieldType.Support); // Додавання Support рядка в початок
        boardConfig.AddNorthRow(FieldType.Support); // Додавання Support рядка в початок
        gameBoard.SetBoardSettings(boardConfig);
       

        // 5. Складні сценарії
        await UniTask.Delay(taskDelay);
        boardConfig.SetWestColumns(GenerateRandomList(4, 5)); // Встановлення 4 колонок
        boardConfig.AddSouthRow(FieldType.Support); // Додавання Support рядка
        boardConfig.AddSouthRow(FieldType.Attack); // Додавання Attack рядка
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.AddNorthRow(FieldType.Support); // Додавання Support рядка
        boardConfig.RemoveNorthRowAt(1);
        gameBoard.SetBoardSettings(boardConfig);

        // 6. Масштабування конфігурації
        await UniTask.Delay(taskDelay);
        boardConfig.SetEastColumns(GenerateRandomList(10, 15));  // Встановлення великої кількості колонок
        gameBoard.SetBoardSettings(boardConfig);
        /*
        // 6. Масштабування конфігурації
        await UniTask.Delay(taskDelay);
        boardConfig.AddSouthRow(FieldType.Attack); // Додавання Attack рядка
        gameBoard.SetBoardSettings(boardConfig); 
        //*/
        Debug.Log("DebugLogic завершено.");
    }


    public void AssignOpponent(Opponent opponent) {
        AssignHPCellToOpponent(opponent);
        gameBoard.OpponentManager.RegisterOpponent(opponent);
    }

    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }

    internal void UpdateCursorPosition(Vector3? mouseWorldPosition) {
        if (!mouseWorldPosition.HasValue) {
            gameBoard.DeselectField();
            return;
        }
        Vector2Int? indexes = tableController.GetGridIndex(mouseWorldPosition.Value);
        if (!indexes.HasValue) {
            gameBoard.DeselectField();
            return;
        }

        Field field = gridManager.GridBoard.GetFieldAt(indexes.Value.x, indexes.Value.y);
        //debugObject.transform.position = mouseWorldPosition;
        if (field != null) {
            gameBoard.SelectField(field);
        } else {
            gameBoard.DeselectField();
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
        if (boardConfig != null) {
            boardConfig.ResetSettings();
        }
    }
}