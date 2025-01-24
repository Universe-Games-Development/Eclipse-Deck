using System.Collections.Generic;

public class GridUpdateData {
    public List<Field> addedFields = new();
    public List<Field> removedFields = new();

    public bool IsInitialized { get; internal set; }
}