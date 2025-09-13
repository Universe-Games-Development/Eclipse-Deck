using UnityEngine;

public class CardHand3DView : CardHandView {
    [Header("Pool")]
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Card3DPool cardPool;

    private void Awake() {
        ClearContainer();
    }

    public override CardView CreateCardView(Card card) {
        if (cardPool == null) {
            Debug.LogError("Card pool is not assigned!");
            return null;
        }

        Card3DView card3DView = cardPool.Get();
        if (card3DView != null && cardsContainer != null) {
            card3DView.transform.SetParent(cardsContainer);
            card3DView.transform.position = cardsContainer.position;
            card3DView.transform.rotation = cardsContainer.rotation;
        }

        return card3DView;
    }

    protected override void HandleCardViewRemoval(CardView cardView) {
        // Soon be as animation task
        if (cardView is Card3DView card3DView && cardPool != null) {
            cardPool.Release(card3DView);
        } else {
            Debug.LogWarning($"Trying to remove a CardView that is not a Card3DView or pool is null. Destroying: {cardView?.name}");
            if (cardView != null) {
                Destroy(cardView.gameObject);
            }
        }
    }

    private void ClearContainer() {
        if (cardsContainer == null) return;

        foreach (Transform child in cardsContainer) {
            if (child != null) {
                Destroy(child.gameObject);
            }
        }
    }
}

