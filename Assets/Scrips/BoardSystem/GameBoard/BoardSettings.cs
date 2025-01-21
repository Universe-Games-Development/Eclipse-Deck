using System;
using System.Collections.Generic;

[Serializable]
public class BoardSettings {
    public List<FieldType> rowTypes;
    public CellSize cellSize = new CellSize { width = 1f, height = 1f };

    public int columns;
    public int minPlayers;
}

[Serializable]
public struct CellSize {
    public float width;
    public float height;
}