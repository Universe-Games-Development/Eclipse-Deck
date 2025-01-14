using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class TableController : MonoBehaviour {
    [SerializeField] private BoardSettings _boardSettings;

    [SerializeField] private HealthCell playerCell;
    [SerializeField] private HealthCell enemyCell;
    private GameBoard gameBoard;

    //DEBUG
    [SerializeField] private Opponent player;
    [SerializeField] private Opponent enemy;

    [Inject] ResourceManager resManager; 
    private void Awake() {
        gameBoard = new GameBoard(_boardSettings == null ? GenerateDefaultBoardSettings() : _boardSettings);
    }

    private void Start() {
        DebugLogic().Forget();
    }


    private async UniTaskVoid DebugLogic() {
        gameBoard.RegisterOpponent(player);
        gameBoard.RegisterOpponent(enemy);
        gameBoard.StartGame();


        Card playerCard = player.GetTestCard();
        Card enemyCard = enemy.GetTestCard();

        
        CreatureSO data = resManager.GetRandomResource<CreatureSO>(ResourceType.CREATURE);

        Creature playerCreature = new Creature(playerCard, data);
        Creature enemyCreature = new(enemyCard, data);

        Field fieldToPlace = gameBoard.boardOverseer.GetFieldAt(player, 0, 0);

        await gameBoard.SummonCreature(player, fieldToPlace, playerCreature);
        
        int turn = 0;
        for (int i = 0; i < 20; i++) {
            Opponent currentOpponent = gameBoard.GetCurrentPlayer();
            await gameBoard.PerformTurn(currentOpponent);
        }

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