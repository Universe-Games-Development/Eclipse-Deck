using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class TableController : MonoBehaviour {
    [SerializeField] private HealthCellController playerCell;
    [SerializeField] private HealthCellController enemyCell;


    //DEBUG
    [SerializeField] private PlayerController player_c;
    [SerializeField] private EnemyController enemy_c;
    private Enemy enemy;
    private Player player;

    // Scene Context
    [Inject] private GameBoard gameBoard;
    [Inject] private GridManager gridManager;
    // GAme context
    [Inject] ResourceManager resManager;

    private void Start() {
        player = player_c.player;
        enemy = enemy_c.enemy;
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
        if (opponent is PlayerController) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }


}