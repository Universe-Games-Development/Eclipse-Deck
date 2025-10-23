
public class CellPool : ComponentPool<Cell3DView> {
    // Можна додати специфічну логіку
    protected override void OnTakeFromPool(Cell3DView item) {
        base.OnTakeFromPool(item);
    }
}
