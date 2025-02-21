using UnityEngine;
using Zenject;

public class GameBoardHealthSystem : MonoBehaviour {
    [SerializeField] private HealthCellController playerCell;
    [SerializeField] private HealthCellController enemyCell;
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

    private void OnDestroy() {
        if (OpponentRegistrator != null)
            OpponentRegistrator.OnOpponentRegistered -= AssignHPCellToOpponent;
    }
}
