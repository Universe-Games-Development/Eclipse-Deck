using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour {
    [SerializeField] private HealthCell playerCell;
    [SerializeField] private HealthCell enemyCell;

    private BoardOverseer boardOverseer;
    [SerializeField] private BoardSettings _boardSettings;
    private void Awake() {
        if (_boardSettings == null) {
            _boardSettings = new BoardSettings();
            _boardSettings.rowTypes[0] = FieldType.Attack;
            _boardSettings.rowTypes[1] = FieldType.Attack;
            _boardSettings.columns = 4;
        }
        boardOverseer = new BoardOverseer(_boardSettings);
    }

    public void AssignOpponent(Opponent opponent) {
        AssignHPCellToOpponent(opponent);
        boardOverseer.OccupyGrid(opponent);



        //DEBUG
        Grid grid = boardOverseer.GetGrid(opponent);
        List<List<Field>> fields = grid.Fields;
        foreach (var row in fields) {
            Debug.Log("Row type : " + row[0].Type + "Owner : " + row[0].Owner);
        }
        int totalRows = fields.Count;
        int totalElements = fields.SelectMany(row => row).Count();
        Debug.Log(totalElements);

    }

    private void AssignHPCellToOpponent(Opponent opponent) {
        if (opponent is Player) {
            playerCell.AssignOwner(opponent);
        } else { enemyCell.AssignOwner(opponent); }
    }
}