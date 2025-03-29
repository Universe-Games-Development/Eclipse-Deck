using UnityEngine;
using System.Collections.Generic;

public class GameBoardHealthSystem : MonoBehaviour {
    [SerializeField] private HealthCellView playerCell;
    [SerializeField] private HealthCellView enemyCell;
    
    public void AssignOpponents(List<Opponent> opponents) {
        playerCell.Initialize();
        playerCell.Initialize();
        foreach (var opponent in opponents) {
            if (opponent is Player) {
                playerCell.AssignOwner(opponent);
            } else { enemyCell.AssignOwner(opponent); }
        }
    }
}
