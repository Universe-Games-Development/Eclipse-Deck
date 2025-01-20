using Cysharp.Threading.Tasks;
using System;
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
    // GAme context
    [Inject] ResourceManager resManager;
    [Inject] TableController tableController;

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
        gameBoard.SetBoardSettings(boardConfig);

        gameBoard.opponentManager.RegisterOpponent(player);
        gameBoard.opponentManager.RegisterOpponent(enemy);
        await gameBoard.StartGame();
        gameBoard.SetCurrentPlayer(player);


        Card playerCard = player.GetTestCard();
        Card enemyCard = enemy.GetTestCard();


        CreatureSO data = resManager.GetRandomResource<CreatureSO>(ResourceType.CREATURE);

        Creature playerCreature = new Creature(playerCard, data);
        Creature enemyCreature = new(enemyCard, data);

        Field fieldToPlace = gridManager.PlayerGrid.GetFieldAt(0, 0);

        await gameBoard.SummonCreature(player, fieldToPlace, playerCreature);

        //for (int i = 0; i < 20; i++) {
        //    Opponent currentOpponent = gameBoard.GetCurrentPlayer();
        //    await gameBoard.PerformTurn(currentOpponent);
        //}

        fieldToPlace = gridManager.PlayerGrid.GetFieldAt(0, 0);

        fieldToPlace = gridManager.PlayerGrid.GetFieldAt(1, 0);
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