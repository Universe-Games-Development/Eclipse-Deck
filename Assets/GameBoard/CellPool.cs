
public class CellPool : ComponentPool<Cell3DView> {
    // ����� ������ ���������� �����
    protected override void OnTakeFromPool(Cell3DView item) {
        base.OnTakeFromPool(item);
    }
}
