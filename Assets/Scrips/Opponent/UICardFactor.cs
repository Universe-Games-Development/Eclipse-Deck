using UnityEngine;

public class UICardFactory {
    private readonly IObjectDistributer cardDistributer;
    private readonly IObjectDistributer layoutDistributer;

    public UICardFactory(IObjectDistributer cardDistributer, IObjectDistributer layoutDistributer) {
        this.cardDistributer = cardDistributer;
        this.layoutDistributer = layoutDistributer;
    }

    public CardUI CreateCard(Card card) {
        var cardObj = cardDistributer.CreateObject();
        var cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI == null) {
            Debug.LogError("Card object does not have a CardUI component!");
            return null;
        }

        cardUI.Initialize(card);
        return cardUI;
    }

    public RectTransform CreateLayoutElement() {
        var layoutObj = layoutDistributer.CreateObject();
        var layoutTransform = layoutObj.GetComponent<RectTransform>();
        if (layoutTransform == null) {
            Debug.LogError("Layout object does not have a RectTransform component!");
            return null;
        }

        return layoutTransform;
    }
}