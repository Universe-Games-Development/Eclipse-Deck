
[System.Serializable]
public class CardPool : ComponentPool<CardView> {
    // ����� ������ ���������� ����� ��� ����
    protected override void OnTakeFromPool(CardView item) {
        base.OnTakeFromPool(item);
        // ���������� ����� ��� 3D ����
    }
}
