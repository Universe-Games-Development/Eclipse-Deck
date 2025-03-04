using System.Collections.Generic;

public class GridUpdateData {
    public List<Field> addedFields = new();
    public List<Field> removedFields = new();

    // marked empty fields will exist but not for game logic
    public List<Field> markedEmpty = new();


    public Direction direction;

    public GridUpdateData(Direction direction) {
        this.direction = direction;
    }

    public GridUpdateData(GridUpdateData other) {
        addedFields = new List<Field>(other.addedFields);
        removedFields = new List<Field>(other.removedFields);
        markedEmpty = new List<Field>(other.markedEmpty);
        direction = other.direction;
    }

    public bool HasChanges { get; internal set; }

    public void Clear() {
        addedFields.Clear();
        removedFields.Clear();
        markedEmpty.Clear();
    }
}