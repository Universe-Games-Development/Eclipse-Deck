using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : MonoBehaviour {
    [SerializeField] protected HandLayoutStrategy layoutStrategy;
    [SerializeField] protected float cardMoveDuration = 0.2f;
    [SerializeField] protected int baseRenderOrder = 2800;

    public virtual void Toggle(bool value) => gameObject.SetActive(value);

    public CardTransform[] GetCardTransforms(int cardCount) =>
        layoutStrategy.CalculateCardTransforms(cardCount);

    public virtual void UpdateCardPositions(List<CardView> cardViews, float duration) {
        var transforms = GetCardTransforms(cardViews.Count);

        for (int i = 0; i < transforms.Length && i < cardViews.Count; i++) {
            var cardTransform = transforms[i];
            var cardView = cardViews[i];

            cardView.MoveTo(cardTransform.position, cardTransform.rotation, duration);
            cardView.SetRenderOrder(baseRenderOrder + i);
        }
    }

    public abstract CardView CreateCardView(Card card);
    public abstract void DestroyCardView(CardView cardView);
}

public abstract class HandLayoutStrategy : MonoBehaviour {
    public abstract CardTransform[] CalculateCardTransforms(int cardCount);
}

[System.Serializable]
public struct CardTransform {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public int sortingOrder;

    public CardTransform(Vector3 pos, Quaternion rot, Vector3 scl, int sorting = 0) {
        position = pos;
        rotation = rot;
        scale = scl;
        sortingOrder = sorting;
    }
}


