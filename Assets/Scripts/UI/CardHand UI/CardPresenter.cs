using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardPresenter : MonoBehaviour {
    public Card Model;
    public CardView View;
    public CardPresenter(Card card, CardView cardView) {
        
    }

    public void Initialize(Card card, CardView cardView) {
        Model = card;
        View = cardView;
    }

    private void UpdateCost(int value) {
        View.CardInfo.UpdateCost(value);
    }

    private void UpdateAttack(int value) {
        View.CardInfo.UpdateHealth(value);
    }

    private void UpdateHealth(int value) {
        View.CardInfo.UpdateHealth(value);
    }

    private void UpdateDescriptionContent(Card card) {

    }

    internal void HandleRemoval() {
        View.RemoveCardView().Forget();
    }
}
