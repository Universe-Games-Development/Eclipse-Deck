using UnityEngine;
using Zenject;

public class GameboardController : MonoBehaviour {
    [SerializeField] private HealthCellController playerCell;
    [SerializeField] private HealthCellController enemyCell;
    // Scene Context
    [Inject] private GameBoard gameBoard;
    [Inject] private BoardUpdater gridManager;

    // GAme context
    [Inject] GridVisual gridVisual;

    private OpponentRegistrator OpponentRegistrator;
    [Inject]
    public void Construct(OpponentRegistrator opponentRegistrator) {
        OpponentRegistrator = opponentRegistrator;
        OpponentRegistrator.OnOpponentRegistered += AssignHPCellToOpponent;
    }

    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }

    public bool SummonCreature(Opponent opponent, Card selectedCard, Field field) {
        // Determine is that creature or spell

        // load resources for creature by card data
        Creature playerCreature = null;
        return gameBoard.SummonCreature(opponent, field, playerCreature);
    }

    public Field GetFieldByWorldPosition(Vector3? mouseWorldPosition) {
        if (!mouseWorldPosition.HasValue || gridVisual == null) {
            return null;
        }


        Vector2Int? indexes = gridVisual.GetGridIndex(mouseWorldPosition.Value);
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

    private void OnDestroy() {
        if (OpponentRegistrator != null)
        OpponentRegistrator.OnOpponentRegistered -= AssignHPCellToOpponent;
    }
}