using UnityEngine;

[RequireComponent(typeof(FieldManager), typeof(CreatureSummoner))]
public class GameBoard : MonoBehaviour {
    [SerializeField] private HealthCell playerCell;
    [SerializeField] private HealthCell enemyCell;

    private FieldManager fieldManager;
    private CreatureSummoner summoningManager;

    private void Awake() {
        if (TryGetComponent(out fieldManager)) {
            fieldManager.GenerateGrid();
        }
        summoningManager = GetComponent<CreatureSummoner>();
    }

    public void AssignOpponent(Opponent opponent) {
        AssignHPCellToOpponent(opponent);
        fieldManager.AssignFieldsToOpponent(opponent);
    }

    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }


    public bool SummonCreature(Opponent player, Card card, Field field) {
        return summoningManager.SummonCreature(player, card, field, fieldManager);
    }
}