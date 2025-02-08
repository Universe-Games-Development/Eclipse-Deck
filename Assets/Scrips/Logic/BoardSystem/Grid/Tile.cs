using UnityEngine;

public class Tile {
    // local grids postion
    public Vector2 local;
    // for global logic set after init
    public Vector2 global;

    // Store field logic
    public Field value;

    public Tile(Vector2 local, Field value) {
        this.local = local;
        this.value = value;
    }
}
