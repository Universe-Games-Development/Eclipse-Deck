using System;
using System.Collections.Generic;
using System.Linq;

public class FieldOverseer {
    private List<List<Field>> fieldGrid;
    private List<List<Field>> playerGrid;
    private List<List<Field>> enemyGrid;
    private readonly Dictionary<Opponent, List<List<Field>>> opponentFields = new();

    private IBoardMoveBehaviour moveStrategy = new DefaultMoveStrategy();
    private BoardSettings boardsSetting;

    public BoardSettings BoardsSetting {
        get => boardsSetting;
        set {
            if (value == null) throw new ArgumentNullException(nameof(BoardsSetting));
            boardsSetting = value;
            UpdateGrids();
        }
    }

    public FieldOverseer(BoardSettings format) {
        if (format == null) throw new ArgumentNullException(nameof(format));
        BoardsSetting = format;
        InitializeBoardGrids();
    }

    public void InitializeBoardGrids() {
        if (fieldGrid != null) return;
        GenerateOverallFieldGrid();
        GenerateOpponentsGrid();
    }

    private void UpdateGrids() {
        if (boardsSetting == null) throw new InvalidOperationException("Board settings must be initialized.");
        if (fieldGrid == null) InitializeBoardGrids();

        int newRowCount = boardsSetting.rowTypes.Count;
        int newColumnCount = boardsSetting.columns;

        if (fieldGrid.Count != newRowCount) {
            UpdateGridSize(newRowCount, newColumnCount);
        } else {
            // Only update field types if the size matches
            UpdateFieldsType();
        }

        UpdateOpponentGrids();
    }

    private void GenerateOverallFieldGrid() {
        if (boardsSetting == null) throw new InvalidOperationException("Board settings must be initialized.");
        fieldGrid = new List<List<Field>>(boardsSetting.rowTypes.Count);

        for (int i = 0; i < boardsSetting.rowTypes.Count; i++) {
            var row = new List<Field>(boardsSetting.columns);
            FieldType fieldType = boardsSetting.rowTypes[i];
            for (int j = 0; j < boardsSetting.columns; j++) {
                row.Add(new Field(fieldType));
            }
            fieldGrid.Add(row);
        }
    }

    private void UpdateGridSize(int newRowCount, int newColumnCount) {
        int rowDifference = newRowCount - fieldGrid.Count;
        if (rowDifference > 0) {
            for (int i = 0; i < rowDifference; i++) AddRow();
        } else {
            for (int i = 0; i < -rowDifference; i++) RemoveRow();
        }

        // Update column size
        foreach (var row in fieldGrid) {
            int columnDifference = newColumnCount - row.Count;
            if (columnDifference > 0) {
                for (int i = 0; i < columnDifference; i++) AddColumn(row);
            } else {
                for (int i = 0; i < -columnDifference; i++) RemoveColumn(row);
            }
        }
    }

    private void GenerateOpponentsGrid() {
        int playerZoneEndIndex = boardsSetting.rowTypes.FindIndex(row => row == FieldType.Attack);
        if (playerZoneEndIndex == -1) {
            throw new InvalidOperationException("Player zone end index not found. Ensure rowTypes contains FieldType.Attack.");
        }

        playerGrid = CreateSubGrid(0, playerZoneEndIndex);
        enemyGrid = CreateSubGrid(playerZoneEndIndex + 1, boardsSetting.rowTypes.Count - 1);
    }

    private void UpdateOpponentGrids() {
        playerGrid = CreateSubGrid(0, boardsSetting.rowTypes.FindIndex(row => row == FieldType.Attack));
        enemyGrid = CreateSubGrid(playerGrid.Count, boardsSetting.rowTypes.Count - 1);
        AssignFieldsToAllOpponents();
    }

    private List<List<Field>> CreateSubGrid(int startRow, int endRow) {
        int rows = endRow - startRow + 1;
        var subGrid = new List<List<Field>>(rows);

        for (int i = startRow; i <= endRow; i++) {
            var row = new List<Field>(boardsSetting.columns);
            for (int j = 0; j < boardsSetting.columns; j++) {
                row.Add(fieldGrid[i][j]);
            }
            subGrid.Add(row);
        }

        return subGrid;
    }

    public List<List<Field>> GetFieldGrid(Opponent opponent) {
        if (opponentFields.TryGetValue(opponent, out var fields)) {
            return fields;
        }

        throw new ArgumentException("No fields are assigned to this opponent.");
    }

    public List<List<Field>> GetFieldGrid() => fieldGrid;

    private void AddRow() {
        var newRow = new List<Field>(boardsSetting.columns);
        for (int i = 0; i < boardsSetting.columns; i++) {
            newRow.Add(new Field(boardsSetting.rowTypes[0]));
        }
        fieldGrid.Add(newRow);
    }

    private void AddColumn(List<Field> row) {
        row.Add(new Field(boardsSetting.rowTypes[0]));
    }

    private void RemoveRow() {
        fieldGrid.RemoveAt(fieldGrid.Count - 1);
    }

    private void RemoveColumn(List<Field> row) {
        row.RemoveAt(row.Count - 1);
    }

    private void UpdateFieldsType() {
        var attackingRows = new HashSet<int>(
            boardsSetting.rowTypes
            .Select((type, index) => new { type, index })
            .Where(x => x.type == FieldType.Attack)
            .Select(x => x.index)
        );

        foreach (var row in fieldGrid) {
            foreach (var field in row) {
                field.Type = attackingRows.Contains(row.IndexOf(field)) ? FieldType.Attack : FieldType.Support;
            }
        }
    }

    public void AssignFieldsToOpponent(Opponent opponent) {
        if (opponent == null) throw new ArgumentNullException(nameof(opponent));

        List<List<Field>> fieldToAssign = opponent switch {
            Player => playerGrid,
            Enemy => enemyGrid,
            _ => throw new ArgumentException("Can't assign grid to this opponent type.")
        };

        if (opponentFields.ContainsKey(opponent)) {
            throw new InvalidOperationException("This opponent already has assigned fields.");
        }

        opponent.OnDefeat += ClearOwner;
        foreach (var row in fieldToAssign) {
            foreach (var field in row) {
                field.AssignOwner(opponent);
            }
        }

        opponentFields[opponent] = fieldToAssign;
    }

    private void AssignFieldsToAllOpponents() {
        foreach (var opponent in opponentFields.Keys) {
            AssignFieldsToOpponent(opponent);
        }
    }

    private void ClearOwner(Opponent opponent) {
        if (opponentFields.TryGetValue(opponent, out var fields)) {
            foreach (var row in fields) {
                foreach (var field in row) {
                    field.AssignOwner(null);
                }
            }
            opponentFields.Remove(opponent);
        } else {
            throw new ArgumentException("There are no fields assigned to this opponent in the dictionary.");
        }
    }
}
