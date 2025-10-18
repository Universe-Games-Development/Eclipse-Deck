using System.Collections.Generic;
/// <summary>
/// Классы для представления состояния доски
/// </summary>
public class BoardState {
    public List<RowState> Rows { get; set; } = new List<RowState>();
}

public class RowState {
    public int RowIndex { get; set; }
    public List<ColumnState> Columns { get; set; } = new List<ColumnState>();
}

public class ColumnState {
    public int ColumnIndex { get; set; }
    public int MaxAreas { get; set; }
    public int OccupiedCount { get; set; }
    public int FreeCount { get; set; }
    public List<AreaState> Areas { get; set; } = new List<AreaState>();
}

public class AreaState {
    public int AreaIndex { get; set; }
    public bool IsOccupied { get; set; }
    public string CreatureId { get; set; }
}
