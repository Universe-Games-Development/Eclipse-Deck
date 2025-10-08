using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandView : UnitView {
    [SerializeField] protected bool allowMultipleHover = true;
    

    public Action<CardView> OnCardClicked { get; internal set; }
    public Action<CardView, bool> OnCardHovered { get; internal set; }

    public virtual void RemoveCardView(CardView cardView) {
        if (cardView == null) return;

        HandleCardViewRemoval(cardView);
    }

    public virtual void Toggle(bool value) => gameObject.SetActive(value);

    public virtual void ToggleCardHover(CardView cardView, bool isHover) {
        if (cardView == null) return;


        if (isHover) {
            HandleCardHovered(cardView);
        } else {
            HandleClearCardHovered(cardView);
        }
    }

    public abstract CardView CreateCardView();
    public abstract void RegisterView(CardView cardView);
    protected abstract void HandleCardViewRemoval(CardView cardView);
    protected abstract void HandleCardHovered(CardView cardView);
    protected abstract void HandleClearCardHovered(CardView cardView);
    public abstract void UpdateCardPositions();
}


