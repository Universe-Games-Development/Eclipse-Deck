using System.Collections.Generic;
using System.Linq;

public class CellLayoutComponent : GridLayoutComponent<Cell3DView> {

    /// <summary>
    /// �������� ������� �� �����������
    /// </summary>
    public Cell3DView GetCellAt(int row, int col) {
        return GetItemAt(row, col);
    }

    /// <summary>
    /// ��������� �� ������� �������
    /// </summary>
    public bool IsCellOccupied(int row, int col) {
        return GetItemAt(row, col) != null;
    }

    /// <summary>
    /// �������� ����� �������
    /// </summary>
    public Cell3DView[] GetNeighborCells(int row, int col) {
        var neighbors = new List<Cell3DView>();

        // �����, ����, ������, ��������
        int[][] directions = new int[][] {
            new int[] {-1, 0}, // �����
            new int[] {1, 0},  // ����
            new int[] {0, -1}, // ������
            new int[] {0, 1}   // ��������
        };

        foreach (var dir in directions) {
            int newRow = row + dir[0];
            int newCol = col + dir[1];

            var cell = GetItemAt(newRow, newCol);
            if (cell != null) {
                neighbors.Add(cell);
            }
        }

        return neighbors.ToArray();
    }
}