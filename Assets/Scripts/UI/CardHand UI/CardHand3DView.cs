using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class CardHand3DView : CardHandView {
    [Header("Pool")]
    [SerializeField] private Card3DView cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private Card3DPool cardPool;

    private void Awake() {
        ClearContainer();
    }

    public override CardView CreateCardView(Card card) {
        Card3DView card3DView = cardPool.Get();
        card3DView.transform.SetParent(cardsContainer);
        card3DView.SetPosition(cardsContainer.transform.position, cardsContainer.transform.rotation);
        return card3DView;
    }

    public override void DestroyCardView(CardView cardView) {
        if (cardView is Card3DView card3DView) {            
            cardPool.Release(card3DView);
        } else {
            Debug.LogWarning("Trying to destroy a CardView that is not a Card3DView.");
            Destroy(cardView.gameObject);
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

