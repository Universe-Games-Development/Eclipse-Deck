using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardPresenter : MonoBehaviour {
    public Card Card;
    public CardView View;
    public CardPresenter(Card card, CardView cardView) {
        
    }

    public void Initialize(Card card, CardView cardView) {
        Card = card;
        View = cardView;

        CardUIInfo cardInfo = cardView.CardInfo;
        cardInfo.BatchUpdate( cardInfo => {
            cardInfo.UpdateCost(card.Cost.CurrentValue);
            cardInfo.UpdateName(card.Data.Name);
        });
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
        View.PlayRemovalAnimation().Forget();
    }
}
