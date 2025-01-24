using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class OpponentGrid : Grid {

    private Opponent owner;

    public OpponentGrid(Grid mainGrid, int startRow, int endRow) {
        BoundToMainGrid(mainGrid, startRow, endRow);
    }

    public OpponentGrid BoundToMainGrid(Grid mainGrid, int startRow, int endRow) {
        if (mainGrid == null) {
            return this;
        }

        if (startRow < 0 || startRow >= mainGrid.Fields.Count || endRow < startRow || endRow > mainGrid.Fields.Count) {
            throw new System.ArgumentException("Invalid main grid or start/end row.");
        }

        // Призначаємо поля з головної сітки
        Fields = mainGrid.Fields.Skip(startRow).Take(endRow - startRow + 1).ToList();

        // Якщо owner не null, перевіряємо всі поля
        if (owner != null) {
            foreach (var row in Fields) {
                AddFields(row);
            }
        }

        return this;
    }

    public OpponentGrid CreateGridFromMainGrid(Grid mainGrid, int startRow, int endRow) {
        if (mainGrid == null) {
            return new OpponentGrid(null, 0, 0);
        }
        return new OpponentGrid(mainGrid, startRow, endRow);
    }

    // INITIALIZATION
    public void AssignGridToOwner(Opponent opponent) {
        if (owner != null) {
            Debug.LogWarning("Trying to assing assigned grid");
            return;
        }

        owner = opponent;
        foreach (var row in Fields) {
            foreach (var field in row) {
                field.AssignOwner(opponent);
            }
        }

        opponent.OnDefeat += UnassignGridOwner;
        Debug.Log($"Grid assigned to opponent {opponent.Name}.");

        foreach (var row in Fields) {
            foreach (var field in row) {
                field.AssignOwner(opponent);
            }
        }
    }

    public void UnassignGridOwner(Opponent opponent) {
        if (opponent != owner) {
            Debug.LogWarning("Wron opponent to unassign!");
            return;
        }
        foreach (var row in Fields) {
            foreach (var field in row) {
                field.RemoveOwner();
            }
        }
    }

    // UPDATE
    public void AddFields(List<Field> fields) {
        foreach (var field in fields) {
            if (field.Owner != owner) {
                field.AssignOwner(owner);
            }
        }
    }

    public void RemoveFields(List<Field> fields) {
        foreach (var field in fields) {
            field.RemoveOwner();
        }
    }
}
