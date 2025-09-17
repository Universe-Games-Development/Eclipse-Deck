
[System.Serializable]
public class Card3DPool : ComponentPool<Card3DView> {
    // Можна додати специфічну логіку для карт
    protected override void OnTakeFromPool(Card3DView item) {
        base.OnTakeFromPool(item);
        // Специфічна логіка для 3D карт
    }
}
