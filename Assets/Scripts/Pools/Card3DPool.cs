
[System.Serializable]
public class Card3DPool : ComponentPool<Card3DView> {
    // ����� ������ ���������� ����� ��� ����
    protected override void OnTakeFromPool(Card3DView item) {
        base.OnTakeFromPool(item);
        // ���������� ����� ��� 3D ����
    }
}
