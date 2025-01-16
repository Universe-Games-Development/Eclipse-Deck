using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class TableController : MonoBehaviour {
    [SerializeField] private HealthCellController playerCell;
    [SerializeField] private HealthCellController enemyCell;


    //DEBUG
    [SerializeField] private Opponent player;
    [SerializeField] private Opponent enemy;
    // Scene Context
    [Inject] private GameBoard gameBoard;
    [Inject] private GridManager gridManager;
    // GAme context
    [Inject] ResourceManager resManager;

    private void Start() {
        DebugLogic().Forget();
    }


    private async UniTaskVoid DebugLogic() {
        gameBoard.opponentManager.RegisterOpponent(player);
        gameBoard.opponentManager.RegisterOpponent(enemy);
        gameBoard.StartGame();


        Card playerCard = player.GetTestCard();
        Card enemyCard = enemy.GetTestCard();


        CreatureSO data = resManager.GetRandomResource<CreatureSO>(ResourceType.CREATURE);

        Creature playerCreature = new Creature(playerCard, data);
        Creature enemyCreature = new(enemyCard, data);

        Field fieldToPlace = gridManager.PlayerGrid.GetFieldAt(0, 0);

        await gameBoard.SummonCreature(player, fieldToPlace, playerCreature);

        for (int i = 0; i < 20; i++) {
            Opponent currentOpponent = gameBoard.GetCurrentPlayer();
            await gameBoard.PerformTurn(currentOpponent);
        }

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


}