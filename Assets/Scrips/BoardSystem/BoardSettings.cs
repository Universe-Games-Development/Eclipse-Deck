using System;
using System.Collections.Generic;

[Serializable]
public class BoardSettings {
    public List<FieldType> rowTypes;
    public int columns;

    public int minPlayers;
}