
[System.Serializable]
public class CardPool : ComponentPool<CardView> {
    // Можна додати специфічну логіку для карт
    protected override void OnTakeFromPool(CardView item) {
        base.OnTakeFromPool(item);
        // Специфічна логіка для 3D карт
    }
}
