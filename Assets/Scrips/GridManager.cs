using static UnityEngine.Rendering.STP;

public class GridManager {
    public Grid MainGrid { get; private set; }
    public SubGrid PlayerGrid { get; private set; }
    public SubGrid EnemyGrid { get; private set; }

    public GridManager(BoardSettings config) {
        UpdateGrid(config);
    }

    public void UpdateGrid(BoardSettings config) {
        int rows = config.rowTypes.Count;
        int columns = config.columns;
        int divider = config.rowTypes.FindIndex(row => row == FieldType.Attack);

        if (MainGrid != null) {
            MainGrid.UpdateGridSize(rows, columns);
            PlayerGrid.BoundToMainGrid(MainGrid, 0, divider);
            EnemyGrid.BoundToMainGrid(MainGrid, divider + 1, rows - 1);
        } else {
            MainGrid = new Grid(rows, columns);
            PlayerGrid = new SubGrid(MainGrid, 0, divider);
            EnemyGrid = new SubGrid(MainGrid, divider + 1, rows - 1);
        }
    }

    private void UpdateFieldTypes(BoardSettings config) {
        MainGrid.Fields
            .ForEach(row =>
            row.ForEach(field => { field.Type = config.rowTypes[field.row] == FieldType.Attack ? FieldType.Attack : FieldType.Support; }));
    }
}
