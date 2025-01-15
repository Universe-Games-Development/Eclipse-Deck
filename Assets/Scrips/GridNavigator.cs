using System.Collections.Generic;
using System.Linq;
using static UnityEditor.ShaderData;

public class GridNavigator {
    private readonly Grid grid;
    private const int DEFAULT_OFFSET = 1;
    public GridNavigator(Grid grid) {
        this.grid = grid;
    }

    public List<Field> GetAdjacentFields(Field currentField) {
        var adjacentFields = new List<Field>();
        var offsets = CompasUtil.GetOffsets();

        foreach (var (rowOffset, colOffset) in offsets) {
            int newRow = currentField.row + rowOffset;
            int newCol = currentField.column + colOffset;

            if (newRow >= 0 && newRow < grid.Fields.Count && newCol >= 0 && newCol < grid.Fields[0].Count) {
                adjacentFields.Add(grid.Fields[newRow][newCol]);
            }
        }

        return adjacentFields;
    }

    public List<Field> GetPath(Field currentField, int pathAmount, Direction direction, bool reversed = false) {
        List<Field> path = new();

        if (reversed) {
            direction = CompasUtil.GetOppositeDirection(direction);
        }

        var (rowOffset, colOffset) = CompasUtil.DirectionOffsets[direction];

        for (int i = 1; i <= pathAmount; i++) {
            int newRow = currentField.row + rowOffset * i;
            int newCol = currentField.column + colOffset * i;

            if (newRow >= 0 && newRow < grid.Fields.Count && newCol >= 0 && newCol < grid.Fields[0].Count) {
                path.Add(grid.Fields[newRow][newCol]);
            } else {
                break;
            }
        }

        return path;
    }

    public List<Field> GetFlankFields(Field field, int flankSize, bool isReversed) {
        List<Field> flankFields = new List<Field>();

        // Ліва сторона
        List<Field> leftFlank = GetPath(field, flankSize, Direction.West, isReversed);
        flankFields.AddRange(leftFlank);

        // Права сторона
        List<Field> rightFlank = GetPath(field, flankSize, Direction.East, isReversed);
        flankFields.AddRange(rightFlank);

        return flankFields;
    }


    public Grid GetGrid() {
        return grid;
    }

    public bool FieldExists(Field field) {
        return grid.Fields.Any(column => column?.Contains(field) == true);

    }

    public Field GetFieldAt(int row, int column) {
        return grid.Fields[row][column];
    }

    public bool IsFieldInEnemyZone(Field field) {
        return field.Owner is Enemy;
    }
}
