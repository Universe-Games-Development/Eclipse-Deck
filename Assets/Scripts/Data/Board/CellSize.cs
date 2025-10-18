using System;

[Serializable]
public struct CellSize {
    public float width;
    public float height;

    // Конструктор
    public CellSize(float width, float height) {
        this.width = width;
        this.height = height;
    }
}
