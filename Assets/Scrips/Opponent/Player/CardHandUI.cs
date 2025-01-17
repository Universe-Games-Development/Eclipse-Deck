using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardHandUI : MonoBehaviour {

    private Dictionary<string, CardUI> idToCardUIMap = new();
    private Dictionary<CardUI, RectTransform> cardToLayoutMap = new();
    public CardUI SelectedCard { get; private set; }
    [Inject] private UIManager uiManager;

    [SerializeField] private ObjectDistributer cardDistributer;
    [SerializeField] private ObjectDistributer layoutDistributer;

    private UICardFactory cardFactory;
    private void Awake() {
        if (cardDistributer == null || layoutDistributer == null) {
            Debug.LogError("Object distributors are not set!");
            return;
        }
        cardFactory = new UICardFactory(cardDistributer, layoutDistributer);
    }

    internal void Initialize(CardHand hand) {
        hand.OnCardAdd += AddCard;
        hand.OnCardRemove += RemoveCard;
    }

    #region ADD / REMOVE
    public void AddCard(Card card) {
        var cardUI = cardFactory.CreateCard(card);
        var layoutElement = cardFactory.CreateLayoutElement();
        if (idToCardUIMap.ContainsKey(card.Id)) {
            Debug.LogWarning($"Card with ID {card.Id} already exists in UI.");
            return;
        }

        idToCardUIMap[card.Id] = cardUI;
        LinkCardToLayoutElement(cardUI, layoutElement);
        SubscribeToCardEvents(cardUI);
    }

    public void RemoveCard(Card card) {
        if (!idToCardUIMap.TryGetValue(card.Id, out CardUI cardUI)) {
            Debug.LogWarning($"Card with ID {card.Id} can't be found in UI!");
            return;
        }

        if (cardToLayoutMap.TryGetValue(cardUI, out RectTransform layoutElement)) {
            layoutDistributer.ReleaseObject(layoutElement.gameObject);
            cardToLayoutMap.Remove(cardUI);
        }

        UnsubscribeCardEvents(cardUI);
        idToCardUIMap.Remove(card.Id);
        cardDistributer.ReleaseObject(cardUI.gameObject);
    }

    #endregion

    #region ASSIST Methods

    private void LinkCardToLayoutElement(CardUI cardUI, RectTransform layoutElement) {
        if (cardUI.TryGetComponent(out SmoothLayoutElement smoothLayoutElement)) {
            smoothLayoutElement.Initialize(layoutElement);
        }
        cardToLayoutMap[cardUI] = layoutElement;
    }

    private void SubscribeToCardEvents(CardUI cardUI) {
        cardUI.OnCardClicked += OnCardSelected;
    }

    private void UnsubscribeCardEvents(CardUI cardUI) {
        cardUI.OnCardClicked -= OnCardSelected;
    }

    private CardUI GetCardUIById(string id) {
        idToCardUIMap.TryGetValue(id, out CardUI cardUI);
        return cardUI;
    }
    #endregion

    #region SELECT / DESELECT
    private void OnCardSelected(CardUI clickedCard) {
        if (SelectedCard == clickedCard) {
            return; // Do nothing if the clicked card is already selected
        }

        if (SelectedCard != null) {
            SelectedCard.DeselectCard();
        }

        clickedCard.SelectCard();
        SelectedCard = clickedCard;
        Debug.Log($"Selected card: {SelectedCard.name}");
    }

    public void DeselectCurrentCard() {
        if (SelectedCard != null) {
            SelectedCard.DeselectCard();
            SelectedCard = null;
        }
    }
    #endregion
}
