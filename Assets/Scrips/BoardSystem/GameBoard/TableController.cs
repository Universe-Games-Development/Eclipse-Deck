using Cysharp.Threading.Tasks;
using UnityEngine;

public class TableController : MonoBehaviour {
    [SerializeField] private BoardSettings _boardSettings;

    [SerializeField] private HealthCell playerCell;
    [SerializeField] private HealthCell enemyCell;
    private GameBoard gameBoard;

    //DEBUG
    [SerializeField] private Opponent player;
    [SerializeField] private Opponent enemy;

    private void Awake() {
        gameBoard = new GameBoard(_boardSettings == null ? GenerateDefaultBoardSettings() : _boardSettings);
    }

    private void StartDebug() {
        DebugLogic().Forget();
    }


    private async UniTaskVoid DebugLogic() {
        gameBoard.RegisterOpponent(player);
        gameBoard.RegisterOpponent(enemy);
        gameBoard.StartGame();


        Card playerCard = player.GetTestCard();
        Card enemyCard = enemy.GetTestCard();

        Creature playerCreature = new(playerCard);
        Creature enemyCreature = new(enemyCard);

        Field fieldToPlace = gameBoard.boardOverseer.GetFieldAt(player, 0, 0);

        await gameBoard.SummonCreature(player, fieldToPlace, playerCreature);

        fieldToPlace = gameBoard.boardOverseer.GetFieldAt(enemy, 0, 0);

        fieldToPlace = gameBoard.boardOverseer.GetFieldAt(player, 1, 0);
    }

    public void AssignOpponent(Opponent opponent) {
        AssignHPCellToOpponent(opponent);
        gameBoard.RegisterOpponent(opponent);
    }

    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }

    private BoardSettings GenerateDefaultBoardSettings() {
        _boardSettings = new BoardSettings();
        _boardSettings.rowTypes[0] = FieldType.Attack;
        _boardSettings.rowTypes[1] = FieldType.Attack;
        _boardSettings.columns = 4;
        return _boardSettings;
    }
}