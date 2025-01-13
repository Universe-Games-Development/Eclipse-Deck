using System.Collections.Generic;
using System.Linq;
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

    private void Start() {
        DebugLogic();
    }

    private void DebugLogic() {
        gameBoard.RegisterOpponent(player);
        gameBoard.RegisterOpponent(enemy);
        gameBoard.StartGame();


        Card cardToPlay = player.GetTestCard();
        Creature creature = new Creature(cardToPlay);
        Field fieldToPlace = gameBoard.boardOverseer.GetFieldAt(0, 0);
        gameBoard.PlaceCreature(player, fieldToPlace, creature);
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