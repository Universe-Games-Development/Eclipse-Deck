using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BoardAssigner {
    private BoardSeatSystem boardSeats;
    private BoardPresenter boardPresenter;
    public BoardAssigner(BoardSeatSystem boardSeats, BoardPresenter boardPresenter) {
        this.boardSeats = boardSeats;
        this.boardPresenter = boardPresenter;
    }

    public void HandleGridUpdate(BoardUpdateData data) {
        foreach (var pair in boardSeats.GetAllOpponentDirections()) {
            GridUpdateData gridUpdateData = data.GetUpdateByGlobalDirection(pair.Value);

            gridUpdateData?.addedFields?.ForEach(field => field.AssignOwner(pair.Key));
            gridUpdateData?.removedFields?.ForEach(field => field.UnassignOwner());
        }
    }

    public List<Creature> GetOpponentCreatures(Opponent endTurnOpponent) {
        List<Creature> creatures = new();
        foreach (var grid in GetOpponentGrids(endTurnOpponent)) {
            foreach (var row in grid.Fields) {
                foreach (var field in row) {
                    if (field != null && field.OccupiedCreature != null) {
                        creatures.Add(field.OccupiedCreature);
                    }
                }
            }
        }

        bool isPlayer = endTurnOpponent is Player;
        creatures.Sort((creatureA, creatureB) => {
            // Start comparing by row
            int rowA = creatureA.CurrentField.GetRow();
            int rowB = creatureB.CurrentField.GetRow();

            int rowComparison = isPlayer ? rowA.CompareTo(rowB) : rowB.CompareTo(rowA);
            if (rowComparison != 0) {
                return rowComparison;
            }
            // If rows are same compare by column
            int columnA = creatureA.CurrentField.GetColumn();
            int columnB = creatureB.CurrentField.GetColumn();
            return columnA.CompareTo(columnB);
        });
        return creatures;
    }

    public List<CompasGrid> GetOpponentGrids(Opponent opponent) {
        if (boardPresenter.GridBoard == null) {
            Debug.LogError("GridBoard not initialized during assignment");
            return new List<CompasGrid>();
        }

        if (!boardSeats.GetOpponentDirection(opponent, out var direction)) {
            Debug.LogWarning($"Direction not found for opponent {opponent}");
            return new List<CompasGrid>();
        }

        return boardPresenter.GridBoard.GetGridsByGlobalDirection(direction);
    }

    // When activated
    public void ReAssignGrids() {
        // Use LINQ and flatten the grid structure for more efficient iteration
        foreach (Opponent opponent in boardSeats.GetAllOpponents()) {
            GetOpponentGrids(opponent).ForEach(grid => grid.Fields.SelectMany(row => row).ToList().ForEach(field => field.AssignOwner(opponent)));
        }
    }
}