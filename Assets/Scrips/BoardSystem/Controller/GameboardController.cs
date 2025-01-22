using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameboardController : MonoBehaviour {
    //DEBUG
    [SerializeField] private HealthCellController playerCell;
    [SerializeField] private HealthCellController enemyCell;
    [SerializeField] private PlayerController player_c;
    [SerializeField] private EnemyController enemy_c;

    [SerializeField] private BoardSettings boardConfig;

    private Enemy enemy;
    private Player player;
    // Scene Context
    [Inject] private GameBoard gameBoard;
    [Inject] private GridManager gridManager;
    [Inject] private OpponentManager opponentManager;
    // GAme context
    [Inject] ResourceManager resManager;
    [Inject] BoardVisual tableController;

    //[SerializeField] Transform debugObject;

    private void Awake() {
        gameBoard.SetBoardSettings(boardConfig);
    }

    private void Start() {
        player = player_c.player;
        enemy = enemy_c.enemy;
        DebugLogic().Forget();
    }

    private async UniTaskVoid DebugLogic() {
        // Ініціалізація
        gameBoard.opponentManager.RegisterOpponent(player);
        gameBoard.opponentManager.RegisterOpponent(enemy);
        await gameBoard.StartGame();
        gameBoard.SetCurrentPlayer(player);

        Card playerCard = player.GetTestCard();
        Card enemyCard = enemy.GetTestCard();

        CreatureSO data = resManager.GetRandomResource<CreatureSO>(ResourceType.CREATURE);

        Creature playerCreature = new(playerCard, data);
        Creature enemyCreature = new(enemyCard, data);

        Field fieldToPlace = opponentManager.PlayerGrid.GetFieldAt(0, 0);
        await gameBoard.SummonCreature(player, fieldToPlace, playerCreature);

        int taskDelay = 1000;
        
        // Тестування змін конфігурації дошки
        // 1. Базове тестування колонок
        await UniTask.Delay(taskDelay);
        boardConfig.SetColumns(3); // Встановлення 3 колонок
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.SetColumns(7); // Встановлення 7 колонок
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.SetColumns(1); // Спроба встановити кількість колонок нижче мінімальної
        gameBoard.SetBoardSettings(boardConfig);

        // 2. Базове тестування рядків
        await UniTask.Delay(taskDelay);
        boardConfig.AddRow(FieldType.Support); // Додавання рядка Support
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.RemoveRow(); // Видалення останнього рядка
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.RemoveRowAt(0); // Видалення першого рядка
        gameBoard.SetBoardSettings(boardConfig);

        // 4. Тестування мінімальної та максимальної кількості рядків і колонок
        await UniTask.Delay(taskDelay);
        boardConfig.SetColumns(50); // Тестування великої кількості колонок
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.SetColumns(2); // Тестування мінімальної кількості колонок
        gameBoard.SetBoardSettings(boardConfig);


        await UniTask.Delay(taskDelay);
        boardConfig.AddRow(FieldType.Support); // Додавання нового рядка
        boardConfig.AddRowAt(FieldType.Support, 0); // Додавання Support рядка в початок
        boardConfig.AddRowAt(FieldType.Support, 0); // Додавання Support рядка в початок
        gameBoard.SetBoardSettings(boardConfig);


        // 5. Складні сценарії
        await UniTask.Delay(taskDelay);
        boardConfig.SetColumns(4); // Встановлення 4 колонок
        boardConfig.AddRow(FieldType.Support); // Додавання Support рядка
        boardConfig.AddRow(FieldType.Attack); // Додавання Attack рядка
        gameBoard.SetBoardSettings(boardConfig);

        await UniTask.Delay(taskDelay);
        boardConfig.AddRow(FieldType.Support); // Додавання Support рядка
        boardConfig.RemoveRow();
        gameBoard.SetBoardSettings(boardConfig);

        // 6. Масштабування конфігурації
        await UniTask.Delay(taskDelay);
        boardConfig.SetColumns(10); // Встановлення великої кількості колонок
        gameBoard.SetBoardSettings(boardConfig);

        // 6. Масштабування конфігурації
        await UniTask.Delay(taskDelay);
        boardConfig.AddRowAt(FieldType.Support, 0); // Додавання Support рядка в початок
        gameBoard.SetBoardSettings(boardConfig);
        Debug.Log("DebugLogic завершено.");
    }


    public void AssignOpponent(Opponent opponent) {
        AssignHPCellToOpponent(opponent);
        gameBoard.opponentManager.RegisterOpponent(opponent);
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

        Field field = gridManager.MainGrid.GetFieldAt(indexes.Value.x, indexes.Value.y);
        //debugObject.transform.position = mouseWorldPosition;
        if (field != null) {
            gameBoard.SelectField(field);
        } else {
            gameBoard.DeselectField();
        }
    }
}