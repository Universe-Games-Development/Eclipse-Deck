using System.Linq;

public class SubGrid : Grid {

    public SubGrid(Grid mainGrid, int startRow, int endRow) {
        BoundToMainGrid(mainGrid, startRow, endRow);
    }
    public void BoundToMainGrid(Grid mainGrid, int startRow, int endRow) {
        if (startRow < 0 || startRow >= mainGrid.Fields.Count || endRow < startRow || endRow > mainGrid.Fields.Count) {
            throw new System.ArgumentException("Invalid main grid or start/end row.");
        }

        Fields = mainGrid.Fields.Skip(startRow).Take(endRow - startRow + 1).ToList();
    }
}
