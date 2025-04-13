using System;

public interface ICardHandView {
    event Action<CardView> CardClicked;

    void Cleanup();
    CardView CreateCardView();
    void DeselectCardView(CardView cardView);
    void RemoveCardUI(CardView cardView);
    void SelectCardView(CardView cardView);
    void SetInteractable(bool value);
}