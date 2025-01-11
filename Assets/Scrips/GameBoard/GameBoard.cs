using Cysharp.Threading.Tasks.Triggers;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour {
    [SerializeField] private HealthCell playerCell;
    [SerializeField] private HealthCell enemyCell;

    private FieldOverseer _fieldOverseer;
    [SerializeField] private BoardSettings _boardSettings;
    private void Awake() {
        if (_boardSettings == null) {
            _boardSettings = new BoardSettings();
            _boardSettings.rowTypes[0] = FieldType.Attack;
            _boardSettings.rowTypes[1] = FieldType.Attack;
            _boardSettings.columns = 4;
        }
        _fieldOverseer = new FieldOverseer(_boardSettings);
        _fieldOverseer.InitializeBoardGrids();
    }

    public void AssignOpponent(Opponent opponent) {
        AssignHPCellToOpponent(opponent);
        _fieldOverseer.AssignFieldsToOpponent(opponent);



        //DEBUG
        int totalElements = _fieldOverseer.GetFieldGrid(opponent)
            .Where(row => row != null)
            .Sum(row => row.Count);

        Debug.Log(totalElements);

    }

    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }
}