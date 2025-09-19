using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : MonoBehaviour {
    protected CardView hoveredCard;
    [SerializeField] protected bool allowMultipleHover = true;

    public virtual void RemoveCardView(CardView cardView) {
        if (cardView == null) return;

        if (hoveredCard == cardView) {
            hoveredCard = null;
        }

        HandleCardViewRemoval(cardView);
    }

    public virtual void Toggle(bool value) => gameObject.SetActive(value);

    public virtual void SetCardHover(CardView cardView, bool isHovered) {
        if (cardView == null) return;


        if (isHovered) {
            SetHoveredCard(cardView);
        } else {
            ClearHoveredCard();
        }
    }

    private void SetHoveredCard(CardView cardView) {
        ClearHoveredCard();

        hoveredCard = cardView;
        HandleCardHovered(hoveredCard);
    }

    private void ClearHoveredCard() {
        if (hoveredCard == null) return;

        HandleClearCardHovered(hoveredCard);
        hoveredCard = null;
    }



    public abstract CardView CreateCardView(Card card);
    public abstract void RegisterView(CardView cardView);
    protected abstract void HandleCardViewRemoval(CardView cardView);
    protected abstract void HandleCardHovered(CardView cardView);
    protected abstract void HandleClearCardHovered(CardView cardView);
    public abstract void UpdateCardPositions(List<CardView> cardViews);

    protected virtual void OnDestroy() {
        hoveredCard = null;
    }


}


